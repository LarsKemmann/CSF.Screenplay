using System.Collections.Generic;
using CSF.Screenplay.Actors;
using CSF.Screenplay.Performables;
using CSF.Screenplay.Playwright.Elements;
using CSF.Screenplay.Reporting;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright.Reporting
{
    /// <summary>
    /// An object which saves a screenshot (and the trace, when recorded) as report assets when a performable fails.
    /// </summary>
    public interface ISavesAssetsWhenPerformableFails
    {
        /// <summary>Handler for <see cref="Performances.IHasPerformanceEvents.PerformableFailed"/>.</summary>
        /// <param name="sender">The sender</param>
        /// <param name="ev">The event arguments</param>
        void OnPerformableFailed(object sender, PerformableFailureEventArgs ev);
    }

    /// <summary>
    /// Saves a screenshot named <c>Auto-screenshot on performable failure</c> — and the Playwright trace, if
    /// <see cref="PlaywrightOptions.RecordTraces"/> is on — when a performable fails.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Nothing is saved if the exception is a <see cref="PerformableException"/> (an inner performable is the true source and
    /// has been handled already), if the actor cannot <see cref="BrowseTheWeb"/>, or if they have not opened a page.
    /// Both assets need the actor to have <see cref="Abilities.GetAssetFilePaths"/>; without it nothing is saved either.
    /// </para>
    /// </remarks>
    public class PerformableFailureAssetSaver : ISavesAssetsWhenPerformableFails
    {
        /// <inheritdoc/>
        public void OnPerformableFailed(object sender, PerformableFailureEventArgs ev)
        {
            if (ev.Exception is PerformableException) return;
            var actor = (ICanPerform)ev.Actor;
            if (!actor.TryGetAbility<BrowseTheWeb>(out var browseTheWeb) || !browseTheWeb.HasPage) return;

            actor.PerformAsync(PerformableBuilder.SaveAScreenshot("Auto-screenshot on performable failure")).AsTask().GetAwaiter().GetResult();
            if (browseTheWeb.Options.RecordTraces)
                actor.PerformAsync(PerformableBuilder.SaveTheTrace("Auto-trace on performable failure")).AsTask().GetAwaiter().GetResult();
        }
    }

    /// <summary>Formats an <see cref="ITarget"/> for the report by its name.</summary>
    public class TargetFormatter : IValueFormatter
    {
        /// <inheritdoc/>
        public bool CanFormat(object value) => value is ITarget;

        /// <inheritdoc/>
        public string FormatForReport(object value) => ((ITarget)value).Name;
    }

    /// <summary>Formats a <see cref="NamedUri"/> for the report by its name.</summary>
    public class NamedUriFormatter : IValueFormatter
    {
        /// <inheritdoc/>
        public bool CanFormat(object value) => value is NamedUri;

        /// <inheritdoc/>
        public string FormatForReport(object value) => ((NamedUri)value).Name;
    }

    /// <summary>Formats a collection of cookies for the report as their names.</summary>
    public class CookiesFormatter : IValueFormatter
    {
        /// <inheritdoc/>
        public bool CanFormat(object value) => value is IEnumerable<BrowserContextCookiesResult>;

        /// <inheritdoc/>
        public string FormatForReport(object value)
        {
            var names = new List<string>();
            foreach (var cookie in (IEnumerable<BrowserContextCookiesResult>)value) names.Add(cookie.Name);
            return names.Count == 0 ? "no cookies" : $"the cookies {string.Join(", ", names)}";
        }
    }
}
