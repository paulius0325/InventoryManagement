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

        [Fact]
        public async Task Create_ValidOrder_SavesToDatabase_AndRedirects()
        {
            var ctx = CreateContext("Orders_Create_Valid");
            ctx.Items.Add(new Item { ItemId = 1, Name = "Keyboard" });
            ctx.SaveChanges();

            var controller = new OrdersController(ctx);

            var order = new Order
            {
                ItemId = 1,
                Quantity = 2,
                OrderedBy = "TestUser",
                Status = "Pending",
                OrderDate = DateTime.UtcNow
            };

            var result = await controller.Create(order);

            Assert.Equal(1, ctx.Orders.Count());
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Create_InvalidModel_ReturnsView_AndDoesNotSave()
        {
            var ctx = CreateContext("Orders_Create_Invalid");
            var controller = new OrdersController(ctx);

            controller.ModelState.AddModelError("ItemId", "Required");

            var order = new Order { Quantity = 2 };

            var result = await controller.Create(order);

            Assert.IsType<ViewResult>(result);
            Assert.Empty(ctx.Orders);
        }

        [Fact]
        public async Task Index_ReturnsOrdersList()
        {
            var ctx = CreateContext("Orders_Index");
            ctx.Orders.Add(new Order { ItemId = 1, Quantity = 1, Status = "Pending" });
            ctx.SaveChanges();

            var controller = new OrdersController(ctx);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Order>>(view.Model);
            Assert.Single(model);
        }



        [Fact]
        public async Task Edit_WrongId_ReturnsNotFound()
        {
            var ctx = CreateContext("Orders_Edit_WrongId");
            var controller = new OrdersController(ctx);

            var order = new Order { OrderId = 2, ItemId = 1, Quantity = 5 };

            var result = await controller.Edit(1, order);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesOrder()
        {
            var ctx = CreateContext("Orders_Delete");
            ctx.Orders.Add(new Order { OrderId = 1, ItemId = 1, Quantity = 2 });
            ctx.SaveChanges();

            var controller = new OrdersController(ctx);

            await controller.DeleteConfirmed(1);

            Assert.Empty(ctx.Orders);
        }

        [Fact]
        public async Task Delete_NonExisting_ReturnsNotFound()
        {
            var ctx = CreateContext("Orders_Delete_NotFound");
            var controller = new OrdersController(ctx);

            var result = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            var ctx = CreateContext("Orders_Create_Get");
            var controller = new OrdersController(ctx);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var ctx = CreateContext("Orders_Details_Null");
            var controller = new OrdersController(ctx);

            var result = await controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_OrderNotFound_ReturnsNotFound()
        {
            var ctx = CreateContext("Orders_Details_NotFound");
            var controller = new OrdersController(ctx);

            var result = await controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_Valid_ReturnsViewWithOrder()
        {
            var ctx = CreateContext("Orders_Details_Valid");
            var order = new Order { ItemId = 1, Quantity = 4, Status = "Pending" };
            ctx.Orders.Add(order);
            ctx.SaveChanges();

            var controller = new OrdersController(ctx);

            var result = await controller.Details(order.OrderId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<Order>(view.Model);
        }

        [Fact]
        public async Task Edit_Get_Valid_ReturnsViewWithOrder()
        {
            var ctx = CreateContext("Orders_Edit_Get_Valid");
            var order = new Order { ItemId = 1, Quantity = 3 };
            ctx.Orders.Add(order);
            ctx.SaveChanges();

            var controller = new OrdersController(ctx);

            var result = await controller.Edit(order.OrderId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<Order>(view.Model);
        }

        //Edit POST - Invalid Model State
        [Fact]
        public async Task Edit_InvalidModel_ReturnsViewAndDoesNotUpdate()
        {
            var ctx = CreateContext("Orders_Edit_Invalid");

            var order = new Order { ItemId = 1, Quantity = 5 };
            ctx.Orders.Add(order);
            ctx.SaveChanges();

            var controller = new OrdersController(ctx);
            controller.ModelState.AddModelError("Quantity", "Required");

            var updatedOrder = new Order { OrderId = order.OrderId, ItemId = 1, Quantity = 20 };

            var result = await controller.Edit(order.OrderId, updatedOrder);

            Assert.IsType<ViewResult>(result);
            Assert.NotEqual(20, ctx.Orders.First().Quantity); // should NOT update
        }

        //Delete GET (valid case — missing)
        [Fact]
        public async Task Delete_Valid_ReturnsViewWithOrder()
        {
            var ctx = CreateContext("Orders_Delete_Valid");
            var order = new Order { ItemId = 2, Quantity = 10 };
            ctx.Orders.Add(order);
            ctx.SaveChanges();

            var controller = new OrdersController(ctx);

            var result = await controller.Delete(order.OrderId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<Order>(view.Model);
        }

        //DeleteConfirmed Redirect Action
        [Fact]
        public async Task DeleteConfirmed_RedirectsToIndex()
        {
            var ctx = CreateContext("Orders_DeleteRedirect");
            var order = new Order { ItemId = 1, Quantity = 2 };
            ctx.Orders.Add(order);
            ctx.SaveChanges();

            var controller = new OrdersController(ctx);

            var result = await controller.DeleteConfirmed(order.OrderId);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", ((RedirectToActionResult)result).ActionName);
        }

        //Edit POST — Success Case
        [Fact]
        public async Task Edit_ValidOrder_RedirectsToIndex_AndUpdates()
        {
            var ctx = CreateContext("Orders_Edit_Valid");
            var order = new Order { ItemId = 1, Quantity = 2 };
            ctx.Orders.Add(order);
            ctx.SaveChanges();

            // detach existing tracked instance
            ctx.Entry(order).State = EntityState.Detached;

            var controller = new OrdersController(ctx);

            var updatedOrder = new Order
            {
                OrderId = order.OrderId,
                ItemId = 1,
                Quantity = 5
            };

            var result = await controller.Edit(order.OrderId, updatedOrder);

            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(5, ctx.Orders.First().Quantity);
        }

    }
}
