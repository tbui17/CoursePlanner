using Lib.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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
        try
        {
            db.Database.Migrate();
        }
        catch (SqliteException e) when (e.Message.Contains("already exists"))
        {
            Console.WriteLine($"A database entity already exists. Attempting to re-initialize database. ${e.Message}");
            db.Database.EnsureDeleted();
            db.Database.Migrate();
            Console.WriteLine("Database re-initialized.");
        }
    }
}