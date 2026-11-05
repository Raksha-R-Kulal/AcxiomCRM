using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class LeadsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LeadsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? status)
        {
            var leads = _context.Leads.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                leads = leads.Where(l =>
                    l.LeadName.Contains(search) ||
                    l.Email.Contains(search) ||
                    (l.CompanyName != null &&
                     l.CompanyName.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                leads = leads.Where(l => l.Status == status);

            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(await leads
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Lead lead)
        {
            if (ModelState.IsValid)
            {
                lead.LeadCode = "LEAD-" +
                    Guid.NewGuid().ToString("N")[..8].ToUpper();

                lead.OwnerId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                _context.Leads.Add(lead);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Lead added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(lead);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var lead = await _context.Leads.FindAsync(id);

            if (lead == null)
                return NotFound();

            return View(lead);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lead lead)
        {
            if (id != lead.LeadId)
                return NotFound();

            var existing = await _context.Leads.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                existing.LeadName = lead.LeadName;
                existing.Email = lead.Email;
                existing.Phone = lead.Phone;
                existing.CompanyName = lead.CompanyName;
                existing.Source = lead.Source;
                existing.Status = lead.Status;
                existing.ExpectedValue = lead.ExpectedValue;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Lead updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(lead);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var lead = await _context.Leads.FindAsync(id);

            if (lead == null)
                return NotFound();

            return View(lead);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lead = await _context.Leads.FindAsync(id);

            if (lead != null)
            {
                _context.Leads.Remove(lead);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Lead deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}