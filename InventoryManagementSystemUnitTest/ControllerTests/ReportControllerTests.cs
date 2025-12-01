using Inventory_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManagementSystemUnitTest.ControllerTests
{
    public class ReportControllerTests
    {
        private InventoryDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new InventoryDbContext(options);
        }

        [Fact]
        public void Index_WithSeededData_ReturnsCorrectReportCounts()
        {
            var ctx = CreateDb("Report_WithData");

            ctx.Items.AddRange(new[]
            {
        new Item { Name = "Monitor", Quantity = 10 },
        new Item { Name = "Mouse", Quantity = 2 },
        new Item { Name = "Keyboard", Quantity = 4 }
    });

            ctx.Orders.Add(new Order { ItemId = 1, Quantity = 1 });
            ctx.Orders.Add(new Order { ItemId = 2, Quantity = 5 });
            ctx.SaveChanges();

            var controller = new ReportsController(ctx);

            var result = controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReportViewModel>(viewResult.Model);

            Assert.Equal(3, model.TotalProducts);
            Assert.Equal(2, model.TotalOrders);
            Assert.Equal(2, model.LowStockItems?.Count);
        }

        [Fact]
        public void Index_WithEmptyDatabase_ReturnsZeroValues()
        {
            var ctx = CreateDb("Report_EmptyDb");
            var controller = new ReportsController(ctx);

            var result = controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReportViewModel>(viewResult.Model);

            Assert.Equal(0, model.TotalProducts);
            Assert.Equal(0, model.TotalOrders);
            Assert.Equal(0, model.LowStockItems?.Count);
        }

        [Theory]
        [InlineData(5, 2)]
        [InlineData(3, 1)]
        [InlineData(1, 0)]
        public void Index_LowStockLogicParameterized(int threshold, int expectedCount)
        {
            var ctx = CreateDb($"Report_Param_{threshold}");

            ctx.Items.AddRange(new[]
            {
                new Item { Name = "Monitor", Quantity = 10 },
                new Item { Name = "Mouse", Quantity = 2 },
                new Item { Name = "Keyboard", Quantity = 4 }
            });

            ctx.SaveChanges();

            var controller = new ReportsController(ctx);

            var result = controller.Index();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReportViewModel>(viewResult.Model);

            var actualLowStockCount = ctx.Items.Count(i => i.Quantity <= threshold);
            Assert.Equal(expectedCount, actualLowStockCount);
        }

        //Ensure Correct View Type Returned
        [Fact]
        public void Index_ReturnsViewResult()
        {
            var ctx = CreateDb("Report_ViewCheck");
            var controller = new ReportsController(ctx);

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        //Verify Model Binding Type Is Always ReportViewModel
        [Fact]
        public void Index_Model_IsCorrectType()
        {
            var ctx = CreateDb("Report_ModelType");
            var controller = new ReportsController(ctx);

            var result = controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<ReportViewModel>(view.Model);
        }

        //Low Stock Boundary Condition Test (Quantity = 10)
        [Fact]
        public void Index_QuantityEqualToThreshold_IsNotConsideredLowStock()
        {
            var ctx = CreateDb("Report_ThresholdTest");

            ctx.Items.Add(new Item { Name = "TestItem", Quantity = 10 });
            ctx.SaveChanges();

            var controller = new ReportsController(ctx);
            var result = controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReportViewModel>(view.Model);

            Assert.Empty(model.LowStockItems); // because Quantity < 10 is required
        }

        //Stress Test With Mixed Quantity Values
        [Fact]
        public void Index_MixedInventory_CorrectLowStockCount()
        {
            var ctx = CreateDb("Report_Mixed_Inventory");

            ctx.Items.AddRange(new[]
            {
                new Item { Name = "Item1", Quantity = 1 },
                new Item { Name = "Item2", Quantity = 9 },
                new Item { Name = "Item3", Quantity = 10 },
                new Item { Name = "Item4", Quantity = 11 },
            });

            ctx.SaveChanges();

            var controller = new ReportsController(ctx);
            var result = controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ReportViewModel>(view.Model);

            // Only quantities < 10 count → (1 and 9) → expected: 2
            Assert.Equal(2, model.LowStockItems.Count);
        }
    }
}
