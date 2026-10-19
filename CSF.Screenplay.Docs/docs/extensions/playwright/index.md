---
uid: PlaywrightArticle
---

# CSF.Screenplay.Playwright Extension

The Playwright extension allows [Actors] to control web browsers using [Microsoft Playwright].
It is the Playwright counterpart of [the Selenium extension]; the two share a vocabulary — `BrowseTheWeb`, `UseABaseUri`, `NamedUri`, `ITarget`, actions, questions, tasks and a `PerformableBuilder` — so that performables written against one read like performables written against the other, while each uses its own technology's strengths.

[Actors]: xref:CSF.Screenplay.Actor
[Microsoft Playwright]: https://playwright.dev/dotnet/
[the Selenium extension]: ../selenium/index.md

## Installing

Install the NuGet package **[CSF.Screenplay.Playwright]** and, after the first build, install the browsers Playwright drives: `pwsh bin/Debug/net8.0/playwright.ps1 install chromium` (or `install --with-deps chromium` on a CI agent), as [the Playwright documentation describes].
Then add the extension to your Screenplay's services:

```csharp
using CSF.Screenplay.Playwright;

public class MyScreenplayFactory : IGetsScreenplay
{
    public Screenplay GetScreenplay() => Screenplay.Create(services =>
    {
        services.AddPlaywright(options =>
        {
            options.Headless = true;
            options.RecordTraces = true;
        });
    });
}
```

[`PlaywrightOptions`] may also be bound from configuration — `services.Configure<PlaywrightOptions>(configuration.GetSection("Playwright"))` — so that the browser, headless mode, viewport, locale, timeouts and tracing may differ between a developer's machine and a CI agent without a code change.

[CSF.Screenplay.Playwright]: https://www.nuget.org/packages/CSF.Screenplay.Playwright
[the Playwright documentation describes]: https://playwright.dev/dotnet/docs/browsers
[`PlaywrightOptions`]: xref:CSF.Screenplay.Playwright.PlaywrightOptions

## Core contents of this extension

* The [Ability] [`BrowseTheWeb`]: a Playwright **browser context** and **page** of the actor's own, over one browser shared by the whole Screenplay (launched on first use by [`IGetsBrowser`]).  Every actor has isolated cookies and storage, so two actors in one performance may be signed in to the same application as different users.
* The ability [`UseABaseUri`] and the [`NamedUri`] class, so that pages are named by relative URIs and the application's location is decided at runtime.
* [Targets] — [`ITarget`] implementations which wrap Playwright's lazy, auto-waiting locators: [`CssSelector`], [`ElementId`], [`TestId`], [`Role`], [`Text`], [`Label`], [`Placeholder`], [`XPath`], and a [`PlaywrightElement`] for a locator you already have.  Targets may be narrowed with [`Within`], [`Nth`], [`First`], [`Last`] and [`ContainingText`].
* Actions, questions and tasks, all reached from [`PerformableBuilder`]: navigating; clicking, hovering, filling, typing, pressing keys, checking, selecting; waiting for an element, a URL or a period; reading text, values, attributes, CSS properties, visibility, enabled and checked states, counts, selections, cookies, the page title and URL; executing JavaScript; saving screenshots and traces.

Many of the [Performables] provided by this extension operate in the format _"Actor performs X action upon Y element"_: the performable carries the logic of the interaction, the target names the element, and the report reads as the two together — "Webster clicks on the checkout button".

[Ability]: ../../../glossary/Ability.md
[`BrowseTheWeb`]: xref:CSF.Screenplay.Playwright.BrowseTheWeb
[`IGetsBrowser`]: xref:CSF.Screenplay.Playwright.IGetsBrowser
[`UseABaseUri`]: xref:CSF.Screenplay.Playwright.UseABaseUri
[`NamedUri`]: xref:CSF.Screenplay.Playwright.NamedUri
[Targets]: xref:CSF.Screenplay.Playwright.Elements.ITarget
[`ITarget`]: xref:CSF.Screenplay.Playwright.Elements.ITarget
[`CssSelector`]: xref:CSF.Screenplay.Playwright.Elements.CssSelector
[`ElementId`]: xref:CSF.Screenplay.Playwright.Elements.ElementId
[`TestId`]: xref:CSF.Screenplay.Playwright.Elements.TestId
[`Role`]: xref:CSF.Screenplay.Playwright.Elements.Role
[`Text`]: xref:CSF.Screenplay.Playwright.Elements.Text
[`Label`]: xref:CSF.Screenplay.Playwright.Elements.Label
[`Placeholder`]: xref:CSF.Screenplay.Playwright.Elements.Placeholder
[`XPath`]: xref:CSF.Screenplay.Playwright.Elements.XPath
[`PlaywrightElement`]: xref:CSF.Screenplay.Playwright.Elements.PlaywrightElement
[`Within`]: xref:CSF.Screenplay.Playwright.Elements.TargetExtensions.Within(CSF.Screenplay.Playwright.Elements.ITarget,CSF.Screenplay.Playwright.Elements.ITarget,System.String)
[`Nth`]: xref:CSF.Screenplay.Playwright.Elements.TargetExtensions.Nth(CSF.Screenplay.Playwright.Elements.ITarget,System.Int32,System.String)
[`First`]: xref:CSF.Screenplay.Playwright.Elements.TargetExtensions.First(CSF.Screenplay.Playwright.Elements.ITarget,System.String)
[`Last`]: xref:CSF.Screenplay.Playwright.Elements.TargetExtensions.Last(CSF.Screenplay.Playwright.Elements.ITarget,System.String)
[`ContainingText`]: xref:CSF.Screenplay.Playwright.Elements.TargetExtensions.ContainingText(CSF.Screenplay.Playwright.Elements.ITarget,System.String,System.String)
[`PerformableBuilder`]: xref:CSF.Screenplay.Playwright.PerformableBuilder
[Performables]: ../../../glossary/Performable.md

## Example

A persona grants the abilities; a page class names the targets; a task composes the interactions.

```csharp
using CSF.Screenplay.Playwright;
using CSF.Screenplay.Playwright.Elements;
using static CSF.Screenplay.Playwright.PerformableBuilder;

public class Webster(IGetsBrowser browsers, IGetsAssetFilePath assetPaths) : IPersona
{
    public string Name => "Webster";

    public Actor GetActor(Guid performanceIdentity)
    {
        var webster = new Actor(Name, performanceIdentity);
        webster.IsAbleTo(new BrowseTheWeb(browsers));
        webster.IsAbleTo(new UseABaseUri("https://shop.example.com/"));
        webster.IsAbleTo(new GetAssetFilePaths(assetPaths));   // for screenshots and traces
        return webster;
    }
}

public static class ShoppingCartPage
{
    public static readonly NamedUri Url = new("cart", "the shopping cart");
    public static readonly ITarget RemoveButton = new Role(AriaRole.Button, "Remove", "the remove button");
    public static readonly ITarget Total = new TestId("cart-total", "the cart total");
}

public class RemoveTheItemNamed(string item) : IPerformable, ICanReport
{
    public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
        => formatter.Format("{Actor} removes {Item} from their cart", actor, item);

    public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
    {
        var row = new Role(AriaRole.Row, "the cart rows").ContainingText(item, $"the row for {item}");
        await actor.PerformAsync(ClickOn(ShoppingCartPage.RemoveButton.Within(row)), cancellationToken);
    }
}
```

```csharp
await Given(webster).WasAbleTo(NavigateTo(ShoppingCartPage.Url));
await When(webster).AttemptsTo(new RemoveTheItemNamed("Widgets"));
var total = await Then(webster).Should(ReadFromTheElement(ShoppingCartPage.Total).TheText());
```

## Waiting

Playwright locators auto-wait: an action waits for its element to be attached, visible, stable and enabled (up to [`PlaywrightOptions.DefaultTimeout`], Playwright's own 30 seconds by default), so explicit waits are rarely needed.
When the moment of interest is not an action — a result appearing after a click, a navigation completing — use [`WaitUntil`] for an element state or [`WaitUntilTheUrlMatches`] for a URL; [`ClickOnAndWaitForTheUrl`] combines a click with the navigation it triggers.
Questions which observe state ([`AndSeeWhetherItIsVisible`], [`CountThe`]) do not wait; they answer for the page as it is.

[`PlaywrightOptions.DefaultTimeout`]: xref:CSF.Screenplay.Playwright.PlaywrightOptions.DefaultTimeout
[`WaitUntil`]: xref:CSF.Screenplay.Playwright.PerformableBuilder.WaitUntil(CSF.Screenplay.Playwright.Elements.ITarget,Microsoft.Playwright.WaitForSelectorState,System.Nullable{System.Single})
[`WaitUntilTheUrlMatches`]: xref:CSF.Screenplay.Playwright.PerformableBuilder.WaitUntilTheUrlMatches(System.String,System.Nullable{System.Single})
[`ClickOnAndWaitForTheUrl`]: xref:CSF.Screenplay.Playwright.PerformableBuilder.ClickOnAndWaitForTheUrl(CSF.Screenplay.Playwright.Elements.ITarget,System.String)
[`AndSeeWhetherItIsVisible`]: xref:CSF.Screenplay.Playwright.PerformableBuilder.ElementQuestionBuilder.AndSeeWhetherItIsVisible
[`CountThe`]: xref:CSF.Screenplay.Playwright.PerformableBuilder.CountThe(CSF.Screenplay.Playwright.Elements.ITarget)

## Screenshots, traces and the report

An actor who also has the [`GetAssetFilePaths`] ability may save a screenshot ([`SaveAScreenshot`]) or — when [`PlaywrightOptions.RecordTraces`] is on — the Playwright trace of their browser context ([`SaveTheTrace`]) as [assets of the report].
A trace is a `.zip` for [Playwright's trace viewer]: every action, a DOM snapshot before and after each, network calls and console output.
Whenever a performable fails, the extension saves a screenshot (and the trace, when recorded) automatically; the report then shows what the actor saw at the moment of failure.

[`GetAssetFilePaths`]: xref:CSF.Screenplay.Abilities.GetAssetFilePaths
[`SaveAScreenshot`]: xref:CSF.Screenplay.Playwright.PerformableBuilder.SaveAScreenshot(System.String,System.Boolean)
[`PlaywrightOptions.RecordTraces`]: xref:CSF.Screenplay.Playwright.PlaywrightOptions.RecordTraces
[`SaveTheTrace`]: xref:CSF.Screenplay.Playwright.PerformableBuilder.SaveTheTrace(System.String)
[assets of the report]: ../../../glossary/Asset.md
[Playwright's trace viewer]: https://trace.playwright.dev/

## Differences from the Selenium extension

* **Targets are lazy locators**, not element references: there is no equivalent of `SeleniumElement`'s concrete `IWebElement`, and no `TargetNotFoundException` — a missing element surfaces as Playwright's `TimeoutException` once the auto-wait elapses.  [`FindThe`] returns the matched elements as [`PlaywrightElement`] targets.
* **Browser choice is an option** (`BrowserType`, `Channel`), not a WebDriver factory configuration; Playwright ships its own browsers.
* **No browser quirks**: Playwright's engine abstracts the browsers.
* **Console logs** are not collected by the extension; Playwright exposes them through `IPage.Console` for performables which need them.

[`FindThe`]: xref:CSF.Screenplay.Playwright.PerformableBuilder.FindThe(CSF.Screenplay.Playwright.Elements.ITarget)
