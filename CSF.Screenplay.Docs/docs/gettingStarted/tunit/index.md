---
uid: TUnitGettingStartedArticle
---

# Screenplay & TUnit tutorial

Begin writing [TUnit] tests using Screenplay by following these steps.
Further detail is provided below.

1. Ensure that your test project uses [TUnit version 1.75.0] or higher
1. Install the NuGet package **[CSF.Screenplay.TUnit]** to your test project
1. Write a Screenplay Factory class, implementing [`IGetsScreenplay`]
1. Decorate your test assembly with [`ScreenplayAssemblyAttribute`], referencing that Screenplay Factory
1. Write your tests, decorating each test class with [`ScreenplayAttribute`]
1. Add parameters to your test classes' constructors to access the Screenplay architecture

> [!TIP]
> Developers are encouraged to read [these best practices for writing tests which use Screenplay].

[TUnit]: https://tunit.dev/
[TUnit version 1.75.0]: https://www.nuget.org/packages/TUnit/1.75.0
[CSF.Screenplay.TUnit]: https://www.nuget.org/packages/CSF.Screenplay.TUnit
[`IGetsScreenplay`]: xref:CSF.Screenplay.IGetsScreenplay
[`ScreenplayAssemblyAttribute`]: xref:CSF.Screenplay.ScreenplayAssemblyAttribute
[`ScreenplayAttribute`]:xref:CSF.Screenplay.ScreenplayAttribute
[these best practices for writing tests which use Screenplay]: ../../bestPractice/WritingTests.md

## Step 3: Write a Screenplay factory

TUnit requires a factory class which implements [`IGetsScreenplay`].
This tells the underlying framework how to create and configure the `Screenplay` for your tests.
This factory class is user-customisable, because it is where you would configure **[dependency injection]** for your own Screenplay.

The example below shows a minimal template for a factory; replace the commment with DI service registrations for anything that you would like to be available to Screenplay.
There's no need to use `.AddScreenplay()` here, _the integration does that already_.
For example, to add [the Screenplay/Selenium extension], add `services.AddSelenium()` here.

> [!IMPORTANT]
> The Screenplay Factory class _must have a parameterless constructor_.

```csharp
using CSF.Screenplay;

public class MyScreenplayFactory : IGetsScreenplay
{
    public Screenplay GetScreenplay()
    {
        return Screenplay.Create(services => {
                // Add your own dependency injection service descriptors to the service collection here
                // For example, services which will be used by Screenplay Abilities.
            });
    }
}
```

[dependency injection]: ../../dependencyInjection/index.md
[the Screenplay/Selenium extension]: ../../extensions/selenium/index.md

## Step 4: Decorate your test assembly with `[ScreenplayAssembly]`

One line of boilerplate code is required per test assembly (project).
Its purpose is to signpost the Screenplay Factory which you wrote in step 3 (above) to the TUnit/Screenplay integration.
The attribute also begins the Screenplay as the first test of the session starts and completes it — writing [the report] — as the last test ends.

Place a line of code like the following in your test project.
This should be outside of any type; it may go into its own source file if you wish.
Be sure to replace `MyScreenplayFactory` with the factory class your wrote in step 3.
See the docs for [`ScreenplayAssemblyAttribute`] for more info.

```csharp
[assembly: CSF.Screenplay.ScreenplayAssembly(typeof(MyScreenplayFactory))]
```

> [!IMPORTANT]
> When using TUnit with Screenplay, every Screenplay-using test within a test assembly (thus, within a .NET project) must share the same instance of `Screenplay`.
> This is not expected to be problematic, as all the `Screenplay` object does is set-up the Screenplay architecture and dependency injection for the tests.

[the report]: ../../GettingReports.md

## Writing test classes

When writing tests, the test **class** must be decorated with [`ScreenplayAttribute`], which activates Screenplay for every test in that class.
TUnit constructs a new instance of the test class for every test, and this attribute puts Screenplay in charge of that construction: each test is executed as its own [performance], and the test class's constructor parameters are [provided by dependency injection] from that performance.
This covers **steps 5 & 6** in the list above.

This differs from [the NUnit integration], where services are injected into test _method_ parameters.
In TUnit, method parameters belong to the test's data (`[Arguments]` and the like), so Screenplay's services arrive through the constructor instead — and _any_ service you registered in your Screenplay Factory may be injected, not only the three commonly-used ones.
Because the class is constructed by Screenplay, class-level data sources (`[Arguments]` on the class, `[ClassDataSource]`) cannot be combined with `[Screenplay]`; method-level data sources work as usual.

> [!TIP]
> `[Screenplay]` may also be applied to the assembly, which activates Screenplay for every test class in it.
> It is possible to mix & match Screenplay-based test classes and plain TUnit test classes in one test project.

Here is an example of a TUnit test class which is written using Screenplay; it assumes that the assembly has been decorated with [`ScreenplayAssemblyAttribute`], as noted above.

> [!TIP]
> The ability, performables and `DishwashingBuilder` used in this test, related to washing dishes, are all fictitious.
> See [the documentation for writing performables] to learn about how these could be written.

```csharp
using CSF.Screenplay;
using static CSF.Screenplay.PerformanceStarter;
using static WashingTheDishes.DishwashingBuilder;

[Screenplay]
public class WashingTheDishesTests(ICast cast, IDishWashingAbility washTheDishes)
{
    [Test]
    public async Task TheDishesShouldBeCleanAfterJoeWashesThem()
    {
        var joe = cast.GetActor("Joe");
        joe.IsAbleTo(washTheDishes);

        await Given(joe).WasAbleTo(GetSomeDirtyDishes());
        await When(joe).AttemptsTo(WashTheDishes());
        var dishesCondition = await Then(joe).Should(LookAtTheDishesCondition());

        await Assert.That(dishesCondition).IsEqualTo("Clean");
    }
}
```

The performance begins before the class is constructed, so actors may be created and given abilities in the constructor as well, and it ends with the test — with the test's outcome — when TUnit reports that the test has finished.
Logic which has only a `TestContext` (another TUnit event receiver, say) may get at the performance with [`GetPerformance`].

[performance]: xref:CSF.Screenplay.IPerformance
[provided by dependency injection]: ../../dependencyInjection/index.md
[the NUnit integration]: ../nunit3/index.md
[the documentation for writing performables]: ../../writingPerformables/index.md
[`GetPerformance`]: xref:CSF.Screenplay.TestContextExtensions.GetPerformance(TUnit.Core.TestContext)

## Running the tests

TUnit runs on [Microsoft.Testing.Platform]: the test project is an executable, which `dotnet run` or `dotnet test` (with the platform's own arguments, such as `--treenode-filter`) drives.
If `dotnet test` reports that testing with the VSTest target is no longer supported, add the following property to the test project; the .NET 10 SDK's `dotnet test` needs no such property once `global.json` names `Microsoft.Testing.Platform` as the test runner.

```xml
<PropertyGroup>
  <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
</PropertyGroup>
```

[Microsoft.Testing.Platform]: https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro
