using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lib.Migrations
{
    /// <inheritdoc />
    public partial class Indexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                collation: "NOCASE",
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_Terms_Start_End",
                table: "Terms",
                columns: new[] { "Start", "End" });

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Start_End",
                table: "Courses",
                columns: new[] { "Start", "End" });

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_Start_End",
                table: "Assessments",
                columns: new[] { "Start", "End" });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Username",
                table: "Accounts",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Terms_Start_End",
                table: "Terms");

            migrationBuilder.DropIndex(
                name: "IX_Courses_Start_End",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Assessments_Start_End",
                table: "Assessments");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Username",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldCollation: "NOCASE");
        }
    }
}
