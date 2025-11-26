using Inventory_Management_System.Models;
using Microsoft.AspNetCore.Mvc;

public class ReportsController : Controller
{
    private readonly InventoryDbContext _context;

    public ReportsController(InventoryDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var totalProducts = _context.Items.Count();
        var lowStockItems = _context.Items.Where(i => i.Quantity < 10).ToList();
        var totalOrders = _context.Orders.Count();

        var model = new ReportViewModel
        {
            TotalProducts = totalProducts,
            TotalOrders = totalOrders,
            LowStockItems = lowStockItems
        };

        return View(model);
    }
}
