# TuColmado Auth Service

![Node.js](https://img.shields.io/badge/Node.js-18+-green.svg)
![Type](https://img.shields.io/badge/Language-TypeScript-blue.svg)

**TuColmado Auth Service** is a lightweight, cloud-based microservice governing tenant registrations, identity orchestration, and secure subscription allocations for the `TuColmadoRD` ecosystem. Built in Node.js and TypeScript, this service serves as the cloud anchor ensuring hybrid-offline devices remain synchronized.

---

## 🔒 Security Architecture

### Key Pair Generation
The `scripts/generate-keys.ts` utility programmatically synthesizes `RS256` (RSA) public/private keypairs securely holding signing responsibilities at the core server level.

### Hybrid Offline Authentication Flow
1. **Pairing**: `/pair-device` establishes the foundational trust anchor, broadcasting the public-key to the specific `TuColmadoRD` on-premises terminal.
2. **Licensing**: Using `/auth/renew-license`, the central server authorizes subscription lifespans generating securely signed offline `RS256` JSON Web Tokens holding expiration and ownership claims without requiring internet connections locally.

---

## 💻 Getting Started

### Pre-Requisites
- Node.js > 18
- Install Dependencies via `pnpm`:
```bash
npm install -g pnpm
pnpm install
```

### Development
```bash
pnpm run dev
```

### Generating RSA Certificates
Ensure key files exist locally before booting:
```bash
pnpm run tsx scripts/generate-keys.ts
```

---

## 🐳 Deployment (CI/CD)
The container build profile is available in the root `Dockerfile`. 
The system runs via remote connection orchestration through `.github/workflows/devops.yml`, mapping sequentially to the overarching Docker compose overlay inside the global colmado network. Tests are natively bundled and ran by `Vitest` protecting against bad PR integrations targeting the `QA` and `master` environments.
