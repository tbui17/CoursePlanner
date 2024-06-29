using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Markup;

namespace CoursePlanner.Utils;

public static class PageExtensions
{
    public static void SetBackBehavior<T>(
        this T page,
        string _,
        [CallerArgumentExpression(nameof(_))] string path = null!
    ) where T : Page
    {
        var fullPath = FullPath(path);
        if (!fullPath.Contains("Command"))
        {
            throw new ArgumentException($"Path {fullPath} must be a command");
        }

        Shell.SetBackButtonBehavior(page, new BackButtonBehavior().Bind(fullPath));
    }

  
}