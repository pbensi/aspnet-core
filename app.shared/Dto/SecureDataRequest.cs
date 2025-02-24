namespace app.shared.Dto
{
    public class SecureDataRequest
    {
        public int[] Ciphertext { get; set; }
        public int[] Iv { get; set; }
        public int[] Signature { get; set; }
        public int[] PublicKey { get; set; }
        public int[] AesKey { get; set; }
    }
}
