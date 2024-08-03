using Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services;

public interface ICourseService
{
    Task<Course?> GetFullCourse(int id);
}

public class CourseService(IDbContextFactory<LocalDbCtx> factory) : ICourseService
{


    public async Task<Course?> GetFullCourse(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var course = await db
           .Courses
           .Include(x => x.Instructor)
           .Include(x => x.Assessments)
           .Include(x => x.Notes)
           .FirstOrDefaultAsync(x => x.Id == id);

        return course;
    }

}