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
        // ==========================================
        // GET メソッドのテスト (M-102 ~ M-104)
        // ==========================================
        [Fact]
        public async Task M102_GetCategories_DBに部位データが1件もない場合_空のリストと200OKを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            mockRepo.Setup(repo => repo.GetCategoriesAsync()).ReturnsAsync(new List<Category>());
            var controller = new MasterDataController(mockRepo.Object);

            var result = await controller.GetCategories();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnCategories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);
            Assert.Empty(returnCategories); // 0件であることを検証
        }

        [Fact]
        public async Task M103_GetExercises_DBに種目データが複数件登録されている場合_種目リストと200OKを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            var fakeExercises = new List<Exercise>
            {
                new Exercise { Exercise_Id = 1, Category_Id = 1, Exercise_Name = "ベンチプレス" },
                new Exercise { Exercise_Id = 2, Category_Id = 2, Exercise_Name = "ラットプルダウン" }
            };
            // ※リポジトリのメソッド名が GetExercisesAsync であることを前提としています
            mockRepo.Setup(repo => repo.GetExercisesAsync()).ReturnsAsync(fakeExercises);
            var controller = new MasterDataController(mockRepo.Object);

            var result = await controller.GetExercises();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnExercises = Assert.IsAssignableFrom<IEnumerable<Exercise>>(okResult.Value);
            Assert.Equal(2, returnExercises.Count());
        }

        [Fact]
        public async Task M104_GetExercises_DBに種目データが1件もない場合_空のリストと200OKを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            mockRepo.Setup(repo => repo.GetExercisesAsync()).ReturnsAsync(new List<Exercise>());
            var controller = new MasterDataController(mockRepo.Object);

            var result = await controller.GetExercises();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnExercises = Assert.IsAssignableFrom<IEnumerable<Exercise>>(okResult.Value);
            Assert.Empty(returnExercises);
        }

        // ==========================================
        // POST メソッドのテスト (M-105 ~ M-108)
        // ==========================================
        [Fact]
        public async Task M105_AddCategory_新規部位の場合_DBに保存され200OKを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            var controller = new MasterDataController(mockRepo.Object);
            var newCategory = new Category { Category_Name = "背中" };

            // Act
            var result = await controller.AddCategory(newCategory);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task M106_AddCategory_すでに登録されている部位の場合_400BadRequestを返すこと()
        {
        // Arrange (準備)
        var mockRepo = new Mock<IMasterDataRepository>();

        
        var fakeExistingCategories = new List<Category> { new Category { Category_Name = "胸" } };
        mockRepo.Setup(repo => repo.GetCategoriesAsync()).ReturnsAsync(fakeExistingCategories);

        var controller = new MasterDataController(mockRepo.Object);
        var duplicateCategory = new Category { Category_Name = "胸" }; // 登録しようとする重複データ

        // Act (実行)
        var result = await controller.AddCategory(duplicateCategory);

        // Assert (検証)
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        Assert.Contains("その部位はすでに登録されています", badRequestResult.Value.ToString());
    }

        [Fact]
        public async Task M107_AddExercise_新規種目の場合_DBに保存され200OKを返すこと()
        {
            var mockRepo = new Mock<IMasterDataRepository>();
            var controller = new MasterDataController(mockRepo.Object);
            var newExercise = new Exercise { Category_Id = 1, Exercise_Name = "ダンベルフライ" };

            var result = await controller.AddExercise(newExercise);

            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task M108_AddExercise_すでに登録されている種目の場合_400BadRequestを返すこと()
        {
        // Arrange (準備)
        var mockRepo = new Mock<IMasterDataRepository>();
    
    
        // コントローラーが GetExercisesAsync() を呼んだとき、すでに「ベンチプレス」が登録されている状態を偽装する
        var fakeExistingExercises = new List<Exercise> 
        { 
            new Exercise { Category_Id = 1, Exercise_Name = "ベンチプレス" } 
        };
        mockRepo.Setup(repo => repo.GetExercisesAsync()).ReturnsAsync(fakeExistingExercises);

        var controller = new MasterDataController(mockRepo.Object);
        var duplicateExercise = new Exercise { Category_Id = 1, Exercise_Name = "ベンチプレス" }; // 登録しようとする重複データ

        // Act (実行)
        var result = await controller.AddExercise(duplicateExercise);

        // Assert (検証)
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        Assert.Contains("その種目はすでに登録されています", badRequestResult.Value.ToString());
        }
    }
}