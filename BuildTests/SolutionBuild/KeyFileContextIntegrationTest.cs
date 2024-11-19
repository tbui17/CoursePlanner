using System.Text;
using BuildLib.Secrets;
using BuildLib.SolutionBuild;
using BuildLib.Utils;
using BuildTests.TestSetup;
using FluentAssertions;
using FluentAssertions.Execution;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nuke.Common.ProjectModel;
using Xunit.Abstractions;

namespace BuildTests.SolutionBuild;

[TestSubject(typeof(KeyFileContext))]
public class KeyFileContextTest : BaseContainerSetup
{
    private readonly KeyFileContext _ctx;
    private readonly string _path;

    public KeyFileContextTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        var config = new DotnetPublishAndroidConfiguration()
        {
            KeystoreFile = "hello world"
                .Thru(Encoding.UTF8.GetBytes)
                .Thru(Convert.ToBase64String),
            AndroidSigningKeyStore = $"{Guid.NewGuid()}.keystore",
        };
        var fac = new KeyFileContextFactory(
            NullLogger<KeyFileContextFactory>.Instance,
            Options.Create(config),
            NullLoggerFactory.Instance,
            Resolve<Solution>()
        );
        var ctx = fac.Create();
        _path = ctx.KeyFile.FullName;
        _ctx = ctx;
    }

    protected override ValueTask OnDisposeAsync()
    {
        File.Delete(_path);
        return ValueTask.CompletedTask;
    }


    [Fact]
    public async Task WriteAsync_CreatesValidKeystoreFile()
    {
        await _ctx.WriteAsync();
        var file = new FileInfo(_path);
        using var _ = new AssertionScope();
        file.Exists.Should().BeTrue();
        file.Length.Should().BeGreaterThan(0);
        file.Extension.Should().BeOneOf(".keystore", ".jks", ".pfx", ".p12");
    }

    [Fact]
    public async Task Dispose_DeletesFile()
    {
        await _ctx.WriteAsync();
        new FileInfo(_path).Exists.Should().BeTrue();
        _ctx.Dispose();
        new FileInfo(_path).Exists.Should().BeFalse();
    }
}

[TestSubject(typeof(KeyFileContext))]
public class KeyFileContextIntegrationTest : BaseContainerSetup
{
    private readonly KeyFileContext _ctx;
    private readonly string _path;

    public KeyFileContextIntegrationTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        var fac = Resolve<KeyFileContextFactory>();
        var ctx = fac.Create();
        _path = ctx.KeyFile.FullName;
        _ctx = ctx;
    }

    protected override ValueTask OnDisposeAsync()
    {
        File.Delete(_path);
        return ValueTask.CompletedTask;
    }


    [Fact]
    public async Task WriteAsync_CreatesValidKeystoreFile()
    {
        using var ctx = _ctx;
        await ctx.WriteAsync();
        var file = new FileInfo(_path);
        using var _ = new AssertionScope();
        file.Exists.Should().BeTrue();
        file.Length.Should().BeGreaterThan(0);
        file.Extension.Should().BeOneOf(".keystore", ".jks", ".pfx", ".p12");
    }
}