using Microsoft.AspNetCore.Mvc;
using MuscleTrainingApi.Repositories;
using System.Threading.Tasks;

namespace MuscleTrainingApi.Controllers
{
    [Route("api/Categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IMasterDataRepository _repository;

        // ★ IConfiguration ではなく IMasterDataRepository を受け取るように変更！
        public CategoriesController(IMasterDataRepository repository)
        {
            _repository = repository;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _repository.DeleteCategoryAsync(id);

            if (result == DeleteResult.NotFound)
                return NotFound(new { message = "部位が見つかりませんでした" });

            if (result == DeleteResult.ConstraintViolation)
                return BadRequest(new { message = "この部位に紐づく種目が存在するため削除できません" });

            return Ok(new { message = "部位を削除しました" });
        }
    }
}