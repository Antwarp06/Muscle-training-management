using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MuscleTrainingApi.Models;

namespace MuscleTrainingApi.Controllers;

[Route("api/Cardio")]
[ApiController]
public class CardioController : SecuredController
{
    private readonly string _connectionString;

    public CardioController(IConfiguration configuration)
    {
        // 【修正】"DefaultConneciton" と綴りを間違えていたため、接続文字列が常に空だった。
        // GET の catch が「エラーでも200＋空リスト」を返す作りだったため、
        // 画面上は「記録0件」に見えて、原因が表に出ていなかった。
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    // --- 記録の保存(POST) ---
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Cardio cardio)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "INSERT INTO \"Cardio\" (\"User_Id\", \"Exercise_Name\", \"Duration_Min\", \"Distance_Km\", \"CreatedAt\") VALUES (@userId, @name, @duration, @distance, NOW())",
                conn);

            cmd.Parameters.AddWithValue("userId", CurrentUserId);
            cmd.Parameters.AddWithValue("name", cardio.Exercise_Name);
            cmd.Parameters.AddWithValue("duration", cardio.Duration_Min);
            // 距離は未入力を許すため、null のときは DBNull に変換して渡す。
            // C# の null をそのまま渡すことはできない。
            cmd.Parameters.AddWithValue("distance", (object?)cardio.Distance_Km ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();

            return Ok(new { message = "保存成功!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"【Cardio保存エラー】 : {ex.Message}");
            return StatusCode(500, new { message = "保存に失敗しました。" });
        }
    }

    // --- 記録履歴の一覧取得(GET) ---
    [HttpGet]
    public async Task<ActionResult<IEnumerable<dynamic>>> GetCardios()
    {
        var cardios = new List<dynamic>();
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // WHERE の User_Id 条件が、他人の記録を見えなくしている唯一の防壁
            string sql = @"
                SELECT ""Cardio_Id"", ""Exercise_Name"", ""Duration_Min"", ""Distance_Km"", ""CreatedAt""
                FROM ""Cardio""
                WHERE ""User_Id"" = @userId
                ORDER BY ""Cardio_Id"" DESC";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("userId", CurrentUserId);
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
            // 【修正】以前は Ok(空リスト) を返していたため、DBが落ちていても
            // 「記録0件」に見えてエラーに気づけなかった。500を返して表に出す。
            Console.WriteLine($"【Cardio取得エラー】: {ex.Message}");
            return StatusCode(500, new { message = "履歴の取得に失敗しました。" });
        }
    }

    // --- 記録の削除(DELETE) ---
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // "AND User_Id" が無いと、番号を変えるだけで他人の記録を削除できてしまう
            using var cmd = new NpgsqlCommand(
                "DELETE FROM \"Cardio\" WHERE \"Cardio_Id\" = @id AND \"User_Id\" = @userId", conn);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("userId", CurrentUserId);
            int affectedRows = await cmd.ExecuteNonQueryAsync();

            if (affectedRows == 0)
            {
                return NotFound(new { message = "指定された記録は見つかりませんでした。" });
            }
            return Ok(new { message = "削除完了しました。" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"【Cardio削除エラー】: {ex.Message}");
            return StatusCode(500, new { message = "削除に失敗しました。" });
        }
    }
}
