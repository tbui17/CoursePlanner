using Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services;

public interface ICourseService
{
    Task<Course?> GetFullCourse(int id);
    Task<(Course?, List<Instructor>)> GetDetailedCourseViewData(int id);
}

public class CourseService(ILocalDbCtxFactory factory) : ICourseService
{


    public async Task<Course?> GetFullCourse(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var course = await db
           .Courses
           .Include(x => x.Instructor)
           .Include(x => x.Assessments)
           .Include(x => x.Notes)
           .AsNoTracking()
           .AsSplitQuery()
           .FirstOrDefaultAsync(x => x.Id == id);

        return course;
    }

    public async Task<(Course?, List<Instructor>)> GetDetailedCourseViewData(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var t1 = GetFullCourse(id);
        var t2 = db.Instructors.AsNoTracking().AsSplitQuery().ToListAsync();
        return await (t1, t2);

    }
}