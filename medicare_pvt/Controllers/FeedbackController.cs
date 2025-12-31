using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_pvt.Models;

namespace medicare_pvt.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly MedicareContext _context;

        public FeedbackController(MedicareContext context)
        {
            _context = context;
        }

        // GET: Feedback
        public async Task<IActionResult> Index()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                    return RedirectToAction("Login", "Account");

                IQueryable<Feedback> feedbacksQuery = _context.Feedbacks
                    .Include(f => f.Patient)
                    .Include(f => f.Doctor)
                    .Include(f => f.Appointment);

                if (user.Role == "Patient")
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (patient != null)
                        feedbacksQuery = feedbacksQuery.Where(f => f.PatientId == patient.Id);
                }
                else if (user.Role == "Doctor")
                {
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                    if (doctor != null)
                        feedbacksQuery = feedbacksQuery.Where(f => f.DoctorId == doctor.Id);
                }

                var feedbacks = await feedbacksQuery.OrderByDescending(f => f.CreatedDate).ToListAsync();
                return View(feedbacks);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading feedbacks: " + ex.Message;
                return View(new List<Feedback>());
            }
        }

        // GET: Feedback/Create
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> Create(int? appointmentId)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user!.Id);

                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found.";
                    return RedirectToAction("Index");
                }

                ViewBag.Doctors = await _context.Doctors
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                ViewBag.PatientAppointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == patient.Id && a.Status == "Completed")
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToListAsync();

                if (appointmentId.HasValue)
                {
                    var appointment = await _context.Appointments.FindAsync(appointmentId.Value);
                    if (appointment != null)
                    {
                        ViewBag.SelectedAppointmentId = appointmentId.Value;
                        ViewBag.SelectedDoctorId = appointment.DoctorId;
                    }
                }

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading feedback form: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user!.Id);

                if (patient == null)
                {
                    TempData["Error"] = "Patient profile not found.";
                    return RedirectToAction("Index");
                }

                feedback.PatientId = patient.Id;
                feedback.CreatedDate = DateTime.Now;
                feedback.IsApproved = false;

                if (feedback.Rating < 1 || feedback.Rating > 5)
                {
                    ModelState.AddModelError("Rating", "Rating must be between 1 and 5.");
                }

                if (ModelState.IsValid)
                {
                    _context.Feedbacks.Add(feedback);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Feedback submitted successfully! It will be visible after admin approval.";
                    return RedirectToAction("Index");
                }

                ViewBag.Doctors = await _context.Doctors.Where(d => d.IsActive).ToListAsync();
                ViewBag.PatientAppointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == patient.Id && a.Status == "Completed")
                    .ToListAsync();

                return View(feedback);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error submitting feedback: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Feedback/Approve (Admin/Doctor)
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> Approve()
        {
            try
            {
                var pendingFeedbacks = await _context.Feedbacks
                    .Include(f => f.Patient)
                    .Include(f => f.Doctor)
                    .Where(f => !f.IsApproved)
                    .OrderByDescending(f => f.CreatedDate)
                    .ToListAsync();

                return View(pendingFeedbacks);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading pending feedbacks: " + ex.Message;
                return View(new List<Feedback>());
            }
        }

        // POST: Feedback/ApproveItem
        [HttpPost]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> ApproveItem(int id)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback != null)
                {
                    feedback.IsApproved = true;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Feedback approved successfully.";
                }
                else
                {
                    TempData["Error"] = "Feedback not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error approving feedback: " + ex.Message;
            }

            return RedirectToAction("Approve");
        }

        // POST: Feedback/Delete
        [HttpPost]
        [Authorize(Policy = "DoctorOrAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback != null)
                {
                    _context.Feedbacks.Remove(feedback);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Feedback deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Feedback not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting feedback: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
