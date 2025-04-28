using System.Security.Cryptography;
using System.Text;

namespace app.shared.Securities
{
    public static class Symmetric
    {
        private static readonly int _KeySize = 256;
        private static readonly int _BlockSize = 128;

        public static (string key, string iv) GenerateEnvironmentKeyAndIV(string key, string iv)
        {
            byte[] keyBytes = GetBytesFromString(key, 32);
            byte[] ivBytes = GetBytesFromString(iv, 16);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = _KeySize;
                aes.BlockSize = _BlockSize;
                aes.Key = keyBytes;
                aes.IV = ivBytes;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(keyBytes, 0, keyBytes.Length);
                    csEncrypt.Write(ivBytes, 0, ivBytes.Length);
                    csEncrypt.FlushFinalBlock();

                    byte[] encryptedData = msEncrypt.ToArray();
                    string encryptedKey = Convert.ToBase64String(encryptedData.Take(32).ToArray());
                    string encryptedIv = Convert.ToBase64String(encryptedData.Skip(32).Take(16).ToArray());

                    return (encryptedKey, encryptedIv);
                }
            }
        }

        private static byte[] GetBytesFromString(string data, int length)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            if (bytes.Length > length)
            {
                Array.Resize(ref bytes, length);
            }
            else if (bytes.Length < length)
            {
                Array.Resize(ref bytes, length);
            }

            return bytes;
        }

        public static string Encrypt(string? data, string? key, string? iv)
        {
            if (string.IsNullOrWhiteSpace(data))
                return string.Empty;
            try
            {
                (byte[] keyByte, byte[] ivByte) = DecodeKeyAndIVStringTobase64(key, iv);
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] encryptedBytes = EncryptStringToByte(dataBytes, keyByte, ivByte);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Encryption failed {ex.Message}");
            }
        }

        public static string Decrypt(string? encryptedData, string key, string iv)
        {
            if (string.IsNullOrWhiteSpace(encryptedData))
                return string.Empty;

            try
            {
                (byte[] keyByte, byte[] ivByte) = DecodeKeyAndIVStringTobase64(key, iv);
                byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
                byte[] decryptedBytes = DecryptStringToByte(encryptedBytes, keyByte, ivByte);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Decryption of string failed {ex.Message}");
            }
        }

        public static (string Key, string IV) GenerateRandomKeyAndIV()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = _KeySize;
                aes.BlockSize = _BlockSize;
                aes.GenerateKey();
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                return (Convert.ToBase64String(aes.Key), Convert.ToBase64String(aes.IV));
            }
        }

        private static byte[] EncryptStringToByte(byte[] data, byte[] key, byte[] iv)
        {
            ValidateParameters(data, key, iv);

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    using (MemoryStream msEncrypt = new MemoryStream())
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(data, 0, data.Length);
                        csEncrypt.FlushFinalBlock();
                        return msEncrypt.ToArray();
                    }
                }
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException($"Encryption failed {ex.Message}");
            }
        }
        private static byte[] DecryptStringToByte(byte[] encryptedData, byte[] key, byte[] iv)
        {
            ValidateParameters(encryptedData, key, iv);

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (MemoryStream msPlain = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msPlain);
                        return msPlain.ToArray();
                    }
                }
            }
            catch (CryptographicException ex)
            {
                throw new CryptographicException($"Decryption failed {ex.Message}");
            }
        }
        private static (byte[] key, byte[] iv) DecodeKeyAndIVStringTobase64(string? key, string? iv)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(iv))
                    throw new InvalidOperationException("Encryption key or IV not set in parameters.");

                byte[] keyByte = Convert.FromBase64String(key);
                byte[] ivByte = Convert.FromBase64String(iv);

                ValidateKeySize(keyByte, _KeySize / 8, nameof(keyByte));
                ValidateIVSize(ivByte, _BlockSize / 8, nameof(ivByte));

                return (keyByte, ivByte);
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException($"Failed to decode key or IV from base64. {ex.Message}");
            }
        }
        private static void ValidateParameters(byte[] data, byte[] key, byte[] iv)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be null or empty.", nameof(data));
            ValidateKeySize(key, _KeySize / 8, nameof(key));
            ValidateIVSize(iv, _BlockSize / 8, nameof(iv));
        }
        private static void ValidateKeySize(byte[] key, int expectedSizeInBytes, string paramName)
        {
            if (key == null || key.Length != expectedSizeInBytes)
                throw new ArgumentException($"Invalid {paramName}. Key must be {expectedSizeInBytes} bytes long.", paramName);
        }

        private static void ValidateIVSize(byte[] iv, int expectedSizeInBytes, string paramName)
        {
            if (iv == null || iv.Length != expectedSizeInBytes)
                throw new ArgumentException($"Invalid {paramName}. IV must be {expectedSizeInBytes} bytes long.", paramName);
        }
    }
}
