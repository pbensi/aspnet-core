using System.Security.Cryptography;
using System.Text;

namespace app.shared.Internal
{
    internal static class AESHelper
    {
        private static readonly int _KeySize = 256;
        private static readonly int _BlockSize = 128;

        public enum KeyType
        {
            Public,
            Private
        }

        public static string Encrypt(string? data, string? key, string? iv, KeyType keyType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data))
                    return string.Empty;

                (byte[] keyByte, byte[] ivByte) = DecodeKeyAndIVStringTobase64(key, iv, keyType);
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] encryptedBytes = EncryptStringToByte(dataBytes, keyByte, ivByte);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Encryption failed {ex.Message}");
            }
        }

        public  static string Decrypt(string? encryptedData, string key, string iv, KeyType keyType)
        {
            if (string.IsNullOrWhiteSpace(encryptedData))
                return string.Empty;

            try
            {
                (byte[] keyByte, byte[] ivByte) = DecodeKeyAndIVStringTobase64(key, iv, keyType);
                byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
                byte[] decryptedBytes = DecryptStringToByte(encryptedBytes, keyByte, ivByte);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Decryption of string failed {ex.Message}");
            }
        }

        public static (string Key, string IV) GenerateRandomKeyAndIV(Guid userGuid)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = _KeySize;
                aes.BlockSize = _BlockSize;
                aes.GenerateKey();
                aes.GenerateIV();

                string keyBase64 = $"{Convert.ToBase64String(aes.Key)}:{userGuid}";
                string ivBase64 = $"{Convert.ToBase64String(aes.IV)}:{userGuid}";

                return (keyBase64, ivBase64);
            }
        }

        private static byte[] EncryptStringToByte(byte[] data, byte[] key, byte[] iv)
        {
            ValidateParameters(data, key, iv);

            try
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
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
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
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
        private static (byte[] key, byte[] iv) DecodeKeyAndIVStringTobase64(string? key, string? iv, KeyType keyType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(iv))
                    throw new InvalidOperationException("Encryption key or IV not set in parameters.");

                string keyBase64 = string.Empty;
                string ivBase64 = string.Empty;

                switch (keyType)
                {
                    case KeyType.Public:
                        keyBase64 = key;
                        ivBase64 = iv;
                        break;

                    case KeyType.Private:
                        string[] keyParts = key.Split(":");
                        string[] ivParts = iv.Split(":");

                        if (keyParts.Length < 2 || ivParts.Length < 2)
                            throw new InvalidOperationException("Split index one not found.");

                        keyBase64 = keyParts[0];
                        ivBase64 = ivParts[0];
                        break;

                    default:
                        throw new ArgumentException("Invalid key type specified.");
                }

                byte[] keyByte = Convert.FromBase64String(keyBase64);
                byte[] ivByte = Convert.FromBase64String(ivBase64);

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
