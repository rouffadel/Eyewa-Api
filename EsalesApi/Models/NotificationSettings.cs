using System;
using System.ComponentModel.DataAnnotations;

namespace Eyewa_new_api.Models
{
    public class NotificationSettings
    {
        [Key]
        public int Id { get; set; }

        // Twilio SMS settings
        public string TwilioAccountSid { get; set; }
        public string TwilioAuthToken { get; set; }
        public string TwilioPhoneNumber { get; set; }

        // WhatsApp settings
        public string WhatsAppApiUrl { get; set; }
        public string WhatsAppAccessToken { get; set; }
        public string WhatsAppSenderNumber { get; set; }

        // Push settings (Firebase Cloud Messaging)
        public string FcmServerKey { get; set; }
        public string FcmSenderId { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
