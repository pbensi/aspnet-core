using app.shared.Dto;
using app.shared.Internal;
using System.Security.Cryptography;

namespace app.shared
{
    public static class SecurityUtils
    {
        private static readonly string _PublicKey = EnvironmentManager.PUBLIC_KEY;
        private static readonly string _PublicIV = EnvironmentManager.PUBLIC_IV;

        public static T ProcessSecureData<T>(SecureDataRequest request)
        {
            try
            {
                return AESGCMandECDSAHelper.ProcessSecureData<T>(request);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Process secure data failed. {ex.Message}");
            }
        }

        public static string PublicDecrypt(string data)
        {
            if (string.IsNullOrEmpty(_PublicKey))
            {
                throw new InvalidOperationException("Public key is not set.");
            }

            if (string.IsNullOrEmpty(_PublicIV))
            {
                throw new InvalidOperationException("Public IV is not set.");
            }

            try
            {
                return AESHelper.Decrypt(data, _PublicKey, _PublicIV, AESHelper.KeyType.Public);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Public decryption failed. {ex.Message}");
            }
        }

        public static string PublicEncrypt(string data)
        {
            if (string.IsNullOrEmpty(_PublicKey))
            {
                throw new InvalidOperationException("Public key is not set.");
            }

            if (string.IsNullOrEmpty(_PublicIV))
            {
                throw new InvalidOperationException("Public IV is not set.");
            }

            try
            {
                return AESHelper.Encrypt(data, _PublicKey, _PublicIV, AESHelper.KeyType.Public);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Public encryption failed. {ex.Message}");
            }
        }

        public static string PrivateDecrypt(string data, string userKey, string userIV)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new InvalidOperationException("Data to decrypt cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(userKey))
            {
                throw new InvalidOperationException("User key is not set.");
            }

            if (string.IsNullOrEmpty(userIV))
            {
                throw new InvalidOperationException("User IV is not set.");
            }

            try
            {
                return AESHelper.Decrypt(data, userKey, userIV, AESHelper.KeyType.Private);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Private decryption failed. {ex.Message}");
            }
        }

        public static string PrivateEncrypt(string data, string userKey, string userIV)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("Data to encrypt cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(userKey))
            {
                throw new InvalidOperationException("User key is not set.");
            }

            if (string.IsNullOrEmpty(userIV))
            {
                throw new InvalidOperationException("User IV is not set.");
            }

            try
            {
                return AESHelper.Encrypt(data, userKey, userIV, AESHelper.KeyType.Private);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Private encryption failed. {ex.Message}");
            }
        }

        public static (string Key, string IV) GenerateRandomKeyAndIV(Guid userGuid)
        {
            if (userGuid == Guid.Empty)
            {
                throw new ArgumentNullException("Cannot be null or empty.");
            }

            try
            {
                return AESHelper.GenerateRandomKeyAndIV(userGuid);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Generate random key and iv failed. {ex.Message}");
            }
        }

        public static string Hash(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("Data to hash cannot be null or empty.");
            }

            try
            {
                return HashingHelper.Hash(data);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Hashing failed. {ex}");
            }
        }

        public static bool VerifyHashed(string hashedData, string originalData)
        {
            if (string.IsNullOrEmpty(hashedData) || string.IsNullOrEmpty(originalData))
            {
                throw new ArgumentNullException("Hashed data or original data cannot be null or empty.");
            }

            try
            {
                return HashingHelper.VerifyHashed(hashedData, originalData);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"Hash verification failed. {ex.Message}");
            }
        }

        public static string AppDecrypt(string data, string key, string iv)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("App decryption key is not set.");
            }

            if (string.IsNullOrEmpty(iv))
            {
                throw new InvalidOperationException("App  decryption IV is not set.");
            }

            try
            {
                return AESHelper.Decrypt(data, key, iv, AESHelper.KeyType.Public);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"App decryption failed. {ex.Message}");
            }
        }

        public static string AppEncrypt(string data, string key, string iv)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("App encryption key is not set.");
            }

            if (string.IsNullOrEmpty(iv))
            {
                throw new InvalidOperationException("App  encryption IV is not set.");
            }

            try
            {
                return AESHelper.Encrypt(data, key, iv, AESHelper.KeyType.Public);
            }
            catch (Exception ex)
            {
                throw new CryptographicException($"App encryption failed. {ex.Message}");
            }
        }
    }
}
