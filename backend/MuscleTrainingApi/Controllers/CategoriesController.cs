using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MuscleTrainingApi.Controllers;

[Route("api/Categories")]
[ApiController]
public class CategoriesController : SecuredController{
    private readonly string _connectionString;

    public CategoriesController(IConfiguration configuration){
        _connectionString = configuration.GetConnectionString("DefaultConnection")?? "";
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id){
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        try{
            // "AND User_Id" が無いと、番号を変えるだけで他人の部位を削除できてしまう
            using var cmd = new NpgsqlCommand(
                "DELETE FROM \"Categories\" WHERE \"Category_Id\" = @id AND \"User_Id\" = @userId", conn);
            cmd.Parameters.AddWithValue("id",id);
            cmd.Parameters.AddWithValue("userId", CurrentUserId);

            int affectedRows = await cmd.ExecuteNonQueryAsync();
            if (affectedRows == 0) return NotFound(new { message = "部位が見つかりませんでした" });

            return Ok(new { message = "部位を削除しました"});
        }
        catch (PostgresException ex) when (ex.SqlState == "23503"){
            // 外部キー FK_Exercises_Categories_Category_Id による保護。
            // この制約は2026/08/30のマイグレーションで初めてDBに追加されたため、
            // それ以前はこのメッセージが表示されることはなかった。
            return BadRequest(new { message = "この部位に紐づく種目が存在するため削除できません"});
        }
        catch (Exception ex){
            Console.WriteLine($"【Category削除エラー】: {ex.Message}");
            return StatusCode(500, new { message = "部位の削除に失敗しました。" });
        }
    }
}
