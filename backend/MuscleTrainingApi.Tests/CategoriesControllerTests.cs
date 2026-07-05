using Microsoft.AspNetCore.Mvc;
using Moq;
using MuscleTrainingApi.Controllers;
using MuscleTrainingApi.Repositories;
using System.Threading.Tasks;
using Xunit;

namespace MuscleTrainingApi.Tests
{
    public class CategoriesControllerTests
    {
        [Fact]
        public async Task C201_DeleteCategory_紐づく種目がない場合_200OKを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            mockRepo.Setup(repo => repo.DeleteCategoryAsync(1)).ReturnsAsync(DeleteResult.Success);
            var controller = new CategoriesController(mockRepo.Object);

            var result = await controller.Delete(1);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("部位を削除しました", okResult.Value!.ToString());
        }

        [Fact]
        public async Task C202_DeleteCategory_対象IDが存在しない場合_404NotFoundを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            mockRepo.Setup(repo => repo.DeleteCategoryAsync(999)).ReturnsAsync(DeleteResult.NotFound);
            var controller = new CategoriesController(mockRepo.Object);

            var result = await controller.Delete(999);
            
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("部位が見つかりませんでした", notFoundResult.Value!.ToString());
        }

        [Fact]
        public async Task C203_DeleteCategory_紐づく種目が存在する場合_400BadRequestを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            mockRepo.Setup(repo => repo.DeleteCategoryAsync(1)).ReturnsAsync(DeleteResult.ConstraintViolation);
            var controller = new CategoriesController(mockRepo.Object);

            var result = await controller.Delete(1);
            
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("削除できません", badRequestResult.Value!.ToString());
        }
    }
}