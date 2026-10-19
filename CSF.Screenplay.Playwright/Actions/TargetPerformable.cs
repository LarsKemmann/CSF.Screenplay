using System;
using System.Threading;
using System.Threading.Tasks;
using CSF.Screenplay.Playwright.Elements;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright.Actions
{
    /// <summary>
    /// An object which is similar to <see cref="IPerformable"/> but which operates upon the Playwright locator of a target.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is specialised for performables which interact with the element(s) of one <see cref="ITarget"/>; it
    /// takes care of the boilerplate of getting the actor's page and the target's locator. To use an implementation as a
    /// performable, wrap it in a <see cref="TargetPerformableAdapter"/> — which <see cref="PerformableBuilder"/> does.
    /// </para>
    /// </remarks>
    public interface ITargetPerformable
    {
        /// <summary>
        /// Counterpart to <see cref="IPerformable.PerformAsAsync(ICanPerform, CancellationToken)"/> which also offers the actor's page and the target's locator.
        /// </summary>
        /// <param name="actor">The actor that is performing</param>
        /// <param name="page">The actor's page</param>
        /// <param name="locator">The locator of the target</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task which completes when the performable is complete</returns>
        ValueTask PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a report fragment for the performable, with the target it operates upon.
        /// </summary>
        /// <param name="actor">The actor</param>
        /// <param name="target">The target</param>
        /// <param name="formatter">The report formatter</param>
        /// <returns>A report fragment</returns>
        ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter);
    }

    /// <summary>
    /// The counterpart of <see cref="ITargetPerformable"/> for questions: operates upon a target's locator and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    public interface ITargetPerformableWithResult<TResult>
    {
        /// <summary>
        /// Counterpart to <see cref="IPerformableWithResult{TResult}.PerformAsAsync(ICanPerform, CancellationToken)"/> which also offers the actor's page and the target's locator.
        /// </summary>
        /// <param name="actor">The actor that is performing</param>
        /// <param name="page">The actor's page</param>
        /// <param name="locator">The locator of the target</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task which exposes the result</returns>
        ValueTask<TResult> PerformAsAsync(ICanPerform actor, IPage page, ILocator locator, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a report fragment for the performable, with the target it operates upon.
        /// </summary>
        /// <param name="actor">The actor</param>
        /// <param name="target">The target</param>
        /// <param name="formatter">The report formatter</param>
        /// <returns>A report fragment</returns>
        ReportFragment GetReportFragment(Actor actor, ITarget target, IFormatsReportFragment formatter);
    }

    /// <summary>
    /// Adapter which allows an <see cref="ITargetPerformable"/> to be used as an <see cref="IPerformable"/>.
    /// </summary>
    public sealed class TargetPerformableAdapter : IPerformable, ICanReport, IHasCustomTypeName
    {
        readonly ITargetPerformable performable;
        readonly ITarget target;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter) => performable.GetReportFragment(actor, target, formatter);

        /// <inheritdoc/>
        public async ValueTask PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            var page = await actor.GetPageAsync(cancellationToken).ConfigureAwait(false);
            await performable.PerformAsAsync(actor, page, target.GetLocator(page), cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public string GetHumanReadableTypeName() => $"{performable.GetType().FullName}, via {nameof(TargetPerformableAdapter)}";

        /// <summary>
        /// Initializes a new instance of <see cref="TargetPerformableAdapter"/>.
        /// </summary>
        /// <param name="performable">The performable to adapt</param>
        /// <param name="target">The target it operates upon</param>
        /// <exception cref="ArgumentNullException">If either parameter is <see langword="null" />.</exception>
        public TargetPerformableAdapter(ITargetPerformable performable, ITarget target)
        {
            this.performable = performable ?? throw new ArgumentNullException(nameof(performable));
            this.target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }

    /// <summary>
    /// Adapter which allows an <see cref="ITargetPerformableWithResult{TResult}"/> to be used as an <see cref="IPerformableWithResult{TResult}"/>.
    /// </summary>
    /// <typeparam name="TResult">The result type</typeparam>
    public sealed class TargetPerformableWithResultAdapter<TResult> : IPerformableWithResult<TResult>, ICanReport, IHasCustomTypeName
    {
        readonly ITargetPerformableWithResult<TResult> performable;
        readonly ITarget target;

        /// <inheritdoc/>
        public ReportFragment GetReportFragment(Actor actor, IFormatsReportFragment formatter) => performable.GetReportFragment(actor, target, formatter);

        /// <inheritdoc/>
        public async ValueTask<TResult> PerformAsAsync(ICanPerform actor, CancellationToken cancellationToken = default)
        {
            var page = await actor.GetPageAsync(cancellationToken).ConfigureAwait(false);
            return await performable.PerformAsAsync(actor, page, target.GetLocator(page), cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public string GetHumanReadableTypeName() => $"{performable.GetType().FullName}, via {nameof(TargetPerformableWithResultAdapter<TResult>)}";

        /// <summary>
        /// Initializes a new instance of <see cref="TargetPerformableWithResultAdapter{TResult}"/>.
        /// </summary>
        /// <param name="performable">The performable to adapt</param>
        /// <param name="target">The target it operates upon</param>
        /// <exception cref="ArgumentNullException">If either parameter is <see langword="null" />.</exception>
        public TargetPerformableWithResultAdapter(ITargetPerformableWithResult<TResult> performable, ITarget target)
        {
            this.performable = performable ?? throw new ArgumentNullException(nameof(performable));
            this.target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }
}
