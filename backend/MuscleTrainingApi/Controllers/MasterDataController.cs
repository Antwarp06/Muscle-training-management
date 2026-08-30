using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MuscleTrainingApi.Models;

namespace MuscleTrainingApi.Controllers;

[Route("api/MasterData")]
[ApiController]
public class MasterDataController : SecuredController {
    private readonly string _connectionString;

    public MasterDataController(IConfiguration configuration) {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    // --- 部位一覧の取得 (GET) ---
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories() {
        var categories = new List<Category>();
        try {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // SELECT * をやめて列名を明示している。
            // User_Id が増えたことで列の順番が変わりうるため、番号での取り出しは危険。
            using var cmd = new NpgsqlCommand(
                "SELECT \"Category_Id\", \"Category_Name\" FROM \"Categories\" WHERE \"User_Id\" = @userId ORDER BY \"Category_Id\"",
                conn);
            cmd.Parameters.AddWithValue("userId", CurrentUserId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                categories.Add(new Category {
                    Category_Id = reader.GetInt32(0),
                    Category_Name = reader.GetString(1)
                });
            }
            return Ok(categories);
        }
        catch (Exception ex) {
            Console.WriteLine($"【Categories取得内部エラー】: {ex.Message}");
            return Ok(new List<Category>());
        }
    }

    // --- 種目一覧の取得 (GET) ---
    [HttpGet("exercises")]
    public async Task<ActionResult<IEnumerable<Exercise>>> GetExercises() {
        var exercises = new List<Exercise>();
        try {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "SELECT \"Exercise_Id\", \"Category_Id\", \"Exercise_Name\" FROM \"Exercises\" WHERE \"User_Id\" = @userId ORDER BY \"Exercise_Id\"",
                conn);
            cmd.Parameters.AddWithValue("userId", CurrentUserId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                exercises.Add(new Exercise {
                    Exercise_Id = reader.GetInt32(0),
                    Category_Id = reader.GetInt32(1),
                    Exercise_Name = reader.GetString(2)
                });
            }
            return Ok(exercises);
        }
        catch (Exception ex) {
            Console.WriteLine($"【Exercises取得内部エラー】: {ex.Message}");
            return Ok(new List<Exercise>());
        }
    }

    // --- 部位の追加 ---
    [HttpPost("categories")]
    public async Task<IActionResult> AddCategory([FromBody] Category category) {
        try {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(
                "INSERT INTO \"Categories\" (\"User_Id\", \"Category_Name\") VALUES (@userId, @name) RETURNING \"Category_Id\"",
                conn);
            cmd.Parameters.AddWithValue("userId", CurrentUserId);
            cmd.Parameters.AddWithValue("name", category.Category_Name);

            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return Ok(new { category_Id = newId });
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") {
            // 23505 = 一意制約違反（UQ_Categories_User_Name）。
            // 事前に COUNT(*) で確認する方式をやめ、DB制約に任せている。
            // 確認とINSERTの間に別のリクエストが割り込む隙間が無くなるため。
            return BadRequest(new { message = "その部位はすでに登録されています。" });
        }
        catch (Exception ex) {
            Console.WriteLine($"【Category追加エラー】: {ex.Message}");
            return StatusCode(500, new { message = "部位の追加に失敗しました。" });
        }
    }

    // --- 種目の追加 ---
    [HttpPost("exercises")]
    public async Task<IActionResult> AddExercise([FromBody] Exercise exercise) {
        try {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // 指定された部位が自分のものか確認する。
            // 外部キーは「その部位が存在するか」しか見ないため、
            // 他人の部位IDを指定された場合はここで弾く必要がある。
            using var ownerCmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM \"Categories\" WHERE \"Category_Id\" = @catId AND \"User_Id\" = @userId",
                conn);
            ownerCmd.Parameters.AddWithValue("catId", exercise.Category_Id);
            ownerCmd.Parameters.AddWithValue("userId", CurrentUserId);

            if (Convert.ToInt64(await ownerCmd.ExecuteScalarAsync()) == 0) {
                return BadRequest(new { message = "指定された部位が見つかりません。" });
            }

            using var cmd = new NpgsqlCommand(
                "INSERT INTO \"Exercises\" (\"User_Id\", \"Category_Id\", \"Exercise_Name\") VALUES (@userId, @catId, @name) RETURNING \"Exercise_Id\"",
                conn);
            cmd.Parameters.AddWithValue("userId", CurrentUserId);
            cmd.Parameters.AddWithValue("catId", exercise.Category_Id);
            cmd.Parameters.AddWithValue("name", exercise.Exercise_Name);

            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return Ok(new { exercise_Id = newId });
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") {
            // 23505 = 一意制約違反（UQ_Exercises_User_Cat_Name）
            return BadRequest(new { message = "その種目はすでに登録されています。" });
        }
        catch (Exception ex) {
            Console.WriteLine($"【Exercise追加エラー】: {ex.Message}");
            return StatusCode(500, new { message = "種目の追加に失敗しました。" });
        }
    }
}
