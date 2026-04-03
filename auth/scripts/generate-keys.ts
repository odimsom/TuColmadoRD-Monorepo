import crypto from "crypto";
import fs from "fs";
import path from "path";

const generateKeys = () => {
  const { publicKey, privateKey } = crypto.generateKeyPairSync("rsa", {
    modulusLength: 2048,
    publicKeyEncoding: {
      type: "spki",
      format: "pem",
    },
    privateKeyEncoding: {
      type: "pkcs8",
      format: "pem",
    },
  });

  const keysDir = path.join(__dirname, "..", "keys");
  if (!fs.existsSync(keysDir)) {
    fs.mkdirSync(keysDir, { recursive: true });
  }

  fs.writeFileSync(path.join(keysDir, "private.pem"), privateKey);
  fs.writeFileSync(path.join(keysDir, "public.pem"), publicKey);

  console.log("RSA Key pair generated in keys/ directory.");
};

generateKeys();
