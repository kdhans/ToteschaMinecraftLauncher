using Hardware.Info;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ToteschaMinecraftLauncher.UpdatedScripts.Controllers;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Logic
{
    internal class ToteschaDataEncryptor
    {

        private WebController _webController = new WebController();
        public async Task<string> EncryptStringAsync(string str, string key, string iv)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            string encryptedString = string.Empty;
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetKey(key);
                aes.IV = GetIV(iv);
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

        //Typical use case:
        //var toteschaAPIKeys = await _webController.GetToteschaAPIKeysAsync();
        //var iv = toteschaAPIKeys.Keys.Single(x => x.ID == serverID).Value;
        //var selectedKey = toeschaAPIKeys.Keys.Single(x=> x.ID == );
        //var decryptedKey = await _toteschaDataEncryptor.DecryptStringAsync(selectedKey.Value, modpackName, modpackVersion);

        public async Task<string> DecryptStringAsync(string str, string key, string iv)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            string decryptedString = string.Empty;
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetKey(key);
                aes.IV = GetIV(iv);
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

        private byte[] GetIV(string iv)
        {
            var putTogetherString = $"TCV{iv}";
            var builder = new StringBuilder();
            for (int i = 0; i < putTogetherString.Length / 2; i++)
            {
                builder.Append(putTogetherString[i]);
                builder.Append(putTogetherString[putTogetherString.Length - i - 1]);
            }

            List<byte> byteArray = new List<byte>();

            foreach (var character in builder.ToString())
            {
                if (byteArray.Count < 16)
                    byteArray.Add(Convert.ToByte(character));
            }
            if (byteArray.Count < 16)
                for (int i = byteArray.Count; i < 16; i++)
                    byteArray.Add(Convert.ToByte('#'));

            return byteArray.ToArray();
        }

        private byte[] GetKey(string key)
        {
            var putTogetherString = $"TCK{key}";
            var builder = new StringBuilder();
            for (int i = 0; i < putTogetherString.Length / 2; i++)
            {
                builder.Append(putTogetherString[i]);
                builder.Append(putTogetherString[putTogetherString.Length - i - 1]);
            }

            List<byte> byteArray = new List<byte>();

            foreach (var character in builder.ToString())
            {
                if (byteArray.Count < 32)
                    byteArray.Add(Convert.ToByte(character));
            }
            if (byteArray.Count < 32)
                for (int i = byteArray.Count; i < 32; i++)
                    byteArray.Add(Convert.ToByte('$'));

            return byteArray.ToArray();
        }

    }
}
