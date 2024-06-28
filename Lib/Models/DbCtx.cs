using Microsoft.EntityFrameworkCore;
using static Lib.Utils.TestDataFactory;

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
           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder b)
    {
        var (terms, instructors, courses, notes, assessments) = CreateData();

        b
           .Entity<Term>()
           .HasData(terms);
        b
           .Entity<Instructor>()
           .HasData(instructors);
        b
           .Entity<Course>()
           .HasData(courses);
        b
           .Entity<Note>()
           .HasData(notes);
        b
           .Entity<Assessment>()
           .HasData(assessments);
    }
}