using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lib.Migrations
{
    /// <inheritdoc />
    public partial class Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ObjectiveAssessments",
                columns: new[] { "Id", "End", "Name", "Start" },
                values: new object[,]
                {
                    { 1, new DateTime(2020, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 1", new DateTime(2020, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2020, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 2", new DateTime(2020, 10, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2020, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 3", new DateTime(2020, 10, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2020, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 4", new DateTime(2020, 10, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2020, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 5", new DateTime(2020, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2020, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 6", new DateTime(2020, 10, 6, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, new DateTime(2020, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 7", new DateTime(2020, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, new DateTime(2020, 10, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 8", new DateTime(2020, 10, 8, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, new DateTime(2020, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 9", new DateTime(2020, 10, 9, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 10, new DateTime(2020, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 10", new DateTime(2020, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 11, new DateTime(2020, 10, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 11", new DateTime(2020, 10, 11, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 12, new DateTime(2020, 10, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Objective Assessment 12", new DateTime(2020, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "PerformanceAssessments",
                columns: new[] { "Id", "End", "Name", "Start" },
                values: new object[,]
                {
                    { 1, new DateTime(2020, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 1", new DateTime(2020, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2020, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 2", new DateTime(2020, 11, 2, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2020, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 3", new DateTime(2020, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2020, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 4", new DateTime(2020, 11, 4, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2020, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 5", new DateTime(2020, 11, 5, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2020, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 6", new DateTime(2020, 11, 6, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, new DateTime(2020, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 7", new DateTime(2020, 11, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, new DateTime(2020, 11, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 8", new DateTime(2020, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, new DateTime(2020, 11, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 9", new DateTime(2020, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 10, new DateTime(2020, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 10", new DateTime(2020, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 11, new DateTime(2020, 11, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 11", new DateTime(2020, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 12, new DateTime(2020, 11, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "Performance Assessment 12", new DateTime(2020, 11, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Terms",
                columns: new[] { "Id", "End", "Name", "Start", "UserId" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Term 1", new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 2, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Term 2", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 3, new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Term 3", new DateTime(2023, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { 4, new DateTime(2024, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Term 4", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Name", "Phone" },
                values: new object[,]
                {
                    { 1, "john.doe@example.com", "John Doe", "1234567890" },
                    { 2, "jane.smith@example.com", "Jane Smith", "0987654321" },
                    { 3, "instructor.one@example.com", "Instructor One", "1111111111" },
                    { 4, "instructor.two@example.com", "Instructor Two", "2222222222" },
                    { 5, "instructor.three@example.com", "Instructor Three", "3333333333" }
                });

            migrationBuilder.InsertData(
                table: "Courses",
                columns: new[] { "Id", "End", "InstructorId", "Name", "ObjectiveAssessmentId", "PerformanceAssessmentId", "Start" },
                values: new object[,]
                {
                    { 1, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "Course 1", 1, 1, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "Course 2", 2, 2, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Course 3", 3, 3, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "Course 4", 4, 4, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "Course 5", 5, 5, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Course 6", 6, 6, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "Course 7", 7, 7, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 8, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "Course 8", 8, 8, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Course 9", 9, 9, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 10, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "Course 10", 10, 10, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 11, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, "Course 11", 11, 11, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 12, new DateTime(2020, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Course 12", 12, 12, new DateTime(2020, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Notes",
                columns: new[] { "Id", "CourseId", "UserId", "Value" },
                values: new object[,]
                {
                    { 1, 1, 2, "This is a note for Course 1." },
                    { 2, 2, 1, "This is a note for Course 2." },
                    { 3, 3, 2, "This is a note for Course 3." },
                    { 4, 4, 1, "This is a note for Course 4." },
                    { 5, 5, 2, "This is a note for Course 5." },
                    { 6, 6, 1, "This is a note for Course 6." },
                    { 7, 7, 2, "This is a note for Course 7." },
                    { 8, 8, 1, "This is a note for Course 8." },
                    { 9, 9, 2, "This is a note for Course 9." },
                    { 10, 10, 1, "This is a note for Course 10." },
                    { 11, 11, 2, "This is a note for Course 11." },
                    { 12, 12, 1, "This is a note for Course 12." }
                });

            migrationBuilder.InsertData(
                table: "ObjectiveAssessmentCourses",
                columns: new[] { "Id", "AssessmentId", "CourseId", "NotifyEnd", "NotifyStart", "StudentId" },
                values: new object[,]
                {
                    { 1, 1, 1, true, false, 2 },
                    { 2, 2, 2, false, true, 1 },
                    { 3, 3, 3, true, false, 2 },
                    { 4, 4, 4, false, true, 1 },
                    { 5, 5, 5, true, false, 2 },
                    { 6, 6, 6, false, true, 1 },
                    { 7, 7, 7, true, false, 2 },
                    { 8, 8, 8, false, true, 1 },
                    { 9, 9, 9, true, false, 2 },
                    { 10, 10, 10, false, true, 1 },
                    { 11, 11, 11, true, false, 2 },
                    { 12, 12, 12, false, true, 1 }
                });

            migrationBuilder.InsertData(
                table: "PerformanceAssessmentCourses",
                columns: new[] { "Id", "AssessmentId", "CourseId", "NotifyEnd", "NotifyStart", "StudentId" },
                values: new object[,]
                {
                    { 1, 1, 1, true, false, 2 },
                    { 2, 2, 2, false, true, 1 },
                    { 3, 3, 3, true, false, 2 },
                    { 4, 4, 4, false, true, 1 },
                    { 5, 5, 5, true, false, 2 },
                    { 6, 6, 6, false, true, 1 },
                    { 7, 7, 7, true, false, 2 },
                    { 8, 8, 8, false, true, 1 },
                    { 9, 9, 9, true, false, 2 },
                    { 10, 10, 10, false, true, 1 },
                    { 11, 11, 11, true, false, 2 },
                    { 12, 12, 12, false, true, 1 }
                });

            migrationBuilder.InsertData(
                table: "TermCourses",
                columns: new[] { "Id", "CourseId", "NotifyEnd", "NotifyStart", "Status", "StudentId", "TermId" },
                values: new object[,]
                {
                    { 1, 1, true, false, 0, 2, 2 },
                    { 2, 2, false, true, 0, 1, 3 },
                    { 3, 3, true, false, 0, 2, 4 },
                    { 4, 4, false, true, 0, 1, 1 },
                    { 5, 5, true, false, 0, 2, 2 },
                    { 6, 6, false, true, 0, 1, 3 },
                    { 7, 7, true, false, 0, 2, 4 },
                    { 8, 8, false, true, 0, 1, 1 },
                    { 9, 9, true, false, 0, 2, 2 },
                    { 10, 10, false, true, 0, 1, 3 },
                    { 11, 11, true, false, 0, 2, 4 },
                    { 12, 12, false, true, 0, 1, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessmentCourses",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessmentCourses",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TermCourses",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Courses",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Terms",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Terms",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Terms",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Terms",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ObjectiveAssessments",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "PerformanceAssessments",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
