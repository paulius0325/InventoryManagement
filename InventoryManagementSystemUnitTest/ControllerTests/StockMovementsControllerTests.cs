using Inventory_Management_System.Controllers;
using Inventory_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManagementSystemUnitTest.ControllerTests
{
    public class StockMovementsControllerTests
    {
        private InventoryDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new InventoryDbContext(options);
        }

        // -----------------------------
        // TEST: Index Must Return List
        // -----------------------------
        [Fact]
        public async Task Index_ReturnsViewWithStockMovements()
        {
            var ctx = CreateDb("SM_IndexTest");

            var item = new Item { Name = "Mouse", Quantity = 10 };
            ctx.Items.Add(item);

            ctx.StockMovements.Add(new StockMovement
            {
                ItemId = item.ItemId,
                Action = "Added",
                QuantityChanged = 5
            });

            await ctx.SaveChangesAsync();

            var controller = new StockMovementsController(ctx);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<StockMovement>>(view.Model);

            Assert.Single(model);
        }

        // ----------------------------------------
        // TEST: Valid Stock Movement Saves
        // ----------------------------------------
        [Fact]
        public async Task Create_ValidStockMovement_SavesAndRedirects()
        {
            var ctx = CreateDb("SM_CreateValid");

            var item = new Item { Name = "Keyboard", Quantity = 5 };
            ctx.Items.Add(item);
            await ctx.SaveChangesAsync();

            var controller = new StockMovementsController(ctx);

            var movement = new StockMovement
            {
                ItemId = item.ItemId,
                Action = "Added",
                QuantityChanged = 3
            };

            var result = await controller.Create(movement);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            Assert.Single(ctx.StockMovements);
        }

        // -----------------------------------------
        // TEST: Invalid Model Should Return View
        // -----------------------------------------
        [Fact]
        public async Task Create_InvalidModel_ReturnsSameView()
        {
            var ctx = CreateDb("SM_InvalidCreate");

            var controller = new StockMovementsController(ctx);
            controller.ModelState.AddModelError("Action", "Required");

            var movement = new StockMovement { QuantityChanged = 2 };

            var result = await controller.Create(movement);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(movement, view.Model);
        }

        // ----------------------------------------
        // TEST: DeleteConfirmed Removes Movement
        // ----------------------------------------
        [Fact]
        public async Task DeleteConfirmed_RemovesMovement()
        {
            var ctx = CreateDb("SM_Delete");

            var movement = new StockMovement
            {
                ItemId = 1,
                Action = "Removed",
                QuantityChanged = 1
            };

            ctx.StockMovements.Add(movement);
            await ctx.SaveChangesAsync();

            var controller = new StockMovementsController(ctx);

            var result = await controller.DeleteConfirmed(movement.StockMovementId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            Assert.Empty(ctx.StockMovements);
        }

        // -----------------------------------------
        // TEST: Details returns NotFound if missing
        // -----------------------------------------
        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            var ctx = CreateDb("SM_DetailsInvalid");
            var controller = new StockMovementsController(ctx);

            var result = await controller.Details(-1);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
