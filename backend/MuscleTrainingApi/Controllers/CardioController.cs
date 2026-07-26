using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MuscleTrainingApi.Models;

namespace MuscleTrainingApi.Models;

[Route("api/Cardio")]
[ApiController]
public class CardioController : ControllerBase
{
    private readonly string _connectionString;

    public CardioController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConneciton") ?? "";
    
    }
    
    // --- 記録の保存(POST) ---
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Cardio cardio)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand("INSERT INTO \"Cardio\" (\"Exercise_Name\", \"Duration_Min\", \"Distance_Km\", \"CreatedAt\") VALUES (@name, @duration, @distance, NOW())",
                conn);
            
            cmd.Parameters.AddWithValue("name", cardio.Exercise_Name);
            cmd.Parameters.AddWithValue("duration", cardio.Duration_Min);
            cmd.Parameters.AddWithValue("distance", (object?)cardio.Distance_Km ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            
            return Ok( new{ messsage = "保存成功!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"【Cardio保存エラー】 : {ex.Message}");
            return StatusCode(500, new { message = "保存に失敗しました。"});
        }
    }
    
    // ---記録履歴の一覧取得(GET) ---
    [HttpGet]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetCardios()
    {
        var cardios = new List<dynamic>();
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
                SELECT ""Cardio_Id"", ""Exercise_Name"", ""Duration_Min"", ""Distance_Km"", ""CreatedAt""
                FROM ""Cardio""
                ORDER BY ""Cardio_Id"" DESC";
            
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var rawDistance = reader.GetValue(3);

                cardios.Add(new
                {
                    cardio_Id = Convert.ToInt32(reader.GetValue(0)),
                    exercise_Name = reader.GetValue(1).ToString(),
                    duration_Min = Convert.ToInt32(reader.GetValue(2)),
                    distance_Km = rawDistance != DBNull.Value ? Convert.ToDouble(rawDistance) : (double?)null,
                    createdAt = Convert.ToDateTime(reader.GetValue(4))
                });
            }
            return Ok(cardios);
        }
        catch (Exception ex)
            {
                Console.WriteLine($"【Cardio取得エラー回避】: {ex.Message}");
                return Ok(new List<dynamic>());
            }
    }

    // --- 記録の削除(DELETE) ---
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new NpgsqlCommand("DELETE FROM \"Cardio\" WHERE \"Cardio_Id\" = @id", conn);
        cmd.Parameters.AddWithValue("id",id);
        int affectedRows = await cmd.ExecuteNonQueryAsync();

        if (affectedRows == 0)
        {
            return NotFound(new { message = "指定された記録は見つかりませんでした。"});
        }
        return Ok(new { message = "削除完了しました。"});
    }
}