using System;
using System.Security.Cryptography;
using System.Text;

namespace Eyewa_new_api.Models
{
    public static class ZatcaHashService
    {
        public static string GenerateHashFromBase64(string base64)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(base64);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
