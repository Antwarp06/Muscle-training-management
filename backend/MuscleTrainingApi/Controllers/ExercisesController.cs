using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MuscleTrainingApi.Controllers;

[Route("api/Exercises")]
[ApiController]
public class ExercisesController : SecuredController{
    private readonly string _connectionString;

    public ExercisesController(IConfiguration configuration){
        _connectionString = configuration.GetConnectionString("DefaultConnection")?? "";
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id){
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        try{
            // "AND User_Id" が無いと、番号を変えるだけで他人の種目を削除できてしまう
            using var cmd = new NpgsqlCommand(
                "DELETE FROM \"Exercises\" WHERE \"Exercise_Id\" = @id AND \"User_Id\" = @userId", conn);
            cmd.Parameters.AddWithValue("id",id);
            cmd.Parameters.AddWithValue("userId", CurrentUserId);

            int affectedRows = await cmd.ExecuteNonQueryAsync();
            if(affectedRows == 0) return NotFound(new { message = "種目が見つかりませんでした" });

            return Ok(new {message = "種目を削除しました"});
        }
        catch (PostgresException ex) when (ex.SqlState == "23503"){
            // 外部キー FK_Workout_Exercises_Exercise_Id による保護
            return BadRequest(new {message = "この種目のトレーニング記録が存在するため削除できません。"});
        }
        catch (Exception ex){
            Console.WriteLine($"【Exercise削除エラー】: {ex.Message}");
            return StatusCode(500, new { message = "種目の削除に失敗しました。" });
        }
    }
}
