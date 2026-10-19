using System.IO;
using CSF.Screenplay.Performables;
using CSF.Screenplay.Playwright.Elements;
using CSF.Screenplay.Reporting;
using static CSF.Screenplay.PerformanceStarter;
using static CSF.Screenplay.Playwright.PerformableBuilder;

namespace CSF.Screenplay.Playwright.Tasks;

[Screenplay]
public class AssetTests(IStage stage, IGetsAssetFilePath assetPaths)
{
    static readonly NamedUri testPage = new("OpenUrlTests.html", "the open-URL test page");

    [Test]
    public async Task SavingAScreenshotShouldWriteAPngNextToTheReport()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        await When(webster).AttemptsTo(SaveAScreenshot("A deliberate screenshot"));

        var directory = Path.GetDirectoryName(assetPaths.GetAssetFilePath("probe.png"))!;
        var screenshots = Directory.GetFiles(directory, "*A deliberate screenshot*.png");
        await Assert.That(screenshots).IsNotEmpty();
    }

    [Test]
    public async Task SavingTheTraceShouldWriteAZipNextToTheReport()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        await When(webster).AttemptsTo(SaveTheTrace("A deliberate trace"));

        var directory = Path.GetDirectoryName(assetPaths.GetAssetFilePath("probe.zip"))!;
        var traces = Directory.GetFiles(directory, "*A deliberate trace*.zip");
        await Assert.That(traces).IsNotEmpty();
    }

    [Test]
    public async Task AFailingPerformableShouldLeaveAnAutoScreenshotBehind()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));

        try
        {
            await When(webster).AttemptsTo(ClickOn(new ElementId("nope", "a missing element")));
        }
        catch (PerformableException)
        {
            // Expected: the element is missing; the failure handler should have saved a screenshot.
        }

        var directory = Path.GetDirectoryName(assetPaths.GetAssetFilePath("probe.png"))!;
        var screenshots = Directory.GetFiles(directory, "*Auto-screenshot on performable failure*.png");
        await Assert.That(screenshots).IsNotEmpty();
    }
}

[Screenplay]
public class NavigationTaskTests(IStage stage)
{
    static readonly ITarget
        delaySeconds = new Label("Delay loading by (seconds)", "the delay input"),
        link = new ElementId("clickable", "the delayed-navigation link");

    static readonly NamedUri testPage = new("DelayedNavigation.html", "the delayed navigation page");

    [Test]
    public async Task ClickingAndWaitingForTheUrlShouldFollowALinkToASlowPage()
    {
        var webster = stage.Spotlight<Webster>();
        await Given(webster).WasAbleTo(NavigateTo(testPage));
        await Given(webster).WasAbleTo(Enter("1").Into(delaySeconds));
        await Given(webster).WasAbleTo(PressTheKey("Tab", delaySeconds));

        await When(webster).AttemptsTo(ClickOnAndWaitForTheUrl(link, "**/DelayedOpening?delaySeconds=1"));
        var url = await Then(webster).Should(ReadThePageUrl());

        await Assert.That(url.PathAndQuery).IsEqualTo("/DelayedOpening?delaySeconds=1");
    }
}
