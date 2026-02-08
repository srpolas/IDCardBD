using Microsoft.AspNetCore.Mvc;
using IDCardBD.Web.Data;
using IDCardBD.Web.Services;
using IDCardBD.Web.ViewModels;
using IDCardBD.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace IDCardBD.Web.Controllers
{
    public class PrintController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;

        public PrintController(ApplicationDbContext context, IPdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> Queue(PrintStatus status = PrintStatus.SentToPrint)
        {
            var students = await _context.Students.Where(s => s.PrintStatus == status).ToListAsync();
            var employees = await _context.Employees.Where(e => e.PrintStatus == status).ToListAsync();

            var model = new PrintDashboardViewModel
            {
                CurrentStatus = status,
                Students = students,
                Employees = employees,
                SentToPrintCount = await _context.Students.CountAsync(s => s.PrintStatus == PrintStatus.SentToPrint) + 
                                   await _context.Employees.CountAsync(e => e.PrintStatus == PrintStatus.SentToPrint),
                ProcessingCount = await _context.Students.CountAsync(s => s.PrintStatus == PrintStatus.Processing) +
                                  await _context.Employees.CountAsync(e => e.PrintStatus == PrintStatus.Processing),
                PrintedCount = await _context.Students.CountAsync(s => s.PrintStatus == PrintStatus.Printed) +
                               await _context.Employees.CountAsync(e => e.PrintStatus == PrintStatus.Printed),
                ReadyForDeliveryCount = await _context.Students.CountAsync(s => s.PrintStatus == PrintStatus.ReadyForDelivery) +
                                        await _context.Employees.CountAsync(e => e.PrintStatus == PrintStatus.ReadyForDelivery)
            };

            return View(model);
        }

        public IActionResult Index() => RedirectToAction(nameof(Queue));

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int[] studentIds, int[] employeeIds, PrintStatus baseStatus)
        {
            PrintStatus newStatus = baseStatus switch
            {
                PrintStatus.SentToPrint => PrintStatus.Processing,
                PrintStatus.Processing => PrintStatus.Printed,
                PrintStatus.Printed => PrintStatus.ReadyForDelivery,
                _ => baseStatus
            };

            if (newStatus != baseStatus)
            {
                if (studentIds != null && studentIds.Length > 0)
                {
                    var students = await _context.Students.Where(s => studentIds.Contains(s.Id)).ToListAsync();
                    foreach (var s in students) s.PrintStatus = newStatus;
                }

                if (employeeIds != null && employeeIds.Length > 0)
                {
                    var employees = await _context.Employees.Where(e => employeeIds.Contains(e.Id)).ToListAsync();
                    foreach (var e in employees) e.PrintStatus = newStatus;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Queue), new { status = baseStatus }); 
        }

        public async Task<IActionResult> Generate(int id, string type)
        {
            IdentityBase? person = null;
            if (type == "Student")
            {
                person = await _context.Students.FindAsync(id);
            }
            else if (type == "Employee")
            {
                person = await _context.Employees.FindAsync(id);
            }

            if (person == null) return NotFound();

            var template = await _context.CardTemplates.FirstOrDefaultAsync(t => t.IsActive);
            if (template == null) return BadRequest("No active template found.");

            var pdfBytes = _pdfService.GenerateIdCard(person, template);
            return File(pdfBytes, "application/pdf", $"{person.FullName}_ID.pdf");
        }
    }
}
