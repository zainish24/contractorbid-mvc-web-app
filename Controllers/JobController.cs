using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using System.Security.Claims;
using ContractorDashboard.Data;
using ContractorDashboard.Models;

namespace WebApplication1.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search = "")
        {
            var query = _context.Jobs
                .Where(j => j.Status == "Active" || j.Status == "Open");

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(j =>
                    j.Title.ToLower().Contains(search) ||
                    j.Description.ToLower().Contains(search) ||
                    j.Location.ToLower().Contains(search) ||
                    j.JobType.ToLower().Contains(search) ||
                    j.RequiredMaterials.ToLower().Contains(search)
                );
            }

            var jobs = await query.ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_JobListPartial", jobs);
            }

            return View(jobs);
        }

        public async Task<IActionResult> Details(int id)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
                return NotFound();

            return View(job);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitBid(int jobId, decimal bidAmount, string notes)
        {
            try
            {
                var contractorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(contractorId))
                {
                    return Json(new { success = false, message = "Please login to submit a bid" });
                }

                // Check if contractor already bid on this job
                var existingBid = await _context.Bids
                    .FirstOrDefaultAsync(b => b.JobId == jobId && b.ContractorId == int.Parse(contractorId));

                if (existingBid != null)
                {
                    return Json(new { success = false, message = "You have already submitted a bid for this job" });
                }

                var bid = new Bid
                {
                    ContractorId = int.Parse(contractorId),
                    JobId = jobId,
                    BidAmount = bidAmount,
                    Notes = notes,
                    Status = "Submitted",
                    SubmittedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.Bids.Add(bid);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Bid submitted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error submitting bid: " + ex.Message });
            }
        }

        // New method to get job details for bid calculation
        [HttpGet]
        public async Task<IActionResult> GetJobDetails(int id)
        {
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                job = new
                {
                    title = job.Title,
                    budgetRange = job.BudgetRange,
                    estimatedHours = job.EstimatedHours,
                    requiredMaterials = job.RequiredMaterials
                }
            });
        }

        // API endpoint for search suggestions
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new List<string>());

            var suggestions = await _context.Jobs
                .Where(j => (j.Status == "Active" || j.Status == "Open") &&
                           (j.Title.ToLower().Contains(term.ToLower()) ||
                            j.Location.ToLower().Contains(term.ToLower()) ||
                            j.JobType.ToLower().Contains(term.ToLower())))
                .Select(j => new {
                    j.Title,
                    j.Location,
                    j.JobType
                })
                .Distinct()
                .Take(10)
                .ToListAsync();

            var results = new List<string>();
            foreach (var item in suggestions)
            {
                if (item.Title.ToLower().Contains(term.ToLower()))
                    results.Add(item.Title);
                if (item.Location.ToLower().Contains(term.ToLower()))
                    results.Add(item.Location);
                if (item.JobType.ToLower().Contains(term.ToLower()))
                    results.Add(item.JobType);
            }

            return Json(results.Distinct().Take(8));
        }
    }
}