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
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPhotoService _photoService;

        public StudentsController(ApplicationDbContext context, IPhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
        }

        public async Task<IActionResult> Index(string searchString, int? classId, int? sectionId, int? groupId, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.RollSortParm = sortOrder == "Roll" ? "roll_desc" : "Roll";
            ViewBag.ClassSortParm = sortOrder == "Class" ? "class_desc" : "Class";
            ViewBag.SectionSortParm = sortOrder == "Section" ? "section_desc" : "Section";
            ViewBag.GroupSortParm = sortOrder == "Group" ? "group_desc" : "Group";

            var query = _context.Students
                .Include(s => s.Class)
                .Include(s => s.Section)
                .Include(s => s.Group)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(s => s.FullName.Contains(searchString) || s.RollNumber.Contains(searchString));
            }

            if (classId.HasValue)
            {
                query = query.Where(s => s.ClassId == classId.Value);
            }

            if (sectionId.HasValue)
            {
                query = query.Where(s => s.SectionId == sectionId.Value);
            }

            if (groupId.HasValue)
            {
                query = query.Where(s => s.GroupId == groupId.Value);
            }

            switch (sortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(s => s.FullName);
                    break;
                case "Roll":
                    query = query.OrderBy(s => s.RollNumber);
                    break;
                case "roll_desc":
                    query = query.OrderByDescending(s => s.RollNumber);
                    break;
                case "Class":
                    query = query.OrderBy(s => s.Class!.Name);
                    break;
                case "class_desc":
                    query = query.OrderByDescending(s => s.Class!.Name);
                    break;
                case "Section":
                    query = query.OrderBy(s => s.Section!.Name);
                    break;
                case "section_desc":
                    query = query.OrderByDescending(s => s.Section!.Name);
                    break;
                case "Group":
                    query = query.OrderBy(s => s.Group!.Name);
                    break;
                case "group_desc":
                    query = query.OrderByDescending(s => s.Group!.Name);
                    break;
                default:
                    query = query.OrderBy(s => s.FullName);
                    break;
            }

            ViewBag.Classes = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
            ViewBag.Sections = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
            ViewBag.Groups = new SelectList(await _context.AcademicGroups.ToListAsync(), "Id", "Name");
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentClassId = classId;
            ViewBag.CurrentSectionId = sectionId;
            ViewBag.CurrentGroupId = groupId;

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.ClassId = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
            ViewBag.SectionId = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
            ViewBag.GroupId = new SelectList(await _context.AcademicGroups.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student, IFormFile? photo)
        {
            var photoResult = await _photoService.SaveProfilePhotoAsync(photo);
            if (photoResult.Error is { } photoError)
                ModelState.AddModelError("photo", photoError);

            if (ModelState.IsValid)
            {
                student.Category = UserCategory.Student;
                student.QRCode = student.RollNumber;
                student.PhotoPath = photoResult.RelativePath;

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ClassId = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name", student.ClassId);
            ViewBag.SectionId = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name", student.SectionId);
            ViewBag.GroupId = new SelectList(await _context.AcademicGroups.ToListAsync(), "Id", "Name", student.GroupId);
            return View(student);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            
            ViewBag.ClassId = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name", student.ClassId);
            ViewBag.SectionId = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name", student.SectionId);
            ViewBag.GroupId = new SelectList(await _context.AcademicGroups.ToListAsync(), "Id", "Name", student.GroupId);
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student, IFormFile? photo)
        {
            if (id != student.Id) return NotFound();

            // PhotoPath is preserved from the database, not bound from the form.
            ModelState.Remove(nameof(student.PhotoPath));

            var photoResult = await _photoService.SaveProfilePhotoAsync(photo);
            if (photoResult.Error is { } photoError)
                ModelState.AddModelError("photo", photoError);

            if (!ModelState.IsValid)
            {
                ViewBag.ClassId = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name", student.ClassId);
                ViewBag.SectionId = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name", student.SectionId);
                ViewBag.GroupId = new SelectList(await _context.AcademicGroups.ToListAsync(), "Id", "Name", student.GroupId);
                return View(student);
            }

            var existing = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (existing == null) return NotFound();

            string? replacedPhotoPath = null;
            if (photoResult.RelativePath != null)
            {
                replacedPhotoPath = existing.PhotoPath;
                student.PhotoPath = photoResult.RelativePath;
            }
            else
            {
                student.PhotoPath = existing.PhotoPath;
            }

            try
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
                _photoService.DeletePhotoFile(replacedPhotoPath);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(student.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.FirstOrDefaultAsync(m => m.Id == id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                _photoService.DeletePhotoFile(student.PhotoPath);
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

            var students = await _context.Students.Where(s => ids.Contains(s.Id)).ToListAsync();
            foreach (var student in students)
            {
                student.PrintStatus = PrintStatus.SentToPrint;
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
