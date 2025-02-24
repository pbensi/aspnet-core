class SignatureService {
    async generateKeyPair() {
        return await window.crypto.subtle.generateKey(
            {
                name: "ECDSA",
                namedCurve: "P-256"
            },
            true,
            ["sign", "verify"]
        );
    }

    async signData(privateKey, data) {
        return await window.crypto.subtle.sign(
            {
                name: "ECDSA",
                hash: { name: "SHA-256" }
            },
            privateKey,
            data
        );
    }

    async exportPublicKey(publicKey) {
        const exported = await window.crypto.subtle.exportKey("spki", publicKey);
        return Array.from(new Uint8Array(exported));
    }
}
