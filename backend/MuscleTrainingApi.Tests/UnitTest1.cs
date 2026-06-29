using Microsoft.AspNetCore.Mvc;
using Moq;
using MuscleTrainingApi.Controllers;
using MuscleTrainingApi.Models;
using MuscleTrainingApi.Repositories;
using Xunit;

namespace MuscleTrainingApi.Tests // 💡 セミコロンを削除！
{
    public class MasterDataControllerTests
    {
        [Fact]
        public async Task GetCategories_正常に部位のリストと200OKを返すこと()
        {
            // ==========================================
            // 1. Arrange (準備)
            // ==========================================
            // ① 偽物の作業員（モック）を作成する
            var mockRepo = new Mock<IMasterDataRepository>();

            // ② 本物のDBの代わりに返す「テスト用のダミーデータ」を用意する
            var fakeCategories = new List<Category> // 💡 スペルを修正！
            {
                new Category { Category_Id = 1, Category_Name = "胸" },
                new Category { Category_Id = 2, Category_Name = "背中" }
            };

            // ③ 偽物の作業員に「GetCategoriesAsyncを頼まれたら、このダミーデータを返してね」と教え込む
            mockRepo.Setup(repo => repo.GetCategoriesAsync())
                    .ReturnsAsync(fakeCategories);
            
            // ④ 監督（コントローラー）に、この偽物の作業員を渡して生成する
            var controller = new MasterDataController(mockRepo.Object);

            // ==========================================
            // 2. Act (実行)
            // ==========================================
            // ⑤ 実際にReactから呼ばれたつもりで、コントローラーの機能を使う
            var result = await controller.GetCategories();

            // ==========================================
            // 3. Assert (検証)
            // ==========================================
            // ⑥ 結果が「200 OK」になっているかを検証
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            // ⑦ 200 OK の中身が「Categoryのリスト」になっているかを検証
            var returnCategories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);

            // ⑧ そのリストの中に、ちゃんと2件（胸・背中）が入っているかを検証
            Assert.Equal(2, returnCategories.Count());
        }
    }
}