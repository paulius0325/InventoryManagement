using Inventory_Management_System.Controllers;
using Inventory_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit; // Make sure you have xUnit

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
    }
}

