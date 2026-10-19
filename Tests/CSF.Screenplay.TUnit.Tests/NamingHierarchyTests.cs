using CSF.Screenplay.Performances;

namespace CSF.Screenplay;

[Screenplay]
public class NamingHierarchyTests(IPerformance performance)
{
    [Test]
    public async Task ThePerformanceShouldBeNamedAfterTheClassAndTheTest()
    {
        var expected = new IdentifierAndName[]
        {
            new(typeof(NamingHierarchyTests).FullName!, nameof(NamingHierarchyTests)),
            new(TestContext.Current!.Metadata.TestDetails.TestId, TestContext.Current!.Metadata.DisplayName),
        };

        await Assert.That(performance.NamingHierarchy).IsEquivalentTo(expected);
    }

    [Test]
    [Arguments(1)]
    [Arguments(2)]
    public async Task EachCaseOfAParameterisedTestShouldBeItsOwnNamedPerformance(int number)
    {
        var testName = performance.NamingHierarchy[1];

        using (Assert.Multiple())
        {
            await Assert.That(performance.NamingHierarchy).Count().IsEqualTo(2);
            await Assert.That(testName.Identifier).IsEqualTo(TestContext.Current!.Metadata.TestDetails.TestId);
            await Assert.That(testName.Name).Contains(number.ToString());
        }
    }
}
