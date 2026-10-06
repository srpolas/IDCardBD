using Microsoft.AspNetCore.Mvc;
using IDCardBD.Web.Data;
using IDCardBD.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Authorization;
using IDCardBD.Web.Services;

namespace IDCardBD.Web.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPhotoService _photoService;

        public EmployeesController(ApplicationDbContext context, IPhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
        }

        public async Task<IActionResult> Index(string searchString, string designation, string department, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CodeSortParm = sortOrder == "Code" ? "code_desc" : "Code";
            ViewBag.DesignationSortParm = sortOrder == "Designation" ? "des_desc" : "Designation";
            ViewBag.DepartmentSortParm = sortOrder == "Department" ? "dept_desc" : "Department";

            var query = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(e => e.FullName.Contains(searchString) || e.EmployeeCode.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(designation))
            {
                query = query.Where(e => e.Designation == designation);
            }

            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(e => e.Department == department);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(e => e.FullName);
                    break;
                case "Code":
                    query = query.OrderBy(e => e.EmployeeCode);
                    break;
                case "code_desc":
                    query = query.OrderByDescending(e => e.EmployeeCode);
                    break;
                case "Designation":
                    query = query.OrderBy(e => e.Designation);
                    break;
                case "des_desc":
                    query = query.OrderByDescending(e => e.Designation);
                    break;
                case "Department":
                    query = query.OrderBy(e => e.Department);
                    break;
                case "dept_desc":
                    query = query.OrderByDescending(e => e.Department);
                    break;
                default:
                    query = query.OrderBy(e => e.FullName);
                    break;
            }

            ViewBag.Designations = new SelectList(await _context.Employees.Select(e => e.Designation).Distinct().ToListAsync());
            ViewBag.Departments = new SelectList(await _context.Employees.Select(e => e.Department).Distinct().ToListAsync());
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentDesignation = designation;
            ViewBag.CurrentDepartment = department;

            return View(await query.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee, IFormFile? photo)
        {
            var photoResult = await _photoService.SaveProfilePhotoAsync(photo);
            if (photoResult.Error is { } photoError)
                ModelState.AddModelError("photo", photoError);

            if (ModelState.IsValid)
            {
                employee.Category = UserCategory.Employee;
                employee.QRCode = employee.EmployeeCode;
                employee.PhotoPath = photoResult.RelativePath;

                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee, IFormFile? photo)
        {
            if (id != employee.Id) return NotFound();

            // PhotoPath is preserved from the database, not bound from the form.
            ModelState.Remove(nameof(employee.PhotoPath));

            var photoResult = await _photoService.SaveProfilePhotoAsync(photo);
            if (photoResult.Error is { } photoError)
                ModelState.AddModelError("photo", photoError);

            if (!ModelState.IsValid)
            {
                return View(employee);
            }

            var existing = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
            if (existing == null) return NotFound();

            string? replacedPhotoPath = null;
            if (photoResult.RelativePath != null)
            {
                replacedPhotoPath = existing.PhotoPath;
                employee.PhotoPath = photoResult.RelativePath;
            }
            else
            {
                employee.PhotoPath = existing.PhotoPath;
            }

            try
            {
                _context.Update(employee);
                await _context.SaveChangesAsync();
                _photoService.DeletePhotoFile(replacedPhotoPath);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null) return NotFound();
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                _photoService.DeletePhotoFile(employee.PhotoPath);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToPrint(int[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var employees = await _context.Employees.Where(e => ids.Contains(e.Id)).ToListAsync();
            foreach (var employee in employees)
            {
                employee.PrintStatus = PrintStatus.SentToPrint;
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
