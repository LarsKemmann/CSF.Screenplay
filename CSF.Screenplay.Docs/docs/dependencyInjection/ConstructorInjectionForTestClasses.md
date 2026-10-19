# Constructor injection for test classes

For testing frameworks which construct **a new instance of the test class for every test**, such as [TUnit], services are injected via _constructor injection_ into the test class.
A test class decorated with the `[Screenplay]` attribute is constructed by Screenplay, from the services of the [performance] which corresponds to the test; any service which was added to the Screenplay's service collection may be a constructor parameter.
Use dependencies injected in this way to get access to [commonly-used Screenplay services] and anything else required at the root level of your test logic.

[TUnit]: https://tunit.dev/
[performance]: xref:CSF.Screenplay.IPerformance
[commonly-used Screenplay services]: InjectableServices.md

## Example

This is an example of a TUnit-style test class with constructor-injected dependencies.
This would require [installing the TUnit test integration] to run.
Take note of the Screenplay attribute upon the class, and the `cast` parameter.

```csharp
[Screenplay]
public class SampleTests(ICast cast)
{
    [Test]
    public async Task TheSampleTestShouldBeInformative()
    {
        var janeSmith = cast.GetActor<JaneSmith>();

        // ... use the actor with some performables, then make assertions etc
    }
}
```

[installing the TUnit test integration]: ../gettingStarted/tunit/index.md
