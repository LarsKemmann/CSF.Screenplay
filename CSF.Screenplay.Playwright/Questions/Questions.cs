using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSF.Screenplay.Playwright.Actions;
using CSF.Screenplay.Playwright.Elements;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright.Questions
{
    /// <summary>Question: the text content of an element (its <c>innerText</c>, as rendered).</summary>
    public class TheText : ITargetPerformableWithResult<string>
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} reads the text of {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask<string> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.InnerTextAsync().ConfigureAwait(false);
    }

    /// <summary>Question: the text content of every element a target matches.</summary>
    public class TheTexts : ITargetPerformableWithResult<IReadOnlyList<string>>
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} reads the texts of {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask<IReadOnlyList<string>> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.AllInnerTextsAsync().ConfigureAwait(false);
    }

    /// <summary>Question: the value of an input, textarea or select element.</summary>
    public class TheValue : ITargetPerformableWithResult<string>
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} reads the value of {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask<string> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.InputValueAsync().ConfigureAwait(false);
    }

    /// <summary>Question: the value of an attribute of an element; <see langword="null"/> if absent.</summary>
    public class TheAttribute : ITargetPerformableWithResult<string>
    {
        readonly string attributeName;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} reads the {Attribute} attribute of {Target}", actor.Name, attributeName, target);

        /// <inheritdoc/>
        public async ValueTask<string> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.GetAttributeAsync(attributeName).ConfigureAwait(false);

        /// <summary>Initializes a new instance of <see cref="TheAttribute"/>.</summary>
        /// <param name="attributeName">The attribute name</param>
        /// <exception cref="ArgumentNullException">If <paramref name="attributeName"/> is <see langword="null" />.</exception>
        public TheAttribute(string attributeName) { this.attributeName = attributeName ?? throw new ArgumentNullException(nameof(attributeName)); }
    }

    /// <summary>Question: whether an element is visible. Does not wait: answers for the page as it is.</summary>
    public class TheVisibility : ITargetPerformableWithResult<bool>
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} looks whether {Target} is visible", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask<bool> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.IsVisibleAsync().ConfigureAwait(false);
    }

    /// <summary>Question: whether an element is enabled.</summary>
    public class TheEnabledState : ITargetPerformableWithResult<bool>
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} looks whether {Target} is enabled", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask<bool> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.IsEnabledAsync().ConfigureAwait(false);
    }

    /// <summary>Question: whether a checkbox or radio button is checked.</summary>
    public class TheCheckedState : ITargetPerformableWithResult<bool>
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} looks whether {Target} is checked", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask<bool> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.IsCheckedAsync().ConfigureAwait(false);
    }

    /// <summary>Question: how many elements a target matches, on the page as it is.</summary>
    public class TheCount : ITargetPerformableWithResult<int>
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} counts {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask<int> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.CountAsync().ConfigureAwait(false);
    }

    /// <summary>Question: the values of the selected options of a <c>&lt;select&gt;</c> element.</summary>
    public class TheSelectedValues : ITargetPerformableWithResult<IReadOnlyList<string>>
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} reads the selected values of {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask<IReadOnlyList<string>> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.EvaluateAsync<string[]>("e => Array.from(e.selectedOptions).map(o => o.value)").ConfigureAwait(false);
    }

    /// <summary>Question: a CSS property of an element, as computed by the browser.</summary>
    public class TheCssProperty : ITargetPerformableWithResult<string>
    {
        readonly string propertyName;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} reads the CSS property {Property} of {Target}", actor.Name, propertyName, target);

        /// <inheritdoc/>
        public async ValueTask<string> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
            => await locator.EvaluateAsync<string>("(e, name) => getComputedStyle(e).getPropertyValue(name)", propertyName).ConfigureAwait(false);

        /// <summary>Initializes a new instance of <see cref="TheCssProperty"/>.</summary>
        /// <param name="propertyName">The CSS property name, such as <c>background-color</c></param>
        /// <exception cref="ArgumentNullException">If <paramref name="propertyName"/> is <see langword="null" />.</exception>
        public TheCssProperty(string propertyName) { this.propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName)); }
    }

    /// <summary>Question: the elements a target matches, each as a <see cref="PlaywrightElement"/> target of its own.</summary>
    public class TheElements : ITargetPerformableWithResult<IReadOnlyList<ITarget>>
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} finds {Target}", actor.Name, target);

        /// <inheritdoc/>
        public async ValueTask<IReadOnlyList<ITarget>> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default)
        {
            var all = await locator.AllAsync().ConfigureAwait(false);
            var result = new List<ITarget>(all.Count);
            for (var i = 0; i < all.Count; i++) result.Add(new PlaywrightElement(all[i], $"the {Ordinal(i + 1)} of the elements"));
            return result;
        }

        static string Ordinal(int number)
        {
            if (number % 100 >= 11 && number % 100 <= 13) return $"{number}th";
            switch (number % 10)
            {
                case 1: return $"{number}st";
                case 2: return $"{number}nd";
                case 3: return $"{number}rd";
                default: return $"{number}th";
            }
        }
    }

    /// <summary>Question: the title of the actor's page.</summary>
    public class ThePageTitle : IPerformableWithResult<string>, ICanReport
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} reads the page title", actor.Name);

        /// <inheritdoc/>
        public async ValueTask<string> PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            var page = await actor.GetPageAsync(cancellationToken).ConfigureAwait(false);
            return await page.TitleAsync().ConfigureAwait(false);
        }
    }

    /// <summary>Question: the URL of the actor's page.</summary>
    public class ThePageUrl : IPerformableWithResult<Uri>, ICanReport
    {
        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} reads the page URL", actor.Name);

        /// <inheritdoc/>
        public async ValueTask<Uri> PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            var page = await actor.GetPageAsync(cancellationToken).ConfigureAwait(false);
            return new Uri(page.Url);
        }
    }

    /// <summary>Question: the cookies of the actor's browser context (optionally, for given URLs only).</summary>
    public class TheCookies : IPerformableWithResult<IReadOnlyList<BrowserContextCookiesResult>>, ICanReport
    {
        readonly IEnumerable<string> urls;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} reads their cookies", actor.Name);

        /// <inheritdoc/>
        public async ValueTask<IReadOnlyList<BrowserContextCookiesResult>> PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            var context = await actor.GetBrowserContextAsync(cancellationToken).ConfigureAwait(false);
            return await context.CookiesAsync(urls).ConfigureAwait(false);
        }

        /// <summary>Initializes a new instance of <see cref="TheCookies"/>.</summary>
        /// <param name="urls">Optional URLs to restrict the cookies to; all cookies when omitted</param>
        public TheCookies(IEnumerable<string> urls = null) { this.urls = urls; }
    }

    /// <summary>Question: executes JavaScript in the page and returns its result, deserialised to <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The result type</typeparam>
    public class ExecuteJavaScriptAndGetResult<T> : IPerformableWithResult<T>, ICanReport
    {
        readonly string script;
        readonly object argument;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} executes the JavaScript {Script} and gets the result", actor.Name, script);

        /// <inheritdoc/>
        public async ValueTask<T> PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            var page = await actor.GetPageAsync(cancellationToken).ConfigureAwait(false);
            return await page.EvaluateAsync<T>(script, argument).ConfigureAwait(false);
        }

        /// <summary>Initializes a new instance of <see cref="ExecuteJavaScriptAndGetResult{T}"/>.</summary>
        /// <param name="script">A JavaScript expression or function body (<c>arg => …</c>) to evaluate in the page</param>
        /// <param name="argument">An optional argument, serialised to the function</param>
        /// <exception cref="ArgumentNullException">If <paramref name="script"/> is <see langword="null" />.</exception>
        public ExecuteJavaScriptAndGetResult(string script, object argument = null)
        {
            this.script = script ?? throw new ArgumentNullException(nameof(script));
            this.argument = argument;
        }
    }
}
