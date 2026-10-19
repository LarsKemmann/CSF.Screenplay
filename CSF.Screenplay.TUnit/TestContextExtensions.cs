using System;
using TUnit.Core;

namespace CSF.Screenplay
{
    /// <summary>
    /// Extension methods for TUnit's <see cref="TestContext"/> relating to Screenplay.
    /// </summary>
    public static class TestContextExtensions
    {
        /// <summary>
        /// Gets the Screenplay <see cref="IPerformance"/> in which the specified test is executing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Most test logic should constructor-inject <see cref="IPerformance"/> (or <see cref="IStage"/>/<see cref="ICast"/>) instead;
        /// this method serves logic which has only a test context, such as other TUnit event receivers.
        /// </para>
        /// </remarks>
        /// <param name="testContext">The test context</param>
        /// <returns>The performance</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="testContext"/> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">If the test's class was not constructed via Screenplay (see <see cref="ScreenplayAttribute"/>).</exception>
        public static IPerformance GetPerformance(this TestContext testContext)
        {
            if (testContext is null) throw new ArgumentNullException(nameof(testContext));

            var instance = testContext.Metadata?.TestDetails?.ClassInstance;
            if (instance is null)
                throw new InvalidOperationException("The test context has no test class instance yet; the performance is available once the class has been constructed.");

            return ScreenplayLocator.GetScopedPerformance(instance).Performance;
        }
    }
}
