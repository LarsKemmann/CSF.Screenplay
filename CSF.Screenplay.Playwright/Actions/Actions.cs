using System;
using System.Threading;
using System.Threading.Tasks;
using CSF.Screenplay.Playwright.Elements;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright.Actions
{
    /// <summary>Action: opens a URL in the actor's page.</summary>
    /// <remarks>
    /// <para>
    /// Use this action via <see cref="PerformableBuilder.OpenTheUrl(NamedUri)"/>; it does not rebase relative URIs — see
    /// <see cref="Tasks.NavigateToUrl"/> (via <see cref="PerformableBuilder.NavigateTo(NamedUri)"/>) for that.
    /// </para>
    /// </remarks>
    public class OpenUrl : IPerformable, ICanReport
    {
        readonly NamedUri uri;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} opens {Uri}", actor.Name, uri);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            if (!uri.Uri.IsAbsoluteUri)
                throw new InvalidOperationException($"The URI '{uri.Uri}' is relative; give the actor the {nameof(UseABaseUri)} ability and use {nameof(PerformableBuilder.NavigateTo)}, or use an absolute URI.");

            var page = await actor.GetPageAsync(cancellationToken).ConfigureAwait(false);
            await page.GotoAsync(uri.Uri.ToString()).ConfigureAwait(false);
        }

        /// <summary>Initializes a new instance of <see cref="OpenUrl"/>.</summary>
        /// <param name="uri">The URI to open; absolute</param>
        /// <exception cref="ArgumentNullException">If <paramref name="uri"/> is <see langword="null" />.</exception>
        public OpenUrl(NamedUri uri) { this.uri = uri ?? throw new ArgumentNullException(nameof(uri)); }
    }

    /// <summary>Action: clicks on an element.</summary>
    public class Click : ITargetPerformable
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} clicks on {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.ClickAsync().ConfigureAwait(false);
    }

    /// <summary>Action: double-clicks on an element.</summary>
    public class DoubleClick : ITargetPerformable
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} double-clicks on {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.DblClickAsync().ConfigureAwait(false);
    }

    /// <summary>Action: hovers the pointer over an element.</summary>
    public class Hover : ITargetPerformable
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} hovers over {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.HoverAsync().ConfigureAwait(false);
    }

    /// <summary>Action: fills an input or textarea with text, replacing its contents.</summary>
    public class Fill : ITargetPerformable
    {
        readonly string text;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} fills {Target} with {Text}", actor.Name, target, text);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.FillAsync(text).ConfigureAwait(false);

        /// <summary>Initializes a new instance of <see cref="Fill"/>.</summary>
        /// <param name="text">The text</param>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <see langword="null" />.</exception>
        public Fill(string text) { this.text = text ?? throw new ArgumentNullException(nameof(text)); }
    }

    /// <summary>Action: types text into an element one character at a time, as a user would, firing key events.</summary>
    public class Type : ITargetPerformable
    {
        readonly string text;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} types {Text} into {Target}", actor.Name, text, target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.PressSequentiallyAsync(text).ConfigureAwait(false);

        /// <summary>Initializes a new instance of <see cref="Type"/>.</summary>
        /// <param name="text">The text</param>
        /// <exception cref="ArgumentNullException">If <paramref name="text"/> is <see langword="null" />.</exception>
        public Type(string text) { this.text = text ?? throw new ArgumentNullException(nameof(text)); }
    }

    /// <summary>Action: presses a key (or key combination, such as <c>Control+A</c>) with an element focused.</summary>
    public class Press : ITargetPerformable
    {
        readonly string key;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} presses {Key} in {Target}", actor.Name, key, target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.PressAsync(key).ConfigureAwait(false);

        /// <summary>Initializes a new instance of <see cref="Press"/>.</summary>
        /// <param name="key">The key, in Playwright's key syntax</param>
        /// <exception cref="ArgumentNullException">If <paramref name="key"/> is <see langword="null" />.</exception>
        public Press(string key) { this.key = key ?? throw new ArgumentNullException(nameof(key)); }
    }

    /// <summary>Action: clears the contents of an input or textarea.</summary>
    public class ClearTheContents : ITargetPerformable
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} clears the contents of {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.ClearAsync().ConfigureAwait(false);
    }

    /// <summary>Action: checks (ticks) a checkbox or radio button.</summary>
    public class Check : ITargetPerformable
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} checks {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.CheckAsync().ConfigureAwait(false);
    }

    /// <summary>Action: unchecks (unticks) a checkbox.</summary>
    public class Uncheck : ITargetPerformable
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} unchecks {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.UncheckAsync().ConfigureAwait(false);
    }

    /// <summary>Action: selects option(s) of a <c>&lt;select&gt;</c> element by value or visible label.</summary>
    public class SelectOptions : ITargetPerformable
    {
        readonly string[] options;
        readonly bool byLabel;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format(byLabel ? "{Actor} selects the option(s) labelled {Options} from {Target}" : "{Actor} selects the option(s) with value(s) {Options} from {Target}",
                                actor.Name, string.Join(", ", options), target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
        {
            if (byLabel)
            {
                var values = new SelectOptionValue[options.Length];
                for (var i = 0; i < options.Length; i++) values[i] = new SelectOptionValue { Label = options[i] };
                await locator.SelectOptionAsync(values).ConfigureAwait(false);
            }
            else
            {
                await locator.SelectOptionAsync(options).ConfigureAwait(false);
            }
        }

        /// <summary>Initializes a new instance of <see cref="SelectOptions"/>.</summary>
        /// <param name="options">The option values or labels</param>
        /// <param name="byLabel">Whether <paramref name="options"/> are visible labels rather than values</param>
        /// <exception cref="ArgumentNullException">If <paramref name="options"/> is <see langword="null" />.</exception>
        public SelectOptions(string[] options, bool byLabel)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.byLabel = byLabel;
        }
    }

    /// <summary>Action: deselects every option of a multi-<c>&lt;select&gt;</c> element.</summary>
    public class DeselectAll : ITargetPerformable
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} deselects every option of {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.SelectOptionAsync(Array.Empty<string>()).ConfigureAwait(false);
    }

    /// <summary>Action: scrolls an element into view, if it is not already.</summary>
    public class ScrollIntoView : ITargetPerformable
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} scrolls {Target} into view", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.ScrollIntoViewIfNeededAsync().ConfigureAwait(false);
    }

    /// <summary>Action: waits until an element reaches a state — visible (the default), hidden, attached or detached.</summary>
    public class WaitForTheElement : ITargetPerformable
    {
        readonly WaitForSelectorState state;
        readonly float? timeout;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} waits until {Target} is {State}", actor.Name, target, state.ToString().ToLowerInvariant());

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.WaitForAsync(new LocatorWaitForOptions { State = state, Timeout = timeout }).ConfigureAwait(false);

        /// <summary>Initializes a new instance of <see cref="WaitForTheElement"/>.</summary>
        /// <param name="state">The state to wait for</param>
        /// <param name="timeout">An optional timeout in milliseconds; the context's default otherwise</param>
        public WaitForTheElement(WaitForSelectorState state = WaitForSelectorState.Visible, float? timeout = null)
        {
            this.state = state;
            this.timeout = timeout;
        }
    }

    /// <summary>Action: waits until the page's URL matches a pattern (a glob, or a regular expression).</summary>
    public class WaitForTheUrl : IPerformable, ICanReport
    {
        readonly string urlGlob;
        readonly float? timeout;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} waits until the URL matches {Pattern}", actor.Name, urlGlob);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            var page = await actor.GetPageAsync(cancellationToken).ConfigureAwait(false);
            await page.WaitForURLAsync(urlGlob, new PageWaitForURLOptions { Timeout = timeout }).ConfigureAwait(false);
        }

        /// <summary>Initializes a new instance of <see cref="WaitForTheUrl"/>.</summary>
        /// <param name="urlGlob">A glob the URL must match, such as <c>**/orders/*</c></param>
        /// <param name="timeout">An optional timeout in milliseconds</param>
        /// <exception cref="ArgumentNullException">If <paramref name="urlGlob"/> is <see langword="null" />.</exception>
        public WaitForTheUrl(string urlGlob, float? timeout = null)
        {
            this.urlGlob = urlGlob ?? throw new ArgumentNullException(nameof(urlGlob));
            this.timeout = timeout;
        }
    }

    /// <summary>Action: waits for a fixed period. Prefer waiting for an element or a URL; this is for the cases nothing observable marks.</summary>
    public class WaitForSomeTime : IPerformable, ICanReport
    {
        readonly TimeSpan duration;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} waits for {Duration}", actor.Name, duration);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
            => await Task.Delay(duration, cancellationToken).ConfigureAwait(false);

        /// <summary>Initializes a new instance of <see cref="WaitForSomeTime"/>.</summary>
        /// <param name="duration">The duration</param>
        public WaitForSomeTime(TimeSpan duration) { this.duration = duration; }
    }

    /// <summary>Action: executes JavaScript in the page and discards the result; see <see cref="Questions.ExecuteJavaScriptAndGetResult{T}"/> for one which returns it.</summary>
    public class ExecuteJavaScript : IPerformable, ICanReport
    {
        readonly string script;
        readonly object argument;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} executes the JavaScript {Script}", actor.Name, script);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            var page = await actor.GetPageAsync(cancellationToken).ConfigureAwait(false);
            await page.EvaluateAsync(script, argument).ConfigureAwait(false);
        }

        /// <summary>Initializes a new instance of <see cref="ExecuteJavaScript"/>.</summary>
        /// <param name="script">A JavaScript expression or function body (<c>arg => …</c>) to evaluate in the page</param>
        /// <param name="argument">An optional argument, serialised to the function</param>
        /// <exception cref="ArgumentNullException">If <paramref name="script"/> is <see langword="null" />.</exception>
        public ExecuteJavaScript(string script, object argument = null)
        {
            this.script = script ?? throw new ArgumentNullException(nameof(script));
            this.argument = argument;
        }
    }

    /// <summary>Action: deletes every cookie of the actor's browser context.</summary>
    public class ClearCookies : IPerformable, ICanReport
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} clears their cookies", actor.Name);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            var context = await actor.GetBrowserContextAsync(cancellationToken).ConfigureAwait(false);
            await context.ClearCookiesAsync().ConfigureAwait(false);
        }
    }
}
