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

            // service provider setup
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            services.AddSession();
            var serviceProvider = services.BuildServiceProvider();


            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            var session = new TestSession();
            httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });


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

        [Fact]
        public void Logout_Should_ClearSession_And_Redirect_To_Login()
        {
            // Arrange
            var ctx = CreateContext("Account_Logout");
            var controller = new AccountController(ctx);

            var httpContext = new DefaultHttpContext();
            var session = new TestSession();
            httpContext.Features.Set<ISessionFeature>(new SessionFeature { Session = session });
            httpContext.Session = session;

            // Simulate logged user
            session.SetString("Username", "TestUser");
            session.SetString("Role", "User");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = controller.Logout();

            // Assert the session is cleared
            Assert.Null(session.GetString("Username"));
            Assert.Null(session.GetString("Role"));

            // Assert redirect
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
        }

        //GET Login Returns View
        [Fact]
        public void Login_Get_ReturnsView()
        {
            var ctx = CreateContext("Account_Login_Get");
            var controller = new AccountController(ctx);

            var result = controller.Login();

            Assert.IsType<ViewResult>(result);
        }

        //Login with Empty Username/Password
        [Fact]
        public void Login_EmptyCredentials_ReturnsError()
        {
            var ctx = CreateContext("Account_Login_Empty");
            var controller = new AccountController(ctx);

            var result = controller.Login("", "");

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Username and password are required.", controller.ViewBag.Error);
        }

        //Valid Login: WarehouseStaff Routing
        [Fact]
        public void Login_WarehouseStaff_RedirectsCorrectly()
        {
            var ctx = CreateContext("Account_Login_Warehouse");
            ctx.Users.Add(new User { Username = "staff", Password = "pw", Role = "WarehouseStaff" });
            ctx.SaveChanges();

            var controller = new AccountController(ctx);

            var http = new DefaultHttpContext();
            var session = new TestSession();
            http.Features.Set<ISessionFeature>(new SessionFeature { Session = session });
            http.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = http };

            var result = controller.Login("staff", "pw");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("WarehouseDashboard", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        //Valid Login: Supplier Routing
        [Fact]
        public void Login_Supplier_RedirectsCorrectly()
        {
            var ctx = CreateContext("Account_Login_Supplier");
            ctx.Users.Add(new User { Username = "sup", Password = "pw", Role = "Supplier" });
            ctx.SaveChanges();

            var controller = new AccountController(ctx);

            var http = new DefaultHttpContext();
            var session = new TestSession();
            http.Features.Set<ISessionFeature>(new SessionFeature { Session = session });
            http.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = http };

            var result = controller.Login("sup", "pw");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SupplierDashboard", redirect.ActionName);
        }

        //Valid Login: User Role
        [Fact]
        public void Login_NormalUser_RedirectsToUserDashboard()
        {
            var ctx = CreateContext("Account_Login_User");
            ctx.Users.Add(new User { Username = "user", Password = "pw", Role = "User" });
            ctx.SaveChanges();

            var controller = new AccountController(ctx);

            var http = new DefaultHttpContext();
            var session = new TestSession();
            http.Features.Set<ISessionFeature>(new SessionFeature { Session = session });
            http.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = http };

            var result = controller.Login("user", "pw");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserDashboard", redirect.ActionName);
        }

        //Valid Login: Unknown Role -> redirect back to Login
        [Fact]
        public void Login_UnknownRole_RedirectsToLogin()
        {
            var ctx = CreateContext("Account_Login_UnknownRole");
            ctx.Users.Add(new User { Username = "ghost", Password = "pw", Role = "SomethingElse" });
            ctx.SaveChanges();

            var controller = new AccountController(ctx);

            var http = new DefaultHttpContext();
            var session = new TestSession();
            http.Features.Set<ISessionFeature>(new SessionFeature { Session = session });
            http.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = http };

            var result = controller.Login("ghost", "pw");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
        }

        //Username empty & password provided
        [Fact]
        public void Login_MissingUsername_ReturnsError()
        {
            // Arrange
            var ctx = CreateContext("Account_MissingUsername");
            var controller = new AccountController(ctx);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            // Act
            var result = controller.Login("", "123");

            // Assert
            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Username and password are required.", controller.ViewBag.Error);
        }

        //Password empty
        [Fact]
        public void Login_MissingPassword_ReturnsError()
        {
            var ctx = CreateContext("Account_MissingPassword");
            var controller = new AccountController(ctx);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var result = controller.Login("manager", "");

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal("Username and password are required.", controller.ViewBag.Error);
        }

        //Valid Supplier login
        [Fact]
        public void Login_ValidSupplier_RedirectsToSupplierDashboard()
        {
            var ctx = CreateContext("SupplierLogin");
            ctx.Users.Add(new User { Username = "sup", Password = "pw", Role = "Supplier" });
            ctx.SaveChanges();

            var controller = new AccountController(ctx);

            var http = new DefaultHttpContext();
            var session = new TestSession();
            http.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = http };

            var result = controller.Login("sup", "pw");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("SupplierDashboard", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
            Assert.Equal("sup", session.GetString("Username"));
            Assert.Equal("Supplier", session.GetString("Role"));
        }

        //Valid WarehouseStaff login
        [Fact]
        public void Login_WarehouseStaff_RedirectsToWarehouseDashboard()
        {
            var ctx = CreateContext("WarehouseLogin");
            ctx.Users.Add(new User { Username = "wh", Password = "pw", Role = "WarehouseStaff" });
            ctx.SaveChanges();

            var controller = new AccountController(ctx);

            var http = new DefaultHttpContext();
            var session = new TestSession();
            http.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = http };

            var result = controller.Login("wh", "pw");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("WarehouseDashboard", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        //Valid regular User login
        [Fact]
        public void Login_ValidUser_RedirectsToUserDashboard()
        {
            var ctx = CreateContext("UserLogin");
            ctx.Users.Add(new User { Username = "usr", Password = "pw", Role = "User" });
            ctx.SaveChanges();

            var controller = new AccountController(ctx);

            var http = new DefaultHttpContext();
            var session = new TestSession();
            http.Session = session;
            controller.ControllerContext = new ControllerContext { HttpContext = http };

            var result = controller.Login("usr", "pw");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("UserDashboard", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }
    }
}
