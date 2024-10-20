using BuildLib.CloudServices.GooglePlay;
using BuildLib.Utils;

namespace BuildLib.SolutionBuild.Versioning;

[Inject]
public class LatestAndroidServiceVersionProvider(IEditService editService)
{
    public async Task<int> GetLatestVersionCode()
    {
        var resp = await editService.GetBundles();
        return resp.Bundles.Max(x => x.VersionCode) ?? throw new InvalidDataException("No version code found");
    }
}