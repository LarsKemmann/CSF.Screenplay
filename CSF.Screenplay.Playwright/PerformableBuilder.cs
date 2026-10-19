using System;
using System.Collections.Generic;
using CSF.Screenplay.Playwright.Actions;
using CSF.Screenplay.Playwright.Elements;
using CSF.Screenplay.Playwright.Questions;
using CSF.Screenplay.Playwright.Tasks;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright
{
    /// <summary>
    /// Builder for the performables of the Playwright extension, for use with <c>using static CSF.Screenplay.Playwright.PerformableBuilder;</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The element-related builders take an <see cref="ITarget"/>, which names the element(s) for the report; the questions
    /// about an element are reached through <see cref="ReadFromTheElement(ITarget)"/> / <see cref="LookAtTheElement(ITarget)"/>,
    /// which return a <see cref="ElementQuestionBuilder"/>.
    /// </para>
    /// </remarks>
    public static class PerformableBuilder
    {
        // Navigation

        /// <summary>Opens an absolute URL; see <see cref="NavigateTo(NamedUri)"/> for relative URIs.</summary>
        public static IPerformable OpenTheUrl(NamedUri uri) => new OpenUrl(uri);

        /// <summary>Navigates to a URL, rebasing a relative one to the actor's base URI.</summary>
        public static IPerformable NavigateTo(NamedUri uri) => new NavigateToUrl(uri);

        /// <summary>Waits until the page's URL matches a glob.</summary>
        public static IPerformable WaitUntilTheUrlMatches(string urlGlob, float? timeout = null) => new WaitForTheUrl(urlGlob, timeout);

        /// <summary>Clicks on an element and waits until the page's URL matches a glob.</summary>
        public static IPerformable ClickOnAndWaitForTheUrl(ITarget target, string urlGlob) => new ClickAndWaitForTheUrl(target, urlGlob);

        // Element actions

        /// <summary>Clicks on an element.</summary>
        public static IPerformable ClickOn(ITarget target) => Adapt(new Click(), target);

        /// <summary>Double-clicks on an element.</summary>
        public static IPerformable DoubleClickOn(ITarget target) => Adapt(new DoubleClick(), target);

        /// <summary>Hovers over an element.</summary>
        public static IPerformable HoverOver(ITarget target) => Adapt(new Hover(), target);

        /// <summary>Fills an input with text, replacing its contents.</summary>
        public static IPerformable FillIn(ITarget target, string text) => Adapt(new Fill(text), target);

        /// <summary>Begins building a fill: <c>Enter("Jane").Into(nameField)</c>.</summary>
        public static TextIntoElementBuilder Enter(string text) => new TextIntoElementBuilder(text, false);

        /// <summary>Begins building a key-by-key type: <c>TypeTheText("Jane").Into(nameField)</c>.</summary>
        public static TextIntoElementBuilder TypeTheText(string text) => new TextIntoElementBuilder(text, true);

        /// <summary>Presses a key (Playwright key syntax, e.g. <c>Enter</c>, <c>Control+A</c>) with an element focused.</summary>
        public static IPerformable PressTheKey(string key, ITarget target) => Adapt(new Press(key), target);

        /// <summary>Clears the contents of an input.</summary>
        public static IPerformable ClearTheContentsOf(ITarget target) => Adapt(new ClearTheContents(), target);

        /// <summary>Checks a checkbox or radio button.</summary>
        public static IPerformable CheckThe(ITarget target) => Adapt(new Check(), target);

        /// <summary>Unchecks a checkbox.</summary>
        public static IPerformable UncheckThe(ITarget target) => Adapt(new Uncheck(), target);

        /// <summary>Begins building a selection: <c>SelectFrom(select).TheOptionsWithValues("a", "b")</c>.</summary>
        public static SelectBuilder SelectFrom(ITarget target) => new SelectBuilder(target);

        /// <summary>Deselects every option of a multi-select.</summary>
        public static IPerformable DeselectEverythingIn(ITarget target) => Adapt(new DeselectAll(), target);

        /// <summary>Scrolls an element into view.</summary>
        public static IPerformable ScrollIntoView(ITarget target) => Adapt(new Actions.ScrollIntoView(), target);

        /// <summary>Waits until an element is visible (or another state).</summary>
        public static IPerformable WaitUntil(ITarget target, WaitForSelectorState state = WaitForSelectorState.Visible, float? timeout = null)
            => Adapt(new WaitForTheElement(state, timeout), target);

        /// <summary>Waits for a fixed period.</summary>
        public static IPerformable WaitFor(TimeSpan duration) => new WaitForSomeTime(duration);

        // Page and context

        /// <summary>Executes JavaScript in the page, discarding the result.</summary>
        public static IPerformable ExecuteTheJavaScript(string script, object argument = null) => new ExecuteJavaScript(script, argument);

        /// <summary>Executes JavaScript in the page and gets its result.</summary>
        public static IPerformableWithResult<T> ExecuteTheJavaScriptAndGet<T>(string script, object argument = null) => new ExecuteJavaScriptAndGetResult<T>(script, argument);

        /// <summary>Deletes the actor's cookies.</summary>
        public static IPerformable ClearTheCookies() => new ClearCookies();

        /// <summary>Reads the actor's cookies.</summary>
        public static IPerformableWithResult<IReadOnlyList<BrowserContextCookiesResult>> ReadTheCookies(IEnumerable<string> urls = null) => new TheCookies(urls);

        /// <summary>Reads the page title.</summary>
        public static IPerformableWithResult<string> ReadThePageTitle() => new ThePageTitle();

        /// <summary>Reads the page URL.</summary>
        public static IPerformableWithResult<Uri> ReadThePageUrl() => new ThePageUrl();

        /// <summary>Saves a screenshot as a report asset.</summary>
        public static IPerformable SaveAScreenshot(string name = null, bool fullPage = false) => new Actions.SaveAScreenshot(name, fullPage);

        /// <summary>Saves the Playwright trace as a report asset (requires <see cref="PlaywrightOptions.RecordTraces"/>).</summary>
        public static IPerformable SaveTheTrace(string name = null) => new Actions.SaveTheTrace(name);

        // Element questions

        /// <summary>Begins building a question about an element: <c>ReadFromTheElement(display).TheText()</c>.</summary>
        public static ElementQuestionBuilder ReadFromTheElement(ITarget target) => new ElementQuestionBuilder(target);

        /// <summary>Begins building a question about an element: <c>LookAtTheElement(button).AndSeeWhetherItIsEnabled()</c>.</summary>
        public static ElementQuestionBuilder LookAtTheElement(ITarget target) => new ElementQuestionBuilder(target);

        /// <summary>Counts the elements a target matches.</summary>
        public static IPerformableWithResult<int> CountThe(ITarget target) => Adapt(new TheCount(), target);

        /// <summary>Finds the elements a target matches, each as a target of its own.</summary>
        public static IPerformableWithResult<IReadOnlyList<ITarget>> FindThe(ITarget target) => Adapt(new TheElements(), target);

        static IPerformable Adapt(ITargetPerformable performable, ITarget target) => new TargetPerformableAdapter(performable, target);
        static IPerformableWithResult<T> Adapt<T>(ITargetPerformableWithResult<T> performable, ITarget target) => new TargetPerformableWithResultAdapter<T>(performable, target);

        /// <summary>Builder for questions about one element.</summary>
        public sealed class ElementQuestionBuilder
        {
            readonly ITarget target;

            /// <summary>The element's rendered text.</summary>
            public IPerformableWithResult<string> TheText() => Adapt(new Questions.TheText(), target);

            /// <summary>The texts of every element the target matches.</summary>
            public IPerformableWithResult<IReadOnlyList<string>> TheTexts() => Adapt(new Questions.TheTexts(), target);

            /// <summary>The element's input value.</summary>
            public IPerformableWithResult<string> TheValue() => Adapt(new Questions.TheValue(), target);

            /// <summary>An attribute of the element.</summary>
            public IPerformableWithResult<string> TheAttribute(string name) => Adapt(new Questions.TheAttribute(name), target);

            /// <summary>A computed CSS property of the element.</summary>
            public IPerformableWithResult<string> TheCssProperty(string name) => Adapt(new Questions.TheCssProperty(name), target);

            /// <summary>The selected values of a select element.</summary>
            public IPerformableWithResult<IReadOnlyList<string>> TheSelectedValues() => Adapt(new Questions.TheSelectedValues(), target);

            /// <summary>Whether the element is visible.</summary>
            public IPerformableWithResult<bool> AndSeeWhetherItIsVisible() => Adapt(new TheVisibility(), target);

            /// <summary>Whether the element is enabled.</summary>
            public IPerformableWithResult<bool> AndSeeWhetherItIsEnabled() => Adapt(new TheEnabledState(), target);

            /// <summary>Whether the element is checked.</summary>
            public IPerformableWithResult<bool> AndSeeWhetherItIsChecked() => Adapt(new TheCheckedState(), target);

            internal ElementQuestionBuilder(ITarget target) { this.target = target ?? throw new ArgumentNullException(nameof(target)); }
        }

        /// <summary>Builder for entering text into an element.</summary>
        public sealed class TextIntoElementBuilder
        {
            readonly string text;
            readonly bool keyByKey;

            /// <summary>Names the element to enter the text into.</summary>
            public IPerformable Into(ITarget target) => keyByKey ? Adapt(new Actions.Type(text), target) : Adapt(new Fill(text), target);

            internal TextIntoElementBuilder(string text, bool keyByKey) { this.text = text ?? throw new ArgumentNullException(nameof(text)); this.keyByKey = keyByKey; }
        }

        /// <summary>Builder for selecting options of a select element.</summary>
        public sealed class SelectBuilder
        {
            readonly ITarget target;

            /// <summary>Selects the option(s) with the given value(s).</summary>
            public IPerformable TheOptionsWithValues(params string[] values) => Adapt(new SelectOptions(values, byLabel: false), target);

            /// <summary>Selects the option with the given value.</summary>
            public IPerformable TheOptionWithValue(string value) => TheOptionsWithValues(value);

            /// <summary>Selects the option(s) with the given visible label(s).</summary>
            public IPerformable TheOptionsLabelled(params string[] labels) => Adapt(new SelectOptions(labels, byLabel: true), target);

            /// <summary>Selects the option with the given visible label.</summary>
            public IPerformable TheOptionLabelled(string label) => TheOptionsLabelled(label);

            internal SelectBuilder(ITarget target) { this.target = target ?? throw new ArgumentNullException(nameof(target)); }
        }
    }
}
