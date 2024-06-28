namespace CoursePlanner;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    public static async Task<string?> DisplayNamePromptAsync()
    {
        return await Current.CurrentPage.DisplayPromptAsync("Enter name", "");
    }

    public static async Task GoToAsync<T>(IDictionary<string, object>? query = null) where T : ContentPage
    {
        var route = $"/{typeof(T).Name}";
        if (query is not null)
        {
            await Current.GoToAsync(route, query);
        }
        else
        {
            await Current.GoToAsync(route);
        }
    }

    public static async Task GoBackAsync()
    {
        const string route = "..";
        await Current.GoToAsync(route);
    }
}