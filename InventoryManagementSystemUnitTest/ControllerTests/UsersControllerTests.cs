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
    }
}
