using Inventory_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManagementSystemUnitTest.ControllerTests
{
    public class ManageControllerTests
    {
        private InventoryDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new InventoryDbContext(options);
        }

        //Normal seeded DB values are computed correctly
        [Fact]
        public void Index_WithSeededData_ReturnsCorrectStats()
        {
            var ctx = CreateDb("Manager_Seeded");

            ctx.Items.AddRange(
                new Item { Name = "Monitor", Quantity = 20 },
                new Item { Name = "Mouse", Quantity = 5 },
                new Item { Name = "Keyboard", Quantity = 10 }
            );

            ctx.Orders.Add(new Order { ItemId = 1, Quantity = 1 });
            ctx.Orders.Add(new Order { ItemId = 2, Quantity = 3 });

            ctx.SaveChanges();

            var controller = new ManagerController(ctx);

            var result = controller.Index();
            var view = Assert.IsType<ViewResult>(result);

            Assert.Equal(3, controller.ViewBag.TotalProducts);
            Assert.Equal(2, controller.ViewBag.TotalReports);
            Assert.Equal(2, controller.ViewBag.LowStockItems); // Mouse (5), Keyboard (10)
        }

        //Empty database returns zeroes
        [Fact]
        public void Index_EmptyDb_ReturnsZeroValues()
        {
            var ctx = CreateDb("Manager_Empty");

            var controller = new ManagerController(ctx);

            var result = controller.Index();
            var view = Assert.IsType<ViewResult>(result);

            Assert.Equal(0, controller.ViewBag.TotalProducts);
            Assert.Equal(0, controller.ViewBag.TotalReports);
            Assert.Equal(0, controller.ViewBag.LowStockItems);
        }

        //Boundary condition for low stock threshold
        [Fact]
        public void Index_LowStockThreshold_IncludesQuantityEqualTo10()
        {
            var ctx = CreateDb("Manager_Threshold");

            ctx.Items.AddRange(
                new Item { Name = "A", Quantity = 10 },
                new Item { Name = "B", Quantity = 11 },
                new Item { Name = "C", Quantity = 1 }
            );
            ctx.SaveChanges();

            var controller = new ManagerController(ctx);

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
            Assert.Equal(2, controller.ViewBag.LowStockItems); // A and C
        }

        //Ensure return type model structure remains view
        [Fact]
        public void Index_ReturnsViewResult()
        {
            var ctx = CreateDb("Manager_ViewTest");
            var controller = new ManagerController(ctx);

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }
    }
}
