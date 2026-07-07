using Eyewa.Domain.Entities;
using Eyewa.Application.Interfaces;
using Eyewa.Application.DTOs;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static Eyewa.Application.DTOs.Common;

namespace Eyewa.Application.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IApplicationDbContext _context;
        private readonly IDbLoggerService _dbLogger;

        public SettingsService(
            IApplicationDbContext context,
            IDbLoggerService dbLogger)
        {
            _context = context;
            _dbLogger = dbLogger;
        }

        public async Task<TransactResult> GetNotificationSettings()
        {
            try
            {
                var settings = await _context.NotificationSettings.FirstOrDefaultAsync();
                if (settings == null)
                {
                    settings = new NotificationSettings();
                }
                return new TransactResult { Status = "200", Message = "Success", objresult = settings };
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("GetNotificationSettings error: " + ex.Message, ex.StackTrace);
                return new TransactResult { Status = "-100", Message = ex.Message };
            }
        }

        public async Task<TransactResult> SaveNotificationSettings(NotificationSettings settings)
        {
            try
            {
                var existing = await _context.NotificationSettings.FirstOrDefaultAsync();
                if (existing == null)
                {
                    settings.UpdatedAt = DateTime.UtcNow;
                    _context.NotificationSettings.Add(settings);
                }
                else
                {
                    existing.TwilioAccountSid = settings.TwilioAccountSid;
                    existing.TwilioAuthToken = settings.TwilioAuthToken;
                    existing.TwilioPhoneNumber = settings.TwilioPhoneNumber;
                    existing.WhatsAppApiUrl = settings.WhatsAppApiUrl;
                    existing.WhatsAppAccessToken = settings.WhatsAppAccessToken;
                    existing.WhatsAppSenderNumber = settings.WhatsAppSenderNumber;
                    existing.FcmServerKey = settings.FcmServerKey;
                    existing.FcmSenderId = settings.FcmSenderId;
                    existing.UpdatedAt = DateTime.UtcNow;
                    _context.NotificationSettings.Update(existing);
                }
                await _context.SaveChangesAsync();
                return new TransactResult { Status = "200", Message = "Notification settings saved successfully", objresult = settings };
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("SaveNotificationSettings error: " + ex.Message, ex.StackTrace);
                return new TransactResult { Status = "-100", Message = ex.Message };
            }
        }
    }
}




