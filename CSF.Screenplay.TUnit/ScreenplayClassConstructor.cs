using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSF.Screenplay.Performances;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;
using TUnit.Core.Enums;
using TUnit.Core.Interfaces;

namespace CSF.Screenplay
{
    /// <summary>
    /// The TUnit class constructor which builds a test class via Screenplay: one <see cref="IPerformance"/> (and dependency
    /// injection scope) per test, with the test class's constructor parameters resolved from that performance's services.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Developers do not usually use this type directly; decorate the test class (or the assembly) with <see cref="ScreenplayAttribute"/>,
    /// which names this type as the class constructor.  It is public so that <c>[ClassConstructor&lt;ScreenplayClassConstructor&gt;]</c>
    /// may be used instead, if preferred.
    /// </para>
    /// <para>
    /// The lifecycle is as follows.  When TUnit asks for the test class instance, the <see cref="Screenplay"/> for the test assembly
    /// (<see cref="ScreenplayLocator.GetScreenplay(System.Reflection.Assembly)"/>) is begun if it has not been already, a scoped
    /// performance is created (named after the test class and the test), the performance is begun and the test class is constructed
    /// from the performance's <see cref="IHasServiceProvider.ServiceProvider"/>.  When TUnit reports that the test has ended (or
    /// was skipped), the performance is finished with the test's outcome and the scope is disposed.  When TUnit reports the last
    /// test of the session, the Screenplay is completed (if <see cref="ScreenplayAssemblyAttribute"/> has not done so already),
    /// which writes the report.
    /// </para>
    /// </remarks>
    /// <seealso cref="ScreenplayAttribute"/>
    /// <seealso cref="ScreenplayAssemblyAttribute"/>
    public sealed class ScreenplayClassConstructor : IClassConstructor,
                                                      ITestStartEventReceiver,
                                                      ITestEndEventReceiver,
                                                      ITestSkippedEventReceiver,
                                                      ILastTestInTestSessionEventReceiver
    {
        /// <inheritdoc/>
        public int Order => 0;

        /// <summary>The performance has already begun by the time the test starts; nothing further happens here.</summary>
        EventReceiverStage ITestStartEventReceiver.Stage => EventReceiverStage.Early;

        /// <summary>The performance finishes after every other end-of-test receiver has seen the test's result.</summary>
        EventReceiverStage ITestEndEventReceiver.Stage => EventReceiverStage.Late;

        /// <summary>
        /// Creates the test class instance within a new Screenplay performance.
        /// </summary>
        /// <param name="type">The test class type</param>
        /// <param name="classConstructorMetadata">TUnit's metadata about the test being constructed</param>
        /// <returns>The test class instance</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="type"/> is <see langword="null" />.</exception>
        public Task<object> Create(Type type, ClassConstructorMetadata classConstructorMetadata)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));

            var screenplay = ScreenplayLocator.GetScreenplay(type.Assembly);
            ScreenplayLocator.BeginScreenplay(screenplay);

            var scopeAndPerformance = screenplay.CreateScopedPerformance(GetNamingHierarchy(type, classConstructorMetadata));
            scopeAndPerformance.Performance.BeginPerformance();

            object instance;
            try
            {
                instance = ActivatorUtilities.GetServiceOrCreateInstance(scopeAndPerformance.Performance.ServiceProvider, type);
            }
            catch
            {
                scopeAndPerformance.Performance.FinishPerformance(false);
                scopeAndPerformance.Dispose();
                throw;
            }

            ScreenplayLocator.RegisterPerformance(instance, scopeAndPerformance);
            return Task.FromResult(instance);
        }

        /// <inheritdoc/>
        public ValueTask OnTestStart(TestContext testContext) => default;

        /// <summary>
        /// Finishes the test's performance with the test's outcome and disposes of its scope.
        /// </summary>
        /// <param name="testContext">The test context</param>
        public ValueTask OnTestEnd(TestContext testContext)
        {
            FinishPerformance(testContext, GetOutcome(testContext));
            return default;
        }

        /// <summary>
        /// Finishes the test's performance without an outcome (the test did not run) and disposes of its scope.
        /// </summary>
        /// <param name="testContext">The test context</param>
        public ValueTask OnTestSkipped(TestContext testContext)
        {
            FinishPerformance(testContext, null);
            return default;
        }

        /// <summary>
        /// Completes the Screenplay of the last test's assembly, unless <see cref="ScreenplayAssemblyAttribute"/> has already done so.
        /// </summary>
        /// <param name="context">The test session context</param>
        /// <param name="testContext">The context of the last test</param>
        public ValueTask OnLastTestInTestSession(TestSessionContext context, TestContext testContext)
        {
            var classType = testContext?.Metadata?.TestDetails?.ClassType;
            if (classType != null) ScreenplayLocator.CompleteScreenplay(ScreenplayLocator.GetScreenplay(classType.Assembly));
            return default;
        }

        static void FinishPerformance(TestContext testContext, bool? outcome)
        {
            var instance = testContext?.Metadata?.TestDetails?.ClassInstance;
            if (!ScreenplayLocator.TryRemovePerformance(instance, out var scopeAndPerformance)) return;

            if (scopeAndPerformance.Performance.PerformanceState == PerformanceState.InProgress)
                scopeAndPerformance.Performance.FinishPerformance(outcome);
            scopeAndPerformance.Dispose();
        }

        static bool? GetOutcome(TestContext testContext)
        {
            switch (testContext?.Execution?.Result?.State)
            {
                case TestState.Passed: return true;
                case TestState.Failed:
                case TestState.Timeout: return false;
                default: return null;
            }
        }

        /// <summary>
        /// Names the performance after the test class and the test: TUnit's display name when the test context is at hand
        /// (which includes the arguments of a parameterised test), otherwise the method name from the metadata.
        /// </summary>
        static IList<IdentifierAndName> GetNamingHierarchy(Type type, ClassConstructorMetadata metadata)
        {
            var hierarchy = new List<IdentifierAndName> { new IdentifierAndName(type.FullName, type.Name) };

            var current = TestContext.Current;
            if (current?.Metadata?.TestDetails != null && current.Metadata.TestDetails.ClassType == type)
            {
                hierarchy.Add(new IdentifierAndName(current.Metadata.TestDetails.TestId, current.Metadata.DisplayName ?? current.Metadata.TestName));
                return hierarchy;
            }

            var method = metadata?.TestBuilderContext?.TestMetadata;
            if (method != null)
                hierarchy.Add(new IdentifierAndName($"{type.FullName}.{method.Name}", method.Name));

            return hierarchy;
        }
    }
}
