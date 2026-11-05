using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var customers = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                customers = customers.Where(c =>
                    c.CustomerName.Contains(search) ||
                    c.Email.Contains(search) ||
                    (c.CompanyName != null &&
                     c.CompanyName.Contains(search)));
            }

            ViewBag.Search = search;

            return View(await customers
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (await _context.Customers.AnyAsync(c =>
                c.Email == customer.Email ||
                c.Phone == customer.Phone))
            {
                ModelState.AddModelError("Email",
                    "Email or phone number already exists.");
            }

            if (ModelState.IsValid)
            {
                customer.CustomerCode =
                    "CUS-" + Guid.NewGuid().ToString("N")[..8].ToUpper();

                customer.CreatedBy = User.Identity?.Name;
                customer.OwnerId = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Customer added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.CustomerId)
                return NotFound();

            var existing = await _context.Customers.FindAsync(id);

            if (existing == null)
                return NotFound();

            if (await _context.Customers.AnyAsync(c =>
                c.CustomerId != id &&
                (c.Email == customer.Email || c.Phone == customer.Phone)))
            {
                ModelState.AddModelError("Email",
                    "Email or phone number already exists.");
            }

            if (ModelState.IsValid)
            {
                existing.CustomerName = customer.CustomerName;
                existing.Email = customer.Email;
                existing.Phone = customer.Phone;
                existing.CompanyName = customer.CompanyName;
                existing.Address = customer.Address;
                existing.City = customer.City;
                existing.State = customer.State;
                existing.Status = customer.Status;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}