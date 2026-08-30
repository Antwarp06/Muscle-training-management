using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MuscleTrainingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCardioOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cardio",
                schema: "public",
                columns: table => new
                {
                    Cardio_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User_Id = table.Column<int>(type: "integer", nullable: false),
                    Exercise_Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Duration_Min = table.Column<int>(type: "integer", nullable: false),
                    Distance_Km = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cardio", x => x.Cardio_Id);
                    table.ForeignKey(
                        name: "FK_Cardio_Users_User_Id",
                        column: x => x.User_Id,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "User_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cardio_User_CreatedAt",
                schema: "public",
                table: "Cardio",
                columns: new[] { "User_Id", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cardio",
                schema: "public");
        }
    }
}
