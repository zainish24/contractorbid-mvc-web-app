using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ContractorDashboard.Data;
using ContractorDashboard.Models;
using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login (for both Contractors and Admins)
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login (handles both user types)
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // DEBUG: Add logging
            Console.WriteLine($"=== LOGIN ATTEMPT ===");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Password: {password}");

            // First check if it's an Admin
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
            Console.WriteLine($"Admin found: {admin != null}");

            if (admin != null)
            {
                var inputHash = HashPassword(password);
                Console.WriteLine($"Input hash: {inputHash}");
                Console.WriteLine($"Stored hash: {admin.PasswordHash}");
                Console.WriteLine($"Password match: {inputHash == admin.PasswordHash}");

                if (VerifyPassword(password, admin.PasswordHash))
                {
                    Console.WriteLine("ADMIN LOGIN SUCCESS!");

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                        new Claim(ClaimTypes.Email, admin.Email),
                        new Claim(ClaimTypes.Name, admin.Name),
                        new Claim(ClaimTypes.Role, "Admin")
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Dashboard", "Admin");
                }
            } // ← CLOSING BRACE ADDED HERE

            // If not admin, check if it's a Contractor
            var contractor = await _context.Contractors.FirstOrDefaultAsync(c => c.Email == email);
            Console.WriteLine($"Contractor found: {contractor != null}");

            if (contractor != null)
            {
                var inputHash = HashPassword(password);
                Console.WriteLine($"Contractor input hash: {inputHash}");
                Console.WriteLine($"Contractor stored hash: {contractor.PasswordHash}");
                Console.WriteLine($"Contractor password match: {inputHash == contractor.PasswordHash}");
            }

            if (contractor != null && VerifyPassword(password, contractor.PasswordHash))
            {
                Console.WriteLine("CONTRACTOR LOGIN SUCCESS!");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, contractor.Id.ToString()),
                    new Claim(ClaimTypes.Email, contractor.Email),
                    new Claim(ClaimTypes.Name, contractor.CompanyName),
                    new Claim(ClaimTypes.Role, "Contractor")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Dashboard");
            }

            Console.WriteLine("LOGIN FAILED - Invalid credentials");
            ViewBag.Error = "Invalid email or password";
            return View();
        }

        // GET: /Auth/Register (Only for Contractors)
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register (Only for Contractors)
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string companyName, string phone)
        {
            try
            {
                // Check if email exists in either Admins or Contractors
                var existingAdmin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
                var existingContractor = await _context.Contractors.FirstOrDefaultAsync(c => c.Email == email);

                if (existingAdmin != null || existingContractor != null)
                {
                    ViewBag.Error = "Email already exists";
                    return View();
                }

                var contractor = new Contractor
                {
                    Email = email,
                    PasswordHash = HashPassword(password),
                    CompanyName = companyName,
                    Phone = phone,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Contractors.Add(contractor);
                await _context.SaveChangesAsync();

                // Create default settings for contractor
                var settings = new ContractorSettings
                {
                    ContractorId = contractor.Id,
                    LaborRate = 45.00m,
                    MaterialMargin = 15.00m,
                    TravelCost = 25.00m,
                    ProfitMargin = 20.00m,
                    PreferredLocations = "Downtown,Suburban",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ContractorSettings.Add(settings);
                await _context.SaveChangesAsync();

                Console.WriteLine($"REGISTRATION SUCCESS for contractor: {companyName}");
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"REGISTRATION ERROR: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ViewBag.Error = "An error occurred during registration. Please try again.";
                return View();
            }
        }

        // POST: /Auth/Logout (for both)
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            var hash = HashPassword(password);
            return hash == storedHash;
        }
    }
}