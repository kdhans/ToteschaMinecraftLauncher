using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.Scripts.Helpers;
using ToteschaMinecraftLauncher.UpdatedScripts.Controllers;

namespace ToteschaMinecraftLauncher.Scripts.UIHelpers
{
    internal class OldToteschaEncryptor
    {
        public async Task<string> EncryptStringAsync(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            string encryptedString = string.Empty;
            var systemHelper = new SystemHelper();
            using (Aes aes = Aes.Create())
            {
                aes.Key = systemHelper.GetEnvironmentHashKey();
                aes.IV = systemHelper.GetEnvironmentIV();
                var data = Encoding.Unicode.GetBytes(str);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        await cryptoStream.WriteAsync(data);
                        await cryptoStream.FlushFinalBlockAsync();
                    }
                    encryptedString = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encryptedString;
        }

        public async Task<string> DecryptStringAsync(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            string decryptedString = string.Empty;
            var systemHelper = new SystemHelper();
            using (Aes aes = Aes.Create())
            {
                aes.Key = systemHelper.GetEnvironmentHashKey();
                aes.IV = systemHelper.GetEnvironmentIV();
                var data = Convert.FromBase64String(str);

                using (MemoryStream input = new MemoryStream(data))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (MemoryStream output = new MemoryStream())
                        {
                            await cryptoStream.CopyToAsync(output);
                            decryptedString = Encoding.Unicode.GetString(output.ToArray());
                        }
                    }
                }
            }
            return decryptedString;
        }

    }
}
