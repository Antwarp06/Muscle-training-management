using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MuscleTrainingApi.Models;

namespace MuscleTrainingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WorkoutsController : ControllerBase
{
    private readonly string _connectionString;

    public WorkoutsController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Workout workout)
    {
        // 1. データベースに接続
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // 2. SQLの準備（新しいテーブル構造に合わせる）
        using var cmd = new NpgsqlCommand(
            "INSERT INTO \"Workout\" (exercise_id, weight, reps) VALUES (@exId, @weight, @reps)", 
            conn
        );

        // 3. パラメータのセット（大文字小文字はモデルに合わせてください）
        cmd.Parameters.AddWithValue("exId", workout.Exercise_Id);
        cmd.Parameters.AddWithValue("weight", workout.Weight);
        cmd.Parameters.AddWithValue("reps", workout.Reps);

        // 4. 実行
        await cmd.ExecuteNonQueryAsync();

        return Ok(new { message = "保存成功！" });
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetWorkouts()
    {
        var workouts = new List<dynamic>();
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        // SQLでExercisesテーブルと結合して、種目名(exercise_name)を取得する
        // record_idの降順(DESC)にすることで、新しい記録を上に持ってくる
        string sql = @"
            SELECT 
                w.record_id, 
                e.exercise_name, 
                w.weight, 
                w.reps 
            FROM ""Workout"" w
            JOIN Exercises e ON w.exercise_id = e.exercise_id
            ORDER BY w.record_id DESC";

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            workouts.Add(new
            {
                Record_Id = reader.GetInt32(0),
                Exercise_Name = reader.GetString(1), // ここで「種目名」が入る
                Weight = reader.GetDouble(2),
                Reps = reader.GetInt32(3)
            });
        }
        return Ok(workouts);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id){
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        //指定されたrecord_idの行を削除するSQL
        using var cmd = new NpgsqlCommand("DELETE FROM \"Workout\" record_id = @id",conn);
        cmd.Parameters.AddWithValue( "id",id );
        int affectedRows = await cmd.ExecuteNonQueryAsync();

        if( affectedRows == 0 ){
            return NotFound(new { message = "指定された記録は見つかりませんでした。"});
        }
        return Ok(new { message = "削除完了しました。"});
    }
}