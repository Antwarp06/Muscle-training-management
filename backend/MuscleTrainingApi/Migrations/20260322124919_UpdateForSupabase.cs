using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuscleTrainingApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateForSupabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Event_Name",
                table: "Workout");

            migrationBuilder.AddColumn<int>(
                name: "Exercise_Id",
                table: "Workout",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exercise_Id",
                table: "Workout");

            migrationBuilder.AddColumn<string>(
                name: "Event_Name",
                table: "Workout",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
