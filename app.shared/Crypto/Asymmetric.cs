using System.Security.Cryptography;
using System.Text;
using app.shared.Crypto.Dto;
using Newtonsoft.Json;

namespace app.shared.Securities
{
    public class Asymmetric
    {
        public static T ProcessSecureData<T>(DataRequestDto request)
        {
            byte[] ciphertext = request.Ciphertext.Select(b => (byte)b).ToArray();
            byte[] iv = request.Iv.Select(b => (byte)b).ToArray();
            byte[] signature = request.Signature.Select(b => (byte)b).ToArray();
            byte[] aesKey = request.AesKey.Select(b => (byte)b).ToArray();
            byte[] publicKey = request.PublicKey.Select(b => (byte)b).ToArray();

            using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);

            if (!ecdsa.VerifyData(ciphertext, signature, HashAlgorithmName.SHA256))
            {
                throw new UnauthorizedAccessException("Invalid signature.");
            }

            int tagSize = 16;
            Span<byte> tag = new byte[tagSize];
            ciphertext.AsSpan(ciphertext.Length - tagSize).CopyTo(tag);

            Span<byte> actualCiphertext = new byte[ciphertext.Length - tagSize];
            ciphertext.AsSpan(0, actualCiphertext.Length).CopyTo(actualCiphertext);

            Span<byte> plaintext = new byte[actualCiphertext.Length];

            using (var aes = new AesGcm(aesKey))
            {
                aes.Decrypt(iv, actualCiphertext, tag, plaintext);
            }

            string decryptedData = Encoding.UTF8.GetString(plaintext.ToArray());

            return JsonConvert.DeserializeObject<T>(decryptedData);
        }
    }
}
