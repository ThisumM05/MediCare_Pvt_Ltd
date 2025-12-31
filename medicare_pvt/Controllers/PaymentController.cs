using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using medicare_pvt.Models;

namespace medicare_pvt.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly MedicareContext _context;

        public PaymentController(MedicareContext context)
        {
            _context = context;
        }

        // GET: Payment
        public async Task<IActionResult> Index()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (user == null)
                    return RedirectToAction("Login", "Account");

                IQueryable<Payment> paymentsQuery = _context.Payments
                    .Include(p => p.Appointment)
                        .ThenInclude(a => a.Doctor)
                    .Include(p => p.Patient);

                if (user.Role == "Patient")
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (patient != null)
                        paymentsQuery = paymentsQuery.Where(p => p.PatientId == patient.Id);
                }
                else if (user.Role == "Doctor")
                {
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                    if (doctor != null)
                        paymentsQuery = paymentsQuery.Where(p => p.Appointment.DoctorId == doctor.Id);
                }

                var payments = await paymentsQuery.OrderByDescending(p => p.CreatedAt).ToListAsync();
                return View(payments);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading payments: " + ex.Message;
                return View(new List<Payment>());
            }
        }

        // GET: Payment/Create
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

                ViewBag.PendingAppointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == patient.Id && a.PaymentStatus == "Pending")
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();

                if (appointmentId.HasValue)
                {
                    var appointment = await _context.Appointments
                        .Include(a => a.Doctor)
                        .FirstOrDefaultAsync(a => a.Id == appointmentId.Value);
                    
                    if (appointment != null)
                    {
                        ViewBag.SelectedAppointment = appointment;
                        var payment = new Payment
                        {
                            AppointmentId = appointment.Id,
                            PatientId = patient.Id,
                            Amount = appointment.Fee ?? 0,
                            PaymentMethod = "Card"
                        };
                        return View(payment);
                    }
                }

                return View(new Payment { PatientId = patient.Id, PaymentMethod = "Card" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading payment form: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "PatientOnly")]
        public async Task<IActionResult> Create(Payment payment)
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

                payment.PatientId = patient.Id;
                payment.CreatedAt = DateTime.Now;
                payment.Status = "Pending";

                if (payment.Amount <= 0)
                {
                    ModelState.AddModelError("Amount", "Payment amount must be greater than zero.");
                }

                var appointment = await _context.Appointments.FindAsync(payment.AppointmentId);
                if (appointment == null)
                {
                    ModelState.AddModelError("AppointmentId", "Selected appointment not found.");
                }

                if (ModelState.IsValid)
                {
                    // Generate transaction ID
                    payment.TransactionId = "TXN" + DateTime.Now.ToString("yyyyMMddHHmmss") + payment.AppointmentId;
                    payment.Status = "Completed";
                    payment.CompletedAt = DateTime.Now;

                    _context.Payments.Add(payment);

                    // Update appointment payment status
                    if (appointment != null)
                    {
                        appointment.PaymentStatus = "Paid";
                        appointment.UpdatedAt = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Payment processed successfully!";
                    return RedirectToAction("Receipt", new { id = payment.Id });
                }

                ViewBag.PendingAppointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Where(a => a.PatientId == patient.Id && a.PaymentStatus == "Pending")
                    .ToListAsync();

                return View(payment);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error processing payment: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Payment/Receipt
        public async Task<IActionResult> Receipt(int id)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Appointment)
                        .ThenInclude(a => a.Doctor)
                    .Include(p => p.Patient)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                {
                    TempData["Error"] = "Payment not found.";
                    return RedirectToAction("Index");
                }

                var userEmail = User.Identity?.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

                // Check authorization
                if (user?.Role == "Patient")
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (patient?.Id != payment.PatientId)
                    {
                        TempData["Error"] = "Unauthorized access to payment receipt.";
                        return RedirectToAction("Index");
                    }
                }

                return View(payment);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading receipt: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Payment/Details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Appointment)
                        .ThenInclude(a => a.Doctor)
                    .Include(p => p.Patient)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (payment == null)
                {
                    TempData["Error"] = "Payment not found.";
                    return RedirectToAction("Index");
                }

                return View(payment);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading payment details: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
