using Microsoft.AspNetCore.Mvc;
using IDCardBD.Web.Data;
using IDCardBD.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Authorization;
using IDCardBD.Web.Models;

namespace IDCardBD.Web.Controllers
{
    [Authorize]
    public class SectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SectionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sections = await _context.Sections.Include(s => s.Class).ToListAsync();
            return View(sections);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Classes = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Section section)
        {
            if (ModelState.IsValid)
            {
                _context.Add(section);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Classes = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name", section.ClassId);
            return View(section);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var section = await _context.Sections.FindAsync(id);
            if (section == null) return NotFound();

            ViewBag.Classes = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name", section.ClassId);
            return View(section);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Section section)
        {
            if (id != section.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(section);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SectionExists(section.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Classes = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name", section.ClassId);
            return View(section);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var section = await _context.Sections.Include(s => s.Class).FirstOrDefaultAsync(m => m.Id == id);
            if (section == null) return NotFound();
            return View(section);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var section = await _context.Sections.FindAsync(id);
            if (section != null)
            {
                _context.Sections.Remove(section);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionsByClass(int classId)
        {
            var sections = await _context.Sections
                .Where(s => s.ClassId == classId)
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();
            return Json(sections);
        }

        private bool SectionExists(int id)
        {
            return _context.Sections.Any(e => e.Id == id);
        }
    }
}
