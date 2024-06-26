﻿using Microsoft.EntityFrameworkCore;

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
        CreateBasicData();
        CreateComplexData();

        return;


        void CreateBasicData()
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
                    },
                    new User
                    {
                        Id = 3,
                        Name = "Instructor One",
                        Phone = "1111111111",
                        Email = "instructor.one@example.com"
                    },
                    new User
                    {
                        Id = 4,
                        Name = "Instructor Two",
                        Phone = "2222222222",
                        Email = "instructor.two@example.com"
                    },
                    new User
                    {
                        Id = 5,
                        Name = "Instructor Three",
                        Phone = "3333333333",
                        Email = "instructor.three@example.com"
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
                        Name = "Term 1",
                        Start = new DateTime(2023, 9, 1),
                        End = new DateTime(2023, 12, 31)
                    },
                    new Term
                    {
                        Id = 2,
                        UserId = 1,
                        Name = "Term 2",
                        Start = new DateTime(2024, 1, 1),
                        End = new DateTime(2024, 5, 31)
                    },
                    new Term
                    {
                        Id = 3,
                        UserId = 2,
                        Name = "Term 3",
                        Start = new DateTime(2023, 9, 1),
                        End = new DateTime(2023, 12, 31)
                    },
                    new Term
                    {
                        Id = 4,
                        UserId = 2,
                        Name = "Term 4",
                        Start = new DateTime(2024, 1, 1),
                        End = new DateTime(2024, 5, 31)
                    }
                );
        }

        void CreateComplexData()
        {
            var objectiveAssessments = new List<ObjectiveAssessment>();
            var performanceAssessments = new List<PerformanceAssessment>();
            var objectiveAssessmentCourses = new List<ObjectiveAssessmentCourse>();
            var performanceAssessmentCourses = new List<PerformanceAssessmentCourse>();
            var courses = new List<Course>();
            var notes = new List<Note>();
            var termCourses = new List<TermCourse>();


            for (var i = 1; i <= 12; i++)
            {
                objectiveAssessments.Add
                (
                    new ObjectiveAssessment
                    {
                        Id = i,
                        Name = $"Objective Assessment {i}",
                        Start = new DateTime(2023, 10, 15),
                        End = new DateTime(2023, 10, 15)
                    }
                );

                performanceAssessments.Add
                (
                    new PerformanceAssessment
                    {
                        Id = i,
                        Name = $"Performance Assessment {i}",
                        Start = new DateTime(2023, 12, 1),
                        End = new DateTime(2023, 12, 15)
                    }
                );

                objectiveAssessmentCourses.Add
                (
                    new ObjectiveAssessmentCourse
                    {
                        Id = i,
                        AssessmentId = i,
                        CourseId = i,
                        StudentId = (i % 2) + 1,
                        NotifyStart = true,
                        NotifyEnd = true
                    }
                );

                performanceAssessmentCourses.Add
                (
                    new PerformanceAssessmentCourse
                    {
                        Id = i,
                        AssessmentId = i,
                        CourseId = i,
                        StudentId = (i % 2) + 1,
                        NotifyStart = true,
                        NotifyEnd = true
                    }
                );

                courses.Add
                (
                    new Course
                    {
                        Id = i,
                        InstructorId = (i % 3) + 3,
                        PerformanceAssessmentId = i,
                        ObjectiveAssessmentId = i,
                        Name = $"Course {i}",
                        Start = new DateTime(2023, 9, 1),
                        End = new DateTime(2023, 12, 31)
                    }
                );

                notes.Add
                (
                    new Note
                    {
                        Id = i,
                        CourseId = i,
                        UserId = (i % 2) + 1,
                        Value = $"This is a note for Course {i}."
                    }
                );

                termCourses.Add
                (
                    new TermCourse
                    {
                        Id = i,
                        TermId = (i % 4) + 1,
                        CourseId = i,
                        StudentId = (i % 2) + 1,
                        Status = Status.PlanToTake,
                        NotifyStart = true,
                        NotifyEnd = true
                    }
                );
            }


            b
               .Entity<ObjectiveAssessment>()
               .HasData(objectiveAssessments);
            b
               .Entity<PerformanceAssessment>()
               .HasData(performanceAssessments);
            b
               .Entity<ObjectiveAssessmentCourse>()
               .HasData(objectiveAssessmentCourses);
            b
               .Entity<PerformanceAssessmentCourse>()
               .HasData(performanceAssessmentCourses);
            b
               .Entity<Course>()
               .HasData(courses);
            b
               .Entity<Note>()
               .HasData(notes);
            b
               .Entity<TermCourse>()
               .HasData(termCourses);
        }
    }
}