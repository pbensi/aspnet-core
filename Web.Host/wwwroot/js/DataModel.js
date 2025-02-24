class SecureDataRequest {
    constructor(ciphertext, iv, signature, publicKey, aesKey) {
        this.ciphertext = ciphertext;
        this.iv = iv;
        this.signature = signature;
        this.publicKey = publicKey;
        this.aesKey = aesKey;
    }
}
