using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MuscleTrainingApi.Models;

namespace MuscleTrainingApi.Controllers;

[Route("api/Workouts")]
[ApiController]
public class WorkoutsController : SecuredController
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
                "INSERT INTO \"Workout\" (\"User_Id\", \"Exercise_Id\", \"Weight\", \"Reps\", \"CreatedAt\") VALUES (@userId, @exId, @weight, @reps, NOW())",
                conn
            );

            // workout.User_Id は使わない。あちらは利用者が送ってきた値なので信用できない。
            cmd.Parameters.AddWithValue("userId", CurrentUserId);
            cmd.Parameters.AddWithValue("exId", workout.Exercise_Id);
            cmd.Parameters.AddWithValue("weight", workout.Weight);
            cmd.Parameters.AddWithValue("reps", workout.Reps);

            await cmd.ExecuteNonQueryAsync();

            return Ok(new { message = "保存成功！" });
        }
        catch (PostgresException ex) when (ex.SqlState == "23503")
        {
            // 外部キー違反。他人の種目IDや、存在しない種目IDを指定した場合にここへ来る。
            return BadRequest(new { message = "指定された種目が見つかりません。" });
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

            // WHERE の User_Id 条件が、他人の記録を見えなくしている唯一の防壁。
            // ここを消すと全員の記録が混ざって表示される。
            string sql = @"
                SELECT
                    w.""Record_Id"",
                    e.""Exercise_Name"",
                    w.""Weight"",
                    w.""Reps"",
                    w.""Exercise_Id"",
                    w.""CreatedAt""
                FROM ""Workout"" w
                JOIN ""Exercises"" e ON w.""Exercise_Id"" = e.""Exercise_Id""
                WHERE w.""User_Id"" = @userId
                ORDER BY w.""Record_Id"" DESC";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("userId", CurrentUserId);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var rawRecordId = reader.GetValue(0);
                var rawExerciseName = reader.GetValue(1);
                var rawWeight = reader.GetValue(2);
                var rawReps = reader.GetValue(3);
                var rawExerciseId = reader.GetValue(4);
                var rawCreatedAt = reader.GetValue(5);

                workouts.Add(new
                {
                    record_Id = rawRecordId != DBNull.Value ? Convert.ToInt32(rawRecordId) : 0,
                    exercise_Name = rawExerciseName != DBNull.Value ? rawExerciseName.ToString() : "",
                    weight = rawWeight != DBNull.Value ? Convert.ToDouble(rawWeight) : 0.0,
                    reps = rawReps != DBNull.Value ? Convert.ToInt32(rawReps) : 0,
                    exercise_Id = rawExerciseId != DBNull.Value ? Convert.ToInt32(rawExerciseId) : 0,
                    createdAt = rawCreatedAt != DBNull.Value ? Convert.ToDateTime(rawCreatedAt) : (DateTime?)null
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

        // 【最重要】"AND User_Id" を必ず併用する。
        // Record_Id だけを条件にすると、番号を書き換えるだけで
        // 他人の記録を削除できてしまう。
        using var cmd = new NpgsqlCommand(
            "DELETE FROM \"Workout\" WHERE \"Record_Id\" = @id AND \"User_Id\" = @userId", conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("userId", CurrentUserId);
        int affectedRows = await cmd.ExecuteNonQueryAsync();

        // 他人の記録を指定した場合もここに来る。
        // 「あなたのものではありません」と返すと記録の存在を教えることになるため、
        // 存在しない場合と同じ扱いにする。
        if (affectedRows == 0)
        {
            return NotFound(new { message = "指定された記録は見つかりませんでした。" });
        }
        return Ok(new { message = "削除完了しました。" });
    }
}
