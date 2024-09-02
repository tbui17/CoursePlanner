using Lib.Interfaces;
using Lib.Utils;
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
    public DbSet<UserSetting> UserSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("DataSource=database.db");
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Course>()
            .HasOne(x => x.Instructor)
            .WithMany(x => x.Courses)
            .HasForeignKey(x => x.InstructorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .Property(x => x.Username)
            .UseCollation(Collation.CaseInsensitive);

        modelBuilder.Entity<UserSetting>()
            .Property(x => x.NotificationRange)
            .HasConversion(
                x => x.Ticks,
                x => TimeSpan.FromTicks(x)
            );


        var dateTimeEntities = modelBuilder.Model.GetEntityTypes()
            .Where(x => typeof(IDateTimeEntity).IsAssignableFrom(x.ClrType));


        foreach (var entity in dateTimeEntities)
        {
            modelBuilder
                .Entity(entity.ClrType)
                .HasIndex(nameof(IDateTimeEntity.Start), nameof(IDateTimeEntity.End));
        }
    }


    public IEnumerable<IQueryable<T>> GetDbSets<T>() where T : class =>
        DbContextUtil.GetDbSets<LocalDbCtx, T>()
            .Select(x => x.GetValue(this))
            .Cast<IQueryable<T>>();
}

file static class Collation
{
    public const string CaseInsensitive = "NOCASE";
}