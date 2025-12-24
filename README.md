# PreuveTierce

**PreuveTierce** is a high-performance, lightweight digital evidence service designed to act as a neutral Technical Witness for document integrity.

In an era where digital tampering is effortless, PreuveTierce provides a cryptographically secure method to prove that a document existed in a specific state at a specific point in time without ever compromising user privacy.
---

## 🔐 The "Zero-Knowledge" Concept

PreuveTierce solves the "Trust Gap" in digital transactions. We don't ask you to trust us with your files; we ask you to trust the mathematics of cryptography.

PreuveTierce provides:
- Proof of **existence:** Prove a file existed at a specific UTC second
- Proof of **integrity:** Ensure not a single pixel or character has changed since timestamping.
- Proof of **Zero-Storage Policy:** Your documents never leave your local environment. Only the 64-character SHA-256 hash (fingerprint) is transmitted to our vault.

By recording only the document hash, PreuveTierce never stores the document itself.

---

## ⚙️ How it works

1. The user uploads a document (or submits its hash).
2. The system computes a cryptographic hash (SHA-256).
3. The hash is recorded with:
   - Timestamp (UTC)
   - User identifier
   - Unique certificate reference
4. A PDF proof certificate is generated, including:
   - Hash value
   - Timestamp
   - Unique reference
   - QR Code for verification

---

## 🧱 Technical Stack (planned)

- **Backend**: .NET (ASP.NET Core)
- **Web**: MVC / Minimal API
- **Database**: SQLite
- **Web Server**: Nginx (reverse proxy)
- **OS**: Ubuntu 22.04 LTS
- **TLS**: Let's Encrypt TLS with A+ Security Rating.
- **Hash Algorithm**: SHA-256

---

## 📦 Privacy by Design (GDPR+)

- No document files are stored.
- Only cryptographic hashes and metadata are recorded.
- Designed to be compatible with GDPR principles (data minimization).

> ⚠️ PreuveTierce is **not a certification authority** and does not claim legal qualification under eIDAS.

---

## 🛡️ Legal Disclaimer

[!IMPORTANT] PreuveTierce provides **technical evidence**, While built on international standards (similar to RFC 3161), it is currently a private technical service and not a "Qualified Trust Service Provider" (QTSP) under eIDAS.

The legal value of the generated proof depends on:
- Jurisdiction
- Context of use
- Judicial interpretation

Users remain responsible for how the evidence is used.

---

## 🚀 Project Status

- [x] Domain & VPS configured
- [x] HTTPS (Let's Encrypt)
- [ ] .NET backend implementation
- [ ] SQLite integration
- [ ] PDF certificate generation
- [ ] Public verification endpoint

---

## 🧭 Roadmap

- Phase 1: Minimal Proof API
- Phase 2: User accounts
- Phase 3: PDF certificate + QR verification
- Phase 4: Public proof verification page

---

## 👤 Author

Project initiated and maintained by **Chaker Aich**.

---

## 📄 License

MIT License
