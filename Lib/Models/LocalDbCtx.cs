using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

public class LocalDbCtx : DbContext
{
    public LocalDbCtx()
    {
    }

    public LocalDbCtx(DbContextOptions<LocalDbCtx> options) : base(options)
    {
    }

    public DbSet<Instructor> Instructors { get; set; }
    public DbSet<Term> Terms { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Assessment> Assessments { get; set; }
    public DbSet<User> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Course>()
            .HasOne(x => x.Instructor)
            .WithMany(x => x.Courses)
            .HasForeignKey(x => x.InstructorId)
            .OnDelete(DeleteBehavior.SetNull);
    }


    public IEnumerable<IQueryable<T>> GetImplementingSets<T>() where T : class =>
        GetImplementingSetsBase<T>()
            .Select(x => x.GetValue(this))
            .Cast<IQueryable<T>>();

    private static IEnumerable<PropertyInfo> GetImplementingSetsBase<T>() where T : class =>
        typeof(LocalDbCtx)
            .GetProperties()
            .Where(x => x.PropertyType.IsGenericType)
            .Where(x => x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Where(x => x.PropertyType.GenericTypeArguments[0].IsAssignableTo(typeof(T)));





}