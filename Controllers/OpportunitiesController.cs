using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class OpportunitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OpportunitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all opportunities
        public async Task<IActionResult> Index(string? search, string? stage)
        {
            var opportunities = _context.Opportunities
                .Include(o => o.Customer)
                .Include(o => o.Lead)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                opportunities = opportunities.Where(o =>
                    o.OpportunityName.Contains(search) ||
                    o.Customer!.CustomerName.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(stage))
            {
                opportunities = opportunities.Where(o => o.Stage == stage);
            }

            ViewBag.Search = search;
            ViewBag.Stage = stage;

            return View(await opportunities
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync());
        }

        // Load dropdowns
        private async Task LoadDropdowns()
        {
            ViewBag.Customers = new SelectList(
                await _context.Customers.ToListAsync(),
                "CustomerId", "CustomerName");

            ViewBag.Leads = new SelectList(
                await _context.Leads.ToListAsync(),
                "LeadId", "LeadName");
        }

        // Create page
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        // Save opportunity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Opportunity opportunity)
        {
            if (opportunity.ExpectedCloseDate.Date < DateTime.Today &&
                opportunity.Stage != "Won" &&
                opportunity.Stage != "Lost")
            {
                ModelState.AddModelError("ExpectedCloseDate",
                    "Expected closing date cannot be in the past.");
            }

            if (ModelState.IsValid)
            {
                opportunity.OwnerId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                _context.Opportunities.Add(opportunity);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Opportunity created successfully!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns();
            return View(opportunity);
        }

        // Edit page
        public async Task<IActionResult> Edit(int id)
        {
            var opportunity = await _context.Opportunities.FindAsync(id);

            if (opportunity == null)
                return NotFound();

            await LoadDropdowns();
            return View(opportunity);
        }

        // Update opportunity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Opportunity opportunity)
        {
            if (id != opportunity.OpportunityId)
                return NotFound();

            var existing = await _context.Opportunities.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (opportunity.ExpectedCloseDate.Date < DateTime.Today &&
                opportunity.Stage != "Won" &&
                opportunity.Stage != "Lost")
            {
                ModelState.AddModelError("ExpectedCloseDate",
                    "Expected closing date cannot be in the past.");
            }

            if (ModelState.IsValid)
            {
                existing.OpportunityName = opportunity.OpportunityName;
                existing.CustomerId = opportunity.CustomerId;
                existing.LeadId = opportunity.LeadId;
                existing.DealAmount = opportunity.DealAmount;
                existing.Probability = opportunity.Probability;
                existing.ExpectedCloseDate = opportunity.ExpectedCloseDate;
                existing.Stage = opportunity.Stage;
                existing.Notes = opportunity.Notes;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Opportunity updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns();
            return View(opportunity);
        }

        // Delete confirmation page
        public async Task<IActionResult> Delete(int id)
        {
            var opportunity = await _context.Opportunities
                .Include(o => o.Customer)
                .Include(o => o.Lead)
                .FirstOrDefaultAsync(o => o.OpportunityId == id);

            if (opportunity == null)
                return NotFound();

            return View(opportunity);
        }

        // Delete opportunity
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var opportunity = await _context.Opportunities.FindAsync(id);

            if (opportunity != null)
            {
                _context.Opportunities.Remove(opportunity);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Opportunity deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}