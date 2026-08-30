using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MuscleTrainingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            // 【手動で削除】自動生成された RenameTable 3件をここから削除している。
            // モデルの [Table(Schema = "public")] が既存スナップショットに記録されていなかったため、
            // EF が「public スキーマへ移動が必要」と誤判定して生成したもの。
            // 実際のテーブルは既に public にあり、そのまま実行すると
            // 「table "Workout" is already in schema "public"」でマイグレーションが失敗する。

            migrationBuilder.AlterColumn<string>(
                name: "Exercise_Name",
                schema: "public",
                table: "Exercises",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Category_Name",
                schema: "public",
                table: "Categories",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "public",
                columns: table => new
                {
                    User_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User_Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Password_Hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.User_Id);
                });

            // 【手動で追加】ユーザー名の一意制約。
            // LOWER() を使った関数インデックスは EF のモデルでは表現できないため、生のSQLで作成する。
            // これにより "naoki" と "Naoki" が同一人物として扱われ、紛らわしい重複登録を防げる。
            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"UQ_Users_UserName\" ON public.\"Users\" (LOWER(\"User_Name\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // インデックスはテーブルごと消えるため、DropTable だけでよい
            migrationBuilder.DropTable(
                name: "Users",
                schema: "public");

            // 【手動で削除】Up 側と対になる RenameTable 3件を削除している（理由は Up のコメント参照）

            migrationBuilder.AlterColumn<string>(
                name: "Exercise_Name",
                table: "Exercises",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Category_Name",
                table: "Categories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);
        }
    }
}
