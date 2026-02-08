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
            var students = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Section)
                .Where(s => s.PrintStatus == status)
                .ToListAsync();
            var employees = await _context.Employees.Where(e => e.PrintStatus == status).ToListAsync();
            var teachers = await _context.Teachers.Where(t => t.PrintStatus == status).ToListAsync();

            var model = new PrintDashboardViewModel
            {
                CurrentStatus = status,
                Students = students,
                Employees = employees,
                Teachers = teachers,
                SentToPrintCount = await _context.Students.CountAsync(s => s.PrintStatus == PrintStatus.SentToPrint) + 
                                   await _context.Employees.CountAsync(e => e.PrintStatus == PrintStatus.SentToPrint) +
                                   await _context.Teachers.CountAsync(t => t.PrintStatus == PrintStatus.SentToPrint),
                ProcessingCount = await _context.Students.CountAsync(s => s.PrintStatus == PrintStatus.Processing) +
                                  await _context.Employees.CountAsync(e => e.PrintStatus == PrintStatus.Processing) +
                                  await _context.Teachers.CountAsync(t => t.PrintStatus == PrintStatus.Processing),
                PrintedCount = await _context.Students.CountAsync(s => s.PrintStatus == PrintStatus.Printed) +
                               await _context.Employees.CountAsync(e => e.PrintStatus == PrintStatus.Printed) +
                               await _context.Teachers.CountAsync(t => t.PrintStatus == PrintStatus.Printed),
                ReadyForDeliveryCount = await _context.Students.CountAsync(s => s.PrintStatus == PrintStatus.ReadyForDelivery) +
                                        await _context.Employees.CountAsync(e => e.PrintStatus == PrintStatus.ReadyForDelivery) +
                                        await _context.Teachers.CountAsync(t => t.PrintStatus == PrintStatus.ReadyForDelivery)
            };

            return View(model);
        }

        public IActionResult Index() => RedirectToAction(nameof(Queue));

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int[] studentIds, int[] employeeIds, int[] teacherIds, PrintStatus baseStatus)
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

                if (teacherIds != null && teacherIds.Length > 0)
                {
                    var teachers = await _context.Teachers.Where(t => teacherIds.Contains(t.Id)).ToListAsync();
                    foreach (var t in teachers) t.PrintStatus = newStatus;
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
                person = await _context.Students
                    .Include(s => s.Class)
                    .Include(s => s.Section)
                    .FirstOrDefaultAsync(s => s.Id == id);
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
