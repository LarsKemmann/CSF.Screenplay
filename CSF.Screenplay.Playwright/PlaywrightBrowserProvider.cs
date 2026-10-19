using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright
{
    /// <summary>
    /// An object which gets the one Playwright browser of a <see cref="Screenplay"/>, launching it on first use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A browser is costly to launch and may be shared between performances, since every actor works within a browser
    /// context of their own; the <see cref="BrowseTheWeb"/> ability asks this service for the browser when the actor first
    /// needs a page.  The default implementation is <see cref="PlaywrightBrowserProvider"/>, a singleton which is disposed —
    /// closing the browser — when the Screenplay's service provider is.
    /// </para>
    /// </remarks>
    public interface IGetsBrowser
    {
        /// <summary>
        /// Gets the browser, launching it if it has not been launched yet.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The browser</returns>
        Task<IBrowser> GetBrowserAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the Playwright options the browser (and its contexts) are created with.
        /// </summary>
        PlaywrightOptions Options { get; }
    }

    /// <summary>
    /// The default <see cref="IGetsBrowser"/>: one <see cref="IPlaywright"/> and one <see cref="IBrowser"/> of the configured
    /// <see cref="PlaywrightOptions.BrowserType"/>, launched on first use and closed on disposal.
    /// </summary>
    public sealed class PlaywrightBrowserProvider : IGetsBrowser, IAsyncDisposable, IDisposable
    {
        readonly SemaphoreSlim launchLock = new SemaphoreSlim(1, 1);
        IPlaywright playwright;
        IBrowser browser;
        bool disposed;

        /// <inheritdoc/>
        public PlaywrightOptions Options { get; }

        /// <inheritdoc/>
        public async Task<IBrowser> GetBrowserAsync(CancellationToken cancellationToken = default)
        {
            if (disposed) throw new ObjectDisposedException(nameof(PlaywrightBrowserProvider));
            if (browser != null) return browser;

            await launchLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (browser != null) return browser;
                playwright = await Microsoft.Playwright.Playwright.CreateAsync().ConfigureAwait(false);
                browser = await GetBrowserType(playwright).LaunchAsync(Options.GetLaunchOptions()).ConfigureAwait(false);
                return browser;
            }
            finally
            {
                launchLock.Release();
            }
        }

        IBrowserType GetBrowserType(IPlaywright playwright)
        {
            var name = Options.BrowserType ?? Microsoft.Playwright.BrowserType.Chromium;
            if (string.Equals(name, Microsoft.Playwright.BrowserType.Chromium, StringComparison.OrdinalIgnoreCase)) return playwright.Chromium;
            if (string.Equals(name, Microsoft.Playwright.BrowserType.Firefox, StringComparison.OrdinalIgnoreCase)) return playwright.Firefox;
            if (string.Equals(name, Microsoft.Playwright.BrowserType.Webkit, StringComparison.OrdinalIgnoreCase)) return playwright.Webkit;
            throw new InvalidOperationException($"'{name}' is not a Playwright browser type; use {Microsoft.Playwright.BrowserType.Chromium}, {Microsoft.Playwright.BrowserType.Firefox} or {Microsoft.Playwright.BrowserType.Webkit}.");
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;

            if (browser != null) await browser.DisposeAsync().ConfigureAwait(false);
            playwright?.Dispose();
            launchLock.Dispose();
        }

        /// <inheritdoc/>
        public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();

        /// <summary>
        /// Initializes a new instance of <see cref="PlaywrightBrowserProvider"/>.
        /// </summary>
        /// <param name="options">The Playwright options</param>
        /// <exception cref="ArgumentNullException">If <paramref name="options"/> is <see langword="null" />.</exception>
        public PlaywrightBrowserProvider(IOptions<PlaywrightOptions> options)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));
            Options = options.Value;
        }
    }
}
