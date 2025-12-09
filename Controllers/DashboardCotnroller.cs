using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using System.Security.Claims;
using ContractorDashboard.Data;
using ContractorDashboard.Models;

namespace WebApplication1.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.IsInRole("Contractor"))
                return RedirectToAction("AccessDenied", "Auth");

            var contractorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(contractorId))
                return RedirectToAction("Login", "Auth");

            var contractor = await _context.Contractors
                .Include(c => c.Settings)
                .Include(c => c.Bids)
                    .ThenInclude(b => b.Job)
                .FirstOrDefaultAsync(c => c.Id == int.Parse(contractorId));

            return View(contractor);
        }

        public async Task<IActionResult> Settings()
        {
            var contractorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(contractorId))
                return RedirectToAction("Login", "Auth");

            var settings = await _context.ContractorSettings
                .FirstOrDefaultAsync(s => s.ContractorId == int.Parse(contractorId));

            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSettings(ContractorSettings model)
        {
            var contractorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(contractorId))
                return RedirectToAction("Login", "Auth");

            var settings = await _context.ContractorSettings
                .FirstOrDefaultAsync(s => s.ContractorId == int.Parse(contractorId));

            if (settings != null)
            {
                settings.LaborRate = model.LaborRate;
                settings.MaterialMargin = model.MaterialMargin;
                settings.TravelCost = model.TravelCost;
                settings.ProfitMargin = model.ProfitMargin;
                settings.PreferredLocations = model.PreferredLocations;
                settings.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Settings updated successfully!";
            }

            return RedirectToAction("Settings");
        }
    }
}