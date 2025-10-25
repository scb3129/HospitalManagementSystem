using System.Threading.Tasks;

namespace HospitalManagementSystemAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
