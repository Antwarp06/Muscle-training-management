using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MuscleTrainingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExercisesController : ControllerBase{
    private readonly string _connectionString;

    public ExercisesController(IConfiguration configuration){
        _connectionString = configuration.GetConnectionString("DefaultConnection")?? "";
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delte(int id){
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        try{
            using var cmd = new NpgsqlCommand("DELETE FROM \"Exercises\" WHERE \"Exercise_Id\" = @id", conn);
            cmd.Parameters.AddWithValue("id",id);

            int affectedRows = await cmd.ExecuteNonQueryAsync();
            if(affectedRows == 0) return NotFound();

            return Ok(new {message = "種目を削除しました"});
        }
        catch (PostgresException ex) when (ex.SqlState == "23503"){
            return BadRequest(new {message = "この種目のトレーニング記録が存在するため削除できません。"});
        }
    }
}