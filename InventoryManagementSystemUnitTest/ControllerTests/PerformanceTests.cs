using Inventory_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Xunit;

namespace InventoryManagementSystemUnitTest.ControllerTests
{
    public class PerformanceTests
    {
        [Fact]
        public async Task Insert_1000_Items_InMemory_ShouldCompleteQuickly()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase("PerfDb")
                .Options;

            using var ctx = new InventoryDbContext(options);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                ctx.Items.Add(new Item { Name = "PerfItem_" + i, Quantity = i });
            }
            await ctx.SaveChangesAsync();
            sw.Stop();

            // Example threshold — in-memory insert should be very fast; adjust to your machine.
            Assert.True(sw.ElapsedMilliseconds < 2000, $"Inserts took too long: {sw.ElapsedMilliseconds}ms");
        }
    }
}
