using System.Threading.Tasks;

namespace DutyScheduler.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string message);
    }
}
