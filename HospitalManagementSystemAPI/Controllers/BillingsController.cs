using HospitalManagementSystemAPI.Data;
using HospitalManagementSystemAPI.DTOs;
using HospitalManagementSystemAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BillingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/billings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BillingDTO>>> GetBillings()
        {
            var billings = await _context.Billings
                .Include(b => b.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(b => b.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Select(b => new BillingDTO
                {
                    BillingId = b.BillingId,
                    AppointmentId = b.AppointmentId,
                    ConsultationFee = b.ConsultationFee,
                    MedicineCharges = b.MedicineCharges,
                    TotalAmount = b.TotalAmount,
                    PaidAmount = b.PaidAmount,
                    Status = b.Status,
                    PatientName = (b.Appointment != null && b.Appointment.Patient != null) ? b.Appointment.Patient.Name : "",
                    DoctorName = (b.Appointment != null && b.Appointment.Doctor != null) ? b.Appointment.Doctor.Name : "",
                    AppointmentDate = (b.Appointment != null) ? b.Appointment.AppointmentDate : DateTime.MinValue
                })
                .ToListAsync();

            return Ok(billings);
        }

        // GET: api/billings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BillingDTO>> GetBilling(int id)
        {
            var b = await _context.Billings
                .Include(b => b.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(b => b.Appointment)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(b => b.BillingId == id);

            if (b == null) return NotFound();

            var dto = new BillingDTO
            {
                BillingId = b.BillingId,
                AppointmentId = b.AppointmentId,
                ConsultationFee = b.ConsultationFee,
                MedicineCharges = b.MedicineCharges,
                TotalAmount = b.TotalAmount,
                PaidAmount = b.PaidAmount,
                Status = b.Status,
                PatientName = (b.Appointment != null && b.Appointment.Patient != null) ? b.Appointment.Patient.Name : "",
                DoctorName = (b.Appointment != null && b.Appointment.Doctor != null) ? b.Appointment.Doctor.Name : "",
                AppointmentDate = (b.Appointment != null) ? b.Appointment.AppointmentDate : DateTime.MinValue
            };

            return Ok(dto);
        }

        // POST: api/billings
        [HttpPost]
        public async Task<ActionResult<BillingDTO>> PostBilling(Billing billing)
        {
            billing.TotalAmount = billing.ConsultationFee + billing.MedicineCharges;
            billing.Status = billing.PaidAmount >= billing.TotalAmount ? "Paid" : "Pending";

            _context.Billings.Add(billing);
            await _context.SaveChangesAsync();

            // Load Appointment, Patient, and Doctor
            await _context.Entry(billing).Reference(b => b.Appointment).LoadAsync();
            if (billing.Appointment != null)
            {
                await _context.Entry(billing.Appointment).Reference(a => a.Patient).LoadAsync();
                await _context.Entry(billing.Appointment).Reference(a => a.Doctor).LoadAsync();
            }

            var dto = new BillingDTO
            {
                BillingId = billing.BillingId,
                AppointmentId = billing.AppointmentId,
                ConsultationFee = billing.ConsultationFee,
                MedicineCharges = billing.MedicineCharges,
                TotalAmount = billing.TotalAmount,
                PaidAmount = billing.PaidAmount,
                Status = billing.Status,
                PatientName = (billing.Appointment != null && billing.Appointment.Patient != null) ? billing.Appointment.Patient.Name : "",
                DoctorName = (billing.Appointment != null && billing.Appointment.Doctor != null) ? billing.Appointment.Doctor.Name : "",
                AppointmentDate = (billing.Appointment != null) ? billing.Appointment.AppointmentDate : DateTime.MinValue
            };

            return CreatedAtAction(nameof(GetBilling), new { id = billing.BillingId }, dto);
        }

        // PUT: api/billings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBilling(int id, Billing billing)
        {
            if (id != billing.BillingId) return BadRequest();

            billing.TotalAmount = billing.ConsultationFee + billing.MedicineCharges;
            billing.Status = billing.PaidAmount >= billing.TotalAmount ? "Paid" : "Pending";

            _context.Entry(billing).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BillingExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/billings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilling(int id)
        {
            var billing = await _context.Billings.FindAsync(id);
            if (billing == null) return NotFound();

            _context.Billings.Remove(billing);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BillingExists(int id)
        {
            return _context.Billings.Any(b => b.BillingId == id);
        }
    }
}
