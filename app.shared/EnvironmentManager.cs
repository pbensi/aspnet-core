using System.Net;
using System.Security.Cryptography;
using System.Text;
using app.shared.Securities;
using Newtonsoft.Json;
using static app.shared.EnumGroup;

namespace app.shared
{
    public static class EnvironmentManager
    {
        private class MetaData
        {
            public string Salt { get; set; } = string.Empty;
            public string IV { get; set; } = string.Empty;
            public string CipherText { get; set; } = string.Empty;
            public string HostingEnvironment { get; set; } = string.Empty;
            public string ExpirationTimestamp { get; set; } = string.Empty;
            public string TargetEnvironment { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
        }

        private class SecretData
        {
            public string CipherText { get; set; } = string.Empty;
        }

        private class AppSettings
        {
            public string Name { get; set; } = string.Empty;
            public string Meta { get; set; } = string.Empty;
            public string Secret { get; set; } = string.Empty;
        }

        private static (string Key, string IV) EnvironmentKeyAndIV
        {
            get
            {
                return Symmetric.GenerateEnvironmentKeyAndIV(
                    GetSecretEnvironmentVariable("APP_PUBLIC_KEY"),
                    GetSecretEnvironmentVariable("APP_PUBLIC_IV")
                );
            }
        }

        public static string APP_PUBLIC_KEY => EnvironmentKeyAndIV.Key;
        public static string APP_PUBLIC_IV => EnvironmentKeyAndIV.IV;
        public static string APP_SQL_AUTHENTICATION => GetSecretEnvironmentVariable("APP_SQL_AUTHENTICATION");
        public static string APP_SECRET_KEY => GetSecretEnvironmentVariable("APP_SECRET_KEY");
        public static string APP_CORS_NAME => GetSecretEnvironmentVariable("APP_CORS_NAME");
        public static string APP_CORS_SERVER_ORIGINS => GetSecretEnvironmentVariable("APP_CORS_SERVER_ORIGINS");
        public static string APP_CORS_ALLOWED_ORIGINS => GetSecretEnvironmentVariable("APP_CORS_ALLOWED_ORIGINS");
        public static string APP_ISSUER => GetSecretEnvironmentVariable("APP_ISSUER");
        public static string APP_AUDIENCE => GetSecretEnvironmentVariable("APP_AUDIENCE");
        public static string APP_HOSTING_ENVIRONMENT => GetSecretEnvironmentVariable("APP_HOSTING_ENVIRONMENT");
        public static string WEBHOST_FILES => GetEnvironmentVariable("WEBHOST_FILES");

        private static string GetEnvironmentVariable(string variable)
        {
            var value = Environment.GetEnvironmentVariable(variable);

            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Environment variable '{variable}' is not set.");
            }
            return value;
        }

        public static string GetSecretEnvironmentVariable(string variable)
        {
            if (!Directory.Exists(WEBHOST_FILES))
            {
                throw new InvalidOperationException($"The specified directory '{WEBHOST_FILES}' does not exist. Please check the path.");
            }

            string hostName = Dns.GetHostName();
            string appSettingPath = Path.Combine(WEBHOST_FILES, $"app-{hostName}-settings.json");

            if (!File.Exists(appSettingPath))
            {
                throw new FileNotFoundException($"The app settings file '{appSettingPath}' was not found. Please ensure it exists.");
            }

            string appSettingJson = File.ReadAllText(appSettingPath);
            AppSettings appSettings = JsonConvert.DeserializeObject<AppSettings>(appSettingJson) ?? throw new InvalidOperationException("Failed to deserialize Settings.");

            string secretName = appSettings.Name;
            string metaDataFilePath = appSettings.Meta;
            string secretFilePath = appSettings.Secret;

            string metadataJson = File.ReadAllText(metaDataFilePath);
            MetaData metadata = JsonConvert.DeserializeObject<MetaData>(metadataJson) ?? throw new InvalidOperationException("Failed to deserialize metadata.");

            byte[] metadataKey;
            byte[] secretNameBytes = Encoding.UTF8.GetBytes(secretName);
            byte[] saltBytes = Encoding.UTF8.GetBytes(metadata.Salt);
            int iterations = 10000;
            int keySize = 256;
            int blocSize = 8;

            using (Rfc2898DeriveBytes keyDeriver = new Rfc2898DeriveBytes(secretNameBytes, saltBytes, iterations, HashAlgorithmName.SHA256))
            {
                metadataKey = keyDeriver.GetBytes(keySize / blocSize);
            }

            MetaData decryptedMetadata;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = metadataKey;
                aesAlg.IV = Convert.FromBase64String(metadata.IV);
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                byte[] cipherBytes = Convert.FromBase64String(metadata.CipherText);

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (MemoryStream msPlain = new MemoryStream())
                {
                    csDecrypt.CopyTo(msPlain);
                    string decryptedJson = Encoding.UTF8.GetString(msPlain.ToArray());
                    decryptedMetadata = JsonConvert.DeserializeObject<MetaData>(decryptedJson) ?? throw new InvalidOperationException("Failed to deserialize metadata.");
                }
            }

            if (!Enum.TryParse<HostingEnvironment>(decryptedMetadata.HostingEnvironment, out var decryptedType))
            {
                throw new InvalidOperationException($"Invalid environment type: {decryptedMetadata.HostingEnvironment}");
            }

            if (decryptedType == HostingEnvironment.Development)
            {
                if (!DateTime.TryParse(decryptedMetadata.ExpirationTimestamp, out DateTime expirationTimestamp))
                {
                    throw new InvalidOperationException($"Invalid expiration timestamp format: {decryptedMetadata.ExpirationTimestamp}");
                }

                if (expirationTimestamp < DateTime.Now)
                {
                    throw new InvalidOperationException("The secret has expired.");
                }
            }

            byte[] key = Convert.FromBase64String(decryptedMetadata.Key);
            byte[] iv = Convert.FromBase64String(decryptedMetadata.IV);

            string cipherTextJson = File.ReadAllText(secretFilePath);
            SecretData cipherTextObj = JsonConvert.DeserializeObject<SecretData>(cipherTextJson)!;
            byte[] cipherTextBytes = Convert.FromBase64String(cipherTextObj.CipherText);

            string plainText = string.Empty;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (MemoryStream msPlain = new MemoryStream())
                {
                    csDecrypt.CopyTo(msPlain);
                    plainText = Encoding.UTF8.GetString(msPlain.ToArray());
                }
            }

            return plainText
                .Split("=;")
                .Select(item => new
                {
                    Key = item.Substring(0, item.IndexOf('=')),
                    Value = item.Substring(item.IndexOf('=') + 1)
                })
                .FirstOrDefault(x => x.Key == variable)?.Value ?? string.Empty;
        }
    }
}
