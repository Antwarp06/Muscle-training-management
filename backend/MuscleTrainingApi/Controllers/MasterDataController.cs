using Microsoft.AspNetCore.Mvc;
using MuscleTrainingApi.Models;
using MuscleTrainingApi.Repositories; 

namespace MuscleTrainingApi.Controllers
{
    [Route("api/MasterData")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        // IConfiguration（設定ファイル）ではなく、IMasterDataRepository を受け取る
        private readonly IMasterDataRepository _repository;

        public MasterDataController(IMasterDataRepository repository)
        {
            _repository = repository;
        }

        // --- 部位一覧の取得 (GET) ---
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            try
            {
                // DB操作はリポジトリにお任せ
                var categories = await _repository.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"【Categories取得内部エラー】: {ex.Message}");
                return Ok(new List<Category>());
            }
        }

        // --- 種目一覧の取得 (GET) ---
        [HttpGet("exercises")]
        public async Task<ActionResult<IEnumerable<Exercise>>> GetExercises()
        {
            try
            {
                var exercises = await _repository.GetExercisesAsync();
                return Ok(exercises);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"【Exercises取得内部エラー】: {ex.Message}");
                return Ok(new List<Exercise>());
            }
        }

        // --- 部位の追加 ---
        [HttpPost("categories")]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            // 1. 重複チェック（リポジトリから一覧をもらって、同じ名前がないか探す）
            var existingCategories = await _repository.GetCategoriesAsync();
            if (existingCategories.Any(c => c.Category_Name == category.Category_Name))
            {
                return BadRequest(new { message = "その部位はすでに登録されています。" });
            }

            // 2. 問題なければリポジトリに保存を依頼
            await _repository.AddCategoryAsync(category);
            return Ok();
        }

        // --- 種目の追加 ---
        [HttpPost("exercises")]
        public async Task<IActionResult> AddExercise([FromBody] Exercise exercise)
        {
            // 1. 重複チェック
            var existingExercises = await _repository.GetExercisesAsync();
            if (existingExercises.Any(e => e.Category_Id == exercise.Category_Id && e.Exercise_Name == exercise.Exercise_Name))
            {
                return BadRequest(new { message = "その種目はすでに登録されています。" });
            }

            // 2. 問題なければ保存
            await _repository.AddExerciseAsync(exercise);
            return Ok();
        }
    }
}