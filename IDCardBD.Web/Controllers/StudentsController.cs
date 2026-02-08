using Microsoft.AspNetCore.Mvc;
using IDCardBD.Web.Data;
using IDCardBD.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace IDCardBD.Web.Controllers
{
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public StudentsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Students.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student, IFormFile photo)
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
                     student.PhotoPath = "/uploads/profiles/" + fileName;
                }

                student.Category = UserCategory.Student;
                // Generate logic for QR code if needed
                student.QRCode = student.RollNumber; 
                
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student, IFormFile photo)
        {
            if (id != student.Id) return NotFound();
            
            // Remove PhotoPath from model state validation
            ModelState.Remove(nameof(student.PhotoPath));

            if (ModelState.IsValid)
            {
                try
                {
                     // Retrieve existing entity to keep existing photo if new one is not provided, 
                     // OR we just update the properties. But EF Core tracking might overwrite.
                     // Better approach: Get AsNoTracking or detach, but here simpler:
                     // We need to fetch existing photo path if photo is null.
                     
                     if (photo != null)
                     {
                         string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                         if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                         
                         string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                         using (var stream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create))
                         {
                             await photo.CopyToAsync(stream);
                         }
                         student.PhotoPath = "/uploads/profiles/" + fileName;
                     }
                     else 
                     {
                         // Keep existing photo path. Since 'student' is bound from form, 
                         // we need a hidden input for PhotoPath in the view, OR fetch DB value.
                         // Let's rely on hidden input or fetch. 
                         // If we rely on fetch:
                         var existing = await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                         if (existing != null) student.PhotoPath = existing.PhotoPath;
                     }

                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
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
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
