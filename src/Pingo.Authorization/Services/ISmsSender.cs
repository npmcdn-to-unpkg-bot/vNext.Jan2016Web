using System.Threading.Tasks;

namespace Pingo.Authorization.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
