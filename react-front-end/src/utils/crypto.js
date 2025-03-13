// ** Use RSA to encrypt password**
export async function encryptPassword(password, publicKeyPem) {
  const encoder = new TextEncoder();
  const encodedPassword = encoder.encode(password);

  // Convert PEM public key into CryptoKey
  const publicKey = await importPublicKey(publicKeyPem);

  // Use RSA encryption
  const encrypted = await window.crypto.subtle.encrypt(
    { name: "RSA-OAEP" },
    publicKey,
    encodedPassword
  );

  return btoa(String.fromCharCode(...new Uint8Array(encrypted))); // Convert to Base64
}

// **Decrypt PEM public key**
export async function importPublicKey(pem) {
  const binaryDer = window.atob(
    pem.replace(/(-----(BEGIN|END) PUBLIC KEY-----|\n|\r|\s)/g, "")
  );
  const binaryArray = new Uint8Array(binaryDer.length);
  for (let i = 0; i < binaryDer.length; i++) {
    binaryArray[i] = binaryDer.charCodeAt(i);
  }

  return window.crypto.subtle.importKey(
    "spki",
    binaryArray.buffer,
    { name: "RSA-OAEP", hash: "SHA-256" },
    true,
    ["encrypt"]
  );
}
