using System;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Eyewa.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace Eyewa.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IDbLoggerService _dbLogger;
        private static readonly HttpClient _httpClient = new HttpClient();

        public NotificationService(
            IApplicationDbContext context,
            IConfiguration configuration,
            IDbLoggerService dbLogger)
        {
            _context = context;
            _configuration = configuration;
            _dbLogger = dbLogger;
        }

        public async Task<(bool Success, string ErrorMessage)> SendEmailAsync(string toEmail, string subject, string body, string embeddedImageBase64 = null)
        {
            try
            {
                if (string.IsNullOrEmpty(toEmail)) return (false, "Email address is empty");

                var host = _configuration["EmailSettings:Host"];
                var portStr = _configuration["EmailSettings:Port"];
                var username = _configuration["EmailSettings:Username"];
                var password = _configuration["EmailSettings:Password"];
                var enableSslStr = _configuration["EmailSettings:EnableSsl"] ?? "true";
                var fromEmail = _configuration["EmailSettings:FromEmail"];

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
                {
                    string err = "Email settings (Host/Username) are missing in appsettings.";
                    _dbLogger.LogError(err, "");
                    return (false, err);
                }

                int port = int.TryParse(portStr, out int p) ? p : 587;
                bool enableSsl = bool.TryParse(enableSslStr, out bool s) ? s : true;

                using (var client = new SmtpClient(host, port))
                {
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = enableSsl;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail ?? username),
                        Subject = subject,
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(toEmail);

                    if (!string.IsNullOrEmpty(embeddedImageBase64))
                    {
                        string cleanBase64 = embeddedImageBase64;
                        if (cleanBase64.Contains(","))
                        {
                            cleanBase64 = cleanBase64.Substring(cleanBase64.IndexOf(",") + 1);
                        }

                        byte[] imageBytes = Convert.FromBase64String(cleanBase64);
                        var imageStream = new MemoryStream(imageBytes);
                        
                        var linkedResource = new LinkedResource(imageStream, "image/png")
                        {
                            ContentId = "qrcodeimg"
                        };

                        var alternateView = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
                        alternateView.LinkedResources.Add(linkedResource);
                        mailMessage.AlternateViews.Add(alternateView);
                    }
                    else
                    {
                        mailMessage.Body = body;
                    }

                    await client.SendMailAsync(mailMessage);
                    return (true, "");
                }
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("SendEmailAsync error: " + ex.Message, ex.StackTrace);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> SendWhatsAppMessageAsync(string toPhone, string message, string base64Image = null)
        {
            try
            {
                if (string.IsNullOrEmpty(toPhone)) return (false, "Phone number is empty.");

                var apiUrl = _configuration["WhatsAppSettings:ApiUrl"];
                var accessToken = _configuration["WhatsAppSettings:AccessToken"];

                if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(accessToken))
                {
                    string err = "WhatsApp settings (ApiUrl/AccessToken) are missing in appsettings.";
                    _dbLogger.LogError(err, "");
                    return (false, err);
                }

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                string errorMsgs = "";
                bool success = true;

                // Step 1: ALWAYS send the Text Message first
                var textPayload = new
                {
                    messaging_product = "whatsapp",
                    to = toPhone,
                    type = "text",
                    text = new { body = message }
                };

                var textJson = JsonConvert.SerializeObject(textPayload);
                var textContent = new StringContent(textJson, Encoding.UTF8, "application/json");

                var textResponse = await _httpClient.PostAsync(apiUrl, textContent);
                if (!textResponse.IsSuccessStatusCode)
                {
                    var errorResult = await textResponse.Content.ReadAsStringAsync();
                    _dbLogger.LogError($"WhatsApp Text Message failed: {textResponse.StatusCode}", errorResult);
                    errorMsgs += $"Text API Error {textResponse.StatusCode}: {errorResult} | ";
                    success = false;
                }

                // Step 2: If image is provided, upload and send Image Message
                if (!string.IsNullOrEmpty(base64Image))
                {
                    string mediaApiUrl = apiUrl.Replace("/messages", "/media");
                    string cleanBase64 = base64Image;

                    if (cleanBase64.Contains(","))
                    {
                        cleanBase64 = cleanBase64.Substring(cleanBase64.IndexOf(",") + 1);
                    }

                    byte[] imageBytes = Convert.FromBase64String(cleanBase64);
                    string mediaId = null;

                    using (var content = new MultipartFormDataContent())
                    {
                        var fileContent = new ByteArrayContent(imageBytes);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                        content.Add(fileContent, "file", "qrcode.png");
                        content.Add(new StringContent("image/png"), "type");
                        content.Add(new StringContent("whatsapp"), "messaging_product");

                        var uploadResponse = await _httpClient.PostAsync(mediaApiUrl, content);
                        string uploadResult = await uploadResponse.Content.ReadAsStringAsync();

                        if (!uploadResponse.IsSuccessStatusCode)
                        {
                            _dbLogger.LogError($"WhatsApp Media Upload failed: {uploadResponse.StatusCode}", uploadResult);
                            errorMsgs += $"Media Upload Error: {uploadResult}";
                            success = false;
                        }
                        else
                        {
                            var jObj = JObject.Parse(uploadResult);
                            mediaId = jObj["id"]?.ToString();
                        }
                    }

                    // Send the Image message
                    if (!string.IsNullOrEmpty(mediaId))
                    {
                        var imagePayload = new
                        {
                            messaging_product = "whatsapp",
                            to = toPhone,
                            type = "image",
                            image = new 
                            { 
                                id = mediaId
                            }
                        };

                        var imageJson = JsonConvert.SerializeObject(imagePayload);
                        var imageStringContent = new StringContent(imageJson, Encoding.UTF8, "application/json");

                        var imageResponse = await _httpClient.PostAsync(apiUrl, imageStringContent);

                        if (!imageResponse.IsSuccessStatusCode)
                        {
                            var errorResult = await imageResponse.Content.ReadAsStringAsync();
                            _dbLogger.LogError($"WhatsApp Image Message failed: {imageResponse.StatusCode}", errorResult);
                            errorMsgs += $"Image API Error {imageResponse.StatusCode}: {errorResult}";
                            success = false;
                        }
                    }
                }

                return (success, errorMsgs);
            }
            catch (Exception ex)
            {
                _dbLogger.LogError("SendWhatsAppMessageAsync error: " + ex.Message, ex.StackTrace);
                return (false, ex.Message);
            }
        }
    }
}
