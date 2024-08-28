namespace CoursePlanner;

public partial class App
{
    public App(IServiceProvider provider)
    {
        InitializeComponent();
        MainPage = provider.GetRequiredService<AppShell>();
    }
}