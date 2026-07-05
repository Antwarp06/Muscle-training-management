using MuscleTrainingApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MuscleTrainingApi.Repositories;

// コントローラーに返すための「削除結果」の定義
public enum DeleteResult
{
    Success,
    NotFound,
    ConstraintViolation
}

// 画面に返すための「種目名入りの記録データ」の定義
public class WorkoutResponse
{
    public int record_Id { get; set; }
    public string exercise_Name { get; set; } = "";
    public double weight { get; set; }
    public int reps { get; set; }
}

// リポジトリの設計図（インターフェース）
public interface IMasterDataRepository
{
    // --- Mシリーズ ---
    Task<IEnumerable<Category>> GetCategoriesAsync();
    Task<IEnumerable<Exercise>> GetExercisesAsync();
    Task AddCategoryAsync(Category category);
    Task AddExerciseAsync(Exercise exercise);

    // --- C・Eシリーズ ---
    Task<DeleteResult> DeleteCategoryAsync(int id);
    Task<DeleteResult> DeleteExerciseAsync(int id);

    // --- Wシリーズ ---
    Task AddWorkoutAsync(Workout workout);
    Task<IEnumerable<WorkoutResponse>> GetWorkoutsAsync();
    Task<bool> DeleteWorkoutAsync(int id);
}