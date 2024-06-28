using Lib.Models;

namespace Lib.Utils;

public class TestDataFactory
{
    private static bool ToBool(int i) => i % 2 == 0;

    public static (List<Term> terms, List<Instructor> instructors, List<Course> courses, List<Note> notes,
        List<Assessment> assessments) CreateData()
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
                var course = Course.From(term);
                course.Id = courseIdCounter;
                course.Name = ToName(courseIdCounter, i);
                course.InstructorId = instructors[i % instructors.Count].Id;
                course.Status = (Status)(courseIdCounter % 4);
                course.TermId = term.Id;
                course.ShouldNotify = ToBool(i);
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
                var note = Note.From(course);
                note.Id = noteIdCounter;
                note.Value = ToName(noteIdCounter, i);
                note.CourseId = course.Id;
                notes.Add(note);
                noteIdCounter++;

                var assessment = Assessment.From(course);
                assessment.Id = assessmentIdCounter;
                assessment.Name = ToName(assessmentIdCounter, i);
                assessment.CourseId = course.Id;
                assessment.ShouldNotify = ToBool(i);
                assessments.Add(assessment);
                assessmentIdCounter++;
            }
        }

        return (terms, instructors, courses, notes, assessments);
    }

    private static string ToName(int id, int i) => $"ID: {id}, Index: {i}";

    public static List<Instructor> CreateInstructors()
    {
        return
        [
            new()
            {
                Id = 1,
                Name = "Instructor One",
                Phone = "1111111111",
                Email = "instructor.one@example.com"
            },
            new()
            {
                Id = 2,
                Name = "Instructor Two",
                Phone = "2222222222",
                Email = "instructor.two@example.com"
            },
            new()
            {
                Id = 3,
                Name = "Instructor Three",
                Phone = "3333333333",
                Email = "instructor.three@example.com"
            },
            new()
            {
                Id = 4,
                Name = "Instructor Four",
                Phone = "4444444444",
                Email = "instructor.four@example.com"
            },
            new()
            {
                Id = 5,
                Name = "Instructor Five",
                Phone = "5555555555",
                Email = "instructor.five@example.com"
            }
        ];
    }

    public static List<Term> CreateTerms()
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
}