class EncryptionService {
    async generateAesKey() {
        return await window.crypto.subtle.generateKey(
            {
                name: "AES-GCM",
                length: 256
            },
            true,
            ["encrypt", "decrypt"]
        );
    }

    async encryptData(key, data) {
        const iv = window.crypto.getRandomValues(new Uint8Array(12)); // 96-bit nonce
        const encodedData = new TextEncoder().encode(data);
        const ciphertext = await window.crypto.subtle.encrypt(
            {
                name: "AES-GCM",
                iv: iv
            },
            key,
            encodedData
        );

        return {
            ciphertext: new Uint8Array(ciphertext),
            iv: iv
        };
    }

    async exportAesKey(key) {
        const exported = await window.crypto.subtle.exportKey("raw", key);
        return Array.from(new Uint8Array(exported));
    }
}
