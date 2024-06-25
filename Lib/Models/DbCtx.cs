using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

public class DbCtx : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder b)
    {
        b.UseSqlite
        (
            "Data Source=db.sqlite",
            x => x.MigrationsAssembly("Data")
        );
    }
}