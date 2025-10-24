using Inventory_Management_System.Controllers;
using Inventory_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManagementSystemUnitTest.ControllerTests
{
    public class OrdersControllerTests
    {
        private InventoryDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new InventoryDbContext(options);
        }

        [Fact]
        public async Task Create_ValidOrder_RedirectsToIndex_And_OrderSaved()
        {
            // Arrange
            var ctx = CreateContext("Orders_Create_Db");
            var item = new Item { Name = "Monitor", Quantity = 20 };
            ctx.Items.Add(item);
            ctx.SaveChanges();

            var controller = new OrdersController(ctx);
            var order = new Order { ItemId = item.ItemId, Quantity = 2, OrderedBy = "user1" };

            // Act
            var result = await controller.Create(order);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(1, ctx.Orders.Count());
            var saved = ctx.Orders.Include(o => o.Item).First();
            Assert.Equal("Monitor", saved.Item?.Name);
        }
    }
}
