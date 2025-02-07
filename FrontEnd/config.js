// config.js
const config = {
  // apiUrl: "https://FoodMart-env.eba-gymnwyjm.us-east-2.elasticbeanstalk.com/",
  apiUrl: "https://localhost:7221/",
  publicKey: `-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0U57xt00937OSlt/QWbg
sK06UPEoEftbosYRKRecPXN5EA4c3PDECVwHjPTFDaoTcGWpgZkUkrpfxpenjF0O
WCd4PYrEhHCf9L+bSGd0b1Cu/Es/i2fpLA6HKc0t46gBokkFdIiia9xes2tggFAO
JyqTBm91vzHG6R0VypkMSobPgQF5AvhN6MHXFbT74KCBVyTZV/UDP+QQLDawHpeY
77MxKgBB1nVXusab/vkc9/mwqd5Vs4yJGkLb7PCfX1E2Uvq9k9WzjU7q8AFRXBZE
Xk/4aqF0ot3YiK3BCyI3YOWWhBynePDtijLJRcdaRG08jdUCUVaPM8fNOXaC6vGT
lQIDAQAB
-----END PUBLIC KEY-----
`,
};

// ** Use RSA to encrypt password**
async function encryptPassword(password, publicKeyPem) {
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
async function importPublicKey(pem) {
  const binaryDer = window.atob(
    pem.replace(/(-----(BEGIN|END) PUBLIC KEY-----|\n)/g, "")
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

// export config
window.config = config; // Link config to window object for global access
