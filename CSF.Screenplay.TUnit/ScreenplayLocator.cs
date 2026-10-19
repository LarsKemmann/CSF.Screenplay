using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using TUnit.Core;

namespace CSF.Screenplay
{
    /// <summary>
    /// A small static service locator of sorts, dedicated to getting the <see cref="Screenplay"/> for a test assembly
    /// and the <see cref="ScopeAndPerformance"/> for a test class instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type uses reflection to find the <see cref="ScreenplayAssemblyAttribute"/> which decorates the assembly in which
    /// a test class resides, and creates the Screenplay it names exactly once per assembly: the runtime instantiates a fresh
    /// attribute object every time an attribute is read (and TUnit reads it for its event receivers), so the attribute cannot
    /// hold the Screenplay itself.  Every part of the integration gets the Screenplay from here.
    /// It also tracks which performance belongs to which test class instance (<see cref="ScreenplayClassConstructor"/> creates
    /// both), so that the performance may be finished when TUnit reports the end of the test, and so that test logic which
    /// has only a <see cref="TestContext"/> can get at its performance via <see cref="TestContextExtensions.GetPerformance(TestContext)"/>.
    /// </para>
    /// <para>
    /// Beginning and completing a Screenplay is idempotent here: <see cref="ScreenplayAssemblyAttribute"/> does so from TUnit's
    /// first/last-test-in-session events and <see cref="ScreenplayClassConstructor"/> does so as a safety net, so whichever
    /// of the two TUnit invokes first wins and the other is a no-op.
    /// </para>
    /// </remarks>
    public static class ScreenplayLocator
    {
        static readonly ConcurrentDictionary<Assembly, Screenplay> screenplayCache = new ConcurrentDictionary<Assembly, Screenplay>();
        static readonly ConcurrentDictionary<Screenplay, LifecycleState> lifecycles = new ConcurrentDictionary<Screenplay, LifecycleState>();
        static readonly ConditionalWeakTable<object, ScopeAndPerformance> performances = new ConditionalWeakTable<object, ScopeAndPerformance>();

        /// <summary>
        /// Gets the <see cref="Screenplay"/> for the specified test <see cref="System.Reflection.Assembly"/>, from the <see cref="ScreenplayAssemblyAttribute"/>
        /// which decorates it.
        /// </summary>
        /// <param name="assembly">A test assembly</param>
        /// <returns>The Screenplay for that assembly</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="assembly"/> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">If the assembly is not decorated with <see cref="ScreenplayAssemblyAttribute"/>,
        /// or if the Screenplay factory it names is invalid.</exception>
        public static Screenplay GetScreenplay(Assembly assembly)
        {
            if (assembly is null) throw new ArgumentNullException(nameof(assembly));
            return screenplayCache.GetOrAdd(assembly, GetScreenplayFromAttribute);
        }

        /// <summary>
        /// Gets the <see cref="Screenplay"/> for the assembly in which the specified test class resides.
        /// </summary>
        /// <param name="testClassType">The type of a test class</param>
        /// <returns>The Screenplay for that type's assembly</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="testClassType"/> is <see langword="null" />.</exception>
        public static Screenplay GetScreenplay(Type testClassType)
        {
            if (testClassType is null) throw new ArgumentNullException(nameof(testClassType));
            return GetScreenplay(testClassType.Assembly);
        }

        /// <summary>
        /// Gets the <see cref="ScopeAndPerformance"/> which was created for the specified test class instance by
        /// <see cref="ScreenplayClassConstructor"/>.
        /// </summary>
        /// <param name="testClassInstance">An instance of a test class which was constructed via Screenplay.</param>
        /// <returns>The scope and performance for that test</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="testClassInstance"/> is <see langword="null" />.</exception>
        /// <exception cref="InvalidOperationException">If the instance was not constructed via Screenplay, or its performance has already ended.</exception>
        public static ScopeAndPerformance GetScopedPerformance(object testClassInstance)
        {
            if (TryGetScopedPerformance(testClassInstance, out var scopeAndPerformance)) return scopeAndPerformance;

            throw new InvalidOperationException($"No Screenplay performance is in progress for the test class instance of type {testClassInstance.GetType().FullName}. " +
                                                $"Decorate the test class with {nameof(ScreenplayAttribute)} so that it is constructed via Screenplay.");
        }

        /// <summary>
        /// Attempts to get the <see cref="ScopeAndPerformance"/> which was created for the specified test class instance.
        /// </summary>
        /// <param name="testClassInstance">An instance of a test class.</param>
        /// <param name="scopeAndPerformance">Exposes the scope and performance, if the method returns <see langword="true" />.</param>
        /// <returns><see langword="true" /> if a performance is in progress for the instance; <see langword="false" /> otherwise.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="testClassInstance"/> is <see langword="null" />.</exception>
        public static bool TryGetScopedPerformance(object testClassInstance, out ScopeAndPerformance scopeAndPerformance)
        {
            if (testClassInstance is null) throw new ArgumentNullException(nameof(testClassInstance));
            return performances.TryGetValue(testClassInstance, out scopeAndPerformance);
        }

        /// <summary>
        /// Begins the specified Screenplay unless it has already begun.
        /// </summary>
        /// <param name="screenplay">The Screenplay</param>
        /// <exception cref="ArgumentNullException">If <paramref name="screenplay"/> is <see langword="null" />.</exception>
        public static void BeginScreenplay(Screenplay screenplay)
        {
            if (screenplay is null) throw new ArgumentNullException(nameof(screenplay));
            var lifecycle = lifecycles.GetOrAdd(screenplay, _ => new LifecycleState());

            // Held while the Screenplay begins, so that no performance begins (TUnit constructs test classes in
            // parallel) before the Screenplay's reporter has subscribed to the performance events.
            lock (lifecycle)
            {
                if (lifecycle.HasBegun) return;
                screenplay.BeginScreenplay();
                lifecycle.HasBegun = true;
            }
        }

        /// <summary>
        /// Completes the specified Screenplay unless it has already been completed (or never begun).
        /// </summary>
        /// <param name="screenplay">The Screenplay</param>
        /// <exception cref="ArgumentNullException">If <paramref name="screenplay"/> is <see langword="null" />.</exception>
        public static void CompleteScreenplay(Screenplay screenplay)
        {
            if (screenplay is null) throw new ArgumentNullException(nameof(screenplay));
            var lifecycle = lifecycles.GetOrAdd(screenplay, _ => new LifecycleState());

            lock (lifecycle)
            {
                if (!lifecycle.HasBegun || lifecycle.HasCompleted) return;
                screenplay.CompleteScreenplay();
                lifecycle.HasCompleted = true;
            }
        }

        internal static void RegisterPerformance(object testClassInstance, ScopeAndPerformance scopeAndPerformance)
            => performances.Add(testClassInstance, scopeAndPerformance);

        internal static bool TryRemovePerformance(object testClassInstance, out ScopeAndPerformance scopeAndPerformance)
        {
            if (testClassInstance is null)
            {
                scopeAndPerformance = null;
                return false;
            }

            lock (performances)
            {
                if (!performances.TryGetValue(testClassInstance, out scopeAndPerformance)) return false;
                performances.Remove(testClassInstance);
                return true;
            }
        }

        static Screenplay GetScreenplayFromAttribute(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttribute<ScreenplayAssemblyAttribute>();
            if (attribute is null)
                throw new InvalidOperationException($"The assembly {assembly.FullName} is not decorated with {nameof(ScreenplayAssemblyAttribute)}; " +
                                                    "Screenplay-based TUnit tests require it, so that the integration knows how to create the Screenplay: " +
                                                    "[assembly: CSF.Screenplay.ScreenplayAssembly(typeof(MyScreenplayFactory))]");
            return attribute.CreateScreenplay();
        }

        sealed class LifecycleState
        {
            public bool HasBegun { get; set; }
            public bool HasCompleted { get; set; }
        }
    }
}
