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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder b)
    {
        b
           .Entity<User>()
           .HasData
            (
                new User
                {
                    Id = 1,
                    Name = "John Doe",
                    Phone = "1234567890",
                    Email = "john.doe@example.com"
                },
                new User
                {
                    Id = 2,
                    Name = "Jane Smith",
                    Phone = "0987654321",
                    Email = "jane.smith@example.com"
                }
            );


        b
           .Entity<Term>()
           .HasData
            (
                new Term
                {
                    Id = 1,
                    UserId = 1,
                    Name = "Fall 2023",
                    Start = new DateTime(2023, 9, 1),
                    End = new DateTime(2023, 12, 31)
                }
            );


        b
           .Entity<ObjectiveAssessment>()
           .HasData
            (
                new ObjectiveAssessment
                {
                    Id = 1,
                    Name = "Midterm Exam",
                    Start = new DateTime(2023, 10, 15),
                    End = new DateTime(2023, 10, 15)
                }
            );


        b
           .Entity<PerformanceAssessment>()
           .HasData
            (
                new PerformanceAssessment
                {
                    Id = 1,
                    Name = "Final Project",
                    Start = new DateTime(2023, 12, 1),
                    End = new DateTime(2023, 12, 15)
                }
            );


        b
           .Entity<Course>()
           .HasData
            (
                new Course
                {
                    Id = 1,
                    InstructorId = 1,
                    PerformanceAssessmentId = 1,
                    ObjectiveAssessmentId = 1,
                    Name = "Computer Science 101",
                    Start = new DateTime(2023, 9, 1),
                    End = new DateTime(2023, 12, 31)
                }
            );


        b
           .Entity<Note>()
           .HasData
            (
                new Note
                {
                    Id = 1,
                    CourseId = 1,
                    UserId = 1,
                    Value = "This is a note for Computer Science 101."
                }
            );


        b
           .Entity<ObjectiveAssessmentCourse>()
           .HasData
            (
                new ObjectiveAssessmentCourse
                {
                    Id = 1,
                    AssessmentId = 1,
                    CourseId = 1,
                    StudentId = 1,
                    NotifyStart = true,
                    NotifyEnd = true
                }
            );


        b
           .Entity<PerformanceAssessmentCourse>()
           .HasData
            (
                new PerformanceAssessmentCourse
                {
                    Id = 1,
                    AssessmentId = 1,
                    CourseId = 1,
                    StudentId = 1,
                    NotifyStart = true,
                    NotifyEnd = true
                }
            );


        b
           .Entity<TermCourse>()
           .HasData
            (
                new TermCourse
                {
                    Id = 1,
                    TermId = 1,
                    CourseId = 1,
                    StudentId = 1,
                    Status = Status.PlanToTake,
                    NotifyStart = true,
                    NotifyEnd = true
                }
            );
    }
}