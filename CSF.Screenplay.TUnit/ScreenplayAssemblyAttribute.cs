using System;
using System.Threading.Tasks;
using TUnit.Core;
using TUnit.Core.Interfaces;

namespace CSF.Screenplay
{
    /// <summary>
    /// An attribute used to mark an assembly which contains Screenplay-based TUnit tests.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute is the core of the TUnit <xref href="IntegrationGlossaryItem?text=test+framework+integration"/> with Screenplay.
    /// In order to run tests with Screenplay, the assembly must be decorated with this attribute.
    /// Its purpose is to signpost the Screenplay Factory (see below) to the TUnit/Screenplay integration.
    /// </para>
    /// <para>
    /// This attribute has one mandatory parameter; that is the <see cref="Type"/> of the Screenplay Factory which should be used in this test project.
    /// A Screenplay Factory is a class which provides a concrete implementation of <see cref="IGetsScreenplay"/>.
    /// An instance of the factory will be instantiated by the TUnit integration and will be used to
    /// build and retrieve the <see cref="Screenplay"/> instance for running the Screenplay-based tests within the decorated
    /// assembly.
    /// </para>
    /// <para>
    /// Each test class in the project must additionally be decorated with the <see cref="ScreenplayAttribute"/> in order to
    /// activate the Screenplay architecture for its tests (or the assembly may be, to activate it for every test class).
    /// It is possible to mix &amp; match Screenplay-based and non-Screenplay-based tests within a single test project.
    /// </para>
    /// <para>
    /// All Screenplay-based tests within a test project/assembly will use the Screenplay Factory which is identified by this attribute.
    /// The attribute also participates in TUnit's test session events: it begins the Screenplay when the first test of the
    /// session starts and completes it (writing the report) when the last test of the session ends.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>
    /// Decorate your assembly with this attribute using the syntax <c>[assembly: CSF.Screenplay.ScreenplayAssembly(typeof(MyScreenplayFactory))]</c>.  You may place this
    /// into any source file, outside of any type declaration.  By convention it would be put into a dedicated source file
    /// within the <c>Properties</c> project directory.
    /// </para>
    /// </example>
    /// <seealso cref="ScreenplayAttribute"/>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class ScreenplayAssemblyAttribute : Attribute, IFirstTestInTestSessionEventReceiver, ILastTestInTestSessionEventReceiver
    {
        /// <summary>
        /// Gets the type of the Screenplay Factory (an implementation of <see cref="IGetsScreenplay"/>) for the decorated assembly.
        /// </summary>
        public Type FactoryType { get; }

        /// <summary>
        /// Creates a new <see cref="Screenplay"/> from the factory named by this attribute.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Do not use this method to get the Screenplay for an assembly; use <see cref="ScreenplayLocator.GetScreenplay(System.Reflection.Assembly)"/>,
        /// which creates the Screenplay once per assembly and caches it.  Attribute instances are not singletons — the
        /// runtime (and TUnit) instantiate a fresh copy every time the attribute is read — so the Screenplay cannot be
        /// cached here.
        /// </para>
        /// </remarks>
        /// <returns>A new Screenplay.</returns>
        /// <exception cref="InvalidOperationException">If the Screenplay factory used with the constructor to this
        /// attribute is invalid or fails to return a non-<see langword="null" /> <see cref="Screenplay"/> instance.</exception>
        public Screenplay CreateScreenplay()
        {
            try
            {
                var factory = InstantiateFactory(FactoryType);
                var screenplay = factory.GetScreenplay();
                if (screenplay is null)
                    throw new InvalidOperationException($"The factory type {FactoryType.FullName} must return a non-null Screenplay instance from its {nameof(IGetsScreenplay.GetScreenplay)} method.");

                return screenplay;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected exception was encountered creating the Screenplay for this test assembly; see the inner exception for details.", ex);
            }
        }

        /// <inheritdoc/>
        public int Order => 0;

        /// <summary>
        /// Begins the Screenplay of the first test's assembly as the first test of the session starts.
        /// </summary>
        /// <param name="context">The test session context</param>
        /// <param name="testContext">The context of the first test</param>
        public ValueTask OnFirstTestInTestSession(TestSessionContext context, TestContext testContext)
        {
            var assembly = GetAssembly(testContext);
            if (assembly != null) ScreenplayLocator.BeginScreenplay(ScreenplayLocator.GetScreenplay(assembly));
            return default;
        }

        /// <summary>
        /// Completes the Screenplay of the last test's assembly as the last test of the session ends.
        /// </summary>
        /// <param name="context">The test session context</param>
        /// <param name="testContext">The context of the last test</param>
        public ValueTask OnLastTestInTestSession(TestSessionContext context, TestContext testContext)
        {
            var assembly = GetAssembly(testContext);
            if (assembly != null) ScreenplayLocator.CompleteScreenplay(ScreenplayLocator.GetScreenplay(assembly));
            return default;
        }

        static System.Reflection.Assembly GetAssembly(TestContext testContext) => testContext?.Metadata?.TestDetails?.ClassType?.Assembly;

        /// <summary>
        /// Initializes a new instance of <see cref="ScreenplayAssemblyAttribute"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="factoryType"/> specified in this constructor must meet all of the following criteria:
        /// </para>
        /// <list type="bullet">
        /// <item><description>It must be a non-<see langword="null" />, non-<see langword="abstract"/> <see cref="Type"/> which derives from <see cref="IGetsScreenplay"/></description></item>
        /// <item><description>It must have a public parameterless constructor</description></item>
        /// <item><description>It must return a non-<see langword="null" /> instance of <see cref="Screenplay"/> from its <see cref="IGetsScreenplay.GetScreenplay"/> method</description></item>
        /// </list>
        /// </remarks>
        /// <param name="factoryType">The concrete type of a class which implements <see cref="IGetsScreenplay"/>.</param>
        public ScreenplayAssemblyAttribute(Type factoryType)
        {
            if (factoryType is null)
                throw new ArgumentNullException(nameof(factoryType));
            if (!typeof(IGetsScreenplay).IsAssignableFrom(factoryType))
                throw new ArgumentException($"The factory type {factoryType.FullName} must derive from {nameof(IGetsScreenplay)}.", nameof(factoryType));
            if (factoryType.IsAbstract)
                throw new ArgumentException($"The factory type {factoryType.FullName} must not be abstract.", nameof(factoryType));
            if (factoryType.GetConstructor(Type.EmptyTypes) is null)
                throw new ArgumentException($"The factory type {factoryType.FullName} must have a public parameterless constructor.", nameof(factoryType));

            FactoryType = factoryType;
        }

        static IGetsScreenplay InstantiateFactory(Type factoryType)
        {
            try
            {
                return (IGetsScreenplay)Activator.CreateInstance(factoryType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"The factory type {factoryType.FullName} must not raise an exception from its public parameterless constructor.", ex);
            }
        }
    }
}
