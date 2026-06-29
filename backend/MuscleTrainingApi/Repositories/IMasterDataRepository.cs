using MuscleTrainingApi.Models;

namespace MuscleTrainingApi.Repositories
{
    public interface IMasterDataRepository
    {
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<IEnumerable<Exercise>> GetExercisesAsync();    
        Task<(IEnumerable<Category> Categories, IEnumerable<Exercise> Exercises)> GetAllMasterDataAsync();
        Task AddCategoryAsync(Category category);
        Task AddExerciseAsync(Exercise exercise);
    }
}