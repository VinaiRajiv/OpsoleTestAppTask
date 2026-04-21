using Microsoft.Win32;
using OpsoleTestApp.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace OpsoleTestApp.Services
{
    public class SecurityService
    {
        private readonly byte[] key;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="base64Key"></param>
        public SecurityService(string base64Key)
        {
            key = Convert.FromBase64String(base64Key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Encrypt(string text)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = key;
                aes.GenerateIV();

                var enc = aes.CreateEncryptor();
                var bytes = Encoding.UTF8.GetBytes(text);
                var cipher = enc.TransformFinalBlock(bytes, 0, bytes.Length);

                return Convert.ToBase64String(aes.IV) + ":" + Convert.ToBase64String(cipher);
            }
            catch (Exception ex)
            {          
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string Decrypt(string data)
        {
            try
            {
                var parts = data.Split(':');
                var iv = Convert.FromBase64String(parts[0]);
                var cipher = Convert.FromBase64String(parts[1]);

                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;

                var dec = aes.CreateDecryptor();
                var plain = dec.TransformFinalBlock(cipher, 0, cipher.Length);

                return Encoding.UTF8.GetString(plain);
            }
            catch (Exception ex)
            {
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encrypted"></param>
        public void SaveToRegistry(string encrypted)
        {
            try
            {
                using var keyReg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\OpsoleTest\ApiConfig");
                keyReg.SetValue("Data", encrypted);
            }
            catch (Exception ex)
            {
                SystemUtility.LogToFile($"Exception occurred, Message: {ex.Message}");
            }
        }
    }
}
