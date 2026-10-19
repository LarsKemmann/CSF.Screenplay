using System;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright.Elements
{
    /// <summary>
    /// A target is an object which represents one or more elements on a web page, and which may serve as a parameter for
    /// performables which interact with elements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A target is a specification for finding elements — a Playwright <see cref="ILocator"/> — together with a human-readable
    /// <see cref="IHasName.Name"/> for reporting. Playwright locators are lazy and auto-waiting: the elements are resolved when
    /// an action or question uses them, and Playwright waits for them to appear, become visible, stable and enabled as the
    /// operation requires. Targets are therefore cheap, reusable, and may be declared once, as <c>static readonly</c>
    /// fields of a class which describes a page.
    /// </para>
    /// <para>
    /// The locator-based targets — <see cref="CssSelector"/>, <see cref="TestId"/>, <see cref="Role"/>, <see cref="Text"/>,
    /// <see cref="Label"/>, <see cref="Placeholder"/>, <see cref="XPath"/> — build their locator from the page; a
    /// <see cref="PlaywrightElement"/> wraps a locator which was already obtained (from a question, say) and returns it as-is.
    /// </para>
    /// </remarks>
    public interface ITarget : IHasName
    {
        /// <summary>
        /// Gets a Playwright locator for the element(s) matching the current target, on the specified page.
        /// </summary>
        /// <param name="page">The page on which to locate elements</param>
        /// <returns>A locator</returns>
        ILocator GetLocator(IPage page);
    }

    /// <summary>
    /// Base type for targets which are specifications for finding elements, built from the page.
    /// </summary>
    public abstract class Locator : ITarget
    {
        readonly string name;

        /// <summary>
        /// Gets the specification string: a selector, a test id, a role name, a text, a label, a placeholder, an XPath expression.
        /// </summary>
        protected string Specification { get; }

        /// <inheritdoc/>
        public string Name => name ?? GetDefaultName();

        /// <inheritdoc/>
        public abstract ILocator GetLocator(IPage page);

        /// <summary>
        /// Gets a value for <see cref="Name"/> if none has been specified.
        /// </summary>
        /// <returns>A default name</returns>
        protected abstract string GetDefaultName();

        /// <inheritdoc/>
        public override string ToString() => Name;

        /// <summary>
        /// Initializes a new instance of <see cref="Locator"/>.
        /// </summary>
        /// <param name="specification">The specification string</param>
        /// <param name="name">An optional human-readable name of the elements the target matches</param>
        /// <exception cref="ArgumentNullException">If <paramref name="specification"/> is <see langword="null" />.</exception>
        protected Locator(string specification, string name = null)
        {
            Specification = specification ?? throw new ArgumentNullException(nameof(specification));
            this.name = name;
        }
    }

    /// <summary>A target which matches elements by CSS selector.</summary>
    public sealed class CssSelector : Locator
    {
        /// <inheritdoc/>
        public override ILocator GetLocator(IPage page) => page.Locator($"css={Specification}");

        /// <inheritdoc/>
        protected override string GetDefaultName() => $"the element(s) matching the CSS selector '{Specification}'";

        /// <summary>Initializes a new instance of <see cref="CssSelector"/>.</summary>
        /// <param name="selector">The CSS selector</param>
        /// <param name="name">An optional human-readable name</param>
        public CssSelector(string selector, string name = null) : base(selector, name) { }
    }

    /// <summary>A target which matches the element with a given <c>id</c> attribute.</summary>
    public sealed class ElementId : Locator
    {
        /// <inheritdoc/>
        public override ILocator GetLocator(IPage page) => page.Locator($"id={Specification}");

        /// <inheritdoc/>
        protected override string GetDefaultName() => $"the element with id '{Specification}'";

        /// <summary>Initializes a new instance of <see cref="ElementId"/>.</summary>
        /// <param name="id">The element id</param>
        /// <param name="name">An optional human-readable name</param>
        public ElementId(string id, string name = null) : base(id, name) { }
    }

    /// <summary>A target which matches elements by their <c>data-testid</c> attribute (Playwright's <c>GetByTestId</c>).</summary>
    public sealed class TestId : Locator
    {
        /// <inheritdoc/>
        public override ILocator GetLocator(IPage page) => page.GetByTestId(Specification);

        /// <inheritdoc/>
        protected override string GetDefaultName() => $"the element(s) with test id '{Specification}'";

        /// <summary>Initializes a new instance of <see cref="TestId"/>.</summary>
        /// <param name="testId">The test id</param>
        /// <param name="name">An optional human-readable name</param>
        public TestId(string testId, string name = null) : base(testId, name) { }
    }

    /// <summary>A target which matches elements by their ARIA role and, optionally, accessible name (Playwright's <c>GetByRole</c>).</summary>
    public sealed class Role : Locator
    {
        readonly AriaRole role;
        readonly bool exact;

        /// <inheritdoc/>
        public override ILocator GetLocator(IPage page)
            => page.GetByRole(role, Specification.Length == 0 ? null : new PageGetByRoleOptions { Name = Specification, Exact = exact });

        /// <inheritdoc/>
        protected override string GetDefaultName()
            => Specification.Length == 0 ? $"the {role} element(s)" : $"the {role} named '{Specification}'";

        /// <summary>Initializes a new instance of <see cref="Role"/>.</summary>
        /// <param name="role">The ARIA role</param>
        /// <param name="accessibleName">An optional accessible name to match</param>
        /// <param name="name">An optional human-readable name</param>
        /// <param name="exact">Whether the accessible name must match exactly (case-sensitive, whole string); a case-insensitive substring match otherwise</param>
        public Role(AriaRole role, string accessibleName = null, string name = null, bool exact = false) : base(accessibleName ?? string.Empty, name)
        {
            this.role = role;
            this.exact = exact;
        }
    }

    /// <summary>A target which matches elements by their text content (Playwright's <c>GetByText</c>).</summary>
    public sealed class Text : Locator
    {
        readonly bool exact;

        /// <inheritdoc/>
        public override ILocator GetLocator(IPage page) => page.GetByText(Specification, new PageGetByTextOptions { Exact = exact });

        /// <inheritdoc/>
        protected override string GetDefaultName() => $"the element(s) with the text '{Specification}'";

        /// <summary>Initializes a new instance of <see cref="Text"/>.</summary>
        /// <param name="text">The text</param>
        /// <param name="name">An optional human-readable name</param>
        /// <param name="exact">Whether the text must match exactly (case-sensitive, whole string); a case-insensitive substring match otherwise</param>
        public Text(string text, string name = null, bool exact = false) : base(text, name) { this.exact = exact; }
    }

    /// <summary>A target which matches form controls by the text of their label (Playwright's <c>GetByLabel</c>).</summary>
    public sealed class Label : Locator
    {
        readonly bool exact;

        /// <inheritdoc/>
        public override ILocator GetLocator(IPage page) => page.GetByLabel(Specification, new PageGetByLabelOptions { Exact = exact });

        /// <inheritdoc/>
        protected override string GetDefaultName() => $"the control labelled '{Specification}'";

        /// <summary>Initializes a new instance of <see cref="Label"/>.</summary>
        /// <param name="label">The label text</param>
        /// <param name="name">An optional human-readable name</param>
        /// <param name="exact">Whether the label must match exactly; a case-insensitive substring match otherwise</param>
        public Label(string label, string name = null, bool exact = false) : base(label, name) { this.exact = exact; }
    }

    /// <summary>A target which matches inputs by their placeholder text (Playwright's <c>GetByPlaceholder</c>).</summary>
    public sealed class Placeholder : Locator
    {
        readonly bool exact;

        /// <inheritdoc/>
        public override ILocator GetLocator(IPage page) => page.GetByPlaceholder(Specification, new PageGetByPlaceholderOptions { Exact = exact });

        /// <inheritdoc/>
        protected override string GetDefaultName() => $"the input with the placeholder '{Specification}'";

        /// <summary>Initializes a new instance of <see cref="Placeholder"/>.</summary>
        /// <param name="placeholder">The placeholder text</param>
        /// <param name="name">An optional human-readable name</param>
        /// <param name="exact">Whether the placeholder must match exactly; a case-insensitive substring match otherwise</param>
        public Placeholder(string placeholder, string name = null, bool exact = false) : base(placeholder, name) { this.exact = exact; }
    }

    /// <summary>A target which matches elements by XPath expression.</summary>
    public sealed class XPath : Locator
    {
        /// <inheritdoc/>
        public override ILocator GetLocator(IPage page) => page.Locator($"xpath={Specification}");

        /// <inheritdoc/>
        protected override string GetDefaultName() => $"the element(s) matching the XPath '{Specification}'";

        /// <summary>Initializes a new instance of <see cref="XPath"/>.</summary>
        /// <param name="xpath">The XPath expression</param>
        /// <param name="name">An optional human-readable name</param>
        public XPath(string xpath, string name = null) : base(xpath, name) { }
    }

    /// <summary>
    /// A target which wraps a Playwright locator that has already been obtained — from a question, or by narrowing another
    /// target with <see cref="TargetExtensions.Within(ITarget, ITarget, string)"/> — and returns it as-is.
    /// </summary>
    public sealed class PlaywrightElement : ITarget
    {
        /// <inheritdoc/>
        public string Name { get; }

        /// <summary>Gets the wrapped locator.</summary>
        public ILocator Locator { get; }

        /// <inheritdoc/>
        public ILocator GetLocator(IPage page) => Locator;

        /// <inheritdoc/>
        public override string ToString() => Name;

        /// <summary>Initializes a new instance of <see cref="PlaywrightElement"/>.</summary>
        /// <param name="locator">The locator</param>
        /// <param name="name">An optional human-readable name; the locator's own description when omitted</param>
        /// <exception cref="ArgumentNullException">If <paramref name="locator"/> is <see langword="null" />.</exception>
        public PlaywrightElement(ILocator locator, string name = null)
        {
            Locator = locator ?? throw new ArgumentNullException(nameof(locator));
            Name = name ?? $"the element {locator}";
        }
    }
}
