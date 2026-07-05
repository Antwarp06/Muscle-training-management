using Microsoft.AspNetCore.Mvc;
using MuscleTrainingApi.Repositories;
using System.Threading.Tasks;

namespace MuscleTrainingApi.Controllers
{
    [Route("api/Exercises")]
    [ApiController]
    public class ExercisesController : ControllerBase
    {
        private readonly IMasterDataRepository _repository;

        public ExercisesController(IMasterDataRepository repository)
        {
            _repository = repository;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repository.DeleteExerciseAsync(id);

            if (result == DeleteResult.NotFound)
                return NotFound(new { message = "種目が見つかりませんでした" });

            if (result == DeleteResult.ConstraintViolation)
                return BadRequest(new { message = "この種目のトレーニング記録が存在するため削除できません。" });

            return Ok(new { message = "種目を削除しました" });
        }
    }
}