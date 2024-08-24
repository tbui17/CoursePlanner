using Lib.Models;

namespace Lib.Utils;

public class TestDataFactory
{
    private bool ToBool(int i) => i % 2 == 0;


    public Term CreateC6Data()
    {

        var baseDate = new DateTime(2020, 1, 1);
        var oneMonthLater = baseDate.AddMonths(1);
        var sixMonthsLater = baseDate.AddMonths(6);

        var term = new Term
        {
            Name = "Test Term 1",
            Start = baseDate,
            End = sixMonthsLater,
            Courses = [
                new()
                {
                    Name = "Test Course 1",
                    Assessments = [
                        new()
                        {
                            Name = "Test Assessment 1",
                            Type = Assessment.Objective,
                            Start = baseDate,
                            End = oneMonthLater,
                            ShouldNotify = true,
                        },
                        new()
                        {
                            Name = "Test Assessment 2",
                            Type = Assessment.Performance,
                            Start = baseDate,
                            End = oneMonthLater,
                            ShouldNotify = false,
                        }
                    ],
                    Instructor = new()
                    {
                        Name = "Anika Patel",
                        Phone = "555-123-4567",
                        Email = "anika.patel@strimeuniversity.edu",
                    },
                    Status = Course.PlanToTake,
                    Start = baseDate,
                    End = oneMonthLater,
                }
            ]
        };

        return term;
    }

    public TestDataResult CreateData()
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
                var course = term.CreateCourse();

                course.Id = courseIdCounter;
                course.Name = $"Course {courseIdCounter}";
                course.InstructorId = instructors[i % instructors.Count].Id;
                course.Status = Course.Statuses.ElementAt(courseIdCounter % Course.Statuses.Count);
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
            2.Times(i =>
                {
                    var assessment = course.CreateAssessment();
                    assessment.Id = assessmentIdCounter;
                    assessment.Name = $"Assessment {assessmentIdCounter}";
                    assessment.CourseId = course.Id;
                    assessment.ShouldNotify = ToBool(i);
                    assessments.Add(assessment);
                    assessmentIdCounter++;
                    assessment.Type = i == 0
                        ? Assessment.Objective
                        : Assessment.Performance;
                }
            );


            foreach (var i in range)
            {
                var text = new List<string>();
                var note = course.CreateNote();
                i.Times(() => text.Add($"Note Text {note.Id}"));
                note.Id = noteIdCounter;
                note.Value = text.StringJoin($"/{i}/");
                note.CourseId = course.Id;
                note.Name = $"Note {noteIdCounter}";
                notes.Add(note);
                noteIdCounter++;
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


    public List<Instructor> CreateInstructors()
    {
        return new List<Instructor>
        {
            new()
            {
                Id = 1,
                Name = "Instructor One",
                Phone = "(111) 111-1111",
                Email = "instructor.one@example.com"
            },
            new()
            {
                Id = 2,
                Name = "Instructor Two",
                Phone = "(222) 222-2222",
                Email = "instructor.two@example.com"
            },
            new()
            {
                Id = 3,
                Name = "Instructor Three",
                Phone = "(333) 333-3333",
                Email = "instructor.three@example.com"
            },
            new()
            {
                Id = 4,
                Name = "Instructor Four",
                Phone = "(444) 444-4444",
                Email = "instructor.four@example.com"
            },
            new()
            {
                Id = 5,
                Name = "Instructor Five",
                Phone = "(555) 555-5555",
                Email = "instructor.five@example.com"
            }
        };
    }

    public List<Term> CreateTerms()
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
        db.Terms.AddRange(data.Terms);
        db.Instructors.AddRange(data.Instructors);
        db.Courses.AddRange(data.Courses);
        db.Notes.AddRange(data.Notes);
        db.Assessments.AddRange(data.Assessments);
        await db.SaveChangesAsync();
        var c6data = CreateC6Data();
        db.Terms.Add(c6data);
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

    public void Deconstruct(
        out List<Term> terms,
        out List<Instructor> instructors,
        out List<Course> courses,
        out List<Note> notes,
        out List<Assessment> assessments
    )
    {
        terms = Terms;
        instructors = Instructors;
        courses = Courses;
        notes = Notes;
        assessments = Assessments;
    }
}