using CSF.Screenplay.Performances;
using Microsoft.Extensions.DependencyInjection;

namespace CSF.Screenplay;

/// <summary>
/// Proves that a performance ends with its test: the second test depends on the first and inspects what the first left behind.
/// </summary>
[Screenplay]
[NotInParallel(nameof(PerformanceLifecycleTests))]
public class PerformanceLifecycleTests(IPerformance performance, ICast cast)
{
    static IPerformance? earlierPerformance;
    static IServiceProvider? earlierServices;

    [Test]
    public async Task ATestRecordsItsPerformance()
    {
        earlierPerformance = performance;
        earlierServices = performance.ServiceProvider;
        cast.GetActor("Rebecca");

        await Assert.That(performance.PerformanceState).IsEqualTo(PerformanceState.InProgress);
    }

    [Test]
    [DependsOn(nameof(ATestRecordsItsPerformance))]
    public async Task TheEarlierPerformanceShouldHaveSucceededAndItsScopeBeDisposed()
    {
        using (Assert.Multiple())
        {
            await Assert.That(earlierPerformance).IsNotNull();
            await Assert.That(earlierPerformance!.PerformanceState).IsEqualTo(PerformanceState.Success);
            await Assert.That(earlierPerformance).IsNotSameReferenceAs(performance);
            await Assert.That(() => earlierServices!.GetRequiredService<ICast>()).Throws<ObjectDisposedException>();
            await Assert.That(cast.GetCastList()).IsEmpty();
        }
    }
}

/// <summary>A test class without the Screenplay attribute coexists with Screenplay-based ones in the same assembly.</summary>
public class PlainTests
{
    [Test]
    public async Task APlainTestIsNotConstructedViaScreenplay()
    {
        await Assert.That(() => TestContext.Current!.GetPerformance()).Throws<InvalidOperationException>();
    }
}

/// <summary>The Screenplay must have been completed (its end actions run) by the time the session's after-hooks run.</summary>
public static class SessionAssertions
{
    [After(TestSession)]
    public static async Task TheScreenplayShouldHaveCompletedExactlyOnce()
    {
        await Assert.That(LifecycleRecorder.BeginCount).IsEqualTo(1);
        await Assert.That(LifecycleRecorder.EndCount).IsEqualTo(1);
    }
}
