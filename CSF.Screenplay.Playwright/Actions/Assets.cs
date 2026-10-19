using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSF.Screenplay.Abilities;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright.Actions
{
    /// <summary>
    /// Action: takes a screenshot of the actor's page and saves it as an asset file of the report.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This performable requires the actor to have the <see cref="GetAssetFilePaths"/> ability as well as <see cref="BrowseTheWeb"/>;
    /// without an asset path (reporting disabled) it does nothing.  The file is PNG; the precise file name is determined by
    /// <see cref="CSF.Screenplay.Reporting.AssetPathProvider"/>.
    /// </para>
    /// </remarks>
    public class SaveAScreenshot : IPerformable, ICanReport
    {
        readonly string name;
        readonly bool fullPage;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} saves a screenshot named {Name}", actor.Name, name);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            if (!actor.HasAbility<BrowseTheWeb>() || !actor.GetAbility<BrowseTheWeb>().HasPage) return;
            if (!actor.TryGetAbility<GetAssetFilePaths>(out var assets)) return;

            var path = assets.GetAssetFilePath($"{name}.png");
            if (path == null) return;

            var page = await actor.GetPageAsync(cancellationToken).ConfigureAwait(false);
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = fullPage, Type = ScreenshotType.Png }).ConfigureAwait(false);
            actor.RecordAsset(this, path, name);
        }

        /// <summary>Initializes a new instance of <see cref="SaveAScreenshot"/>.</summary>
        /// <param name="name">An optional human-readable name of the screenshot; <c>screenshot</c> by default</param>
        /// <param name="fullPage">Whether to capture the whole scrollable page rather than the viewport</param>
        public SaveAScreenshot(string name = null, bool fullPage = false)
        {
            this.name = name ?? "screenshot";
            this.fullPage = fullPage;
        }
    }

    /// <summary>
    /// Action: stops the Playwright trace of the actor's browser context and saves it as an asset file of the report.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tracing must have been started — <see cref="PlaywrightOptions.RecordTraces"/> starts it for every context. The trace is a
    /// <c>.zip</c> for <see href="https://trace.playwright.dev/">Playwright's trace viewer</see>. Requires <see cref="GetAssetFilePaths"/>.
    /// After saving, tracing is stopped: a context has one trace.
    /// </para>
    /// </remarks>
    public class SaveTheTrace : IPerformable, ICanReport
    {
        readonly string name;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter)
            => formatter.Format("{Actor} saves the Playwright trace named {Name}", actor.Name, name);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            if (!actor.HasAbility<BrowseTheWeb>() || !actor.GetAbility<BrowseTheWeb>().HasPage) return;
            if (!actor.GetAbility<BrowseTheWeb>().Options.RecordTraces) return;
            if (!actor.TryGetAbility<GetAssetFilePaths>(out var assets)) return;

            var path = assets.GetAssetFilePath($"{name}.zip");
            if (path == null) return;

            var context = await actor.GetBrowserContextAsync(cancellationToken).ConfigureAwait(false);
            await context.Tracing.StopAsync(new TracingStopOptions { Path = path }).ConfigureAwait(false);
            if (File.Exists(path)) actor.RecordAsset(this, path, name);
        }

        /// <summary>Initializes a new instance of <see cref="SaveTheTrace"/>.</summary>
        /// <param name="name">An optional human-readable name of the trace; <c>trace</c> by default</param>
        public SaveTheTrace(string name = null) { this.name = name ?? "trace"; }
    }
}
