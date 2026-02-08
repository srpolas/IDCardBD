using Microsoft.AspNetCore.Mvc;
using IDCardBD.Web.Data;
using IDCardBD.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Authorization;

namespace IDCardBD.Web.Controllers
{
    [Authorize]
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public TeachersController(ApplicationDbContext context, IWebHostEnvironment environment)
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

            var query = _context.Teachers.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(t => t.FullName.Contains(searchString) || t.TeacherCode.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(designation))
            {
                query = query.Where(t => t.Designation == designation);
            }

            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(t => t.Department == department);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(t => t.FullName);
                    break;
                case "Code":
                    query = query.OrderBy(t => t.TeacherCode);
                    break;
                case "code_desc":
                    query = query.OrderByDescending(t => t.TeacherCode);
                    break;
                case "Designation":
                    query = query.OrderBy(t => t.Designation);
                    break;
                case "des_desc":
                    query = query.OrderByDescending(t => t.Designation);
                    break;
                case "Department":
                    query = query.OrderBy(t => t.Department);
                    break;
                case "dept_desc":
                    query = query.OrderByDescending(t => t.Department);
                    break;
                default:
                    query = query.OrderBy(t => t.FullName);
                    break;
            }

            ViewBag.Designations = new SelectList(await _context.Teachers.Select(t => t.Designation).Distinct().ToListAsync());
            ViewBag.Departments = new SelectList(await _context.Teachers.Select(t => t.Department).Distinct().ToListAsync());
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
        public async Task<IActionResult> Create(Teacher teacher, IFormFile photo)
        {
                if (photo != null)
                {
                    if (photo.Length > 100 * 1024)
                    {
                        ModelState.AddModelError("photo", "Photo size must be within 100KB.");
                    }
                    else if (Path.GetExtension(photo.FileName).ToLower() != ".jpg")
                    {
                        ModelState.AddModelError("photo", "Only .jpg files are allowed.");
                    }
                    else
                    {
                        string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                        if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                        using (var stream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }
                        teacher.PhotoPath = "/uploads/profiles/" + fileName;
                    }
                }

                if (ModelState.IsValid)
                {
                    teacher.Category = UserCategory.Teacher;
                    teacher.QRCode = teacher.TeacherCode;

                    _context.Add(teacher);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            return View(teacher);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Teacher teacher, IFormFile photo)
        {
            if (id != teacher.Id) return NotFound();

            ModelState.Remove(nameof(teacher.PhotoPath));

            if (ModelState.IsValid)
            {
                try
                {
                    if (photo != null)
                    {
                        if (photo.Length > 100 * 1024)
                        {
                            ModelState.AddModelError("photo", "Photo size must be within 100KB.");
                        }
                        else if (Path.GetExtension(photo.FileName).ToLower() != ".jpg")
                        {
                            ModelState.AddModelError("photo", "Only .jpg files are allowed.");
                        }
                        else
                        {
                            string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                            using (var stream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create))
                            {
                                await photo.CopyToAsync(stream);
                            }
                            teacher.PhotoPath = "/uploads/profiles/" + fileName;
                        }
                    }
                    else
                    {
                        var existing = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                        if (existing != null) teacher.PhotoPath = existing.PhotoPath;
                    }

                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var teacher = await _context.Teachers.FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher != null)
            {
                _context.Teachers.Remove(teacher);
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

            var teachers = await _context.Teachers.Where(t => ids.Contains(t.Id)).ToListAsync();
            foreach (var teacher in teachers)
            {
                teacher.PrintStatus = PrintStatus.SentToPrint;
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }
    }
}
