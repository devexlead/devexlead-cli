using DevExLead.Core.Storage;
using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace DevExLead.Core.Helpers
{
    public class SecurityHelper
    {
        private static readonly byte[] _key = GenerateMachineSpecificKey();
        private static readonly byte[] _iv = GenerateIV();

        private static byte[] GenerateMachineSpecificKey()
        {
            string machineId = GetMachineIdentifier();
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(machineId));
            }
        }

        private static byte[] GenerateIV()
        {
            // You can generate a constant IV or use other methods if needed
            return new byte[16]; // Using a zero IV for simplicity
        }

        private static string GetMachineIdentifier()
        {
            string cpuId = string.Empty;
            string motherboardSerial = string.Empty;

            // Get CPU ID
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select ProcessorId from Win32_Processor"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    cpuId = obj["ProcessorId"].ToString();
                    break;
                }
            }

            // Get Motherboard Serial Number
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select SerialNumber from Win32_BaseBoard"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    motherboardSerial = obj["SerialNumber"].ToString();
                    break;
                }
            }

            return cpuId + motherboardSerial;
        }

        public static string EncryptKey(string encryptionKey)
        {
            if (encryptionKey == null || encryptionKey.Length <= 0)
                throw new ArgumentNullException(nameof(encryptionKey));
            if (_key == null || _key.Length <= 0)
                throw new ArgumentNullException(nameof(_key));
            if (_iv == null || _iv.Length <= 0)
                throw new ArgumentNullException(nameof(_iv));

            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(encryptionKey);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Convert the encrypted byte array to a Base64 string
            string result = Convert.ToBase64String(encrypted);
            return result;
        }

        public static string DecryptKey(string encryptionKey)
        {
            if (string.IsNullOrEmpty(encryptionKey))
                throw new ArgumentNullException(nameof(encryptionKey));
            if (_key == null || _key.Length <= 0)
                throw new ArgumentNullException(nameof(_key));
            if (_iv == null || _iv.Length <= 0)
                throw new ArgumentNullException(nameof(_iv));

            byte[] cipherText = Convert.FromBase64String(encryptionKey);
            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        public static string EncryptVaultEntry(string vaultEntryValue)
        {
            var userStorage = UserStorageManager.GetUserStorage();
            var keys = DecryptKey(userStorage.EncryptionKeys);

            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(keys);
                var encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(vaultEntryValue), RSAEncryptionPadding.OaepSHA1);
                return Convert.ToBase64String(encryptedData);
            }
        }

        public static string DecryptVaultEntry(string vaultEntryValue)
        {
            var userStorage = UserStorageManager.GetUserStorage();
            var keys = DecryptKey(userStorage.EncryptionKeys);

            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(keys);
                var decryptedData = rsa.Decrypt(Convert.FromBase64String(vaultEntryValue), RSAEncryptionPadding.OaepSHA1);
                return Encoding.UTF8.GetString(decryptedData);
            }
        }
    }
}
