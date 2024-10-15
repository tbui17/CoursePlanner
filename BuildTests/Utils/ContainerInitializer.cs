using BuildLib.Utils;
using Microsoft.Extensions.Hosting;

namespace BuildTests.Utils;

public class ContainerInitializer
{
    public Container GetContainer() => Container.Init();
    public HostApplicationBuilder GetBuilder() => Container.CreateBuilder();
}