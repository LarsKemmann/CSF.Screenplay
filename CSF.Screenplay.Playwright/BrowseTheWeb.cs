using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright
{
    /// <summary>
    /// Screenplay ability which allows an <see cref="Actor"/> to browse the web using Microsoft Playwright.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The 'Browse the Web' ability gives the actor a Playwright <see cref="IBrowserContext"/> of their own — an incognito-like
    /// profile with isolated cookies and storage — and a <see cref="IPage"/> within it, both created on first use and
    /// closed when the ability is disposed (at the end of the actor's performance).  Because every actor has their own
    /// context, two actors in one performance may be signed in to the same application as different users.
    /// </para>
    /// <para>
    /// The browser itself is shared: it comes from <see cref="IGetsBrowser"/>, which the Playwright Extension to Screenplay
    /// makes available via dependency injection (<see cref="ServiceCollectionExtensions.AddPlaywright(Microsoft.Extensions.DependencyInjection.IServiceCollection, Action{PlaywrightOptions})"/>).
    /// The recommended way in which to grant an Actor this ability is via an <see cref="IPersona"/>; the browser provider may
    /// be safely constructor-injected into the persona class.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code language="csharp">
    /// using CSF.Screenplay.Playwright;
    ///
    /// public class Webster(IGetsBrowser browsers) : IPersona
    /// {
    ///     public string Name => "Webster";
    ///
    ///     public Actor GetActor(Guid performanceIdentity)
    ///     {
    ///         var webster = new Actor(Name, performanceIdentity);
    ///         webster.IsAbleTo(new BrowseTheWeb(browsers));
    ///         return webster;
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IGetsBrowser"/>
    /// <seealso cref="PlaywrightOptions"/>
    public class BrowseTheWeb : ICanReport, IAsyncDisposable, IDisposable
    {
        readonly IGetsBrowser browserProvider;
        readonly SemaphoreSlim pageLock = new SemaphoreSlim(1, 1);
        IBrowserContext context;
        IPage page;
        bool disposed;

        /// <summary>
        /// Gets the options with which the browser and this actor's context are created.
        /// </summary>
        public PlaywrightOptions Options => browserProvider.Options;

        /// <summary>
        /// Gets a value indicating whether the actor has opened a page yet; until they have, there is no browser context either.
        /// </summary>
        public bool HasPage => page != null;

        /// <summary>
        /// Gets the actor's browser context, creating it (and the page within it) on first use.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The browser context</returns>
        public async Task<IBrowserContext> GetContextAsync(CancellationToken cancellationToken = default)
        {
            await GetPageAsync(cancellationToken).ConfigureAwait(false);
            return context;
        }

        /// <summary>
        /// Gets the actor's page, creating the browser context and the page on first use.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The page</returns>
        public async Task<IPage> GetPageAsync(CancellationToken cancellationToken = default)
        {
            if (disposed) throw new ObjectDisposedException(nameof(BrowseTheWeb));
            if (page != null) return page;

            await pageLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (page != null) return page;

                var browser = await browserProvider.GetBrowserAsync(cancellationToken).ConfigureAwait(false);
                var options = browserProvider.Options;
                context = await browser.NewContextAsync(options.GetContextOptions()).ConfigureAwait(false);
                if (options.DefaultTimeout.HasValue) context.SetDefaultTimeout(options.DefaultTimeout.Value);
                if (options.RecordTraces)
                    await context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true, Sources = false }).ConfigureAwait(false);

                page = await context.NewPageAsync().ConfigureAwait(false);
                return page;
            }
            finally
            {
                pageLock.Release();
            }
        }

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} is able to browse the web with Playwright ({Browser})", actor.Name, Options.BrowserType);

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;

            if (context != null)
            {
                if (browserProvider.Options.RecordTraces)
                {
                    try { await context.Tracing.StopAsync().ConfigureAwait(false); }
                    catch (PlaywrightException) { /* the trace was already stopped and saved, or the context is gone */ }
                }

                await context.CloseAsync().ConfigureAwait(false);
            }

            pageLock.Dispose();
        }

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

        /// <summary>
        /// Initializes a new instance of <see cref="BrowseTheWeb"/>.
        /// </summary>
        /// <param name="browserProvider">The provider of the (shared) browser</param>
        /// <exception cref="ArgumentNullException">If <paramref name="browserProvider"/> is <see langword="null" />.</exception>
        public BrowseTheWeb(IGetsBrowser browserProvider)
        {
            this.browserProvider = browserProvider ?? throw new ArgumentNullException(nameof(browserProvider));
        }
    }
}
