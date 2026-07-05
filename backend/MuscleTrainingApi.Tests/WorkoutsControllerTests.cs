using Microsoft.AspNetCore.Mvc;
using Moq;
using MuscleTrainingApi.Controllers;
using MuscleTrainingApi.Models;
using MuscleTrainingApi.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MuscleTrainingApi.Tests
{
    public class WorkoutsControllerTests
    {
        [Fact]
        public async Task W401_PostWorkout_正常に登録される場合_200OKを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            var controller = new WorkoutsController(mockRepo.Object);
            var newWorkout = new Workout { Exercise_Id = 1, Weight = 80, Reps = 10 };

            var result = await controller.Post(newWorkout);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("保存成功！", okResult.Value!.ToString());
        }

        [Fact]
        public async Task W402_GetWorkouts_記録が取得できること()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            var fakeWorkouts = new List<WorkoutResponse>
            {
                new WorkoutResponse { record_Id = 1, exercise_Name = "ベンチプレス", weight = 100, reps = 10 }
            };
            mockRepo.Setup(repo => repo.GetWorkoutsAsync()).ReturnsAsync(fakeWorkouts);
            var controller = new WorkoutsController(mockRepo.Object);

            var result = await controller.GetWorkouts();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnWorkouts = Assert.IsAssignableFrom<IEnumerable<WorkoutResponse>>(okResult.Value);
            Assert.Single(returnWorkouts);
        }

        [Fact]
        public async Task W403_GetWorkouts_記録が1件もない場合_空リストを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            mockRepo.Setup(repo => repo.GetWorkoutsAsync()).ReturnsAsync(new List<WorkoutResponse>());
            var controller = new WorkoutsController(mockRepo.Object);

            var result = await controller.GetWorkouts();
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnWorkouts = Assert.IsAssignableFrom<IEnumerable<WorkoutResponse>>(okResult.Value);
            Assert.Empty(returnWorkouts);
        }

        [Fact]
        public async Task W404_DeleteWorkout_対象IDが存在する場合_200OKを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            mockRepo.Setup(repo => repo.DeleteWorkoutAsync(1)).ReturnsAsync(true);
            var controller = new WorkoutsController(mockRepo.Object);

            var result = await controller.Delete(1);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("削除完了しました", okResult.Value!.ToString());
        }

        [Fact]
        public async Task W405_DeleteWorkout_対象IDが存在しない場合_404NotFoundを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            mockRepo.Setup(repo => repo.DeleteWorkoutAsync(999)).ReturnsAsync(false);
            var controller = new WorkoutsController(mockRepo.Object);

            var result = await controller.Delete(999);
            
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("見つかりませんでした", notFoundResult.Value!.ToString());
        }
    }
}