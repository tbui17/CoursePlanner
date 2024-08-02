using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lib.Models;

public class LocalDbCtx : DbCtx
{
    private string DbFileName { get; set; } = "db.sqlite";

    public string ApplicationDirectoryPath { get; init; } =
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
           .UseSqlite($"Filename={DbFileName}")
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors()
           .LogTo(Console.WriteLine, LogLevel.Information);
    }
}