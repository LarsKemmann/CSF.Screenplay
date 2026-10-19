using System;
using System.Threading;
using System.Threading.Tasks;
using CSF.Screenplay.Playwright.Actions;
using CSF.Screenplay.Playwright.Elements;

namespace CSF.Screenplay.Playwright.Tasks
{
    /// <summary>
    /// Task: navigates to a URL, rebasing a relative one to the actor's base URI (<see cref="UseABaseUri"/>) first.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this task via <see cref="PerformableBuilder.NavigateTo(NamedUri)"/>. An actor without the <see cref="UseABaseUri"/>
    /// ability may still navigate to absolute URIs.
    /// </para>
    /// </remarks>
    public class NavigateToUrl : IPerformable, ICanReport
    {
        readonly NamedUri uri;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} navigates to {Uri}", actor.Name, uri);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            var resolved = uri.Uri.IsAbsoluteUri || !actor.TryGetAbility<UseABaseUri>(out var baseUri) ? uri : uri.RebaseTo(baseUri.BaseUri);
            await actor.PerformAsync(new OpenUrl(resolved), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Initializes a new instance of <see cref="NavigateToUrl"/>.</summary>
        /// <param name="uri">The URI; relative or absolute</param>
        /// <exception cref="ArgumentNullException">If <paramref name="uri"/> is <see langword="null" />.</exception>
        public NavigateToUrl(NamedUri uri) { this.uri = uri ?? throw new ArgumentNullException(nameof(uri)); }
    }

    /// <summary>
    /// Task: clicks on an element and waits until the page's URL matches a pattern — a navigation the click triggers.
    /// </summary>
    public class ClickAndWaitForTheUrl : IPerformable, ICanReport
    {
        readonly ITarget target;
        readonly string urlGlob;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} clicks on {Target} and waits until the URL matches {Pattern}", actor.Name, target, urlGlob);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            await actor.PerformAsync(new TargetPerformableAdapter(new Click(), target), cancellationToken).ConfigureAwait(false);
            await actor.PerformAsync(new WaitForTheUrl(urlGlob), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>Initializes a new instance of <see cref="ClickAndWaitForTheUrl"/>.</summary>
        /// <param name="target">The element to click</param>
        /// <param name="urlGlob">A glob the URL must match afterwards</param>
        /// <exception cref="ArgumentNullException">If either parameter is <see langword="null" />.</exception>
        public ClickAndWaitForTheUrl(ITarget target, string urlGlob)
        {
            this.target = target ?? throw new ArgumentNullException(nameof(target));
            this.urlGlob = urlGlob ?? throw new ArgumentNullException(nameof(urlGlob));
        }
    }
}
