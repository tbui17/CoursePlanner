using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

public class DbCtx : DbContext
{
    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Term> Terms { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Assessment> Assessments { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder b)
    {
        b
           .UseSqlite("Data Source=db.sqlite")
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
           .Entity<Course>()
           .HasOne(x => x.Instructor)
           .WithMany(x => x.Courses)
           .HasForeignKey(x => x.InstructorId)
           .OnDelete(DeleteBehavior.SetNull);
    }


}