using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MuscleApp.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase{
    private readonly string _connectionString;

    public CategoriesController(IConfiguration configuration){
        _connectionString = configuration.GetConnectionString("DefaultConnection")?? "";
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id){
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        try{
            using var cmd = new NpgsqlCommand("DELLETE FROM \"Categories\" WHER \"Category_Id\" = @id", conn);
            cmd.Parameters.AddWithValue("id",id);

            int affectedRows = await cmd.ExecuteNonQueryAsync();
            if (affectedRows == 0) return NotFound(new { message = "部位が見つかりませんでした" });

            return Ok(new { message = "部位を削除しました"});
        } 
        catch (PostgresException ex) when (ex.SqlState == "23503"){
            return BadRequest(new { message = "この部位に紐づく種目が存在するため削除できません"});
        }
    }
}
