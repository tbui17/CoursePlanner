using Lib.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;



namespace CoursePlanner;

public partial class App
{
    public App()
    {
        LocalDbCtx.ApplicationDirectoryPath = FileSystem.Current.AppDataDirectory;
        using var db = new LocalDbCtx();
        try
        {
            db.Database.Migrate();
        }
        catch (SqliteException e) when (e.Message.Contains("already exists"))
        {
            Console.Error.WriteLine(e);
            db.Database.EnsureDeleted();
            db.Database.Migrate();
        }
        
        InitializeComponent();
        
        MainPage = new AppShell();
    }
}