using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace CSF.Screenplay.Playwright;

/// <summary>
/// Starts the Selenium extension's test web app (whose pages these tests browse) for the test session, on a port of
/// its own so that it does not collide with the Selenium tests' instance, and stops it afterwards.
/// </summary>
public static class TestWebapp
{
    const int maxAttempts = 15;
    const int secondsDelay = 2;

    static Process? webAppProcess;

    public static Uri BaseUri { get; } = new("http://localhost:5103/");

    [Before(TestSession)]
    public static async Task StartAsync()
    {
        webAppProcess = Process.Start(new ProcessStartInfo("dotnet", $"run --project {GetPathToWebappProject()} --no-build --urls {BaseUri}")
        {
            UseShellExecute = false,
        });
        await WaitForWebAppToBeAvailableAsync();
    }

    [After(TestSession)]
    public static void Stop()
    {
        webAppProcess?.Kill(true);
        webAppProcess?.Dispose();
    }

    static async Task WaitForWebAppToBeAvailableAsync()
    {
        using var client = new HttpClient();

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(secondsDelay));
                (await client.GetAsync(new Uri(BaseUri, "index.html"))).EnsureSuccessStatusCode();
                return;
            }
            catch (Exception)
            {
                // Not up yet; try again
            }
        }

        throw new TimeoutException($"The web app was not available at {BaseUri} after {secondsDelay * maxAttempts} seconds.");
    }

    static string GetPathToWebappProject() => Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "CSF.Screenplay.Selenium.TestWebapp");
}
