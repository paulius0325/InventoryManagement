using Inventory_Management_System.Controllers;
using Inventory_Management_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryManagementSystemUnitTest.ControllerTests
{
    public class UsersControllerTests
    {
        private InventoryDbContext CreateDb(string name)
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new InventoryDbContext(options);
        }

        // -------------------------------
        // INDEX SHOULD RETURN LIST
        // -------------------------------
        [Fact]
        public async Task Index_ReturnsViewWithUserList()
        {
            var ctx = CreateDb("Users_Index");

            ctx.Users.AddRange(
                new User { Username = "A", Email = "a@test.com", Password = "123", Role = "User" },
                new User { Username = "B", Email = "b@test.com", Password = "456", Role = "Admin" }
            );

            await ctx.SaveChangesAsync();

            var controller = new UsersController(ctx);

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(view.Model);

            Assert.Equal(2, model.Count);
        }

        // -------------------------------
        // CREATE VALID USER
        // -------------------------------
        [Fact]
        public async Task Create_ValidUser_RedirectsAndSaves()
        {
            var ctx = CreateDb("Users_Create");

            var controller = new UsersController(ctx);

            var user = new User
            {
                Username = "newuser",
                Password = "pw",
                Email = "new@test.com",
                Role = "User"
            };

            var result = await controller.Create(user);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            Assert.Single(ctx.Users);
            Assert.Equal("newuser", ctx.Users.First().Username);
        }

        // -------------------------------
        // CREATE INVALID USER
        // -------------------------------
        [Fact]
        public async Task Create_InvalidModel_ReturnsView()
        {
            var ctx = CreateDb("Users_Invalid");

            var controller = new UsersController(ctx);
            controller.ModelState.AddModelError("Username", "Required");

            var user = new User();

            var result = await controller.Create(user);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(user, view.Model);

            Assert.Empty(ctx.Users);
        }

        // -------------------------------
        // DETAILS INVALID ID
        // -------------------------------
        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            var ctx = CreateDb("Users_Details");

            var controller = new UsersController(ctx);

            var result = await controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        // -------------------------------
        // EDIT VALID UPDATE
        // -------------------------------
        [Fact]
        public async Task Edit_ValidUpdate_Redirects()
        {
            var ctx = CreateDb("Users_Edit");

            var user = new User { Username = "old", Password = "pw", Email = "test@x.com", Role = "User" };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            // Detach tracked instance to simulate real request lifecycle
            ctx.Entry(user).State = EntityState.Detached;

            var controller = new UsersController(ctx);

            var updated = new User
            {
                UserId = user.UserId,
                Username = "updated",
                Password = "pw2",
                Email = "update@x.com",
                Role = "Admin"
            };

            var result = await controller.Edit(user.UserId, updated);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            var saved = ctx.Users.First();
            Assert.Equal("updated", saved.Username);
        }

        // -------------------------------
        // EDIT WRONG ID MUST NOT UPDATE
        // -------------------------------
        [Fact]
        public async Task Edit_WrongId_ReturnsNotFound()
        {
            var ctx = CreateDb("Users_EditWrong");

            var controller = new UsersController(ctx);

            var user = new User { UserId = 999, Username = "test" };

            var result = await controller.Edit(1, user);

            Assert.IsType<NotFoundResult>(result);
        }

        // -------------------------------
        // DELETE REMOVES USER
        // -------------------------------
        [Fact]
        public async Task DeleteConfirmed_RemovesUser()
        {
            var ctx = CreateDb("Users_Delete");

            var user = new User { Username = "delete", Email = "x@test.com", Password = "pw", Role = "User" };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var controller = new UsersController(ctx);

            var result = await controller.DeleteConfirmed(user.UserId);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            Assert.Empty(ctx.Users);
        }

        //GET Create() Should Return View
        [Fact]
        public void Create_Get_ReturnsView()
        {
            var ctx = CreateDb("Users_Create_Get");
            var controller = new UsersController(ctx);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        //GET Details Valid User Returns View
        [Fact]
        public async Task Details_ValidId_ReturnsViewWithUser()
        {
            var ctx = CreateDb("Users_Details_Valid");
            var user = new User { Username = "test", Email = "t@test.com", Password = "pw", Role = "User" };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var controller = new UsersController(ctx);

            var result = await controller.Details(user.UserId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<User>(view.Model);
        }

        //GET Edit(id) Valid ID -> View
        [Fact]
        public async Task Edit_Get_ValidId_ReturnsView()
        {
            var ctx = CreateDb("Users_Edit_Get");
            var user = new User { Username = "edit", Email = "e@test.com", Password = "pw", Role = "User" };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var controller = new UsersController(ctx);

            var result = await controller.Edit(user.UserId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<User>(view.Model);
        }

        //Edit POST Invalid Model -> Return View
        [Fact]
        public async Task Edit_InvalidModel_ReturnsView()
        {
            var ctx = CreateDb("Users_Edit_Invalid");
            var user = new User { Username = "temp", Email = "temp@test.com", Password = "pw", Role = "User" };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var controller = new UsersController(ctx);
            controller.ModelState.AddModelError("Username", "Required");

            // detached simulating new POST
            ctx.Entry(user).State = EntityState.Detached;

            var result = await controller.Edit(user.UserId, user);

            Assert.IsType<ViewResult>(result);
        }

        //GET Delete Valid ID -> View
        [Fact]
        public async Task Delete_Get_ValidId_ReturnsView()
        {
            var ctx = CreateDb("Users_Delete_Get");
            var user = new User { Username = "del", Email = "d@test.com", Password = "pw", Role = "User" };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            var controller = new UsersController(ctx);

            var result = await controller.Delete(user.UserId);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<User>(view.Model);
        }

        [Fact]
        public async Task Edit_Concurrency_UserMissing_ReturnsNotFound()
        {
            var ctx = CreateDb("Users_Edit_Concurrency");
            var user = new User { Username = "x", Email = "x@test.com", Password = "pw", Role = "User" };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();

            ctx.Entry(user).State = EntityState.Detached;

            // Simulate deletion before Update()
            ctx.Users.RemoveRange(ctx.Users);
            await ctx.SaveChangesAsync();

            var controller = new UsersController(ctx);

            var updated = new User { UserId = user.UserId, Username = "updated" };

            var result = await controller.Edit(user.UserId, updated);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
