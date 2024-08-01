﻿using Lib.Models;

namespace Lib.Utils;



public class TestDataFactory
{
    private  bool ToBool(int i) => i % 2 == 0;

    public  TestDataResult CreateData()
    {
        var terms = CreateTerms();
        var instructors = CreateInstructors();
        var courses = new List<Course>();
        var notes = new List<Note>();
        var assessments = new List<Assessment>();

        var range = Enumerable
           .Range(1, 6)
           .ToList();

        var courseIdCounter = 1;
        foreach (var term in terms)
        {
            foreach (var i in range)
            {
                var course = new Course(term)
                {
                    Id = courseIdCounter,
                    Name = $"Course {courseIdCounter}",
                    InstructorId = instructors[i % instructors.Count].Id,
                    Status = Course.Statuses.ElementAt(courseIdCounter % Course.Statuses.Count),
                    TermId = term.Id,
                    ShouldNotify = ToBool(i)
                };
                courses.Add(course);
                courseIdCounter++;
            }
        }


        var noteIdCounter = 1;
        var assessmentIdCounter = 1;

        foreach (var course in courses)
        {
            foreach (var i in range)
            {
                var text = new List<string>();
                var note = Note.From(course);
                i.Times(() => text.Add($"Note Text {note.Id}"));
                note.Id = noteIdCounter;
                note.Value = text.StringJoin($"/{i}/");
                note.CourseId = course.Id;
                note.Name = $"Note {noteIdCounter}";
                notes.Add(note);
                noteIdCounter++;

                var assessment = Assessment.From(course);
                assessment.Id = assessmentIdCounter;
                assessment.Name = $"Assessment {assessmentIdCounter}";
                assessment.CourseId = course.Id;
                assessment.ShouldNotify = ToBool(i);
                assessments.Add(assessment);
                assessmentIdCounter++;
            }
        }

        return new TestDataResult
        {
            Terms = terms,
            Instructors = instructors,
            Courses = courses,
            Notes = notes,
            Assessments = assessments
        };
    }



    public  List<Instructor> CreateInstructors()
    {
        return
        [
            new()
            {
                Id = 1,
                Name = "Instructor One",
                Phone = "1111111",
                Email = "instructor.one@example.com"
            },
            new()
            {
                Id = 2,
                Name = "Instructor Two",
                Phone = "2222222",
                Email = "instructor.two@example.com"
            },
            new()
            {
                Id = 3,
                Name = "Instructor Three",
                Phone = "3333333",
                Email = "instructor.three@example.com"
            },
            new()
            {
                Id = 4,
                Name = "Instructor Four",
                Phone = "4444444",
                Email = "instructor.four@example.com"
            },
            new()
            {
                Id = 5,
                Name = "Instructor Five",
                Phone = "5555555",
                Email = "instructor.five@example.com"
            }
        ];
    }

    public  List<Term> CreateTerms()
    {
        return
        [
            new Term
            {
                Id = 1,
                Name = "Term 1",
                Start = new DateTime(2023, 9, 1),
                End = new DateTime(2023, 12, 31)
            },
            new Term
            {
                Id = 2,
                Name = "Term 2",
                Start = new DateTime(2024, 1, 1),
                End = new DateTime(2024, 5, 31)
            },
            new Term
            {
                Id = 3,
                Name = "Term 3",
                Start = new DateTime(2023, 9, 1),
                End = new DateTime(2023, 12, 31)
            },
            new Term
            {
                Id = 4,
                Name = "Term 4",
                Start = new DateTime(2024, 1, 1),
                End = new DateTime(2024, 5, 31)
            }
        ];
    }

    public async Task SeedDatabase(LocalDbCtx db)
    {
        var data = CreateData();
        await db.Terms.AddRangeAsync(data.Terms);
        await db.Instructors.AddRangeAsync(data.Instructors);
        await db.Courses.AddRangeAsync(data.Courses);
        await db.Notes.AddRangeAsync(data.Notes);
        await db.Assessments.AddRangeAsync(data.Assessments);
        await db.SaveChangesAsync();
    }
}

public record TestDataResult
{
    public required List<Term> Terms { get; set; }
    public required List<Instructor> Instructors { get; set; }
    public required List<Course> Courses { get; set; }
    public required List<Note> Notes { get; set; }
    public required List<Assessment> Assessments { get; set; }

    public void Deconstruct(out List<Term> terms, out List<Instructor> instructors, out List<Course> courses, out List<Note> notes, out List<Assessment> assessments)
    {
        terms = Terms;
        instructors = Instructors;
        courses = Courses;
        notes = Notes;
        assessments = Assessments;
    }
}