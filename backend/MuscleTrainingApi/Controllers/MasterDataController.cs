using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MuscleTrainingApi.Models;

[Route("api/[controller]")]
[ApiController]
public class MasterDataController : ControllerBase {
    private readonly string _connectionString;

    public MasterDataController(IConfiguration configuration) {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories() {
        var categories = new List<Category>();
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand("SELECT * FROM \"Categories\"", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            categories.Add(new Category {
                Category_Id = reader.GetInt32(0),
                Category_Name = reader.GetString(1)
            });
        }
        return categories;
    }

    [HttpGet("exercises")]
    public async Task<ActionResult<IEnumerable<Exercise>>> GetExercises() {
        var exercises = new List<Exercise>();
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand("SELECT * FROM \"Exercises\"", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            exercises.Add(new Exercise {
                Exercise_Id = reader.GetInt32(0),
                Category_Id = reader.GetInt32(1),
                Exercise_Name = reader.GetString(2)
            });
        }
        return exercises;
    }

    // --- 部位の追加 ---
    [HttpPost("categories")]
    public async Task<IActionResult> AddCategory([FromBody] Category category) {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
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
        using var cmd = new NpgsqlCommand("INSERT INTO \"Exercises\" (\"Category_Id\", \"Exercise_Name\") VALUES (@catId, @name)", conn);
        cmd.Parameters.AddWithValue("catId", exercise.Category_Id);
        cmd.Parameters.AddWithValue("name", exercise.Exercise_Name);
        await cmd.ExecuteNonQueryAsync();
        return Ok();
    }
}