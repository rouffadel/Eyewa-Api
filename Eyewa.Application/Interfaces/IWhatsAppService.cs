using System.Threading.Tasks;

namespace Eyewa.Application.Interfaces
{
    public interface IWhatsAppService
    {
        Task<bool> SendMessageAsync(string phoneNumber, string message);
        string FormatMessage(string template, string customerName, string orderNumber, string status);
    }
}
