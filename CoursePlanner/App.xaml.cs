using Lib.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
#if ANDROID
using Android.Util;
#endif

namespace CoursePlanner;

public partial class App
{
    public App()
    {
        InitDb();

        InitializeComponent();

        MainPage = new AppShell();
    }

    public static void InitDb()
    {
        LocalDbCtx.ApplicationDirectoryPath = FileSystem.Current.AppDataDirectory;
        using var db = new LocalDbCtx();

        Info("DATABASE", "Attempting to migrate database.");
        try
        {
            db.Database.Migrate();
            Info("DATABASE", "Database migrated.");
        }
        catch (SqliteException e) when (e.Message.Contains("already exists"))
        {
            Info("DATABASE", $"A database entity already exists. Attempting to re-initialize database. ${e.Message}");
            db.Database.EnsureDeleted();
            db.Database.Migrate();
            Info("DATABASE", "Database re-initialized.");
        }
    }

    private static void Info(string tag, string message)
    {
#if ANDROID
        Log.Info(tag, message);
#endif
    }
}