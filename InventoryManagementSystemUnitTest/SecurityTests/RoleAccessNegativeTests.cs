using Inventory_Management_System.Controllers;
using InventoryManagementSystemUnitTest.HelperMethods;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace InventoryManagementSystemUnitTest.SecurityTests
{
    public class RoleAccessNegativeTests
    {
        private T CreateControllerWithRole<T>(string role) where T : Controller
        {
            var ctx = new InventoryDbContext(
                new DbContextOptionsBuilder<InventoryDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);

            object controller;

            // HomeController needs ILogger + DbContext
            if (typeof(T) == typeof(HomeController))
            {
                var logger = new Mock<ILogger<HomeController>>();
                controller = new HomeController(logger.Object, ctx);
            }
            else
            {
                controller = Activator.CreateInstance(typeof(T), ctx);
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role ?? string.Empty)
            }, "TestAuth");

            var httpContext = new DefaultHttpContext();

            // In-memory session
            var session = new TestSession();
            httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });
            httpContext.Session = session;

            if (role != null)
                httpContext.Session.SetString("Role", role);

            httpContext.User = new ClaimsPrincipal(identity);

            ((Controller)controller).ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return (T)controller;
        }

        // -------------------------
        // SUPPLIER SECURITY TESTS
        // -------------------------

        // Unit-test the [Authorize] attribute – framework-level protection
        [Fact]
        public void SupplierController_Should_Require_Supplier_Role()
        {
            var authorizeAttr = typeof(SupplierController)
                .GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(authorizeAttr);
            Assert.Equal("Supplier", authorizeAttr.Roles);
        }

        // Positive path: when role = Supplier, Index must return a View
        [Fact]
        public void SupplierDashboard_WithSupplierRole_ReturnsView()
        {
            var controller = CreateControllerWithRole<SupplierController>("Supplier");

            var result = controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        // -------------------------
        // HOME CONTROLLER SECURITY TESTS
        // -------------------------

        [Theory]
        [InlineData("User")]
        [InlineData("Supplier")]
        [InlineData("WarehouseStaff")]
        [InlineData(null)]
        public void ManagerDashboard_Should_Redirect_When_NotManager(string role)
        {
            var home = CreateControllerWithRole<HomeController>(role);

            var result = home.ManagerDashboard();

            if (role == "Manager")
                Assert.IsType<ViewResult>(result);
            else
                Assert.IsType<RedirectToActionResult>(result);
        }

        [Theory]
        [InlineData("User")]
        [InlineData("Supplier")]
        [InlineData("Manager")]
        [InlineData(null)]
        public void WarehouseDashboard_Should_Redirect_When_NotWarehouseStaff(string role)
        {
            var home = CreateControllerWithRole<HomeController>(role);

            var result = home.WarehouseDashboard();

            if (role == "WarehouseStaff")
                Assert.IsType<ViewResult>(result);
            else
                Assert.IsType<RedirectToActionResult>(result);
        }
    }
}
