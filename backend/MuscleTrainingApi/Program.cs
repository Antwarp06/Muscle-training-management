using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MuscleTrainingApi.Models;

var builder = WebApplication.CreateBuilder(args);
// --- 1. サービスの登録 (builder.Buildの前) ---
// データベースの設定 (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString( "DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// --- JWT（ログイン用トークン）の設定 ---
// 秘密鍵が無い・短すぎる場合は起動時に停止させる。
// 弱い鍵のまま動き続けると、トークンを偽造されても気づけないため。
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key が未設定、または短すぎます（32バイト以上必要）。" +
        "ローカルは appsettings.Development.json、本番は環境変数 Jwt__Key に設定してください。");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // 受け取ったトークンを「何を根拠に信用するか」の設定。
        // ここで署名を検証するので、中身が書き換えられていれば弾かれる。
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,//有効期限切れのトークンを拒否する
            ClockSkew = TimeSpan.FromMinutes(1)//サーバー間の時計のズレの許容範囲
        };
    });
builder.Services.AddAuthorization();

// APIコントローラーを有効にする
builder.Services.AddControllers();
// APIコントローラーを有効にする
builder.Services.AddEndpointsApiExplorer();

// Swagger画面から、トークンを付けたリクエストを試せるようにする
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "ログインAPIで取得した token の値を、そのまま貼り付けてください（Bearer は不要）。",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
    });
});
// --- CORS 設定 ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// --- 2. アプリの動作設定 ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// 【CORSの適用】フロントエンド（Reactなど）からのアクセスを許可する
app.UseCors("AllowAll");

// セキュリティ設定
//app.UseHttpsRedirection();

// 【認証】トークンを読み取って「誰からのリクエストか」を判定する
app.UseAuthentication();
// 【認可】[Authorize] が付いたAPIへの立ち入りを許可するか判断する
// ※ UseAuthentication より後に書くこと。順番を逆にすると常に未ログイン扱いになる
app.UseAuthorization();

// 作成した WorkoutsController を URL に紐付ける
app.MapControllers();

app.Run();