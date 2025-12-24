# PreuveTierce

**PreuveTierce** is a lightweight digital integrity and timestamping service designed to act as a neutral third party for document evidence.

The service allows users to generate cryptographic fingerprints (hashes) of documents and obtain a verifiable timestamp, without uploading or storing the original files.

---

## 🔐 Concept

In many legal and contractual contexts, a common dispute is whether a digital document has been altered after acceptance.

PreuveTierce provides:
- Proof of **existence**
- Proof of **integrity**
- Proof of **timestamp**

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
- **TLS**: Let's Encrypt
- **Hash Algorithm**: SHA-256

---

## 📦 Data Privacy

- No document files are stored.
- Only cryptographic hashes and metadata are recorded.
- Designed to be compatible with GDPR principles (data minimization).

> ⚠️ PreuveTierce is **not a certification authority** and does not claim legal qualification under eIDAS.

---

## 🛡️ Legal Disclaimer

PreuveTierce provides **technical evidence**, not legal certification.

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
