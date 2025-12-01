using Inventory_Management_System.Controllers;
using Inventory_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManagementSystemUnitTest.ControllerTests
{
    public class ItemsControllerTest
    {
        private InventoryDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new InventoryDbContext(options);
        }

        [Fact]
        public async Task Index_Returns_View_With_Items()
        {
            // Arrange
            var ctx = CreateContext("Items_Index_Db");
            ctx.Items.AddRange(
                new Item { Name = "Laptop", Quantity = 5 },
                new Item { Name = "Mouse", Quantity = 15 }
            );
            ctx.SaveChanges();

            var controller = new ItemsController(ctx);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Item>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Create_ValidItem_RedirectsToIndex_And_ItemSaved()
        {
            // Arrange
            var ctx = CreateContext("Items_Create_Db");
            var controller = new ItemsController(ctx);
            var newItem = new Item { Name = "Keyboard", Quantity = 10 };

            // Act
            var result = await controller.Create(newItem);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(1, ctx.Items.Count());
            var saved = ctx.Items.First();
            Assert.Equal("Keyboard", saved.Name);
        }

        [Fact]
        public async Task Edit_InvalidId_ReturnsNotFound()
        {
            var ctx = CreateContext("Items_InvalidEdit");
            var controller = new ItemsController(ctx);

            var result = await controller.Edit(99, new Item { ItemId = 1 });

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Should_Return_NotFound_When_Item_Not_Exist()
        {
            // Arrange
            var ctx = CreateContext("Items_Edit_NotFound");
            var controller = new ItemsController(ctx);

            // No item added to DB → simulate missing record

            // Act
            var result = await controller.Edit(1, new Item { ItemId = 1, Name = "Test" });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        //Negative case test
        [Fact]
        public async Task Create_InvalidModel_ReturnsView_WithModelErrors()
        {
            var ctx = CreateContext("Items_Invalid_Create");
            var controller = new ItemsController(ctx);

            controller.ModelState.AddModelError("Name", "Required");

            var item = new Item { Name = "", Quantity = 10 };

            var result = await controller.Create(item);

            Assert.IsType<ViewResult>(result);
            Assert.Empty(ctx.Items); // ensures nothing was saved
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var ctx = CreateContext("Details_Null");
            var controller = new ItemsController(ctx);

            var result = await controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ItemNotFound_ReturnsNotFound()
        {
            var ctx = CreateContext("Details_NotFound");
            var controller = new ItemsController(ctx);

            var result = await controller.Details(100);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsViewWithModel()
        {
            var ctx = CreateContext("Details_Valid");
            var item = new Item { Name = "Monitor", Quantity = 3 };
            ctx.Items.Add(item);
            ctx.SaveChanges();

            var controller = new ItemsController(ctx);

            var result = await controller.Details(item.ItemId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<Item>(view.Model);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            var ctx = CreateContext("Create_Get");
            var controller = new ItemsController(ctx);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ItemNotFound_ReturnsNotFound()
        {
            var ctx = CreateContext("Edit_Get_NotFound");
            var controller = new ItemsController(ctx);

            var result = await controller.Edit(50);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ValidItem_ReturnsViewWithModel()
        {
            var ctx = CreateContext("Edit_Get_Valid");
            var item = new Item { Name = "Chair", Quantity = 50 };
            ctx.Items.Add(item);
            ctx.SaveChanges();

            var controller = new ItemsController(ctx);

            var result = await controller.Edit(item.ItemId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<Item>(view.Model);
        }

        [Fact]
        public async Task Delete_Get_ItemNotFound_ReturnsNotFound()
        {
            var ctx = CreateContext("Delete_Get_NotFound");
            var controller = new ItemsController(ctx);

            var result = await controller.Delete(10);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_Get_ValidItem_ReturnsViewResult()
        {
            var ctx = CreateContext("Delete_Get_Valid");
            var item = new Item { Name = "Tablet", Quantity = 2 };
            ctx.Items.Add(item);
            ctx.SaveChanges();

            var controller = new ItemsController(ctx);

            var result = await controller.Delete(item.ItemId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<Item>(view.Model);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesItem_AndRedirects()
        {
            var ctx = CreateContext("DeleteConfirmed_Test");
            var item = new Item { Name = "Phone", Quantity = 99 };
            ctx.Items.Add(item);
            ctx.SaveChanges();

            var controller = new ItemsController(ctx);

            var result = await controller.DeleteConfirmed(item.ItemId);

            Assert.Equal(0, ctx.Items.Count());
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_ConcurrencyFailure_ReturnsNotFound_WhenItemMissing()
        {
            var ctx = CreateContext("Items_Concurrency");
            var controller = new ItemsController(ctx);

            // Seed item first
            var item = new Item { ItemId = 1, Name = "Test", Quantity = 1 };
            ctx.Items.Add(item);
            await ctx.SaveChangesAsync();

            // Now simulate concurrency delete:
            ctx.Items.Remove(item);
            await ctx.SaveChangesAsync();

            // Detach so controller sees clean instance
            ctx.Entry(item).State = EntityState.Detached;

            var result = await controller.Edit(1, item);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}

