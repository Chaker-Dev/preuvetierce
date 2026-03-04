# 🛡️ PreuveTierce — Infrastructure de Confiance Numérique (SaaS-in-a-Box)

[![Framework - .NET 8](https://img.shields.io/badge/.NET-8.0-512bd4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Security - Cloudflare](https://img.shields.io/badge/Security-Cloudflare_WAF-f38020?logo=cloudflare)](https://www.cloudflare.com/)
[![Database - Firestore](https://img.shields.io/badge/Database-Firestore-ffca28?logo=firebase)](https://firebase.google.com/)
[![Licence - Propriétaire](https://img.shields.io/badge/Licence-Propriétaire-red)]()

---

## 📌 Résumé Exécutif

**PreuveTierce** est une solution complète de certification numérique et d’horodatage permettant de garantir l’intégrité et la preuve d’antériorité de documents numériques.

Basée sur le standard **RFC 3161**, l’application adopte une architecture **Zero-Knowledge** stricte : seul le hachage cryptographique (SHA-256) d’un document est traité.  
Les fichiers originaux ne sont jamais stockés ni conservés en clair sur le serveur.

Cette approche garantit un haut niveau de confidentialité tout en assurant une valeur probatoire solide.

---

## 🏗️ Architecture & Stack Technique

L’infrastructure est conçue pour être scalable, sécurisée et exploitable en production.

| Couche | Technologie | Rôle |
|--------|------------|------|
| **Application** | ASP.NET Core 8 (Razor Pages) | Moteur web performant et sécurisé |
| **Sécurité Edge** | Cloudflare (WAF, Rate Limiting, Proxy DNS) | Protection DDoS, filtrage IP, anti-bot |
| **Base de données** | Google Cloud Firestore | Stockage NoSQL des métadonnées & identités |
| **Horodatage (TSA)** | RFC 3161 (BouncyCastle) | Connexion aux autorités d’horodatage |
| **Email Transactionnel** | AWS SES (SMTP sécurisé) | 2FA, confirmations, notifications |
| **Génération PDF** | QuestPDF | Production d’attestations d’authenticité |
| **Logs & Audit** | Serilog | Journalisation structurée et traçabilité |
| **Hébergement** | Ubuntu 22.04 + Nginx + Systemd | Environnement Linux durci |

---

## 🔐 Sécurité & Conformité

### Modèle Zero-Knowledge

Le processus de certification suit cette séquence :

1. Calcul du **hash SHA-256** en mémoire
2. Envoi du hash à une Autorité d’Horodatage (TSA)
3. Réception du jeton d’horodatage `.tsr`
4. Stockage du jeton et des métadonnées uniquement

Aucun document original n’est conservé sur le serveur.

---

### Sécurité Applicative

- Authentification multi-facteurs (validation email)
- Limitation de débit sur endpoints sensibles (`/Login`, `/Upload`)
- Journalisation complète des actions
- Reverse proxy Nginx isolé
- Protection Cloudflare WAF
- Blocage des accès directs IP via firewall (UFW restreint aux plages Cloudflare)

---

## 🚀 Déploiement en Production

### Prérequis

- VPS Ubuntu 22.04+
- Runtime .NET 8 installé
- Nginx configuré en reverse proxy
- DNS Cloudflare activé
- Firewall UFW configuré

---

### Gestion des Secrets

Les identifiants sensibles ne doivent jamais être versionnés.

Utiliser des variables d’environnement :

```bash
export EmailSettings__SmtpUser="VOTRE_USER_SMTP"
export EmailSettings__SmtpPass="VOTRE_MDP_SMTP"
export Tsa__Username=""
export Tsa__Password=""
```

Ou via le service systemd :

```
Environment="EmailSettings__SmtpUser=..."
Environment="EmailSettings__SmtpPass=..."
```

---

### Exemple `appsettings.json` (Template Production)

```json
{
  "Tsa": {
    "Url": "https://freetsa.org/tsr"
  },
  "EmailSettings": {
    "SmtpServer": "email-smtp.eu-west-3.amazonaws.com",
    "SmtpPort": 587,
    "SenderEmail": "contact@preuvetierce.com"
  },
  "Firebase": {
    "ProjectId": "votre-projet-id"
  }
}
```

---

### Configuration Firebase

Un fichier de clé de service doit être présent sur le serveur :

```
firebase-auth.json
```

⚠️ Ce fichier est exclu du dépôt Git.

---

### Gestion du Service Linux

```
sudo systemctl status preuvetierce.service
sudo systemctl restart preuvetierce.service
sudo journalctl -u preuvetierce.service -f
```

---

## 📂 Structure du Projet

```
/PreuveTierce
├── /Pages
├── /Services
├── /Models
├── /Helpers
├── /wwwroot
├── firebase-auth.json (exclu)
├── appsettings.json
└── Program.cs
```

---

## 📈 Potentiel de Commercialisation

PreuveTierce est conçu comme une infrastructure prête à être commercialisée.

Modèles possibles :

- SaaS B2B (LegalTech, industrie, conformité)
- Licence marque blanche
- API d’horodatage pour applications tierces
- Déploiement on-premise pour environnements régulés

---

## 🧩 Transfert de Propriété

Inclut :

- Code source complet versionné
- Documentation de déploiement
- Assistance transfert infrastructure
- Support technique de transition (15 jours)

---

## 📅 Informations Techniques

Version architecture : Production v1  
Statut : Opérationnel  
Dernière mise à jour : 03/03/2026  

---

## ⚖️ Mentions Légales

PreuveTierce est un logiciel propriétaire.  
Toute reproduction ou revente sans autorisation est interdite.

---

## 🎯 Positionnement

PreuveTierce constitue une brique d’infrastructure de confiance numérique permettant la certification et l’horodatage probatoire de documents dans des environnements sensibles.

L’architecture permet une montée en charge horizontale et l’ouverture future d’API publiques.

---

**Fin du document**
