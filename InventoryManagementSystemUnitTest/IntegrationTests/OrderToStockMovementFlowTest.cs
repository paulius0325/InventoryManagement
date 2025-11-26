using Inventory_Management_System.Controllers;
using Inventory_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManagementSystemUnitTest.IntegrationTests
{
    public class OrderToStockMovementFlowTest
    {
        private InventoryDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new InventoryDbContext(options);
        }

        [Fact]
        public async Task CreatingOrder_ShouldStoreOrder_AndAllowManualStockMovementTracking()
        {
            // Arrange
            var db = CreateDb("Integration_Order_Stock_Flow");

            // Seed one inventory item
            var item = new Item { Name = "HDMI Cable", Quantity = 10 };
            db.Items.Add(item);
            await db.SaveChangesAsync();

            // placing an order
            var orderController = new OrdersController(db);
            var order = new Order
            {
                ItemId = item.ItemId,
                Quantity = 3,
                OrderedBy = "TestUser"
            };

            // Act → Create Order in DB
            await orderController.Create(order);

            // Manually simulate inventory deduction + stock movement logging
            item.Quantity -= order.Quantity;
            db.StockMovements.Add(new StockMovement
            {
                ItemId = item.ItemId,
                QuantityChanged = order.Quantity,
                Action = "Removed"
            });

            await db.SaveChangesAsync();

            // Assert → Check DB
            var savedOrder = db.Orders.Include(o => o.Item).FirstOrDefault();
            var updatedItem = db.Items.First();
            var stockLog = db.StockMovements.First();

            Assert.NotNull(savedOrder);
            Assert.Equal("HDMI Cable", savedOrder.Item?.Name);

            Assert.Equal(7, updatedItem.Quantity);   // 10 - 3 = 7

            Assert.Equal(order.Quantity, stockLog.QuantityChanged);
            Assert.Equal("Removed", stockLog.Action);
            Assert.Equal(item.ItemId, stockLog.ItemId);
        }
    }
}
