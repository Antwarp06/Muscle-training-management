using Microsoft.Extensions.Configuration;
using MuscleTrainingApi.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MuscleTrainingApi.Repositories;

public class MasterDataRepository : IMasterDataRepository
{
    private readonly string _connectionString;

    public MasterDataRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    // ==========================================
    // Mシリーズ (Categories / Exercises の取得・追加)
    // ==========================================
    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        var categories = new List<Category>();
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand("SELECT * FROM \"Categories\"", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            categories.Add(new Category
            {
                Category_Id = reader.GetValue(0) != DBNull.Value ? Convert.ToInt32(reader.GetValue(0)) : 0,
                Category_Name = reader.GetValue(1) != DBNull.Value ? reader.GetValue(1).ToString()! : ""
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
            exercises.Add(new Exercise
            {
                Exercise_Id = reader.GetValue(0) != DBNull.Value ? Convert.ToInt32(reader.GetValue(0)) : 0,
                Category_Id = reader.GetValue(1) != DBNull.Value ? Convert.ToInt32(reader.GetValue(1)) : 0,
                Exercise_Name = reader.GetValue(2) != DBNull.Value ? reader.GetValue(2).ToString()! : ""
            });
        }
        return exercises;
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

    // ==========================================
    // Cシリーズ (Category の削除)
    // ==========================================
    public async Task<DeleteResult> DeleteCategoryAsync(int id)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("DELETE FROM \"Categories\" WHERE \"Category_Id\" = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            
            int affectedRows = await cmd.ExecuteNonQueryAsync();
            if (affectedRows == 0) return DeleteResult.NotFound; // データが存在しない場合
            
            return DeleteResult.Success; // 削除成功
        }
        catch (PostgresException ex) when (ex.SqlState == "23503")
        {
            // 外部キー制約エラー (紐づく種目が存在する)
            return DeleteResult.ConstraintViolation;
        }
    }

    // ==========================================
    // Eシリーズ (Exercise の削除)
    // ==========================================
    public async Task<DeleteResult> DeleteExerciseAsync(int id)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new NpgsqlCommand("DELETE FROM \"Exercises\" WHERE \"Exercise_Id\" = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            
            int affectedRows = await cmd.ExecuteNonQueryAsync();
            if (affectedRows == 0) return DeleteResult.NotFound;
            
            return DeleteResult.Success;
        }
        catch (PostgresException ex) when (ex.SqlState == "23503")
        {
            // 外部キー制約エラー (トレーニング記録が存在する)
            return DeleteResult.ConstraintViolation;
        }
    }

    // ==========================================
    // Wシリーズ (Workout の追加・取得・削除)
    // ==========================================
    public async Task AddWorkoutAsync(Workout workout)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand("INSERT INTO \"Workout\" (\"Exercise_Id\", \"Weight\", \"Reps\") VALUES (@exId, @weight, @reps)", conn);
        cmd.Parameters.AddWithValue("exId", workout.Exercise_Id);
        cmd.Parameters.AddWithValue("weight", workout.Weight);
        cmd.Parameters.AddWithValue("reps", workout.Reps);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<WorkoutResponse>> GetWorkoutsAsync()
    {
        var workouts = new List<WorkoutResponse>();
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        
        // 元の WorkoutsController にあった JOIN の SQL
        string sql = @"
            SELECT 
                w.""Record_Id"", 
                e.""Exercise_Name"", 
                w.""Weight"", 
                w.""Reps"" 
            FROM ""Workout"" w
            JOIN ""Exercises"" e ON w.""Exercise_Id"" = e.""Exercise_Id""
            ORDER BY w.""Record_Id"" DESC";

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            // ここで DTO (WorkoutResponse) にデータを詰める
            workouts.Add(new WorkoutResponse
            {
                record_Id = reader.GetValue(0) != DBNull.Value ? Convert.ToInt32(reader.GetValue(0)) : 0,
                exercise_Name = reader.GetValue(1) != DBNull.Value ? reader.GetValue(1).ToString()! : "",
                weight = reader.GetValue(2) != DBNull.Value ? Convert.ToDouble(reader.GetValue(2)) : 0.0,
                reps = reader.GetValue(3) != DBNull.Value ? Convert.ToInt32(reader.GetValue(3)) : 0
            });
        }
        return workouts;
    }

    public async Task<bool> DeleteWorkoutAsync(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new NpgsqlCommand("DELETE FROM \"Workout\" WHERE \"Record_Id\" = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        
        int affectedRows = await cmd.ExecuteNonQueryAsync();
        // 1件以上削除されていれば true (成功)、0件なら false (NotFound)
        return affectedRows > 0;
    }
}