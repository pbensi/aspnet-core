class ApiService {
    async signIn(encryptedData, signature, publicKey, aesKey) {
        const payload = {
            ciphertext: Array.from(encryptedData.ciphertext),
            iv: Array.from(encryptedData.iv),
            signature: Array.from(new Uint8Array(signature)),
            publicKey: publicKey,
            aesKey: aesKey
        };

        const response = await fetch('/Home/SwaggerSignIn', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        return await response.json();
    }

}
