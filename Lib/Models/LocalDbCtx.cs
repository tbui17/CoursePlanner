using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

public class LocalDbCtx : DbCtx
{
    public static string File { get; protected set; } = "";

    public LocalDbCtx()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        File = Path.Combine(path, "db.sqlite");
        Init();
    }

    private void Init()
    {
        Database.Migrate();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
           .UseSqlite($"Filename={File}");
    }
}