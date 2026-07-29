using System.Threading.Tasks;

namespace Eyewa.Application.Interfaces
{
    public interface INotificationService
    {
        Task<(bool Success, string ErrorMessage)> SendEmailAsync(string toEmail, string subject, string body, string embeddedImageBase64 = null);
        Task<(bool Success, string ErrorMessage)> SendWhatsAppMessageAsync(string toPhone, string message, string base64Image = null);
    }
}
