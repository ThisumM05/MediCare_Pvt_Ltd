using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_pvt.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography;
using System.Text;

namespace medicare_pvt.Controllers
{
    public class AccountController : Controller
    {
        private readonly MedicareContext _context;

        public AccountController(MedicareContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Email is required.";
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ViewBag.Error = "Invalid email.";
                return View();
            }

            if (!user.IsActive)
            {
                ViewBag.Error = "Your account has been deactivated. Please contact support.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);

            // Redirect based on role
            return user.Role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Doctor" => RedirectToAction("Dashboard", "Doctor"),
                "Patient" => RedirectToAction("Dashboard", "Patient"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View(user);
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ViewBag.Error = "An account with this email already exists.";
                return View(user);
            }

            user.PasswordHash = HashPassword(password);
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create corresponding Doctor or Patient record
            if (user.Role == "Doctor")
            {
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Specialty = "General Medicine", // Default
                    ConsultationFee = 0,
                    ExperienceYears = 0,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.Doctors.Add(doctor);
            }
            else if (user.Role == "Patient")
            {
                var patient = new Patient
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.ContactNumber ?? "",
                    Birthdate = user.DateOfBirth,
                    Address = user.Address,
                    BloodType = user.BloodGroup,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _context.Patients.Add(patient);
            }

            await _context.SaveChangesAsync();

            ViewBag.Success = "Registration successful! Please login to continue.";
            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "MediCare_Salt_Key"));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var computedHash = HashPassword(password);
            return computedHash == hash;
        }
    }
}