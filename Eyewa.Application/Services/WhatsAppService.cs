using System;
using System.Threading.Tasks;
using Eyewa.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Eyewa.Application.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(ILogger<WhatsAppService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendMessageAsync(string phoneNumber, string message)
        {
            // TODO: Implement actual WhatsApp API provider here (e.g. Twilio, Meta Cloud API, MessageBird)
            _logger.LogInformation($"[WhatsAppService] Sending WhatsApp to {phoneNumber}:\n{message}");
            
            // Simulate network delay
            await Task.Delay(100);
            
            return true;
        }

        public string FormatMessage(string template, string customerName, string orderNumber, string status)
        {
            if (string.IsNullOrWhiteSpace(template)) return string.Empty;

            return template
                .Replace("{{CustomerName}}", customerName ?? "Customer")
                .Replace("{{OrderNumber}}", orderNumber ?? "Unknown")
                .Replace("{{Status}}", status ?? "Unknown");
        }
    }
}
