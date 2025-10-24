using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanDienThoai.Models;

namespace WebBanDienThoai.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Employer")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate, string type = "month")
        {
            var orders = _context.Orders
                .Include(o => o.ApplicationUser)
                .Where(o => o.Status == OrderStatus.HoanTat);

            if (fromDate.HasValue)
                orders = orders.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                orders = orders.Where(o => o.OrderDate <= toDate.Value);

            var orderList = await orders.ToListAsync();

            var totalRevenue = orderList.Sum(o => o.TotalPrice);
            var totalOrders = orderList.Count;

            // Thống kê theo type
            var revenueBy = type switch
            {
                "day" => orderList
                    .GroupBy(o => o.OrderDate.ToString("dd/MM/yyyy"))
                    .Select(g => new { Label = g.Key, Total = g.Sum(x => x.TotalPrice) }),

                "year" => orderList
                    .GroupBy(o => o.OrderDate.ToString("yyyy"))
                    .Select(g => new { Label = g.Key, Total = g.Sum(x => x.TotalPrice) }),

                _ => orderList
                    .GroupBy(o => o.OrderDate.ToString("MM/yyyy")) // mặc định theo tháng
                    .Select(g => new { Label = g.Key, Total = g.Sum(x => x.TotalPrice) }),
            };

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.RevenueBy = revenueBy.OrderBy(x => x.Label).ToList();
            ViewBag.Type = type;

            return View(orderList);
        }
    }
}
