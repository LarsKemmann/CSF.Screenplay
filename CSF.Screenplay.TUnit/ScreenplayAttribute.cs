using System;
using TUnit.Core;

namespace CSF.Screenplay
{
    /// <summary>
    /// Applied to a test class, indicates that the tests of that class are Screenplay tests; applied to an assembly, that
    /// every test class in it is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A test class decorated with this attribute is constructed by Screenplay — TUnit's <see cref="ClassConstructorAttribute"/>
    /// mechanism, with <see cref="ScreenplayClassConstructor"/> as the constructor. TUnit creates a new instance of the test
    /// class for every test, so each test is executed as its own <see cref="IPerformance"/>: a performance (and the
    /// dependency injection scope that goes with it) begins before the class is constructed and ends when TUnit reports
    /// the end of the test, with the test's outcome.
    /// </para>
    /// <para>
    /// The test class's constructor parameters are resolved from the performance's <see cref="IHasServiceProvider.ServiceProvider"/>;
    /// <see cref="IStage"/>, <see cref="ICast"/> and <see cref="IPerformance"/> are the services most tests want, but any
    /// service which was added to the Screenplay's service collection may be injected — abilities, personas and the like.
    /// See <xref href="DependencyInjectionMainArticle?text=the+article+on+dependency+injection+in+Screenplay"/> for more information.
    /// Because the class is constructed via dependency injection, class-level data sources (such as <c>[Arguments]</c> upon
    /// the class, or <c>[ClassDataSource]</c>) cannot be combined with this attribute.
    /// </para>
    /// <para>
    /// Remember that for this attribute to be effective, the assembly which contains the test class must be decorated with
    /// <see cref="ScreenplayAssemblyAttribute"/>.  If it is not, then the test will fail with an exception.
    /// As noted on the documentation for the Screenplay Assembly attribute, all Screenplay-based tests in the test project/assembly must
    /// use the same Screenplay Factory, identified by that attribute.
    /// </para>
    /// <para>
    /// It is possible and supported to mix &amp; match Screenplay-based tests (classes with this attribute) and non-Screenplay-based
    /// tests (without it) in the same test project/assembly.
    /// </para>
    /// </remarks>
    /// <seealso cref="ScreenplayAssemblyAttribute"/>
    /// <seealso cref="ScreenplayClassConstructor"/>
    public sealed class ScreenplayAttribute : ClassConstructorAttribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ScreenplayAttribute"/>.
        /// </summary>
        public ScreenplayAttribute() : base(typeof(ScreenplayClassConstructor)) { }
    }
}
