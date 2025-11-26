using Inventory_Management_System.Models;
using InventoryManagementSystemUnitTest.HelperMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace InventoryManagementSystemUnitTest.ControllerTests
{
    public class SupplierControllerTests
    {
        private InventoryDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new InventoryDbContext(options);
        }

        private SupplierController CreateController(string? role, string? supplierId, InventoryDbContext ctx)
        {
            var controller = new SupplierController(ctx);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            if (role != null)
                httpContext.Session.SetString("Role", role);

            if (supplierId != null)
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, supplierId));
                httpContext.User = new ClaimsPrincipal(identity);
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // --- Unauthorized Role Test ---
        [Fact]
        public void Index_WithInvalidRole_ReturnsView()
        {
            var ctx = CreateDb("Supplier_BadRole");

            var controller = CreateController("User", "SUP001", ctx);

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        // --- Main Behavior Test ---
        [Fact]
        public void Index_ValidSupplier_OnlyShowsOwnData()
        {
            var ctx = CreateDb("Supplier_Valid");

            // Supplier 1
            var item1 = new Item { Name = "Cable", Quantity = 10 };
            ctx.Items.Add(item1);
            ctx.Entry(item1).Property("SupplierId").CurrentValue = "SUP1";

            var order1 = new Order { ItemId = item1.ItemId, Status = "Pending" };
            ctx.Orders.Add(order1);
            ctx.Entry(order1).Property("SupplierId").CurrentValue = "SUP1";

            // Supplier 2 
            var item2 = new Item { Name = "Laptop", Quantity = 5 };
            ctx.Items.Add(item2);
            ctx.Entry(item2).Property("SupplierId").CurrentValue = "SUP2";

            var order2 = new Order { ItemId = item2.ItemId, Status = "Pending" };
            ctx.Orders.Add(order2);
            ctx.Entry(order2).Property("SupplierId").CurrentValue = "SUP2";

            ctx.SaveChanges();

            var controller = CreateController("Supplier", "SUP1", ctx);

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
            Assert.Equal(1, controller.ViewBag.TotalProductsSupplied);
            Assert.Equal(1, controller.ViewBag.SupplyReportCount);
            Assert.Equal(1, controller.ViewBag.PendingDeliveries);
        }

        [Fact]
        public void Index_EmptySupplierData_ReturnsZeroStats()
        {
            var ctx = CreateDb("Supplier_Empty");
            var controller = CreateController("Supplier", "SUP100", ctx);

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
            Assert.Equal(0, controller.ViewBag.PendingDeliveries);
            Assert.Equal(0, controller.ViewBag.TotalProductsSupplied);
            Assert.Equal(0, controller.ViewBag.SupplyReportCount);
        }
    }
}
