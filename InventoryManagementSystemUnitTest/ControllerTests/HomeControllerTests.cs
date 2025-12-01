using Inventory_Management_System.Controllers;
using Inventory_Management_System.Models;
using InventoryManagementSystemUnitTest.HelperMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace InventoryManagementSystemUnitTest.ControllerTests
{
    public class HomeControllerTests
    {
        private InventoryDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new InventoryDbContext(options);
        }

        private HomeController CreateController(string? role, string dbName)
        {
            var ctx = CreateDb(dbName);

            var mockLogger = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(mockLogger.Object, ctx);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();

            if (!string.IsNullOrWhiteSpace(role))
            {
                httpContext.Session.SetString("Role", role);
                httpContext.Session.SetString("Username", "TestUser");
            }

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        // -------------------------------
        // VALID ROLE ACCESS TEST
        // -------------------------------
        [Theory]
        [InlineData("Manager")]
        [InlineData("WarehouseStaff")]
        [InlineData("Supplier")]
        [InlineData("User")]
        public void DashboardAccess_ValidRole_ReturnsView(string role)
        {
            // Arrange
            var controller = CreateController(role, $"Home_Role_{role}");

            // Act
            IActionResult result = role switch
            {
                "Manager" => controller.ManagerDashboard(),
                "WarehouseStaff" => controller.WarehouseDashboard(),
                "Supplier" => controller.SupplierDashboard(),
                "User" => controller.UserDashboard(),
                _ => throw new InvalidOperationException("Invalid test role mapping.")
            };


            Assert.IsType<ViewResult>(result);
        }

        // -------------------------------
        // INVALID / UNAUTHORIZED ROLE TEST
        // -------------------------------
        [Theory]
        [InlineData(null, "ManagerDashboard")]
        [InlineData("", "WarehouseDashboard")]
        [InlineData("WrongRole", "SupplierDashboard")]
        public void DashboardAccess_InvalidRole_RedirectsToLogin(string? role, string target)
        {
            var controller = CreateController(role, $"Home_Invalid_{role}");

            IActionResult result = target switch
            {
                "ManagerDashboard" => controller.ManagerDashboard(),
                "WarehouseDashboard" => controller.WarehouseDashboard(),
                "SupplierDashboard" => controller.SupplierDashboard(),
                "UserDashboard" => controller.UserDashboard(),
                _ => throw new InvalidOperationException("Invalid test mapping.")
            };

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
        }

        // -------------------------------
        // MANAGER METRICS TEST
        // -------------------------------
        [Fact]
        public void ManagerDashboard_ShowsCorrectStatistics()
        {
            var controller = CreateController("Manager", "ManagerStatsDB");
            var ctx = CreateDb("ManagerStatsDB");

            ctx.Items.AddRange(
                new Item { Name = "Monitor", Quantity = 10 },
                new Item { Name = "Mouse", Quantity = 2 },
                new Item { Name = "Keyboard", Quantity = 4 }
            );

            ctx.Orders.Add(new Order { ItemId = 1, Quantity = 1 });
            ctx.SaveChanges();

            controller = CreateController("Manager", "ManagerStatsDB");

            var result = controller.ManagerDashboard();

            Assert.IsType<ViewResult>(result);

            Assert.Equal(3, controller.ViewBag.TotalProducts);
            Assert.Equal(1, controller.ViewBag.TotalReports);
            Assert.Equal(2, controller.ViewBag.LowStockItems); 
        }

        //Index() returns view
        [Fact]
        public void Index_ReturnsView()
        {
            var controller = CreateController("Manager", "Home_IndexTest");

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        //Privacy() returns view
        [Fact]
        public void Privacy_ReturnsView()
        {
            var controller = CreateController("User", "Home_PrivacyTest");

            var result = controller.Privacy();

            Assert.IsType<ViewResult>(result);
        }

        //Error() returns view with ErrorViewModel
        [Fact]
        public void Error_ReturnsViewWithErrorModel()
        {
            var controller = CreateController("Manager", "Home_ErrorTest");

            var result = controller.Error();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ErrorViewModel>(view.Model);

            Assert.False(string.IsNullOrWhiteSpace(model.RequestId));
        }

        [Theory]
        [InlineData("Supplier", "ManagerDashboard")]
        [InlineData("User", "WarehouseDashboard")]
        public void UnauthorizedDashboardAccess_RedirectsToLogin(string role, string target)
        {
            var controller = CreateController(role, $"Unauthorized_{role}");

            IActionResult result = target switch
            {
                "ManagerDashboard" => controller.ManagerDashboard(),
                "WarehouseDashboard" => controller.WarehouseDashboard(),
                _ => throw new NotImplementedException()
            };

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("Account", redirect.ControllerName);
        }
    }
}
