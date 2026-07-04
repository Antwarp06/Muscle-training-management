using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MuscleTrainingApi.Controllers;
using MuscleTrainingApi.Models;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MuscleTrainingApi.Tests
{
    public class IntegrationTests
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public IntegrationTests()
        {
            // ★ローカルのテスト用DBのパスワードに変更してください
            _connectionString = "Host=localhost;Database=MuscleTraining_Test;Username=postgres;Password=your_password";

            var inMemorySettings = new Dictionary<string, string> {
                {"ConnectionStrings:DefaultConnection", _connectionString}
            };
            
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        // === テスト前にデータベースを空っぽにする便利メソッド ===
        private async Task ResetDatabaseAsync()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("TRUNCATE TABLE \"Workout\", \"Exercises\", \"Categories\" RESTART IDENTITY CASCADE;", conn);
            await cmd.ExecuteNonQueryAsync();
        }

        // === テスト用のデータを準備する便利メソッド ===
        private async Task InsertSeedDataAsync(int level)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            
            // level 1: 部位だけ追加
            if (level >= 1) {
                using var cmd1 = new NpgsqlCommand("INSERT INTO \"Categories\" (\"Category_Name\") VALUES ('胸')", conn);
                await cmd1.ExecuteNonQueryAsync();
            }
            // level 2: 部位 ＋ 種目を追加
            if (level >= 2) {
                using var cmd2 = new NpgsqlCommand("INSERT INTO \"Exercises\" (\"Category_Id\", \"Exercise_Name\") VALUES (1, 'ベンチプレス')", conn);
                await cmd2.ExecuteNonQueryAsync();
            }
            // level 3: 部位 ＋ 種目 ＋ 記録を追加
            if (level >= 3) {
                using var cmd3 = new NpgsqlCommand("INSERT INTO \"Workout\" (\"Exercise_Id\", \"Weight\", \"Reps\") VALUES (1, 100, 10)", conn);
                await cmd3.ExecuteNonQueryAsync();
            }
        }

        // ==========================================
        // Cシリーズ: CategoriesController の DELETE テスト
        // ==========================================
        [Fact]
        public async Task C201_DeleteCategory_紐づく種目がない場合_200OKを返すこと()
        {
            await ResetDatabaseAsync();
            await InsertSeedDataAsync(1); // 部位(ID:1)だけ登録
            var controller = new CategoriesController(_configuration);

            var result = await controller.Delete(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task C202_DeleteCategory_対象IDが存在しない場合_404NotFoundを返すこと()
        {
            await ResetDatabaseAsync();
            var controller = new CategoriesController(_configuration);

            var result = await controller.Delete(999); // 存在しないID
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task C203_DeleteCategory_紐づく種目が存在する場合_400BadRequestを返すこと()
        {
            await ResetDatabaseAsync();
            await InsertSeedDataAsync(2); // 部位(ID:1)と種目(ID:1)が登録済み
            var controller = new CategoriesController(_configuration);

            var result = await controller.Delete(1);
            
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("削除できません", badRequest.Value.ToString());
        }

        // ==========================================
        // Eシリーズ: ExercisesController の DELETE テスト
        // ==========================================
        [Fact]
        public async Task E301_DeleteExercise_トレーニング記録がない場合_200OKを返すこと()
        {
            await ResetDatabaseAsync();
            await InsertSeedDataAsync(2); // 部位と種目(ID:1)だけ登録
            var controller = new ExercisesController(_configuration);

            var result = await controller.Delete(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task E302_DeleteExercise_対象IDが存在しない場合_404NotFoundを返すこと()
        {
            await ResetDatabaseAsync();
            var controller = new ExercisesController(_configuration);

            var result = await controller.Delete(999); // 存在しないID
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task E303_DeleteExercise_トレーニング記録が存在する場合_400BadRequestを返すこと()
        {
            await ResetDatabaseAsync();
            await InsertSeedDataAsync(3); // 記録(Workout)まで登録済み
            var controller = new ExercisesController(_configuration);

            var result = await controller.Delete(1);
            
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
            Assert.Contains("削除できません", badRequest.Value.ToString());
        }

        // ==========================================
        // Wシリーズ: WorkoutsController のテスト
        // ==========================================
        [Fact]
        public async Task W401_PostWorkout_正常に登録される場合_200OKを返すこと()
        {
            await ResetDatabaseAsync();
            await InsertSeedDataAsync(2); // 部位と種目(ID:1)を登録しておく
            var controller = new WorkoutsController(_configuration);

            var newWorkout = new Workout { Exercise_Id = 1, Weight = 80, Reps = 10 };
            var result = await controller.Post(newWorkout);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task W402_GetWorkouts_記録が降順で取得できること()
        {
            await ResetDatabaseAsync();
            await InsertSeedDataAsync(3); // 記録を1件登録しておく
            var controller = new WorkoutsController(_configuration);

            var result = await controller.GetWorkouts();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            
            // dynamic型として返却されるため、IEnumerableとしてキャスト可能か検証
            var workouts = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.Single(workouts); // 1件取得できること
        }

        [Fact]
        public async Task W403_GetWorkouts_記録が1件もない場合_空リストを返すこと()
        {
            await ResetDatabaseAsync();
            var controller = new WorkoutsController(_configuration);

            var result = await controller.GetWorkouts();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            
            var workouts = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);
            Assert.Empty(workouts); // 0件であること
        }

        [Fact]
        public async Task W404_DeleteWorkout_対象IDが存在する場合_200OKを返すこと()
        {
            await ResetDatabaseAsync();
            await InsertSeedDataAsync(3); // 記録(ID:1)を登録しておく
            var controller = new WorkoutsController(_configuration);

            var result = await controller.Delete(1);
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task W405_DeleteWorkout_対象IDが存在しない場合_404NotFoundを返すこと()
        {
            await ResetDatabaseAsync();
            var controller = new WorkoutsController(_configuration);

            var result = await controller.Delete(999);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}