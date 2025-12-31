using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using medicare_pvt.Models;
using System.Security.Claims;

namespace medicare_pvt.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly MedicareContext _context;

        public AppointmentController(MedicareContext context)
        {
            _context = context;
        }

        // List appointments based on user role
        [Authorize]
        public async Task<IActionResult> Index()
        {
            IQueryable<Appointment> appointmentsQuery = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient);

            if (User.IsInRole("Patient"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient != null)
                {
                    appointmentsQuery = appointmentsQuery.Where(a => a.PatientId == patient.Id);
                }
            }
            else if (User.IsInRole("Doctor"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                {
                    appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == doctor.Id);
                }
            }

            var appointments = await appointmentsQuery
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            return View(appointments);
        }

        // Book appointment - Patient only
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Book(int? doctorId = null)
        {
            var doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

            ViewBag.Doctors = new SelectList(doctors, "Id", "Name", doctorId);
            ViewBag.SelectedDoctorId = doctorId;

            if (doctorId.HasValue)
            {
                var doctor = await _context.Doctors.FindAsync(doctorId.Value);
                ViewBag.SelectedDoctor = doctor;
            }

            return View(new AppointmentBookingViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Book(AppointmentBookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                var doctor = await _context.Doctors.FindAsync(model.DoctorId);

                if (patient == null || doctor == null)
                {
                    ModelState.AddModelError("", "Patient or Doctor not found.");
                    return View(model);
                }

                // Check for existing appointment at the same time
                var existingAppointment = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == model.DoctorId && 
                                  a.AppointmentDate.Date == model.AppointmentDate.Date &&
                                  a.AppointmentTime == model.AppointmentTime &&
                                  a.Status != "Cancelled");

                if (existingAppointment)
                {
                    ModelState.AddModelError("", "This time slot is already booked. Please choose another time.");
                    await LoadBookingViewData(model.DoctorId);
                    return View(model);
                }

                var appointment = new Appointment
                {
                    DoctorId = model.DoctorId,
                    PatientId = patient.Id,
                    AppointmentDate = model.AppointmentDate,
                    AppointmentTime = model.AppointmentTime,
                    Notes = model.Notes,
                    Status = "Pending",
                    Fee = doctor.ConsultationFee,
                    CreatedAt = DateTime.Now
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Appointment booked successfully! You will receive a confirmation shortly.";
                return RedirectToAction(nameof(Index));
            }

            await LoadBookingViewData(model.DoctorId);
            return View(model);
        }

        // View appointment details
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            // Check permissions
            if (User.IsInRole("Patient"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient?.Id != appointment.PatientId)
                {
                    return Forbid();
                }
            }
            else if (User.IsInRole("Doctor"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor?.Id != appointment.DoctorId)
                {
                    return Forbid();
                }
            }

            return View(appointment);
        }

        // Update appointment status - Doctor/Admin only
        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status, string notes = "")
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found." });
            }

            // Check if doctor owns this appointment
            if (User.IsInRole("Doctor"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor?.Id != appointment.DoctorId)
                {
                    return Json(new { success = false, message = "Unauthorized." });
                }
            }

            appointment.Status = status;
            appointment.UpdatedAt = DateTime.Now;
            
            if (!string.IsNullOrEmpty(notes))
            {
                appointment.Notes = notes;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Appointment status updated successfully." });
        }

        // Cancel appointment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Cancel(int id, string reason = "")
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found." });
            }

            // Check permissions
            bool canCancel = false;
            if (User.IsInRole("Admin"))
            {
                canCancel = true;
            }
            else if (User.IsInRole("Patient"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                canCancel = patient?.Id == appointment.PatientId;
            }
            else if (User.IsInRole("Doctor"))
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                canCancel = doctor?.Id == appointment.DoctorId;
            }

            if (!canCancel)
            {
                return Json(new { success = false, message = "Unauthorized to cancel this appointment." });
            }

            appointment.Status = "Cancelled";
            appointment.Notes = !string.IsNullOrEmpty(reason) 
                ? $"Cancelled: {reason}" 
                : "Cancelled";
            appointment.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Appointment cancelled successfully." });
        }

        // Get available time slots for a doctor on a specific date
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            // Get existing appointments for this doctor on this date
            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && 
                           a.AppointmentDate.Date == date.Date &&
                           a.Status != "Cancelled")
                .Select(a => a.AppointmentTime)
                .ToListAsync();

            // Generate available slots (9 AM to 5 PM, 30-minute intervals)
            var availableSlots = new List<string>();
            var startTime = new TimeSpan(9, 0, 0); // 9:00 AM
            var endTime = new TimeSpan(17, 0, 0);  // 5:00 PM
            var interval = TimeSpan.FromMinutes(30);

            for (var time = startTime; time < endTime; time = time.Add(interval))
            {
                if (!bookedSlots.Contains(time))
                {
                    availableSlots.Add(time.ToString(@"hh\:mm"));
                }
            }

            return Json(availableSlots);
        }

        // GET: Appointment/Create - Admin/Doctor only
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name");
            ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            try
            {
                // Check for existing appointment at the same time
                var existingAppointment = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == appointment.DoctorId && 
                                  a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                                  a.AppointmentTime == appointment.AppointmentTime &&
                                  a.Status != "Cancelled");

                if (existingAppointment)
                {
                    ModelState.AddModelError("", "This time slot is already booked. Please choose another time.");
                }

                if (ModelState.IsValid)
                {
                    appointment.CreatedAt = DateTime.Now;
                    appointment.Status = "Pending";
                    appointment.PaymentStatus = "Pending";

                    var doctor = await _context.Doctors.FindAsync(appointment.DoctorId);
                    if (doctor != null)
                    {
                        appointment.Fee = doctor.ConsultationFee;
                    }

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Appointment created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name", appointment.DoctorId);
                ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "Id", "Name", appointment.PatientId);
                return View(appointment);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating appointment: " + ex.Message;
                ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name");
                ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
                return View(appointment);
            }
        }

        // GET: Appointment/Edit
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name", appointment.DoctorId);
                ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "Id", "Name", appointment.PatientId);
                return View(appointment);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading appointment: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Appointment/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id)
            {
                TempData["Error"] = "Invalid appointment ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Check for conflicting appointments
                var conflictingAppointment = await _context.Appointments
                    .AnyAsync(a => a.Id != id &&
                                  a.DoctorId == appointment.DoctorId && 
                                  a.AppointmentDate.Date == appointment.AppointmentDate.Date &&
                                  a.AppointmentTime == appointment.AppointmentTime &&
                                  a.Status != "Cancelled");

                if (conflictingAppointment)
                {
                    ModelState.AddModelError("", "This time slot is already booked. Please choose another time.");
                }

                if (ModelState.IsValid)
                {
                    appointment.UpdatedAt = DateTime.Now;
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Appointment updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name", appointment.DoctorId);
                ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "Id", "Name", appointment.PatientId);
                return View(appointment);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await AppointmentExists(appointment.Id))
                {
                    TempData["Error"] = "Appointment no longer exists.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating appointment: " + ex.Message;
                ViewBag.Doctors = new SelectList(await _context.Doctors.Where(d => d.IsActive).ToListAsync(), "Id", "Name");
                ViewBag.Patients = new SelectList(await _context.Patients.Where(p => p.IsActive).ToListAsync(), "Id", "Name");
                return View(appointment);
            }
        }

        // GET: Appointment/Delete
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(appointment);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading appointment: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Appointment/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var appointment = await _context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    TempData["Error"] = "Appointment not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Appointment deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting appointment: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Rename View to Details for consistency
        [Authorize]
        public async Task<IActionResult> View(int id)
        {
            return await Details(id);
        }

        private async Task<bool> AppointmentExists(int id)
        {
            return await _context.Appointments.AnyAsync(e => e.Id == id);
        }

        private async Task LoadBookingViewData(int? selectedDoctorId = null)
        {
            var doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

            ViewBag.Doctors = new SelectList(doctors, "Id", "Name", selectedDoctorId);

            if (selectedDoctorId.HasValue)
            {
                var doctor = await _context.Doctors.FindAsync(selectedDoctorId.Value);
                ViewBag.SelectedDoctor = doctor;
            }
        }
    }

    // View Models
    public class AppointmentBookingViewModel
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);
        public TimeSpan AppointmentTime { get; set; }
        public string? Notes { get; set; }
    }
}
