using Inventory_Management_System.Controllers;
using InventoryManagementSystemUnitTest.HelperMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace InventoryManagementSystemUnitTest.SecurityTests
{
    public class AuthorizationTests
    {
        private HomeController CreateControllerWithRole(string role)
        {
            var ctx = new InventoryDbContext(
                new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

            var controller = new HomeController(null, ctx);

            var identity = new ClaimsIdentity(new[]
            {
        new Claim(ClaimTypes.Role, role)
    }, "TestAuth");

            // Configure HttpContext
            var httpContext = new DefaultHttpContext();

            // ----- FIX: Add a working in-memory session -----
            var testSession = new TestSession();
            httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = testSession });
            httpContext.Session = testSession;

            // Set role value into session so controller logic works
            if (role != null)
                httpContext.Session.SetString("Role", role);

            httpContext.User = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

        [Theory]
        [InlineData("Manager", typeof(ViewResult))]
        [InlineData("User", typeof(RedirectToActionResult))]
        [InlineData("Supplier", typeof(RedirectToActionResult))]
        [InlineData(null, typeof(RedirectToActionResult))]
        public void ManagerDashboard_AccessControl_Test(string role, Type expectedType)
        {
            var controller = CreateControllerWithRole(role ?? "");
            var result = controller.ManagerDashboard();

            Assert.IsType(expectedType, result);
        }
    }
}
