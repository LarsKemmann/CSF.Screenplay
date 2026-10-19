using Microsoft.Extensions.DependencyInjection;

namespace CSF.Screenplay;

/// <summary>
/// The Screenplay factory for this test assembly: a default Screenplay, plus a service the tests inject to prove
/// that arbitrary registrations reach a test class's constructor, and a recorder of the Screenplay's lifecycle events.
/// </summary>
public class ScreenplayFactory : IGetsScreenplay
{
    public Screenplay GetScreenplay() => Screenplay.Create(services =>
    {
        services.AddSingleton<InjectableService>();
        services.Configure<ScreenplayOptions>(options =>
        {
            options.OnBeginScreenplayActions.Add(_ => LifecycleRecorder.BeginCount++);
            options.OnEndScreenplayActions.Add(_ => LifecycleRecorder.EndCount++);
        });
    });
}

public class InjectableService
{
    public string Value => "Injected";
}

public static class LifecycleRecorder
{
    public static int BeginCount;
    public static int EndCount;
}
