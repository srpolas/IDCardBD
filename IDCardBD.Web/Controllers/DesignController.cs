using Microsoft.AspNetCore.Mvc;
using IDCardBD.Web.Models;
using IDCardBD.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace IDCardBD.Web.Controllers
{
    public class DesignController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DesignController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.CardTemplates.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CardTemplate model, IFormFile frontImage, IFormFile backImage)
        {
            ModelState.Remove(nameof(model.FrontBgPath));
            ModelState.Remove(nameof(model.BackBgPath));

            if (ModelState.IsValid)
            {
                if (frontImage != null && backImage != null)
                {
                    string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "templates");
                    if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                    string frontFileName = Guid.NewGuid().ToString() + Path.GetExtension(frontImage.FileName);
                    string backFileName = Guid.NewGuid().ToString() + Path.GetExtension(backImage.FileName);

                    using (var stream = new FileStream(Path.Combine(uploadDir, frontFileName), FileMode.Create))
                    {
                        await frontImage.CopyToAsync(stream);
                    }

                    using (var stream = new FileStream(Path.Combine(uploadDir, backFileName), FileMode.Create))
                    {
                        await backImage.CopyToAsync(stream);
                    }

                    model.FrontBgPath = "/uploads/templates/" + frontFileName;
                    model.BackBgPath = "/uploads/templates/" + backFileName;

                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Both Front and Back images are required.");
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var template = await _context.CardTemplates.FindAsync(id);
            if (template == null) return NotFound();

            // Optional: If logic requires only ONE active template at a time
            // we might want to deactivate others here.
            // For now, just toggle.
            
            template.IsActive = !template.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var template = await _context.CardTemplates.FindAsync(id);
            if (template == null) return NotFound();
            return View(template);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CardTemplate model, IFormFile frontImage, IFormFile backImage)
        {
            if (id != model.Id) return NotFound();

            ModelState.Remove(nameof(model.FrontBgPath));
            ModelState.Remove(nameof(model.BackBgPath));

            if (ModelState.IsValid)
            {
                try
                {
                    if (frontImage != null)
                    {
                         string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "templates");
                         if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                         
                         string frontFileName = Guid.NewGuid().ToString() + Path.GetExtension(frontImage.FileName);
                         using (var stream = new FileStream(Path.Combine(uploadDir, frontFileName), FileMode.Create))
                         {
                             await frontImage.CopyToAsync(stream);
                         }
                         model.FrontBgPath = "/uploads/templates/" + frontFileName;
                    }
                    else
                    {
                        // Keep existing path if not updating
                         var existing = await _context.CardTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                         if (existing != null) model.FrontBgPath = existing.FrontBgPath;
                    }

                    if (backImage != null)
                    {
                         string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "templates");
                         if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                         string backFileName = Guid.NewGuid().ToString() + Path.GetExtension(backImage.FileName);
                         using (var stream = new FileStream(Path.Combine(uploadDir, backFileName), FileMode.Create))
                         {
                             await backImage.CopyToAsync(stream);
                         }
                         model.BackBgPath = "/uploads/templates/" + backFileName;
                    }
                    else
                    {
                        // Keep existing path if not updating
                         var existing = await _context.CardTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                         // Note: Double query here is inefficient but safe for now. 
                         // Better to query once before checks.
                         if (existing != null && string.IsNullOrEmpty(model.BackBgPath)) model.BackBgPath = existing.BackBgPath;
                         else if (existing != null) model.BackBgPath = existing.BackBgPath; // Logic simplification
                    }

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CardTemplateExists(model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var template = await _context.CardTemplates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (template == null) return NotFound();

            return View(template);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var template = await _context.CardTemplates.FindAsync(id);
            if (template != null)
            {
                // Delete physical files
                if (!string.IsNullOrEmpty(template.FrontBgPath))
                {
                    var frontPath = Path.Combine(_environment.WebRootPath, template.FrontBgPath.TrimStart('/'));
                    if (System.IO.File.Exists(frontPath)) System.IO.File.Exists(frontPath); // System.IO.File.Delete(frontPath)
                    
                    // Fixed: actually use Delete
                    try { if (System.IO.File.Exists(frontPath)) System.IO.File.Delete(frontPath); } catch { }
                }

                if (!string.IsNullOrEmpty(template.BackBgPath))
                {
                    var backPath = Path.Combine(_environment.WebRootPath, template.BackBgPath.TrimStart('/'));
                    try { if (System.IO.File.Exists(backPath)) System.IO.File.Delete(backPath); } catch { }
                }

                _context.CardTemplates.Remove(template);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CardTemplateExists(int id)
        {
            return _context.CardTemplates.Any(e => e.Id == id);
        }
    }
}
