using Serilog;

namespace MauiConfig;

public interface IMauiServiceBuilder
{
    void AddViews();
    void AddAppServices();
}