using System.Security.Cryptography;
using System.Text;

namespace api.shared.Utilities
{
    public class AES
    {
        public byte[] GenerateRandomKey()
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateKey();
                return aes.Key;
            }
        }

        public string EncryptData(string data, byte[] key)
        {
            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.GenerateIV();

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(data);
                            }
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string DecryptData(string encryptedData, byte[] key)
        {
            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedData);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.Mode = CipherMode.CBC;

                    byte[] iv = new byte[aesAlg.BlockSize / 8];
                    Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, iv);

                    using (MemoryStream msDecrypt = new MemoryStream())
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                        {
                            csDecrypt.Write(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length);
                        }
                        return Encoding.UTF8.GetString(msDecrypt.ToArray());
                    }
                }
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine("Error during decryption: " + ex.Message);
                return string.Empty;
            }
        }
    }
}
