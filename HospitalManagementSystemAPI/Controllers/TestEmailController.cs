using Microsoft.AspNetCore.Mvc;
using HospitalManagementSystemAPI.Services;
using System.Threading.Tasks;

namespace HospitalManagementSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestEmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public TestEmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("send")]
        public async Task<IActionResult> SendTestEmail()
        {
            await _emailService.SendEmailAsync(
                "demo@example.com", // replace with your Mailtrap email
                "Mailtrap Test",
                "<h3>Hello Sidhu Darling❤️!</h3><p>This is a test email from your Hospital Management System using Mailtrap.</p>"
            );
            return Ok("✅ Test email sent! Check Mailtrap inbox.");
        }
    }
}
