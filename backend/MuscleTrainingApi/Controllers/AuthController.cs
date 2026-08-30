using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MuscleTrainingApi.Models;
using Npgsql;

namespace MuscleTrainingApi.Controllers;

[Route("api/Auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly string _connectionString;
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    // --- 新規登録 (POST) ---
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // ここで平文パスワードはハッシュに変換され、以降どこにも残らない。
            // 同じパスワードでも毎回違うハッシュになる（内部でランダムな塩を混ぜるため）。
            var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            using var cmd = new NpgsqlCommand(
                "INSERT INTO \"Users\" (\"User_Name\", \"Password_Hash\") VALUES (@name, @hash) RETURNING \"User_Id\"",
                conn
            );
            cmd.Parameters.AddWithValue("name", request.UserName);
            cmd.Parameters.AddWithValue("hash", hash);

            var userId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            // 登録が済んだらそのままログイン状態にする（登録直後にログインさせ直さない）
            return Ok(new { token = CreateToken(userId, request.UserName), userName = request.UserName });
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            // 23505 = 一意制約違反。UQ_Users_UserName に引っかかった場合にここへ来る。
            // アプリ側で事前に COUNT(*) を確認する方法もあるが、
            // 「確認した直後に他の人が登録する」ズレを防げるのはDB制約だけ。
            return BadRequest(new { message = "そのユーザー名はすでに使われています。" });
        }
        catch (Exception ex)
        {
            // 例外を投げっぱなしにするとCORSヘッダーが消えてブラウザでCORSエラーに化けるため、
            // ここで捕まえて500として返す
            Console.WriteLine($"【登録エラー】: {ex.Message}");
            return StatusCode(500, new { message = "登録に失敗しました。" });
        }
    }

    // --- ログイン (POST) ---
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            // 登録時に LOWER() の一意インデックスを張っているので、検索も LOWER() で揃える
            using var cmd = new NpgsqlCommand(
                "SELECT \"User_Id\", \"User_Name\", \"Password_Hash\" FROM \"Users\" WHERE LOWER(\"User_Name\") = LOWER(@name)",
                conn
            );
            cmd.Parameters.AddWithValue("name", request.UserName);

            using var reader = await cmd.ExecuteReaderAsync();

            // ユーザーが存在しない場合も、パスワード違いとまったく同じ文言を返す。
            // 「そのユーザーは存在しません」と区別して返すと、
            // どの名前が登録済みかを外部から総当たりで調べられてしまうため。
            if (!await reader.ReadAsync())
            {
                return Unauthorized(new { message = "ユーザー名またはパスワードが違います。" });
            }

            var userId = reader.GetInt32(0);
            var userName = reader.GetString(1);
            var storedHash = reader.GetString(2);

            // ハッシュを元に戻すのではなく、入力を同じ方法でハッシュ化して照合する
            if (!BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
            {
                return Unauthorized(new { message = "ユーザー名またはパスワードが違います。" });
            }

            return Ok(new { token = CreateToken(userId, userName), userName });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"【ログインエラー】: {ex.Message}");
            return StatusCode(500, new { message = "ログインに失敗しました。" });
        }
    }

    // --- トークン（ホテルのカードキーに相当）の発行 ---
    private string CreateToken(int userId, string userName)
    {
        // Program.cs の起動時チェックを通っているので、ここでは必ず値が入っている
        var keyString = _configuration["Jwt:Key"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // クレーム＝カードキーに記録する内容。
        // ここは暗号化されず誰でも読めるので、パスワードなどの秘密は絶対に入れない。
        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim("userName", userName)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),//有効期限。切れたら再ログインが必要
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
