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

        public async Task<IActionResult> Queue()
        {
            var model = new PrintQueueViewModel
            {
                Students = await _context.Students.Where(s => !s.IsPrinted).ToListAsync(),
                Employees = await _context.Employees.Where(e => !e.IsPrinted).ToListAsync()
            };
            return View(model);
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

        [HttpPost]
        public async Task<IActionResult> MarkPrinted(int id, string type)
        {
             if (type == "Student")
            {
                var student = await _context.Students.FindAsync(id);
                if (student != null) 
                {
                    student.IsPrinted = true;
                    await _context.SaveChangesAsync();
                }
            }
            else if (type == "Employee")
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee != null) 
                {
                    employee.IsPrinted = true;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Queue));
        }
    }
}
