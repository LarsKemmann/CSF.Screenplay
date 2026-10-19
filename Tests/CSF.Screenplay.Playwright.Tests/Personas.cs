using CSF.Screenplay.Abilities;
using CSF.Screenplay.Reporting;

namespace CSF.Screenplay.Playwright;

/// <summary>
/// Webster is an actor who can browse the web with Playwright, from the test web app's root.
/// </summary>
public class Webster(IGetsBrowser browsers, IGetsAssetFilePath pathProvider) : IPersona
{
    internal static Uri TestWebappBaseUri => TestWebapp.BaseUri;

    public string Name => "Webster";

    public Actor GetActor(Guid performanceIdentity)
    {
        var webster = new Actor(Name, performanceIdentity);
        webster.IsAbleTo(new BrowseTheWeb(browsers));
        webster.IsAbleTo(new UseABaseUri(TestWebappBaseUri));
        webster.IsAbleTo(new GetAssetFilePaths(pathProvider));
        return webster;
    }
}

/// <summary>
/// Pattie is a second actor with a browser context of her own, from the test web app's child path.
/// </summary>
public class Pattie(IGetsBrowser browsers, IGetsAssetFilePath pathProvider) : IPersona
{
    public string Name => "Pattie";

    public Actor GetActor(Guid performanceIdentity)
    {
        var pattie = new Actor(Name, performanceIdentity);
        pattie.IsAbleTo(new BrowseTheWeb(browsers));
        pattie.IsAbleTo(new UseABaseUri(new Uri(TestWebapp.BaseUri, "ChildPath/")));
        pattie.IsAbleTo(new GetAssetFilePaths(pathProvider));
        return pattie;
    }
}
