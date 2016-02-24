using System.Threading.Tasks;

namespace Pingo.Authorization.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
