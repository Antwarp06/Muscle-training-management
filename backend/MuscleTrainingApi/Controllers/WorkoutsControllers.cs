using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MuscleTrainingApi.Models;

namespace MuscleTrainingApi.Controllers;

[Route("api/Workouts")]
[ApiController]
public class WorkoutsController : ControllerBase
{
    private readonly string _connectionString;

    public WorkoutsController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    // --- 記録の保存 (POST) ---
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Workout workout)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // CreatedAt は NOT NULL 制約があるため、DB側の NOW() で現在日時を入れる
            using var cmd = new NpgsqlCommand(
                "INSERT INTO \"Workout\" (\"Exercise_Id\", \"Weight\", \"Reps\", \"CreatedAt\") VALUES (@exId, @weight, @reps, NOW())",
                conn
            );

            cmd.Parameters.AddWithValue("exId", workout.Exercise_Id);
            cmd.Parameters.AddWithValue("weight", workout.Weight);
            cmd.Parameters.AddWithValue("reps", workout.Reps);

            await cmd.ExecuteNonQueryAsync();

            return Ok(new { message = "保存成功！" });
        }
        catch (Exception ex)
        {
            // 例外を投げっぱなしにするとCORSヘッダーが消えてブラウザでCORSエラーに化けるため、
            // ここで捕まえて500として返す
            Console.WriteLine($"【Workouts保存エラー】: {ex.Message}");
            return StatusCode(500, new { message = "保存に失敗しました。" });
        }
    }

    // --- 記録履歴の一覧取得 (GET) ---
    [HttpGet]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetWorkouts()
    {
        var workouts = new List<dynamic>();
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
                SELECT 
                    w.""Record_Id"", 
                    e.""Exercise_Name"", 
                    w.""Weight"", 
                    w.""Reps"" 
                FROM ""Workout"" w
                JOIN ""Exercises"" e ON w.""Exercise_Id"" = e.""Exercise_Id""
                ORDER BY w.""Record_Id"" DESC";

            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var rawRecordId = reader.GetValue(0);
                var rawExerciseName = reader.GetValue(1);
                var rawWeight = reader.GetValue(2);
                var rawReps = reader.GetValue(3);

                workouts.Add(new
                {
                    record_Id = rawRecordId != DBNull.Value ? Convert.ToInt32(rawRecordId) : 0,
                    exercise_Name = rawExerciseName != DBNull.Value ? rawExerciseName.ToString() : "",
                    weight = rawWeight != DBNull.Value ? Convert.ToDouble(rawWeight) : 0.0,
                    reps = rawReps != DBNull.Value ? Convert.ToInt32(rawReps) : 0
                });
            }
            return Ok(workouts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"【Workouts取得エラー回避】: {ex.Message}");
            return Ok(new List<dynamic>());
        }
    } 

    // --- 記録の削除 (DELETE) ---
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        
        using var cmd = new NpgsqlCommand("DELETE FROM \"Workout\" WHERE \"Record_Id\" = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        int affectedRows = await cmd.ExecuteNonQueryAsync();

        if (affectedRows == 0)
        {
            return NotFound(new { message = "指定された記録は見つかりませんでした。" });
        }
        return Ok(new { message = "削除完了しました。" });
    }
} 