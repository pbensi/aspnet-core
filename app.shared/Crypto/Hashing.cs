using System.Security.Cryptography;

namespace app.shared.Securities
{
    public static class Hashing
    {
        private const int SaltSize = 32;
        private const int Iterations = 100000;
        private const int HashSize = 64;

        public static string Hashed(string data)
        {
            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);

            byte[] salt = GenerateSalt();
            byte[] hash = GenerateHash(dataBytes, salt);

            string hashedString = $"{Convert.ToBase64String(salt)}:{Iterations}:{Convert.ToBase64String(hash)}";
            return hashedString;
        }
        public static bool Verify(string hashedData, string originalData)
        {
            string[] parts = hashedData.Split(':');
            if (parts.Length != 3)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            if (salt.Length != SaltSize)
            {
                return false;
            }

            int iterations = int.Parse(parts[1]);
            byte[] expectedHash = Convert.FromBase64String(parts[2]);

            byte[] originalDataBytes = System.Text.Encoding.UTF8.GetBytes(originalData);
            byte[] actualHash = GenerateHash(originalDataBytes, salt, iterations);

            return ByteArrayEquals(expectedHash, actualHash);
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
        private static byte[] GenerateHash(byte[] data, byte[] salt, int iterations = Iterations)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(data, salt, iterations, HashAlgorithmName.SHA512))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }
        private static bool ByteArrayEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}
