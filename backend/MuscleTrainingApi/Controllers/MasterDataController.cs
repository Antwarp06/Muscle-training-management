using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MuscleTrainingApi.Models;

[Route("api/MasterData")]
[ApiController]
public class MasterDataController : ControllerBase {
    private readonly string _connectionString;

    public MasterDataController(IConfiguration configuration) {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories() {
        var categories = new List<Category>();
        try {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM \"Categories\"", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync()) {
                var rawCategoryId = reader.GetValue(0);
                var rawCategoryName = reader.GetValue(1);

                categories.Add(new Category {
                    Category_Id = rawCategoryId != DBNull.Value ? Convert.ToInt32(rawCategoryId) : 0,
                    Category_Name = rawCategoryName != DBNull.Value ? rawCategoryName.ToString() : ""
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
            
            string sql = "SELECT * FROM \"Exercises\"";
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();
            
            while (await reader.ReadAsync()) {
                var rawExerciseId = reader.GetValue(0);
                var rawCategoryId = reader.GetValue(1);
                var rawExerciseName = reader.GetValue(2);

                exercises.Add(new Exercise {
                    Exercise_Id = rawExerciseId != DBNull.Value ? Convert.ToInt32(rawExerciseId) : 0,
                    Category_Id = rawCategoryId != DBNull.Value ? Convert.ToInt32(rawCategoryId) : 0,
                    Exercise_Name = rawExerciseName != DBNull.Value ? rawExerciseName.ToString() : ""
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
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        using var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Categories\" WHERE \"Category_Name\" = @name", conn);
        checkCmd.Parameters.AddWithValue("name", category.Category_Name);
        var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
        if (count > 0) return BadRequest(new { message = "その部位はすでに登録されています。" });

        using var cmd = new NpgsqlCommand("INSERT INTO \"Categories\" (\"Category_Name\") VALUES (@name)", conn);
        cmd.Parameters.AddWithValue("name", category.Category_Name);
        await cmd.ExecuteNonQueryAsync();
        return Ok();
    }

// --- 種目の追加 ---
    [HttpPost("exercises")]
    public async Task<IActionResult> AddExercise([FromBody] Exercise exercise) {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        using var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM \"Exercises\" WHERE \"Category_Id\" = @catId AND \"Exercise_Name\" = @name", conn);
        checkCmd.Parameters.AddWithValue("catId", exercise.Category_Id);
        checkCmd.Parameters.AddWithValue("name", exercise.Exercise_Name);
        var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
        if (count > 0) return BadRequest(new { message = "その種目はすでに登録されています。" });

        using var cmd = new NpgsqlCommand("INSERT INTO \"Exercises\" (\"Category_Id\", \"Exercise_Name\") VALUES (@catId, @name)", conn);
        cmd.Parameters.AddWithValue("catId", exercise.Category_Id);
        cmd.Parameters.AddWithValue("name", exercise.Exercise_Name);
        await cmd.ExecuteNonQueryAsync();
        return Ok();
    }
}