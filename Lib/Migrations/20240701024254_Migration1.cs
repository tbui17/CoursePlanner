using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lib.Migrations
{
    /// <inheritdoc />
    public partial class Migration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instructors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Terms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    ShouldNotify = table.Column<bool>(type: "INTEGER", nullable: false),
                    TermId = table.Column<int>(type: "INTEGER", nullable: false),
                    InstructorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Courses_Instructors_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Instructors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Courses_Terms_TermId",
                        column: x => x.TermId,
                        principalTable: "Terms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ShouldNotify = table.Column<bool>(type: "INTEGER", nullable: false),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    CourseId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Instructors",
                columns: new[] { "Id", "Email", "Name", "Phone" },
                values: new object[,]
                {
                    { 1, "instructor.one@example.com", "Instructor One", "1111111111" },
                    { 2, "instructor.two@example.com", "Instructor Two", "2222222222" },
                    { 3, "instructor.three@example.com", "Instructor Three", "3333333333" },
                    { 4, "instructor.four@example.com", "Instructor Four", "4444444444" },
                    { 5, "instructor.five@example.com", "Instructor Five", "5555555555" }
                });

            migrationBuilder.InsertData(
                table: "Terms",
                columns: new[] { "Id", "End", "Name", "Start" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Term 1", new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Term 2", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Term 3", new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Term 4", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "End", "InstructorId", "Name", "ShouldNotify", "Start", "Status", "TermId" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "ID: 1, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "In Progress", 1 },
                    { 2, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "ID: 2, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 1 },
                    { 3, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "ID: 3, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 1 },
                    { 4, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "ID: 4, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plan to Take", 1 },
                    { 5, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "ID: 5, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "In Progress", 1 },
                    { 6, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "ID: 6, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 1 },
                    { 7, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "ID: 7, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 2 },
                    { 8, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "ID: 8, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plan to Take", 2 },
                    { 9, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "ID: 9, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "In Progress", 2 },
                    { 10, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "ID: 10, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 2 },
                    { 11, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "ID: 11, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 2 },
                    { 12, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "ID: 12, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plan to Take", 2 },
                    { 13, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "ID: 13, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "In Progress", 3 },
                    { 14, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "ID: 14, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 3 },
                    { 15, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "ID: 15, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 3 },
                    { 16, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "ID: 16, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plan to Take", 3 },
                    { 17, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "ID: 17, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "In Progress", 3 },
                    { 18, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "ID: 18, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 3 },
                    { 19, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "ID: 19, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 4 },
                    { 20, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "ID: 20, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plan to Take", 4 },
                    { 21, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "ID: 21, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "In Progress", 4 },
                    { 22, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "ID: 22, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Completed", 4 },
                    { 23, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "ID: 23, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Dropped", 4 },
                    { 24, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "ID: 24, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plan to Take", 4 }
                });

            migrationBuilder.InsertData(
                table: "Assessments",
                columns: new[] { "Id", "CourseId", "End", "Name", "ShouldNotify", "Start", "Type" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 1, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 2, 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 2, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 3, 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 3, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 4, 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 4, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 5, 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 5, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 6, 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 6, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 7, 2, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 7, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 8, 2, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 8, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 9, 2, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 9, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 10, 2, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 10, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 11, 2, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 11, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 12, 2, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 12, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 13, 3, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 13, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 14, 3, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 14, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 15, 3, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 15, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 16, 3, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 16, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 17, 3, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 17, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 18, 3, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 18, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 19, 4, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 19, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 20, 4, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 20, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 21, 4, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 21, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 22, 4, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 22, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 23, 4, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 23, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 24, 4, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 24, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 25, 5, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 25, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 26, 5, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 26, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 27, 5, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 27, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 28, 5, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 28, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 29, 5, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 29, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 30, 5, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 30, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 31, 6, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 31, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 32, 6, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 32, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 33, 6, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 33, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 34, 6, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 34, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 35, 6, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 35, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 36, 6, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 36, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 37, 7, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 37, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 38, 7, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 38, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 39, 7, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 39, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 40, 7, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 40, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 41, 7, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 41, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 42, 7, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 42, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 43, 8, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 43, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 44, 8, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 44, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 45, 8, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 45, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 46, 8, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 46, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 47, 8, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 47, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 48, 8, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 48, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 49, 9, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 49, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 50, 9, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 50, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 51, 9, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 51, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 52, 9, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 52, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 53, 9, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 53, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 54, 9, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 54, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 55, 10, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 55, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 56, 10, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 56, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 57, 10, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 57, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 58, 10, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 58, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 59, 10, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 59, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 60, 10, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 60, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 61, 11, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 61, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 62, 11, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 62, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 63, 11, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 63, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 64, 11, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 64, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 65, 11, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 65, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 66, 11, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 66, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 67, 12, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 67, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 68, 12, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 68, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 69, 12, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 69, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 70, 12, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 70, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 71, 12, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 71, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 72, 12, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 72, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 73, 13, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 73, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 74, 13, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 74, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 75, 13, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 75, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 76, 13, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 76, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 77, 13, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 77, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 78, 13, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 78, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 79, 14, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 79, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 80, 14, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 80, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 81, 14, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 81, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 82, 14, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 82, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 83, 14, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 83, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 84, 14, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 84, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 85, 15, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 85, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 86, 15, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 86, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 87, 15, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 87, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 88, 15, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 88, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 89, 15, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 89, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 90, 15, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 90, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 91, 16, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 91, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 92, 16, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 92, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 93, 16, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 93, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 94, 16, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 94, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 95, 16, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 95, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 96, 16, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 96, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 97, 17, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 97, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 98, 17, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 98, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 99, 17, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 99, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 100, 17, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 100, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 101, 17, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 101, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 102, 17, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 102, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 103, 18, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 103, Index: 1", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 104, 18, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 104, Index: 2", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 105, 18, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 105, Index: 3", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 106, 18, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 106, Index: 4", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 107, 18, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 107, Index: 5", false, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 108, 18, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 108, Index: 6", true, new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 109, 19, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 109, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 110, 19, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 110, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 111, 19, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 111, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 112, 19, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 112, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 113, 19, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 113, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 114, 19, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 114, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 115, 20, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 115, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 116, 20, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 116, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 117, 20, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 117, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 118, 20, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 118, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 119, 20, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 119, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 120, 20, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 120, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 121, 21, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 121, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 122, 21, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 122, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 123, 21, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 123, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 124, 21, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 124, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 125, 21, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 125, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 126, 21, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 126, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 127, 22, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 127, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 128, 22, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 128, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 129, 22, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 129, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 130, 22, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 130, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 131, 22, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 131, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 132, 22, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 132, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 133, 23, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 133, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 134, 23, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 134, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 135, 23, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 135, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 136, 23, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 136, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 137, 23, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 137, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 138, 23, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 138, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 139, 24, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 139, Index: 1", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 140, 24, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 140, Index: 2", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 141, 24, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 141, Index: 3", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 142, 24, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 142, Index: 4", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 143, 24, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 143, Index: 5", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" },
                    { 144, 24, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "ID: 144, Index: 6", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance" }
                });

            migrationBuilder.InsertData(
                table: "Notes",
                columns: new[] { "Id", "CourseId", "Name", "Value" },
                values: new object[,]
                {
                    { 1, 1, "", "ID: 1, Index: 1" },
                    { 2, 1, "", "ID: 2, Index: 2" },
                    { 3, 1, "", "ID: 3, Index: 3" },
                    { 4, 1, "", "ID: 4, Index: 4" },
                    { 5, 1, "", "ID: 5, Index: 5" },
                    { 6, 1, "", "ID: 6, Index: 6" },
                    { 7, 2, "", "ID: 7, Index: 1" },
                    { 8, 2, "", "ID: 8, Index: 2" },
                    { 9, 2, "", "ID: 9, Index: 3" },
                    { 10, 2, "", "ID: 10, Index: 4" },
                    { 11, 2, "", "ID: 11, Index: 5" },
                    { 12, 2, "", "ID: 12, Index: 6" },
                    { 13, 3, "", "ID: 13, Index: 1" },
                    { 14, 3, "", "ID: 14, Index: 2" },
                    { 15, 3, "", "ID: 15, Index: 3" },
                    { 16, 3, "", "ID: 16, Index: 4" },
                    { 17, 3, "", "ID: 17, Index: 5" },
                    { 18, 3, "", "ID: 18, Index: 6" },
                    { 19, 4, "", "ID: 19, Index: 1" },
                    { 20, 4, "", "ID: 20, Index: 2" },
                    { 21, 4, "", "ID: 21, Index: 3" },
                    { 22, 4, "", "ID: 22, Index: 4" },
                    { 23, 4, "", "ID: 23, Index: 5" },
                    { 24, 4, "", "ID: 24, Index: 6" },
                    { 25, 5, "", "ID: 25, Index: 1" },
                    { 26, 5, "", "ID: 26, Index: 2" },
                    { 27, 5, "", "ID: 27, Index: 3" },
                    { 28, 5, "", "ID: 28, Index: 4" },
                    { 29, 5, "", "ID: 29, Index: 5" },
                    { 30, 5, "", "ID: 30, Index: 6" },
                    { 31, 6, "", "ID: 31, Index: 1" },
                    { 32, 6, "", "ID: 32, Index: 2" },
                    { 33, 6, "", "ID: 33, Index: 3" },
                    { 34, 6, "", "ID: 34, Index: 4" },
                    { 35, 6, "", "ID: 35, Index: 5" },
                    { 36, 6, "", "ID: 36, Index: 6" },
                    { 37, 7, "", "ID: 37, Index: 1" },
                    { 38, 7, "", "ID: 38, Index: 2" },
                    { 39, 7, "", "ID: 39, Index: 3" },
                    { 40, 7, "", "ID: 40, Index: 4" },
                    { 41, 7, "", "ID: 41, Index: 5" },
                    { 42, 7, "", "ID: 42, Index: 6" },
                    { 43, 8, "", "ID: 43, Index: 1" },
                    { 44, 8, "", "ID: 44, Index: 2" },
                    { 45, 8, "", "ID: 45, Index: 3" },
                    { 46, 8, "", "ID: 46, Index: 4" },
                    { 47, 8, "", "ID: 47, Index: 5" },
                    { 48, 8, "", "ID: 48, Index: 6" },
                    { 49, 9, "", "ID: 49, Index: 1" },
                    { 50, 9, "", "ID: 50, Index: 2" },
                    { 51, 9, "", "ID: 51, Index: 3" },
                    { 52, 9, "", "ID: 52, Index: 4" },
                    { 53, 9, "", "ID: 53, Index: 5" },
                    { 54, 9, "", "ID: 54, Index: 6" },
                    { 55, 10, "", "ID: 55, Index: 1" },
                    { 56, 10, "", "ID: 56, Index: 2" },
                    { 57, 10, "", "ID: 57, Index: 3" },
                    { 58, 10, "", "ID: 58, Index: 4" },
                    { 59, 10, "", "ID: 59, Index: 5" },
                    { 60, 10, "", "ID: 60, Index: 6" },
                    { 61, 11, "", "ID: 61, Index: 1" },
                    { 62, 11, "", "ID: 62, Index: 2" },
                    { 63, 11, "", "ID: 63, Index: 3" },
                    { 64, 11, "", "ID: 64, Index: 4" },
                    { 65, 11, "", "ID: 65, Index: 5" },
                    { 66, 11, "", "ID: 66, Index: 6" },
                    { 67, 12, "", "ID: 67, Index: 1" },
                    { 68, 12, "", "ID: 68, Index: 2" },
                    { 69, 12, "", "ID: 69, Index: 3" },
                    { 70, 12, "", "ID: 70, Index: 4" },
                    { 71, 12, "", "ID: 71, Index: 5" },
                    { 72, 12, "", "ID: 72, Index: 6" },
                    { 73, 13, "", "ID: 73, Index: 1" },
                    { 74, 13, "", "ID: 74, Index: 2" },
                    { 75, 13, "", "ID: 75, Index: 3" },
                    { 76, 13, "", "ID: 76, Index: 4" },
                    { 77, 13, "", "ID: 77, Index: 5" },
                    { 78, 13, "", "ID: 78, Index: 6" },
                    { 79, 14, "", "ID: 79, Index: 1" },
                    { 80, 14, "", "ID: 80, Index: 2" },
                    { 81, 14, "", "ID: 81, Index: 3" },
                    { 82, 14, "", "ID: 82, Index: 4" },
                    { 83, 14, "", "ID: 83, Index: 5" },
                    { 84, 14, "", "ID: 84, Index: 6" },
                    { 85, 15, "", "ID: 85, Index: 1" },
                    { 86, 15, "", "ID: 86, Index: 2" },
                    { 87, 15, "", "ID: 87, Index: 3" },
                    { 88, 15, "", "ID: 88, Index: 4" },
                    { 89, 15, "", "ID: 89, Index: 5" },
                    { 90, 15, "", "ID: 90, Index: 6" },
                    { 91, 16, "", "ID: 91, Index: 1" },
                    { 92, 16, "", "ID: 92, Index: 2" },
                    { 93, 16, "", "ID: 93, Index: 3" },
                    { 94, 16, "", "ID: 94, Index: 4" },
                    { 95, 16, "", "ID: 95, Index: 5" },
                    { 96, 16, "", "ID: 96, Index: 6" },
                    { 97, 17, "", "ID: 97, Index: 1" },
                    { 98, 17, "", "ID: 98, Index: 2" },
                    { 99, 17, "", "ID: 99, Index: 3" },
                    { 100, 17, "", "ID: 100, Index: 4" },
                    { 101, 17, "", "ID: 101, Index: 5" },
                    { 102, 17, "", "ID: 102, Index: 6" },
                    { 103, 18, "", "ID: 103, Index: 1" },
                    { 104, 18, "", "ID: 104, Index: 2" },
                    { 105, 18, "", "ID: 105, Index: 3" },
                    { 106, 18, "", "ID: 106, Index: 4" },
                    { 107, 18, "", "ID: 107, Index: 5" },
                    { 108, 18, "", "ID: 108, Index: 6" },
                    { 109, 19, "", "ID: 109, Index: 1" },
                    { 110, 19, "", "ID: 110, Index: 2" },
                    { 111, 19, "", "ID: 111, Index: 3" },
                    { 112, 19, "", "ID: 112, Index: 4" },
                    { 113, 19, "", "ID: 113, Index: 5" },
                    { 114, 19, "", "ID: 114, Index: 6" },
                    { 115, 20, "", "ID: 115, Index: 1" },
                    { 116, 20, "", "ID: 116, Index: 2" },
                    { 117, 20, "", "ID: 117, Index: 3" },
                    { 118, 20, "", "ID: 118, Index: 4" },
                    { 119, 20, "", "ID: 119, Index: 5" },
                    { 120, 20, "", "ID: 120, Index: 6" },
                    { 121, 21, "", "ID: 121, Index: 1" },
                    { 122, 21, "", "ID: 122, Index: 2" },
                    { 123, 21, "", "ID: 123, Index: 3" },
                    { 124, 21, "", "ID: 124, Index: 4" },
                    { 125, 21, "", "ID: 125, Index: 5" },
                    { 126, 21, "", "ID: 126, Index: 6" },
                    { 127, 22, "", "ID: 127, Index: 1" },
                    { 128, 22, "", "ID: 128, Index: 2" },
                    { 129, 22, "", "ID: 129, Index: 3" },
                    { 130, 22, "", "ID: 130, Index: 4" },
                    { 131, 22, "", "ID: 131, Index: 5" },
                    { 132, 22, "", "ID: 132, Index: 6" },
                    { 133, 23, "", "ID: 133, Index: 1" },
                    { 134, 23, "", "ID: 134, Index: 2" },
                    { 135, 23, "", "ID: 135, Index: 3" },
                    { 136, 23, "", "ID: 136, Index: 4" },
                    { 137, 23, "", "ID: 137, Index: 5" },
                    { 138, 23, "", "ID: 138, Index: 6" },
                    { 139, 24, "", "ID: 139, Index: 1" },
                    { 140, 24, "", "ID: 140, Index: 2" },
                    { 141, 24, "", "ID: 141, Index: 3" },
                    { 142, 24, "", "ID: 142, Index: 4" },
                    { 143, 24, "", "ID: 143, Index: 5" },
                    { 144, 24, "", "ID: 144, Index: 6" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CourseId",
                table: "Assessments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_InstructorId",
                table: "Courses",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_TermId",
                table: "Courses",
                column: "TermId");

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_Email",
                table: "Instructors",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_CourseId",
                table: "Notes",
                column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "Instructors");

            migrationBuilder.DropTable(
                name: "Terms");
        }
    }
}
