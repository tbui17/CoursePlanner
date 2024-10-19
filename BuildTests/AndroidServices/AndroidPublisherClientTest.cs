using BuildLib.CloudServices.GooglePlay;
using BuildLib.Utils;
using BuildTests.TestSetup;
using BuildTests.Utils;
using FluentAssertions;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace BuildTests.AndroidServices;

[TestSubject(typeof(AndroidPublisherClient))]
public class AndroidPublisherClientTest(ITestOutputHelper testOutputHelper) : BaseContainerSetup(testOutputHelper)
{
    [Fact]
    public async Task METHOD()
    {
        var service = Resolve<AndroidPublisherService>();
        var config = GetConfiguration();
        const string editId = "05666670412153367928";

        // await CreateEdit();
        // await UploadBundle();


        var res = await service.Edits.Tracks.Get(config.ApplicationId, editId, "internal").ExecuteAsync();
        res.Releases.Single(x => x.Name.Contains("1")).Tap(x => res.Releases.Remove(x));
        var res2 = await service.Edits.Tracks.Update(res, config.ApplicationId, editId, "internal").ExecuteAsync();


        await service.Edits.Commit(config.ApplicationId, editId).ExecuteAsync();


        return;

        async Task CreateEdit()
        {
            var edit = await service.Edits.Insert(new AppEdit(), config.ApplicationId).ExecuteAsync();
            edit.Dump();
        }

        async Task UploadBundle()
        {
            await service
                .Edits.Bundles.Upload(config.ApplicationId,editId,
                    File.OpenRead(
                        "C:\\Users\\PCS\\Documents\\repos\\CoursePlanner\\output\\com.tbui17.courseplanner-Signed.aab"
                    ),
                    "application/octet-stream"
                )
                .UploadAsync();
        }

        async Task ListBundle()
        {
            var res = await service.Edits.Bundles.List(config.ApplicationId, editId).ExecuteAsync();
            res.Dump();
            res.Bundles.Should().ContainSingle(x => x.VersionCode == 2);
        }

        async Task ListTracks()
        {
            var res2 = await service.Edits.Tracks.List(config.ApplicationId, editId).ExecuteAsync();
            res2.Dump();
        }

        async Task Commit()
        {
            await service.Edits.Commit(config.ApplicationId, editId).ExecuteAsync();
        }
    }
}