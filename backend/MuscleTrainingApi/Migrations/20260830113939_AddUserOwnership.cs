using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuscleTrainingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 【手動で修正】自動生成では nullable:false / defaultValue:0 で追加されていた。
            // それだと既存の12件（部位4・種目3・記録5）が「存在しないユーザー0番」に紐づき、
            // このあとの外部キー作成が必ず失敗する。
            // そのため「NULL許可で追加 → 既存行を埋める → NOT NULL化」の3段階に分けている。

            // --- 段階1: まず NULL を許可した状態で列だけ追加する ---
            // ここで NOT NULL を付けると、既存行に入れる値が無いためエラーになる。
            migrationBuilder.AddColumn<int>(
                name: "User_Id",
                schema: "public",
                table: "Workout",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "User_Id",
                schema: "public",
                table: "Exercises",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "User_Id",
                schema: "public",
                table: "Categories",
                type: "integer",
                nullable: true);

            // --- 段階2: 既存データの持ち主を設定する ---
            // ログイン機能の導入前に作られたデータなので、すべて User_Id = 1 が引き取る。
            // 新規DBに対して実行した場合は対象0件になるだけで、害はない。
            migrationBuilder.Sql("UPDATE public.\"Workout\"    SET \"User_Id\" = 1 WHERE \"User_Id\" IS NULL;");
            migrationBuilder.Sql("UPDATE public.\"Exercises\"  SET \"User_Id\" = 1 WHERE \"User_Id\" IS NULL;");
            migrationBuilder.Sql("UPDATE public.\"Categories\" SET \"User_Id\" = 1 WHERE \"User_Id\" IS NULL;");

            // --- 段階3: NOT NULL に切り替える ---
            // 段階2で埋め漏らした行があれば、ここで失敗して気づける。
            migrationBuilder.AlterColumn<int>(
                name: "User_Id",
                schema: "public",
                table: "Workout",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "User_Id",
                schema: "public",
                table: "Exercises",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "User_Id",
                schema: "public",
                table: "Categories",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workout_Exercise_Id",
                schema: "public",
                table: "Workout",
                column: "Exercise_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Workout_User_CreatedAt",
                schema: "public",
                table: "Workout",
                columns: new[] { "User_Id", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Category_Id",
                schema: "public",
                table: "Exercises",
                column: "Category_Id");

            migrationBuilder.CreateIndex(
                name: "UQ_Exercises_User_Cat_Name",
                schema: "public",
                table: "Exercises",
                columns: new[] { "User_Id", "Category_Id", "Exercise_Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Categories_User_Name",
                schema: "public",
                table: "Categories",
                columns: new[] { "User_Id", "Category_Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_User_Id",
                schema: "public",
                table: "Categories",
                column: "User_Id",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "User_Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Categories_Category_Id",
                schema: "public",
                table: "Exercises",
                column: "Category_Id",
                principalSchema: "public",
                principalTable: "Categories",
                principalColumn: "Category_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Exercises_Users_User_Id",
                schema: "public",
                table: "Exercises",
                column: "User_Id",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "User_Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workout_Exercises_Exercise_Id",
                schema: "public",
                table: "Workout",
                column: "Exercise_Id",
                principalSchema: "public",
                principalTable: "Exercises",
                principalColumn: "Exercise_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workout_Users_User_Id",
                schema: "public",
                table: "Workout",
                column: "User_Id",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "User_Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_User_Id",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Categories_Category_Id",
                schema: "public",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Exercises_Users_User_Id",
                schema: "public",
                table: "Exercises");

            migrationBuilder.DropForeignKey(
                name: "FK_Workout_Exercises_Exercise_Id",
                schema: "public",
                table: "Workout");

            migrationBuilder.DropForeignKey(
                name: "FK_Workout_Users_User_Id",
                schema: "public",
                table: "Workout");

            migrationBuilder.DropIndex(
                name: "IX_Workout_Exercise_Id",
                schema: "public",
                table: "Workout");

            migrationBuilder.DropIndex(
                name: "IX_Workout_User_CreatedAt",
                schema: "public",
                table: "Workout");

            migrationBuilder.DropIndex(
                name: "IX_Exercises_Category_Id",
                schema: "public",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "UQ_Exercises_User_Cat_Name",
                schema: "public",
                table: "Exercises");

            migrationBuilder.DropIndex(
                name: "UQ_Categories_User_Name",
                schema: "public",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "User_Id",
                schema: "public",
                table: "Workout");

            migrationBuilder.DropColumn(
                name: "User_Id",
                schema: "public",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "User_Id",
                schema: "public",
                table: "Categories");
        }
    }
}
