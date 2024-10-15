using BuildLib.Commands;
using BuildLib.Utils;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = Container.CreateBuilder();
var host = builder.Build();

var app = CoconaApp.CreateBuilder().Build();


app.AddCommand("example_command",
    () =>
    {
        var logger = ResolveLogger<CoconaApp>();
        logger.LogInformation("Hello, World!");
    }
);
app.AddCommand("publish",
    async () =>
    {
        var cmd = Resolve<PublishCommand>();
        await cmd.ExecuteAsync();
    }
);

app.Run();

return;

T Resolve<T>() where T : notnull => host.Services.GetRequiredService<T>();

ILogger<T> ResolveLogger<T>() => host.Services.GetRequiredService<ILogger<T>>();