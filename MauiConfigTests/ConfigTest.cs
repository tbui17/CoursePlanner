using System.Collections.Concurrent;
using System.Threading.Channels;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Attributes;
using Lib.Services;
using Lib.Validators;
using MauiConfig;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Serilog;
using ViewModels.Config;
using ViewModels.Domain;
using ViewModels.ExceptionHandlers;
using ViewModels.Setup;

namespace MauiConfigTests;

public class ConfigTest
{
    private class TestPage1 : ContentPage;

    public async Task TearDown()
    {
        var channel = Channel.CreateBounded<FileSystemInfo>(Math.Max(Environment.ProcessorCount - 2, 1));

        var tasks = new ConcurrentBag<Task>();
        await channel.Writer.WriteAsync(new DirectoryInfo("."));

        while (channel.Reader.TryRead(out var fsi))
        {
            Impl(fsi);
        }

        await Task.WhenAll(tasks);
        return;

        void Impl(FileSystemInfo fsi)
        {
            if (fsi is FileInfo fi)
            {
                if (fi.Name is "test.db" or "log.json")
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception e)
                    {
                    }
                }

                return;
            }

            var dir = (DirectoryInfo)fsi;

            dir.EnumerateFileSystemInfos()
                .AsParallel()
                .Select(x => channel.Writer.WriteAsync(x))
                .Select(x => x.AsTask())
                .ForAll(x => tasks.Add(x));
        }
    }


    [Test]
    public void Configuration_ContainsRequiredDependencies()
    {
        var fixture = CreateFixture();
        var builder = MauiApp.CreateBuilder();

        var serviceBuilder = fixture.CreateMock<IMauiServiceBuilder>();
        var registration = fixture.CreateMock<Action<UnhandledExceptionEventHandler>>();
        var mainPageGetter = fixture.CreateMock<MainPageGetter>();
        var appDataDirectoryGetter = fixture.CreateMock<Func<string>>();


        mainPageGetter.Setup(x => x()).Returns(new TestPage1());
        appDataDirectoryGetter.Setup(x => x()).Returns("test");

        var config = new MauiAppServiceConfiguration
        {
            ServiceBuilder = serviceBuilder.Object,
            Services = builder.Services,
            AppDataDirectory = appDataDirectoryGetter.Object,
            MainPage = mainPageGetter.Object,
            ExceptionHandlerRegistration = registration.Object,
        };

        config.AddServices();


        config.Services.Should()
            .HaveCountGreaterThan(30);
    }


    private static IFixture CreateFixture()
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization());
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }


    [Test]
    public void Config_ShouldRetrieveConcreteClassesFromDomainNamespace()
    {
        var services = new ServiceCollection();

        var clientConfig = new ViewModelConfig(services);
        clientConfig.AddServices().AddInjectables();

        services
            .Should()
            .ContainSingle(x => x.ServiceType == typeof(InstructorFormViewModelFactory))
            .And.ContainSingle(x => x.ImplementationType == typeof(EditAssessmentViewModel))
            .And.ContainSingle(x => x.ServiceType == typeof(IEditAssessmentViewModel));
    }

    [Test]
    public void Config_ShouldInjectBackendServices()
    {
        var services = new ServiceCollection();

        new ViewModelConfig(services).AddServices().AddInjectables();
        using var _ = new AssertionScope();
        services
            .Where(x => x.ImplementationType == typeof(LoginFieldValidator))
            .Should()
            .HaveCount(2);

        services.Should()
            .ContainSingle(x => x.ServiceType == typeof(IAccountService));
    }


    [Test]
    public void Configuration_PerformsExpectedStartupActions()
    {
        var fixture = CreateFixture();
        var builder = MauiApp.CreateBuilder();

        var serviceBuilder = fixture.CreateMock<IMauiServiceBuilder>();
        var registration = fixture.CreateMock<Action<UnhandledExceptionEventHandler>>();
        var mainPageGetter = fixture.CreateMock<MainPageGetter>();
        var appDataDirectoryGetter = fixture.CreateMock<Func<string>>();


        mainPageGetter.Setup(x => x()).Returns(new TestPage1());
        appDataDirectoryGetter.Setup(x => x()).Returns("test");


        var config = new MauiAppServiceConfiguration
        {
            ServiceBuilder = serviceBuilder.Object,
            Services = builder.Services,
            AppDataDirectory = appDataDirectoryGetter.Object,
            MainPage = mainPageGetter.Object,
            ExceptionHandlerRegistration = registration.Object,
        };
        var dbSetupMock = fixture.CreateMock<IDbSetup>();

        config.AddServices();
        builder.Services.AddSingleton(dbSetupMock.Object);
        builder.Services.AddSingleton(registration.Object);
        builder.Services.AddSingleton(mainPageGetter.Object);
        builder.Services.AddSingleton(appDataDirectoryGetter.Object);



        using var app = builder.Build();
        config.RunStartupActions(app);


        dbSetupMock.Verify(x => x.SetupDb());


        registration.Verify(x => x(It.IsAny<UnhandledExceptionEventHandler>()));

        appDataDirectoryGetter.Verify(ap => ap());
    }
}

public static class FixtureExtensions
{
    public static Mock<T> CreateMock<T>(this IFixture fixture) where T : class => fixture.Create<Mock<T>>();
}

public class ProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    public IServiceCollection CreateBuilder(IServiceCollection services)
    {
        return services;
    }

    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        return containerBuilder.BuildServiceProvider();
    }
}