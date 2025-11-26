using Inventory_Management_System.Models;
using Microsoft.EntityFrameworkCore;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public object? User { get; internal set; }
}
