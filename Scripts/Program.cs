using BuildLib.Commands;
using BuildLib.Utils;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = Container.CreateBuilder<Container>();
var host = builder.Build();

var app = CoconaApp.CreateBuilder().Build();


app.AddCommand("run",
    () =>
    {
        var logger = ResolveLogger<CoconaApp>();
        logger.LogInformation("Hello, World!");
    }
);
app.AddCommand("publish",
    async () =>
    {
        const string projectPath = "CoursePlanner.csproj";
        var cmd = Resolve<PublishCommand>();
        await cmd.ExecuteAsync(projectPath);
    }
);

app.Run();

return;

T Resolve<T>() where T : notnull => host.Services.GetRequiredService<T>();

ILogger<T> ResolveLogger<T>() => host.Services.GetRequiredService<ILogger<T>>();