using Eyewa.Domain.Entities;
using Eyewa.Application.Interfaces;
using Eyewa.Application.DTOs;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using static Eyewa.Application.DTOs.Common;

namespace Eyewa.Application.Services
{
    public class ZatcaService : IZatcaService
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<TransactResult> GenerateCsid(string otp)
        {
            TransactResult tres = new TransactResult();
            try
            {
                string csrPath = @"D:\downlauds\zatca-einvoicing-sdk-Java-238-R3.4.8\zatca-einvoicing-sdk-Java-238-R3.4.8\Data\Input\generated-csr-20260402081257.csr";
                string result = await GenerateComplianceCsid(csrPath, otp);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> Generateuuid(string vat)
        {
            TransactResult tres = new TransactResult();
            try
            {
                string result = await GenerateSerialNumber(vat);
                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        public async Task<TransactResult> GenerateProdCsid(string otp)
        {
            TransactResult tres = new TransactResult();
            try
            {
                string csrPath = @"D:\downlauds\zatca-einvoicing-sdk-Java-238-R3.4.8\zatca-einvoicing-sdk-Java-238-R3.4.8\Data\Input\generated-csr-20260402081257.csr";
                string result = await GenerateProductionCsid(csrPath);

                tres.Status = "200";
                tres.Message = "Success";
                tres.objresult = result;
            }
            catch (Exception ex)
            {
                tres.Status = "-100";
                tres.Message = ex.Message;
            }
            return tres;
        }

        private async Task<string> GenerateComplianceCsid(string csrPath, string otp)
        {
            string csrPem = File.ReadAllText(csrPath);
            csrPem = csrPem
                .Replace("-----BEGIN CERTIFICATE REQUEST-----", "")
                .Replace("-----END CERTIFICATE REQUEST-----", "")
                .Replace("\r", "")
                .Replace("\n", "");

            var clientLocal = new HttpClient();
            clientLocal.DefaultRequestHeaders.Clear();
            clientLocal.DefaultRequestHeaders.Add("Accept-Version", "V2");
            clientLocal.DefaultRequestHeaders.Add("OTP", otp);

            var body = new { csr = csrPem };
            string json = JsonConvert.SerializeObject(body);
            
            var response = await clientLocal.PostAsync(
                "https://gw-fatoora.zatca.gov.sa/e-invoicing/core/compliance",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GenerateProductionCsid(string csrPath)
        {
            string binarySecurityToken = "TUlJQ1JEQ0NBZXFnQXdJQkFnSUdBWjFPcUtJeU1Bb0dDQ3FHU000OUJBTUNNQlV4RXpBUkJnTlZCQU1NQ21WSmJuWnZhV05wYm1jd0hoY05Nall3TkRBeU1UUTBOakF5V2hjTk16RXdNDFBMTRFeTNXakJXTVFzd0NRWURWUVFHRXdKVFFURUxNQWtHQTFVRUN3d0NTVlF4SURBZUJnTlZCQW9NRjA1aGFXMWhkQ0JCYkMxQ1lYTmhjaUJQY0hScFkyRnNNUmd3RmdZRFZRUUREQTh6TVRBeU5UUTJOVGszTURBd01ETXdWakFRQmdjcWhrak9QUUlCQmdVcmdRUUFDZ05DQUFSMCtYZkFCZzRYT2E1ZWlCVnQyTkxERXdWTnhDbmxPMVBMK0pzODdBUmJ3M3pVUng3QmlFR0NJV25OdGkwVnh0b2xCTnBVV0JoNG1yNG92Lzh4QmZqWW80SG5NSUhrTUF3R0ExVWRFd0VCL3dRQ01BQXdnZE1HQTFVZEVRU0J5ekNCeUtTQnhUQ0J3akZvTUdZR0ExVUVCQXhmTVMweVlqQmhZVFprTVMwek1UTTFMVFExT1dJdE9HVXdZUzB4WVdGbU4yUXdZelEwT1ROOE1pMHpNVEF5TlRRMk5UazNNREF3TUROOE15MDNPRGxtWTJNMk1DMHhNVGM1TFRRd00yRXRPREpoWWkwMU9XSXpNemd5WXpRek5HTXhIekFkQmdvSmtpYUprL0lzWkFFQkRBOHpNVEF5TlRRMk5UazNNREF3TURNeERUQUxCZ05WQkF3TUJERXhNREF4RGpBTUJnTlZCQm9NQlVodlpuVm1NUll3RkFZRFZRUVBEQTFQY0hScFkyRnNJRk4wYjNKbE1Bb0dDQ3FHU000OUJBTUNNUEF5TFVOQkxFTk9QVkpRU3hEVGoxUWRXSnNhV01sTWpCTFpYa2xNakJUWlhKMmFXTmxjeXhEVGoxVFpYSjJhV05sY3l4RFRqMURiMjVtYVdkMWNtRjBhVzl1TEVSRFBXVjRkSHBoZEdOaExFUkRQV2R2ZGl4RVF6MXNiMk5oYkQ5alFVTmxjblJwWm1sallYUmxQMkpoYzJVL2IySnFaV04wUTJ4aGMzTTlZMlZ5ZEdsbWFXTmhkR2x2YmtGMWRHaHZjbWwwZVRBT0JnTlZIUThCQWY4RUJBTUNCNEF3UEFZSkt3WUJCQUdDTnhVSEJDOHdMUVlsS3dZQkJBR0NOeFVJZ1lhb0hZVFEreEtHN1owa2g4NzdHZFBBVldhSCtxVmxoZG1FUGdJQlpBSUJEakFkQmdOVkhTVUVGakFVQmdnckJnRUZCUWNEQXdZSUt3WUJCUVVIQXdJd0p3WUpLd1lCQkFHQ054VUtCQm93R0RBS0JnZ3JCZ0VGQlFjREF6QUtCZ2dyQmdFRkJRY0RBakFLQmdncWhrak9QUVFEQWdOSEFEQkVBaUFDaUptSGh6K2dzb0hwV0NZQXZtWDNOZm5OdzI1ZHljSkwzV2tpblVsM3JnSWdWam5wcE5Fbm9kSnl3QURrRk01N1FuVEZWcTVsaWtmS29keDZ5aCtQbUI0PQ==";
            string secret = "+XWfmvpwPj1AYtsSgpbEL1mqkrpgLa0IYzWTFZuYL8I=";
            string csrPem = File.ReadAllText(csrPath);

            csrPem = csrPem
                .Replace("-----BEGIN CERTIFICATE REQUEST-----", "")
                .Replace("-----END CERTIFICATE REQUEST-----", "")
                .Replace("\r", "")
                .Replace("\n", "");

            using (var clientLocal = new HttpClient())
            {
                var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{binarySecurityToken}:{secret}"));
                clientLocal.DefaultRequestHeaders.Clear();
                clientLocal.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                clientLocal.DefaultRequestHeaders.Add("Accept-Version", "V2");
                clientLocal.DefaultRequestHeaders.Add("compliance_request_id", "1775141167666");

                var body = new { csr = csrPem };
                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var response = await clientLocal.PostAsync("https://gw-fatoora.zatca.gov.sa/e-invoicing/core/production/csids", content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<string> GenerateSerialNumber(string vatNumber)
        {
            if (string.IsNullOrWhiteSpace(vatNumber))
                throw new ArgumentException("VAT number is required");

            string uuid1 = Guid.NewGuid().ToString();
            string uuid2 = Guid.NewGuid().ToString();
            return $"1-{uuid1}|2-{vatNumber}|3-{uuid2}";
        }
    }
}




