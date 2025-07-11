using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZaiEats.Data;
using ZaiEats.Models;

namespace ZaiEats.Controllers
{
    public class QuotationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuotationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Public: Show form
        [HttpGet]
        public IActionResult Request()
        {
            return View();
        }

        // 2. Public: Handle form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(QuotationRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Status = "Pending";
            model.SubmittedAt = DateTime.UtcNow;

            _context.QuotationRequests.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success");
        }

        // 3. Public: Show success message
        public IActionResult Success()
        {
            return View();
        }

        // 4. Admin: View all requests
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var requests = await _context.QuotationRequests
                .OrderByDescending(q => q.SubmittedAt)
                .ToListAsync();
            return View(requests);
        }

        // 5. Admin: View request details
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.QuotationRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            return View(request);
        }

        // 6. Admin: Update status
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var request = await _context.QuotationRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id });
        }
    }
}
