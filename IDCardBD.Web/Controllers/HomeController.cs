using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using IDCardBD.Web.Models;
using IDCardBD.Web.Data;
using IDCardBD.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IDCardBD.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var today = DateTime.Today;
            var model = new DashboardViewModel
            {
                TotalStudents = await _context.Students.CountAsync(),
                TotalEmployees = await _context.Employees.CountAsync(),
                TotalTeachers = await _context.Teachers.CountAsync(),
                PendingPrints = await _context.Students.CountAsync(s => s.PrintStatus == PrintStatus.SentToPrint) + 
                                await _context.Employees.CountAsync(e => e.PrintStatus == PrintStatus.SentToPrint) +
                                await _context.Teachers.CountAsync(t => t.PrintStatus == PrintStatus.SentToPrint)
            };

            // Fetch activities added today
            var recentStudents = await _context.Students
                .Where(s => s.CreatedDate >= today)
                .Select(s => new RecentActivityViewModel { Name = s.FullName, Category = "Student", AddedDate = s.CreatedDate, PhotoPath = s.PhotoPath })
                .ToListAsync();

            var recentTeachers = await _context.Teachers
                .Where(t => t.CreatedDate >= today)
                .Select(t => new RecentActivityViewModel { Name = t.FullName, Category = "Teacher", AddedDate = t.CreatedDate, PhotoPath = t.PhotoPath })
                .ToListAsync();

            var recentEmployees = await _context.Employees
                .Where(e => e.CreatedDate >= today)
                .Select(e => new RecentActivityViewModel { Name = e.FullName, Category = "Employee", AddedDate = e.CreatedDate, PhotoPath = e.PhotoPath })
                .ToListAsync();

            model.RecentActivities = recentStudents.Concat(recentTeachers).Concat(recentEmployees)
                .OrderByDescending(a => a.AddedDate)
                .ToList();

            return View("Dashboard", model);
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
