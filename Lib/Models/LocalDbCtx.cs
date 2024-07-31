using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lib.Models;

public class LocalDbCtx : DbCtx
{
    public static string File { get; set; } = "";

    public static string DbFileName { get; set; } = "db.sqlite";

    public static string ApplicationDirectoryPath { get; set; } =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);


    public LocalDbCtx()
    {
        File = Path.Combine(ApplicationDirectoryPath, DbFileName);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
           .UseSqlite($"Filename={File}")
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors()
           .LogTo(Console.WriteLine,LogLevel.Information);
    }
}