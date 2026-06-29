using Npgsql;
using MuscleTrainingApi.Models;

namespace MuscleTrainingApi.Repositories
{
    // IMasterDataRepository（設計図）に従って働くクラスであることを宣言します
    public class MasterDataRepository : IMasterDataRepository
    {
        private readonly string _connectionString;

        // DBの接続文字列を受け取ります
        public MasterDataRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            var categories = new List<Category>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM \"Categories\"", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var rawCategoryId = reader.GetValue(0);
                var rawCategoryName = reader.GetValue(1);
                categories.Add(new Category
                {
                    Category_Id = rawCategoryId != DBNull.Value ? Convert.ToInt32(rawCategoryId) : 0,
                    Category_Name = rawCategoryName != DBNull.Value ? rawCategoryName.ToString() : ""
                });
            }
            return categories;
        }

        public async Task<IEnumerable<Exercise>> GetExercisesAsync()
        {
            var exercises = new List<Exercise>();
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("SELECT * FROM \"Exercises\"", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var rawExerciseId = reader.GetValue(0);
                var rawCategoryId = reader.GetValue(1);
                var rawExerciseName = reader.GetValue(2);
                exercises.Add(new Exercise
                {
                    Exercise_Id = rawExerciseId != DBNull.Value ? Convert.ToInt32(rawExerciseId) : 0,
                    Category_Id = rawCategoryId != DBNull.Value ? Convert.ToInt32(rawCategoryId) : 0,
                    Exercise_Name = rawExerciseName != DBNull.Value ? rawExerciseName.ToString() : ""
                });
            }
            return exercises;
        }

        public async Task<(IEnumerable<Category> Categories, IEnumerable<Exercise> Exercises)> GetAllMasterDataAsync()
        {
            var categories = await GetCategoriesAsync();
            var exercises = await GetExercisesAsync();
            return (categories, exercises);
        }

        public async Task AddCategoryAsync(Category category)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("INSERT INTO \"Categories\" (\"Category_Name\") VALUES (@name)", conn);
            cmd.Parameters.AddWithValue("name", category.Category_Name);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddExerciseAsync(Exercise exercise)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("INSERT INTO \"Exercises\" (\"Category_Id\", \"Exercise_Name\") VALUES (@catId, @name)", conn);
            cmd.Parameters.AddWithValue("catId", exercise.Category_Id);
            cmd.Parameters.AddWithValue("name", exercise.Exercise_Name);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}