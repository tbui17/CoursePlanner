using SimpleExec;
using static Bullseye.Targets;
using static SimpleExec.Command;


Target("build", () => RunAsync("dotnet", "build --configuration Release --nologo"));
Target("test", () => RunAsync("dotnet", "test --configuration Release --nologo"));
Target("default", DependsOn("test"));

await RunTargetsAndExitAsync(args, ex => ex is ExitCodeException);