using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

public class DbCtx : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Term> Terms { get; set; }
    public DbSet<TermCourse> TermCourses { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<PerformanceAssessment> PerformanceAssessments { get; set; }
    public DbSet<ObjectiveAssessment> ObjectiveAssessments { get; set; }
    public DbSet<PerformanceAssessmentCourse> PerformanceAssessmentCourses { get; set; }
    public DbSet<ObjectiveAssessmentCourse> ObjectiveAssessmentCourses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder b)
    {
        b.UseSqlite
        (
            "Data Source=db.sqlite",
            x => x.MigrationsAssembly("Data")
        );
    }
}