using System;
using System.Text;
using System.Security.Cryptography;
using NUnit.Framework;
using EsalesApi.Models;

namespace EsalesApi.Tests
{
    [TestFixture]
    public class ZatcaHashTests
    {
        [Test]
        public void GenerateHashFromBase64_MatchesManualComputation()
        {
            // sample signed xml
            string signedXml = "<Invoice><Test>value</Test></Invoice>";

            // base64 of UTF8 bytes without BOM
            byte[] bytes = new UTF8Encoding(false).GetBytes(signedXml);
            string base64 = Convert.ToBase64String(bytes);

            string expected;
            using (var sha = SHA256.Create())
            {
                byte[] h = sha.ComputeHash(new UTF8Encoding(false).GetBytes(base64));
                expected = Convert.ToBase64String(h);
            }

            string actual = ZatcaHashService.GenerateHashFromBase64(base64);

            Assert.AreEqual(expected, actual);
        }
    }
}
