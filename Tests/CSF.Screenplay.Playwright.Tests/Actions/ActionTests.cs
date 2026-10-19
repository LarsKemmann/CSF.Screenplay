using CSF.Screenplay.Performables;
using CSF.Screenplay.Playwright.Elements;
using Microsoft.Playwright;
using static CSF.Screenplay.PerformanceStarter;
using static CSF.Screenplay.Playwright.PerformableBuilder;

namespace CSF.Screenplay.Playwright.Actions;

[Screenplay]
public class ClickTests(IStage stage)
{
    static readonly ITarget
        clickableButton = new ElementId("clickable", "the clickable button"),
        nonExistent = new ElementId("nope", "the non-existent element"),
        displayText = new ElementId("display", "the displayable text");

    static readonly NamedUri testPage = new("ClickTests.html", "the click test page");

    [Test]
    public async Task ClickingAButtonShouldTriggerAnEvent()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await When(webster).AttemptsTo(ClickOn(clickableButton));
        var contents = await Then(webster).Should(ReadFromTheElement(displayText).TheText());

        await Assert.That(contents).IsEqualTo("Clicked!");
    }

    [Test]
    public async Task ClickingByRoleShouldFindTheButtonByItsAccessibleName()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await When(webster).AttemptsTo(ClickOn(new Role(AriaRole.Button, "Click me")));
        var contents = await Then(webster).Should(ReadFromTheElement(displayText).TheText());

        await Assert.That(contents).IsEqualTo("Clicked!");
    }

    [Test]
    public async Task ClickingAnElementWhichDoesNotExistShouldThrowAfterTheTimeout()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));

        await Assert.That(async () => await When(webster).AttemptsTo(ClickOn(nonExistent)))
            .Throws<PerformableException>()
            .WithInnerException<TimeoutException>();
    }
}

[Screenplay]
public class TextEntryTests(IStage stage)
{
    static readonly ITarget
        inputArea = new ElementId("inputArea", "the input area"),
        displayText = new ElementId("display", "the displayable text");

    static readonly NamedUri testPage = new("SendKeysTests.html", "the text entry test page");

    [Test]
    public async Task FillingAnInputShouldUpdateTheDisplay()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await When(webster).AttemptsTo(Enter("Hello, Playwright").Into(inputArea));
        var contents = await Then(webster).Should(ReadFromTheElement(displayText).TheText());

        await Assert.That(contents).IsEqualTo("Hello, Playwright");
    }

    [Test]
    public async Task TypingKeyByKeyShouldUpdateTheDisplayAndTheValue()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await When(webster).AttemptsTo(TypeTheText("abc").Into(inputArea));
        var value = await Then(webster).Should(ReadFromTheElement(inputArea).TheValue());

        await Assert.That(value).IsEqualTo("abc");
    }

    [Test]
    public async Task ClearingTheContentsShouldEmptyTheInput()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await Given(webster).WasAbleTo(Enter("something").Into(inputArea));
        await When(webster).AttemptsTo(ClearTheContentsOf(inputArea));
        var value = await Then(webster).Should(ReadFromTheElement(inputArea).TheValue());

        await Assert.That(value).IsEmpty();
    }

    [Test]
    public async Task PressingAKeyShouldReachTheInput()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await Given(webster).WasAbleTo(Enter("abc").Into(inputArea));
        await When(webster).AttemptsTo(PressTheKey("Backspace", inputArea));
        var contents = await Then(webster).Should(ReadFromTheElement(displayText).TheText());

        await Assert.That(contents).IsEqualTo("ab");
    }
}

[Screenplay]
public class SelectionTests(IStage stage)
{
    static readonly ITarget
        select = new ElementId("selectElement", "the multi-select"),
        displayItems = new CssSelector("#display li", "the displayed selections");

    static readonly NamedUri testPage = new("SelectionTests.html", "the selection test page");

    [Test]
    public async Task SelectingByValueShouldReplaceTheSelection()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await When(webster).AttemptsTo(SelectFrom(select).TheOptionsWithValues("One", "Two"));
        var displayed = await Then(webster).Should(ReadFromTheElement(displayItems).TheTexts());

        await Assert.That(displayed).IsEquivalentTo(["First", "Second"]);
    }

    [Test]
    public async Task SelectingByLabelShouldSelectThatOption()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await When(webster).AttemptsTo(SelectFrom(select).TheOptionLabelled("Second"));
        var values = await Then(webster).Should(ReadFromTheElement(select).TheSelectedValues());

        await Assert.That(values).IsEquivalentTo(["Two"]);
    }

    [Test]
    public async Task DeselectingEverythingShouldLeaveNothingSelected()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await When(webster).AttemptsTo(DeselectEverythingIn(select));
        var count = await Then(webster).Should(CountThe(displayItems));

        await Assert.That(count).IsEqualTo(0);
    }
}

[Screenplay]
public class WaitTests(IStage stage)
{
    static readonly ITarget
        delay = new ElementId("delay", "the delay input"),
        clickableButton = new ElementId("clickable", "the clickable button"),
        elapsed = new CssSelector("#display span", "the elapsed time");

    static readonly NamedUri testPage = new("WaitTests.html", "the wait test page");

    [Test]
    public async Task WaitingForAnElementShouldSucceedOnceItAppears()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await Given(webster).WasAbleTo(Enter("500").Into(delay));
        await When(webster).AttemptsTo(ClickOn(clickableButton));
        await When(webster).AttemptsTo(WaitUntil(elapsed));
        var text = await Then(webster).Should(ReadFromTheElement(elapsed).TheText());

        await Assert.That(text).IsEqualTo("500");
    }

    [Test]
    public async Task WaitingForAnElementWhichNeverAppearsShouldTimeOut()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));

        await Assert.That(async () => await When(webster).AttemptsTo(WaitUntil(elapsed, timeout: 300)))
            .Throws<PerformableException>()
            .WithInnerException<TimeoutException>();
    }
}

[Screenplay]
public class CookieTests(IStage stage)
{
    static readonly ITarget
        getCookies = new ElementId("getCookies", "the get-cookies button"),
        cookiesList = new ElementId("cookiesList", "the cookies list");

    static readonly NamedUri testPage = new("ClearCookiesTests.html", "the cookies test page");

    [Test]
    public async Task ThePageShouldSetTwoCookiesWhichTheActorCanRead()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        var cookies = await Then(webster).Should(ReadTheCookies());

        await Assert.That(cookies.Select(c => c.Name).ToList()).IsEquivalentTo(["sampleCookie", "otherCookie"]);
    }

    [Test]
    public async Task ClearingTheCookiesShouldLeaveThePageWithNone()
    {
        var webster = stage.Spotlight<Webster>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await When(webster).AttemptsTo(ClearTheCookies());
        await When(webster).AttemptsTo(ClickOn(getCookies));
        var listed = await Then(webster).Should(ReadFromTheElement(cookiesList).TheText());

        await Assert.That(listed.Trim()).IsEmpty();
    }

    [Test]
    public async Task TwoActorsShouldHaveIsolatedCookies()
    {
        var webster = stage.Spotlight<Webster>();
        var pattie = stage.Cast.GetActor<Pattie>();

        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await Given(pattie).WasAbleTo(NavigateTo(new NamedUri(new Uri(Webster.TestWebappBaseUri, "OpenUrlTests.html"), "a page which sets no cookies")));

        var pattiesCookies = await Then(pattie).Should(ReadTheCookies());

        await Assert.That(pattiesCookies).IsEmpty();
    }
}
