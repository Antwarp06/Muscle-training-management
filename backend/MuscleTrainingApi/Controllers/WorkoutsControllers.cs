using Microsoft.AspNetCore.Mvc;
using MuscleTrainingApi.Models;
using MuscleTrainingApi.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MuscleTrainingApi.Controllers
{
    [Route("api/Workouts")]
    [ApiController]
    public class WorkoutsController : ControllerBase
    {
        private readonly IMasterDataRepository _repository;

        public WorkoutsController(IMasterDataRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Workout workout)
        {
            await _repository.AddWorkoutAsync(workout);
            return Ok(new { message = "保存成功！" });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkoutResponse>>> GetWorkouts()
        {
            var workouts = await _repository.GetWorkoutsAsync();
            return Ok(workouts);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _repository.DeleteWorkoutAsync(id);

            if (!isDeleted)
                return NotFound(new { message = "指定された記録は見つかりませんでした。" });

            return Ok(new { message = "削除完了しました。" });
        }
    }
}