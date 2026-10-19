using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CSF.Screenplay.Playwright;

/// <summary>
/// The Screenplay factory for the Playwright extension's tests: Playwright options from <c>appsettings.json</c>
/// (overridable by environment variables such as <c>Playwright__Headless=false</c>) and the extension itself.
/// </summary>
public class ScreenplayFactory : IGetsScreenplay
{
    public Screenplay GetScreenplay() => Screenplay.Create(services =>
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        services.Configure<PlaywrightOptions>(configuration.GetSection("Playwright"));
        services.AddPlaywright();
        services.AddTransient<Webster>();
        services.AddTransient<Pattie>();
    });
}
