using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PreuveTierce.Web.Models;
using PreuveTierce.Web.Services;
using PreuveTierce.Web.Services.Interfaces;
using PreuveTierce.Web.ViewModels;
using System.Diagnostics;

namespace PreuveTierce.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICertificationService _certificationService;
        private readonly IPdfGeneratorService _pdfGeneratorService;

        public HomeController(ILogger<HomeController> logger, 
            ICertificationService certificationService,
            IPdfGeneratorService pdfGeneratorService)
        {
            _certificationService = certificationService;
            _logger = logger;
            _pdfGeneratorService = pdfGeneratorService;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
            {
                return View();
            }
            var certification = await _certificationService.GetBySerialAsync(serial);

            if (certification == null)
            {
                ViewBag.VerifyError = "Certificat introuvable";
                return View();
            }

            var model = new VerifyPresenceViewModel
            {
                CertificateSerial = certification.SerialNumber,
                CreatedAtUtc = certification.CertifiedAt,
                FileName = certification.FileName,
                FileSizeBytes = certification.FileSize,
                FileSizeFormatted = FormatFileSize(certification.FileSize),
                Hash = certification.Hash,
                Exists = true
            };

            return View(model);
        }
        private string FormatFileSize(long bytes)
        {
            return bytes >= 1_000_000
                ? $"{bytes / 1_000_000.0:F2} MB"
                : $"{bytes / 1024.0:F2} KB";
        }
        [HttpGet]
        public async Task<IActionResult> VerifyDocument(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return BadRequest(new { success = false, message = "Hash invalide." });

            var certif = await _certificationService.GetByHashAsync(hash);

            if (certif == null)
            {
                return Ok(new { success = false, message = "Aucun certificat trouvé pour ce document." });
            }
            return Ok(new
            {
                success = true,
                fileName = certif.FileName,
                date = certif.CertifiedAt.ToString("dd MMMM yyyy à HH:mm"),
                serial = certif.SerialNumber,
                status = certif.Status
            });
        }
        [HttpGet]
        public async Task<IActionResult> DownloadPublicCertificate(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return BadRequest("L'empreinte numérique (hash) est requise.");

            // 1. On récupère la certification dans Firestore via le hash
            var certification = await _certificationService.GetByHashAsync(hash);

            if (certification == null)
                return NotFound("Aucune preuve d'authenticité n'existe pour ce document.");

            // 💡 Note : On ne vérifie pas l'OwnerId ici car c'est une vérification publique.
            // Quiconque a le hash prouve qu'il détient le document original.

            // 2. Préparation des données pour le PDF
            // On passe l'URL de base pour le futur QR Code
            CertificateData pdfData = certification.ToCertificateData("https://preuvetierce.fr");

            // 3. Génération du PDF "Authentic" (Le design Or/Premium)
            byte[] pdfBytes = _pdfGeneratorService.GenerateAuthenticCertification(pdfData);

            // 4. Nom du fichier
            string fileName = $"Certificat_Authenticite_{certification.SerialNumber}.pdf";

            return File(
                pdfBytes,
                "application/pdf",
                fileName
            );
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
