using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_pvt.Models;

namespace medicare_pvt.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly MedicareContext _context;

        public PatientController(MedicareContext context)
        {
            _context = context;
        }

        // GET: Patient/Dashboard
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                
                if (user == null)
                    return RedirectToAction("Login", "Account");

                var patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == user.Id);

                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Get upcoming appointments
                ViewBag.UpcomingAppointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == patient.Id && 
                                a.AppointmentDate >= DateTime.Today &&
                                a.Status != "Cancelled")
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .Take(5)
                    .ToListAsync();

                // Get recent appointments
                ViewBag.RecentAppointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == patient.Id)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ThenByDescending(a => a.AppointmentTime)
                    .Take(5)
                    .ToListAsync();

                // Get statistics
                ViewBag.TotalAppointments = await _context.Appointments
                    .Where(a => a.PatientId == patient.Id)
                    .CountAsync();

                ViewBag.CompletedAppointments = await _context.Appointments
                    .Where(a => a.PatientId == patient.Id && a.Status == "Completed")
                    .CountAsync();

                ViewBag.PendingPayments = await _context.Appointments
                    .Where(a => a.PatientId == patient.Id && a.PaymentStatus == "Pending")
                    .CountAsync();

                return View(patient);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading dashboard: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Patient/Index
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var patients = await _context.Patients
                    .Include(p => p.User)
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
                return View(patients);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading patients: " + ex.Message;
                return View(new List<Patient>());
            }
        }

        // GET: Patient/View
        public async Task<IActionResult> View(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.Appointments)
                        .ThenInclude(a => a.Doctor)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (patient == null)
                {
                    TempData["Error"] = "Patient not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check authorization
                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user?.Role == "Patient")
                {
                    var currentPatient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (currentPatient?.Id != id)
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }
                }

                return View(patient);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading patient details: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Patient/Create
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patient/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create(Patient patient)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    patient.CreatedAt = DateTime.Now;
                    patient.IsActive = true;

                    _context.Patients.Add(patient);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Patient created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                return View(patient);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating patient: " + ex.Message;
                return View(patient);
            }
        }

        // GET: Patient/Edit
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null)
                {
                    TempData["Error"] = "Patient not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Check authorization
                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user?.Role == "Patient")
                {
                    var currentPatient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (currentPatient?.Id != id)
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }
                }

                return View(patient);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading patient: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Patient/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            if (id != patient.Id)
            {
                TempData["Error"] = "Invalid patient ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    patient.UpdatedAt = DateTime.Now;
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Patient updated successfully.";
                    
                    var userEmail = User.Identity?.Name;
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                    
                    if (user?.Role == "Patient")
                        return RedirectToAction("Dashboard");
                    
                    return RedirectToAction(nameof(Index));
                }

                return View(patient);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PatientExists(patient.Id))
                {
                    TempData["Error"] = "Patient no longer exists.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating patient: " + ex.Message;
                return View(patient);
            }
        }

        // GET: Patient/Delete
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (patient == null)
                {
                    TempData["Error"] = "Patient not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(patient);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading patient: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Patient/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null)
                {
                    TempData["Error"] = "Patient not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Soft delete
                patient.IsActive = false;
                patient.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Patient deactivated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deactivating patient: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<bool> PatientExists(int id)
        {
            return await _context.Patients.AnyAsync(e => e.Id == id);
        }
    }
}
