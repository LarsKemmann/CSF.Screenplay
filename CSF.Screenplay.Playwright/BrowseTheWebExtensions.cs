using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace CSF.Screenplay.Playwright
{
    /// <summary>
    /// Extension methods relating to the <see cref="BrowseTheWeb"/> ability.
    /// </summary>
    public static class BrowseTheWebExtensions
    {
        /// <summary>
        /// Gets the Playwright page of an actor who is able to <see cref="BrowseTheWeb"/>.
        /// </summary>
        /// <param name="actor">The actor</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The actor's page</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="actor"/> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">If the actor does not have the <see cref="BrowseTheWeb"/> ability.</exception>
        public static Task<IPage> GetPageAsync(this ICanPerform actor, CancellationToken cancellationToken = default)
        {
            if (actor is null) throw new ArgumentNullException(nameof(actor));
            return actor.GetAbility<BrowseTheWeb>().GetPageAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the Playwright browser context of an actor who is able to <see cref="BrowseTheWeb"/>.
        /// </summary>
        /// <param name="actor">The actor</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The actor's browser context</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="actor"/> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">If the actor does not have the <see cref="BrowseTheWeb"/> ability.</exception>
        public static Task<IBrowserContext> GetBrowserContextAsync(this ICanPerform actor, CancellationToken cancellationToken = default)
        {
            if (actor is null) throw new ArgumentNullException(nameof(actor));
            return actor.GetAbility<BrowseTheWeb>().GetContextAsync(cancellationToken);
        }
    }
}
