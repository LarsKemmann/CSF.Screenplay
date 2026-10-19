using System;
using System.Collections.Generic;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright
{
    /// <summary>
    /// Options for the Playwright extension to Screenplay: which browser to launch and how, and how each actor's
    /// browser context is created.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These options are consumed via <see href="https://learn.microsoft.com/en-us/dotnet/core/extensions/options">the
    /// .NET Options pattern</see>; bind them from configuration (the section <c>Playwright</c> is conventional) or
    /// configure them in code when adding the extension: <c>services.AddPlaywright(options =&gt; options.Headless = false)</c>.
    /// One browser is launched per <see cref="Screenplay"/> (lazily, when the first actor needs it) and every actor who is
    /// able to <see cref="BrowseTheWeb"/> gets a browser context of their own — isolated cookies and storage — within it.
    /// </para>
    /// </remarks>
    public class PlaywrightOptions
    {
        /// <summary>
        /// The browser type to launch: <c>chromium</c> (the default), <c>firefox</c> or <c>webkit</c>.
        /// </summary>
        public string BrowserType { get; set; } = Microsoft.Playwright.BrowserType.Chromium;

        /// <summary>
        /// Whether the browser runs headless; <see langword="true"/> by default.
        /// </summary>
        public bool Headless { get; set; } = true;

        /// <summary>
        /// A browser distribution channel, such as <c>chrome</c> or <c>msedge</c>, to launch instead of Playwright's bundled browser; <see langword="null"/> for the bundled one.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Slows every Playwright operation down by this many milliseconds, so that a headed browser can be watched; zero by default.
        /// </summary>
        public float SlowMo { get; set; }

        /// <summary>
        /// Whether browser contexts ignore HTTPS errors (self-signed development certificates); <see langword="true"/> by default.
        /// </summary>
        public bool IgnoreHttpsErrors { get; set; } = true;

        /// <summary>
        /// The viewport of each browser context; <see langword="null"/> for Playwright's default (1280 by 720).
        /// </summary>
        public ViewportSize Viewport { get; set; }

        /// <summary>
        /// The locale of each browser context, such as <c>en-GB</c>; <see langword="null"/> for the browser's default.
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// The time zone of each browser context, such as <c>Europe/London</c>; <see langword="null"/> for the browser's default.
        /// </summary>
        public string TimezoneId { get; set; }

        /// <summary>
        /// Whether a Playwright trace is recorded for every browser context, so that it may be saved as a report asset
        /// (see <see cref="Actions.SaveTheTrace"/>) — in particular when a performable fails; <see langword="false"/> by default.
        /// </summary>
        public bool RecordTraces { get; set; }

        /// <summary>
        /// The default timeout, in milliseconds, for every Playwright operation which waits (actions, navigations, assertions);
        /// <see langword="null"/> for Playwright's default of 30 seconds.
        /// </summary>
        public float? DefaultTimeout { get; set; }

        /// <summary>
        /// Further launch options, applied after the ones above; for anything this class does not expose.
        /// </summary>
        public Action<BrowserTypeLaunchOptions> ConfigureLaunch { get; set; }

        /// <summary>
        /// Further browser-context options, applied after the ones above; for anything this class does not expose.
        /// </summary>
        public Action<BrowserNewContextOptions> ConfigureContext { get; set; }

        /// <summary>
        /// Gets the launch options for the browser.
        /// </summary>
        /// <returns>Launch options</returns>
        public BrowserTypeLaunchOptions GetLaunchOptions()
        {
            var options = new BrowserTypeLaunchOptions
            {
                Headless = Headless,
                Channel = Channel,
                SlowMo = SlowMo > 0 ? SlowMo : (float?)null,
            };
            ConfigureLaunch?.Invoke(options);
            return options;
        }

        /// <summary>
        /// Gets the options for a new browser context.
        /// </summary>
        /// <returns>Browser-context options</returns>
        public BrowserNewContextOptions GetContextOptions()
        {
            var options = new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = IgnoreHttpsErrors,
                ViewportSize = Viewport,
                Locale = Locale,
                TimezoneId = TimezoneId,
            };
            ConfigureContext?.Invoke(options);
            return options;
        }
    }
}
