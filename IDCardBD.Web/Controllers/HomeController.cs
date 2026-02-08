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
        var model = new DashboardViewModel
        {
            TotalStudents = await _context.Students.CountAsync(),
            TotalEmployees = await _context.Employees.CountAsync(),
            PendingPrints = await _context.Students.CountAsync(s => !s.IsPrinted) + await _context.Employees.CountAsync(e => !e.IsPrinted)
        };
        return View(model);
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
