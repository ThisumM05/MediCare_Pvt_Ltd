using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using medicare_pvt.Models;
using System.Security.Claims;

namespace medicare_pvt.Controllers
{
    public class DoctorController : Controller
    {
        private readonly MedicareContext _context;

        public DoctorController(MedicareContext context)
        {
            _context = context;
        }

        // Public doctor listing for patients and guests
        public async Task<IActionResult> Index(string specialty = "", string search = "")
        {
            var doctorsQuery = _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsActive);

            if (!string.IsNullOrEmpty(specialty))
            {
                doctorsQuery = doctorsQuery.Where(d => d.Specialty.Contains(specialty));
            }

            if (!string.IsNullOrEmpty(search))
            {
                doctorsQuery = doctorsQuery.Where(d => 
                    d.Name.Contains(search) || 
                    d.Specialty.Contains(search) ||
                    (d.Qualifications != null && d.Qualifications.Contains(search)));
            }

            var doctors = await doctorsQuery.OrderBy(d => d.Name).ToListAsync();

            ViewBag.Specialties = await _context.Doctors
                .Where(d => d.IsActive)
                .Select(d => d.Specialty)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            ViewBag.CurrentSpecialty = specialty;
            ViewBag.CurrentSearch = search;

            return View(doctors);
        }

        // Doctor Dashboard
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return NotFound("Doctor profile not found.");
            }

            ViewBag.Doctor = doctor;
            ViewBag.TodayAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == DateTime.Today)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            ViewBag.UpcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id && 
                           a.AppointmentDate.Date > DateTime.Today &&
                           a.Status != "Cancelled")
                .OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalPatients = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .Select(a => a.PatientId)
                .Distinct()
                .CountAsync();

            ViewBag.TodayCount = ((List<Appointment>)ViewBag.TodayAppointments).Count;

            ViewBag.PendingAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id && a.Status == "Pending")
                .CountAsync();

            var feedbacks = await _context.Feedbacks
                .Where(f => f.DoctorId == doctor.Id && f.IsApproved)
                .ToListAsync();

            ViewBag.AverageRating = feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0;

            ViewBag.RecentFeedback = await _context.Feedbacks
                .Include(f => f.Patient)
                .Where(f => f.DoctorId == doctor.Id && f.IsApproved)
                .OrderByDescending(f => f.CreatedDate)
                .Take(3)
                .ToListAsync();

            return View();
        }

        // Doctor public profile
        public async Task<IActionResult> Profile(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsActive);

            if (doctor == null)
            {
                return NotFound();
            }

            var feedbacks = await _context.Feedbacks
                .Include(f => f.Patient)
                .Where(f => f.DoctorId == id && f.IsApproved)
                .OrderByDescending(f => f.CreatedDate)
                .ToListAsync();

            ViewBag.AverageRating = feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0;
            ViewBag.Feedbacks = feedbacks;

            return View(doctor);
        }

        // Admin/Doctor actions
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                doctor.CreatedAt = DateTime.Now;
                doctor.IsActive = true;
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Doctor added successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            // Doctors can only edit their own profile
            if (User.IsInRole("Doctor"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (doctor.UserId != userId)
                {
                    return Forbid();
                }
            }

            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Edit(int id, Doctor doctor)
        {
            if (id != doctor.Id) return NotFound();

            // Doctors can only edit their own profile
            if (User.IsInRole("Doctor"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (doctor.UserId != userId)
                {
                    return Forbid();
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    doctor.UpdatedAt = DateTime.Now;
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Profile updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                
                return User.IsInRole("Doctor") 
                    ? RedirectToAction(nameof(Dashboard))
                    : RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);
            
            if (doctor == null) return NotFound();
            return View(doctor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                doctor.IsActive = false; // Soft delete
                doctor.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Doctor deactivated successfully!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }
    }

    // View Models
    public class DoctorDashboardViewModel
    {
        public Doctor Doctor { get; set; } = null!;
        public List<Appointment> TodayAppointments { get; set; } = new List<Appointment>();
        public List<Appointment> UpcomingAppointments { get; set; } = new List<Appointment>();
        public int TotalPatients { get; set; }
        public int MonthlyAppointments { get; set; }
        public int PendingAppointments { get; set; }
    }

    public class DoctorProfileViewModel
    {
        public Doctor Doctor { get; set; } = null!;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<Feedback> RecentFeedbacks { get; set; } = new List<Feedback>();
    }
}
