﻿using Microsoft.EntityFrameworkCore;

namespace Lib.Models;

public class LocalDbCtx : DbContext
{

    public LocalDbCtx(){}

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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=database.db");
    }
}