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
    public class AcademicGroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AcademicGroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var groups = await _context.AcademicGroups.Include(g => g.Class).ToListAsync();
            return View(groups);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Classes = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AcademicGroup group)
        {
            if (ModelState.IsValid)
            {
                _context.Add(group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Classes = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name", group.ClassId);
            return View(group);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var group = await _context.AcademicGroups.FindAsync(id);
            if (group == null) return NotFound();

            ViewBag.Classes = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name", group.ClassId);
            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AcademicGroup group)
        {
            if (id != group.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(group);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AcademicGroupExists(group.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Classes = new SelectList(await _context.Classes.ToListAsync(), "Id", "Name", group.ClassId);
            return View(group);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var group = await _context.AcademicGroups.Include(g => g.Class).FirstOrDefaultAsync(m => m.Id == id);
            if (group == null) return NotFound();
            return View(group);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _context.AcademicGroups.FindAsync(id);
            if (group != null)
            {
                _context.AcademicGroups.Remove(group);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupsByClass(int classId)
        {
            var groups = await _context.AcademicGroups
                .Where(g => g.ClassId == classId)
                .Select(g => new { g.Id, g.Name })
                .ToListAsync();
            return Json(groups);
        }

        private bool AcademicGroupExists(int id)
        {
            return _context.AcademicGroups.Any(e => e.Id == id);
        }
    }
}
