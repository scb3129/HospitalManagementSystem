using HospitalManagementSystemAPI.Data;
using HospitalManagementSystemAPI.DTOs;
using HospitalManagementSystemAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetAppointments()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Billing)
                .Select(a => new AppointmentDTO
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    PatientId = a.PatientId,
                    PatientName = a.Patient != null ? a.Patient.Name : "Unknown",
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor != null ? a.Doctor.Name : "Unknown",
                    BillingId = a.Billing != null ? a.Billing.BillingId : (int?)null,
                    TotalAmount = a.Billing != null ? a.Billing.TotalAmount : (decimal?)null,
                    BillingStatus = a.Billing != null ? a.Billing.Status : null
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/appointments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDTO>> GetAppointment(int id)
        {
            var a = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Billing)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (a == null) return NotFound();

            var dto = new AppointmentDTO
            {
                AppointmentId = a.AppointmentId,
                AppointmentDate = a.AppointmentDate,
                PatientId = a.PatientId,
                PatientName = a.Patient != null ? a.Patient.Name : "Unknown",
                DoctorId = a.DoctorId,
                DoctorName = a.Doctor != null ? a.Doctor.Name : "Unknown",
                BillingId = a.Billing != null ? a.Billing.BillingId : (int?)null,
                TotalAmount = a.Billing != null ? a.Billing.TotalAmount : (decimal?)null,
                BillingStatus = a.Billing != null ? a.Billing.Status : null
            };

            return Ok(dto);
        }

        // POST: api/appointments
        [HttpPost]
        public async Task<ActionResult<AppointmentDTO>> PostAppointment(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Reload navigation properties
            await _context.Entry(appointment).Reference(a => a.Patient).LoadAsync();
            await _context.Entry(appointment).Reference(a => a.Doctor).LoadAsync();
            await _context.Entry(appointment).Reference(a => a.Billing).LoadAsync();

            var dto = new AppointmentDTO
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient != null ? appointment.Patient.Name : "Unknown",
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor != null ? appointment.Doctor.Name : "Unknown",
                BillingId = appointment.Billing != null ? appointment.Billing.BillingId : (int?)null,
                TotalAmount = appointment.Billing != null ? appointment.Billing.TotalAmount : (decimal?)null,
                BillingStatus = appointment.Billing != null ? appointment.Billing.Status : null
            };

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, dto);
        }

        // PUT: api/appointments/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAppointment(int id, Appointment appointment)
        {
            if (id != appointment.AppointmentId) return BadRequest();

            _context.Entry(appointment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/appointments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(a => a.AppointmentId == id);
        }
    }
}
