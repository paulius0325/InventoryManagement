using Inventory_Management_System.Controllers;
using Inventory_Management_System.Models;
using InventoryManagementSystemUnitTest.HelperMethods;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Xunit;

namespace InventoryManagementSystemUnitTest.ControllerTests
{
    public class AccountControllerTests
    {
        private InventoryDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new InventoryDbContext(options);
        }

        [Fact]
        public void Login_ValidCredentials_RedirectsToDashboard_And_SetsSession()
        {
            // Arrange
            var ctx = CreateContext("Account_Login_Success");
            ctx.Users.Add(new User { Username = "manager", Password = "123", Role = "Manager", Email = "a@b.com", Phone = "123" });
            ctx.SaveChanges();

            var controller = new AccountController(ctx);

            // Proper service provider setup
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            services.AddSession();
            var serviceProvider = services.BuildServiceProvider();

            // Create HttpContext and attach a working Session
            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            var session = new TestSession();
            httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });

            // ✅ Manually assign Session
            httpContext.Session = session;

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            };

            controller.Url = new Microsoft.AspNetCore.Mvc.Routing.UrlHelper(controller.ControllerContext);

            // Act
            var result = controller.Login("manager", "123");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManagerDashboard", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);

            Assert.Equal("manager", session.GetString("Username"));
            Assert.Equal("Manager", session.GetString("Role"));
        }

        [Fact]
        public void Login_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            var ctx = CreateContext("Account_Login_Fail");
            ctx.Users.Add(new User { Username = "user1", Password = "pw", Role = "User", Email = "x@y.com", Phone = "1" });
            ctx.SaveChanges();

            var controller = new AccountController(ctx);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = controller.Login("wrong", "wrong");

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Invalid username or password.", controller.ViewBag.Error);
        }
    }
}
