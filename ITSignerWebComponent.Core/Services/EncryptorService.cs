using ITSignerWebComponent.Core.Interfaces.Services;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ITSignerWebComponent.Core.Services
{
    public class EncryptorService : IEncryptorService
    {
        private readonly string _reeplaceSlashesValue = "S4lash";
        private readonly string _reeplacePlusValue = "p458s";
        private string _KeyEncryptor = "7hi2iJueIoEVH38K1WhxDkgrauG926bk";

        public EncryptorService()
        {
        }

        public string EncryptString(string text)
        {
            string key = _KeyEncryptor;
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(text);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            string stringEncrypted = Convert.ToBase64String(array);

            RemoveSlashes(ref stringEncrypted);
            RemovePLusSign(ref stringEncrypted);

            return stringEncrypted;
        }

        private void RemovePLusSign(ref string stringEncrypted)
        {
            stringEncrypted = stringEncrypted.Replace("/", _reeplaceSlashesValue);
        }

        private void RemoveSlashes(ref string stringEncrypted)
        {
            stringEncrypted = stringEncrypted.Replace("+", _reeplacePlusValue);
        }
    }
}
