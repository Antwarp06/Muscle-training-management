using Microsoft.EntityFrameworkCore;
using MuscleTrainingApi.Models;

var builder = WebApplication.CreateBuilder(args);
// --- 1. サービスの登録 (builder.Buildの前) ---
// データベースの設定 (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString( "DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

// APIコントローラーを有効にする
builder.Services.AddControllers();
// APIコントローラーを有効にする
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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

// 【CORSの適用】フロントエンド（Reactなど）からのアクセスを許可する
app.UseCors("AllowAll");

// セキュリティ設定
app.UseHttpsRedirection();

// 作成した WorkoutsController を URL に紐付ける
app.MapControllers();

app.Run();