using Microsoft.AspNetCore.Mvc;
using IDCardBD.Web.Data;
using IDCardBD.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IDCardBD.Web.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EmployeesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
        public async Task<IActionResult> Create(Employee employee, IFormFile photo)
        {
            if (ModelState.IsValid)
            {
               if (photo != null)
                {
                     string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                     if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                     
                     string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                     using (var stream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create))
                     {
                         await photo.CopyToAsync(stream);
                     }
                     employee.PhotoPath = "/uploads/profiles/" + fileName;
                }

                employee.Category = UserCategory.Employee;
                employee.QRCode = employee.EmployeeCode;

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
        public async Task<IActionResult> Edit(int id, Employee employee, IFormFile photo)
        {
            if (id != employee.Id) return NotFound();

            ModelState.Remove(nameof(employee.PhotoPath));

            if (ModelState.IsValid)
            {
                try
                {
                    if (photo != null)
                     {
                         string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                         if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                         
                         string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                         using (var stream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create))
                         {
                             await photo.CopyToAsync(stream);
                         }
                         employee.PhotoPath = "/uploads/profiles/" + fileName;
                     }
                     else 
                     {
                         var existing = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                         if (existing != null) employee.PhotoPath = existing.PhotoPath;
                     }

                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
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
