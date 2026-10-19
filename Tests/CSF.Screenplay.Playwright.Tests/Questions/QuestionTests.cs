using CSF.Screenplay.Playwright.Elements;
using static CSF.Screenplay.PerformanceStarter;
using static CSF.Screenplay.Playwright.PerformableBuilder;

namespace CSF.Screenplay.Playwright.Questions;

[Screenplay]
public class QueryTests(IStage stage)
{
    static readonly ITarget
        passwordInput = new ElementId("passwordInput", "the password input"),
        buttons = new CssSelector(".maybeClickable", "the maybe-clickable buttons"),
        backgroundDiv = new ElementId("backgroundDiv", "the coloured div"),
        hiddenDiv = new ElementId("hiddenDiv", "the hidden div"),
        multiSelect = new ElementId("multiSelect", "the multi-select");

    static readonly NamedUri testPage = new("QueriesTests.html", "the queries test page");

    [Test]
    public async Task TheValueOfAnInputShouldBeReadable()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        var value = await Then(webster).Should(ReadFromTheElement(passwordInput).TheValue());

        await Assert.That(value).IsEqualTo("secret");
    }

    [Test]
    public async Task AnAttributeShouldBeReadable()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        var type = await Then(webster).Should(ReadFromTheElement(passwordInput).TheAttribute("type"));

        await Assert.That(type).IsEqualTo("password");
    }

    [Test]
    public async Task EnabledAndDisabledButtonsShouldBeToldApart()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        var firstEnabled = await Then(webster).Should(LookAtTheElement(buttons.First()).AndSeeWhetherItIsEnabled());
        var secondEnabled = await Then(webster).Should(LookAtTheElement(buttons.Nth(1)).AndSeeWhetherItIsEnabled());

        using (Assert.Multiple())
        {
            await Assert.That(firstEnabled).IsTrue();
            await Assert.That(secondEnabled).IsFalse();
        }
    }

    [Test]
    public async Task AHiddenElementShouldNotBeVisible()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        var hidden = await Then(webster).Should(LookAtTheElement(hiddenDiv).AndSeeWhetherItIsVisible());
        var shown = await Then(webster).Should(LookAtTheElement(backgroundDiv).AndSeeWhetherItIsVisible());

        using (Assert.Multiple())
        {
            await Assert.That(hidden).IsFalse();
            await Assert.That(shown).IsTrue();
        }
    }

    [Test]
    public async Task ACssPropertyShouldBeReadableAsComputed()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        var colour = await Then(webster).Should(ReadFromTheElement(backgroundDiv).TheCssProperty("background-color"));

        await Assert.That(colour).IsEqualTo("rgb(170, 170, 255)");
    }

    [Test]
    public async Task CountingAndFindingElementsShouldAgree()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        var count = await Then(webster).Should(CountThe(buttons));
        var found = await Then(webster).Should(FindThe(buttons));
        var secondText = await Then(webster).Should(ReadFromTheElement(found[1]).TheText());

        using (Assert.Multiple())
        {
            await Assert.That(count).IsEqualTo(2);
            await Assert.That(found).Count().IsEqualTo(2);
            await Assert.That(secondText).IsEqualTo("Disabled");
        }
    }

    [Test]
    public async Task TheSelectedValuesOfAMultiSelectShouldBeReadable()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        var selected = await Then(webster).Should(ReadFromTheElement(multiSelect).TheSelectedValues());

        await Assert.That(selected).Contains("option1");
    }

    [Test]
    public async Task JavaScriptShouldBeExecutableWithAResult()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        var sum = await Then(webster).Should(ExecuteTheJavaScriptAndGet<int>("arg => arg.a + arg.b", new { a = 2, b = 3 }));

        await Assert.That(sum).IsEqualTo(5);
    }
}

[Screenplay]
public class PageTests(IStage stage)
{
    static readonly ITarget textContent = new ElementId("textContent", "the text content");
    static readonly NamedUri testPage = new("OpenUrlTests.html", "the open-URL test page");

    [Test]
    public async Task ThePageTitleAndUrlShouldBeReadable()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        var title = await Then(webster).Should(ReadThePageTitle());
        var url = await Then(webster).Should(ReadThePageUrl());

        using (Assert.Multiple())
        {
            await Assert.That(title).IsEqualTo("Open a URL test page");
            await Assert.That(url.AbsolutePath).IsEqualTo("/OpenUrlTests.html");
        }
    }

    [Test]
    public async Task NavigatingShouldRebaseARelativeUriToTheActorsBaseUri()
    {
        var pattie = stage.Spotlight<Pattie>();
        await Given(pattie).WasAbleTo(NavigateTo(testPage));

        var text = await Then(pattie).Should(ReadFromTheElement(textContent).TheText());

        await Assert.That(text).IsEqualTo("This is content at the deeper path.");
    }

    [Test]
    public async Task OpeningARelativeUriWithoutABaseShouldThrow()
    {
        var webster = stage.Spotlight<Webster>();

        await Assert.That(async () => await When(webster).AttemptsTo(OpenTheUrl(testPage)))
            .Throws<Performables.PerformableException>()
            .WithInnerException<InvalidOperationException>();
    }
}
