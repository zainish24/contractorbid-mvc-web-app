using ContractorDashboard.Data;
using ContractorDashboard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using ContractorDashboard.ViewModels;
namespace WebApplication1.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            var stats = new AdminDashboardStats
            {
                TotalContractors = await _context.Contractors.CountAsync(),
                TotalJobs = await _context.Jobs.CountAsync(),
                TotalBids = await _context.Bids.CountAsync(),
                PendingJobs = await _context.Jobs.CountAsync(j => j.Status == "Active"),
                AcceptedBids = await _context.Bids.CountAsync(b => b.Status == "Accepted"),
                PendingBids = await _context.Bids.CountAsync(b => b.Status == "Submitted"),
                RejectedBids = await _context.Bids.CountAsync(b => b.Status == "Rejected"),
                UnderReviewBids = await _context.Bids.CountAsync(b => b.Status == "UnderReview")
            };

            var recentJobs = await _context.Jobs
                .OrderByDescending(j => j.PostedDate)
                .Take(5)
                .ToListAsync();

            var recentBids = await _context.Bids
                .Include(b => b.Contractor)
                .Include(b => b.Job)
                .OrderByDescending(b => b.SubmittedDate)
                .Take(5)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                Stats = stats,
                RecentJobs = recentJobs,
                RecentBids = recentBids
            };

            return View(viewModel);
        }

        // GET: /Admin/Jobs
        public async Task<IActionResult> Jobs()
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            var jobs = await _context.Jobs
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            return View(jobs);
        }

        // GET: /Admin/AddJob
        public IActionResult AddJob()
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            return View();
        }

        // POST: /Admin/AddJob
        [HttpPost]
        public async Task<IActionResult> AddJob(Job job)
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            if (ModelState.IsValid)
            {
                job.PostedDate = DateTime.UtcNow;
                job.Status = "Active";

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Job added successfully!";
                return RedirectToAction("Jobs");
            }

            return View(job);
        }

        // GET: /Admin/EditJob/{id}
        public async Task<IActionResult> EditJob(int id)
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
                return NotFound();

            return View(job);
        }

        // POST: /Admin/EditJob
        [HttpPost]
        public async Task<IActionResult> EditJob(Job job)
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            if (ModelState.IsValid)
            {
                _context.Jobs.Update(job);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Job updated successfully!";
                return RedirectToAction("Jobs");
            }

            return View(job);
        }

        // POST: /Admin/DeleteJob/{id}
        [HttpPost]
        public async Task<IActionResult> DeleteJob(int id)
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Job deleted successfully!";
            }

            return RedirectToAction("Jobs");
        }

        // GET: /Admin/Contractors
        public async Task<IActionResult> Contractors()
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            var contractors = await _context.Contractors
                .Include(c => c.Bids)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(contractors);
        }

        // GET: /Admin/Bids
        public async Task<IActionResult> Bids()
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            var bids = await _context.Bids
                .Include(b => b.Contractor)
                .Include(b => b.Job)
                .OrderByDescending(b => b.SubmittedDate)
                .ToListAsync();

            return View(bids);
        }

        // POST: /Admin/AcceptBid/{id}
        [HttpPost]
        public async Task<IActionResult> AcceptBid(int id)
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                var bid = await _context.Bids
                    .Include(b => b.Job)
                    .Include(b => b.Contractor)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (bid == null)
                {
                    return Json(new { success = false, message = "Bid not found." });
                }

                // Reject all other bids for this job
                var otherBids = await _context.Bids
                    .Where(b => b.JobId == bid.JobId && b.Id != id)
                    .ToListAsync();

                foreach (var otherBid in otherBids)
                {
                    otherBid.Status = "Rejected";
                }

                // Accept the selected bid
                bid.Status = "Accepted";

                // Close the job
                if (bid.Job != null)
                {
                    bid.Job.Status = "Awarded";
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Bid accepted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error accepting bid: " + ex.Message });
            }
        }

        // POST: /Admin/RejectBid/{id}
        [HttpPost]
        public async Task<IActionResult> RejectBid(int id)
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                var bid = await _context.Bids.FindAsync(id);
                if (bid == null)
                {
                    return Json(new { success = false, message = "Bid not found." });
                }

                bid.Status = "Rejected";
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Bid rejected successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error rejecting bid: " + ex.Message });
            }
        }

        // POST: /Admin/UpdateBidStatus/{id}
        [HttpPost]
        public async Task<IActionResult> UpdateBidStatus(int id, string status)
        {
            if (!User.IsInRole("Admin"))
                return RedirectToAction("AccessDenied", "Auth");

            try
            {
                var bid = await _context.Bids.FindAsync(id);
                if (bid == null)
                {
                    return Json(new { success = false, message = "Bid not found." });
                }

                bid.Status = status;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Bid status updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating bid status: " + ex.Message });
            }
        }

        // POST: /Admin/DeleteBid/{id}
        [HttpPost]
        public async Task<IActionResult> DeleteBid(int id)
        {
            if (!User.IsInRole("Admin"))
                return Json(new { success = false, message = "Access denied." });

            try
            {
                var bid = await _context.Bids.FindAsync(id);
                if (bid == null)
                {
                    return Json(new { success = false, message = "Bid not found." });
                }

                _context.Bids.Remove(bid);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Bid deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting bid: " + ex.Message });
            }
        }
    }
}