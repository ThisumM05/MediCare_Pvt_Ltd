using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using medicare_pvt.Models;

namespace medicare_pvt.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly MedicareContext _context;

        public AdminController(MedicareContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var stats = new AdminDashboardViewModel
            {
                TotalDoctors = await _context.Doctors.CountAsync(d => d.IsActive),
                TotalPatients = await _context.Patients.CountAsync(p => p.IsActive),
                TotalAppointments = await _context.Appointments.CountAsync(),
                TodayAppointments = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == DateTime.Today),
                PendingAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == "Pending"),
                CompletedAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == "Completed"),
                TotalRevenue = await _context.Payments
                    .Where(p => p.Status == "Completed")
                    .SumAsync(p => p.Amount),
                MonthlyRevenue = await _context.Payments
                    .Where(p => p.Status == "Completed" && 
                               p.CreatedAt.Month == DateTime.Now.Month &&
                               p.CreatedAt.Year == DateTime.Now.Year)
                    .SumAsync(p => p.Amount)
            };

            // Recent appointments
            stats.RecentAppointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(stats);
        }

        public async Task<IActionResult> ManageDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsActive)
                .ToListAsync();
            return View(doctors);
        }

        public async Task<IActionResult> ManagePatients()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .Where(p => p.IsActive)
                .ToListAsync();
            return View(patients);
        }

        public async Task<IActionResult> Reports()
        {
            var reportData = new ReportsViewModel
            {
                MonthlyAppointments = await GetMonthlyAppointmentStats(),
                SpecialtyStats = await GetSpecialtyStats(),
                RevenueStats = await GetRevenueStats()
            };
            return View(reportData);
        }

        private async Task<List<MonthlyStats>> GetMonthlyAppointmentStats()
        {
            return await _context.Appointments
                .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                .Select(g => new MonthlyStats
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(s => s.Year).ThenBy(s => s.Month)
                .Take(12)
                .ToListAsync();
        }

        private async Task<List<SpecialtyStats>> GetSpecialtyStats()
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .GroupBy(a => a.Doctor.Specialty)
                .Select(g => new SpecialtyStats
                {
                    Specialty = g.Key,
                    AppointmentCount = g.Count(),
                    DoctorCount = g.Select(a => a.DoctorId).Distinct().Count()
                })
                .OrderByDescending(s => s.AppointmentCount)
                .ToListAsync();
        }

        private async Task<List<MonthlyRevenue>> GetRevenueStats()
        {
            return await _context.Payments
                .Where(p => p.Status == "Completed")
                .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
                .Select(g => new MonthlyRevenue
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(p => p.Amount)
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .Take(12)
                .ToListAsync();
        }
    }

    // View Models
    public class AdminDashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new List<Appointment>();
    }

    public class ReportsViewModel
    {
        public List<MonthlyStats> MonthlyAppointments { get; set; } = new List<MonthlyStats>();
        public List<SpecialtyStats> SpecialtyStats { get; set; } = new List<SpecialtyStats>();
        public List<MonthlyRevenue> RevenueStats { get; set; } = new List<MonthlyRevenue>();
    }

    public class MonthlyStats
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Count { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }

    public class SpecialtyStats
    {
        public string Specialty { get; set; } = null!;
        public int AppointmentCount { get; set; }
        public int DoctorCount { get; set; }
    }

    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Amount { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    }
}