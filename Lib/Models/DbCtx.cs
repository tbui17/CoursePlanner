using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

public class DbCtx : DbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<Term> Term { get; set; }
    public DbSet<TermCourse> TermCourse { get; set; }
    public DbSet<PerformanceAssessmentCourse> AssessmentCourse { get; set; }
    public DbSet<ObjectiveAssessmentCourse> ObjectiveAssessmentCourse { get; set; }
    public DbSet<Course> Course { get; set; }
    public DbSet<Assessment> Assessment { get; set; }
    public DbSet<Note> Note { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder b)
    {
        b.UseSqlite
        (
            "Data Source=db.sqlite",
            x => x.MigrationsAssembly("Data")
        );
    }
}