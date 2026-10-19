using CSF.Screenplay.Performances;

namespace CSF.Screenplay;

[Screenplay]
public class InjectableServicesTests(IStage stage, ICast cast, IPerformance performance, InjectableService service)
{
    [Test]
    public async Task TheCastShouldBeInjectable()
    {
        cast.GetActor("Tess");
        cast.GetActor("Patricia");

        await Assert.That(cast.GetCastList()).IsEquivalentTo(["Tess", "Patricia"]);
    }

    [Test]
    public async Task TheStageShouldBeInjectableAndLinkedToTheCast()
    {
        var tess = cast.GetActor("Tess");
        stage.Spotlight(tess);

        await Assert.That(stage.GetSpotlitActor()).IsSameReferenceAs(tess);
        await Assert.That(stage.Cast).IsSameReferenceAs(cast);
    }

    [Test]
    public async Task ThePerformanceShouldBeInjectableAndInProgress()
    {
        await Assert.That(performance.PerformanceState).IsEqualTo(PerformanceState.InProgress);
    }

    [Test]
    public async Task ThePerformanceShouldBeTheRealThingNotAnAdapter()
    {
        await Assert.That(performance).IsTypeOf<Performance>();
    }

    [Test]
    public async Task AnyRegisteredServiceShouldBeInjectable()
    {
        await Assert.That(service.Value).IsEqualTo("Injected");
    }

    [Test]
    public async Task TheCastShouldBelongToTheInjectedPerformance()
    {
        await Assert.That(cast.PerformanceIdentity).IsEqualTo(performance.PerformanceIdentity);
    }

    [Test]
    public async Task TheTestContextShouldExposeTheSamePerformance()
    {
        await Assert.That(TestContext.Current!.GetPerformance()).IsSameReferenceAs(performance);
    }

    [Test]
    public async Task TheScreenplayShouldHaveBegunExactlyOnce()
    {
        await Assert.That(LifecycleRecorder.BeginCount).IsEqualTo(1);
    }
}
