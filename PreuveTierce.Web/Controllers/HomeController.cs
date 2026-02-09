using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PreuveTierce.Web.Models;
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
        private readonly IAuditService _auditService;

        public HomeController(
            ILogger<HomeController> logger,
            ICertificationService certificationService,
            IPdfGeneratorService pdfGeneratorService,
            IAuditService auditService)
        {
            _certificationService = certificationService;
            _logger = logger;
            _pdfGeneratorService = pdfGeneratorService;
            _auditService = auditService;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? serial)
        {
            _logger.LogInformation("Accès à la page d'accueil. Serial fourni : {Serial}", serial ?? "Aucun");

            if (string.IsNullOrWhiteSpace(serial))
            {
                return View();
            }
            try
            {
                var certification = await _certificationService.GetBySerialAsync(serial);

                if (certification == null)
                {
                    _logger.LogWarning("Recherche par Serial échouée : {Serial} introuvable en base.", serial);
                    await _auditService.SaveLogAsync("VERIFY_BY_SERIAL", $"SERIAL:{serial}", "NOT_FOUND", HttpContext);
                    ViewBag.VerifyError = "Certificat introuvable";
                    return View();
                }

                _logger.LogInformation("Certificat trouvé via Serial. Hash associé : {Hash}", certification.Hash);
                await _auditService.SaveLogAsync("VERIFY_BY_SERIAL", certification.Hash, "SUCCESS", HttpContext);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du certificat par Serial : {Serial}", serial);
                return View();
            }
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
            _logger.LogInformation("Tentative de vérification pour le hash {Hash}", hash);
            try
            {
                if (string.IsNullOrWhiteSpace(hash))
                    return BadRequest(new { success = false, message = "Hash invalide." });

                var certif = await _certificationService.GetByHashAsync(hash);
                await _auditService.SaveLogAsync(
                    action: "VERIFY",
                    docHash: hash,
                    status: (certif != null ? "SUCCESS" : "NOT_FOUND"),
                    context: HttpContext
                );
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur critique lors de la vérification du hash {Hash}", hash);
                return StatusCode(500);
            }
        }
        [HttpGet]
        public async Task<IActionResult> DownloadPublicCertificate(string hash)
        {
            _logger.LogInformation("Demande de téléchargement public pour le hash : {Hash}", hash);

            try
            {
                if (string.IsNullOrWhiteSpace(hash))
                {
                    _logger.LogWarning("Téléchargement avorté : Hash manquant.");
                    return BadRequest("L'empreinte numérique (hash) est requise.");
                }
                var certification = await _certificationService.GetByHashAsync(hash);

                if (certification == null)
                {
                    _logger.LogWarning("Téléchargement impossible : Aucun certificat pour le hash {Hash}", hash);
                    await _auditService.SaveLogAsync("DOWNLOAD_PUBLIC_CERT", hash, "NOT_FOUND", HttpContext);
                    return NotFound("Aucune preuve d'authenticité n'existe pour ce document.");
                }
                CertificateData pdfData = certification.ToCertificateData("https://preuvetierce.fr");
                _logger.LogInformation("Génération du certificat PDF en cours pour {Serial}...", certification.SerialNumber);
                await _auditService.SaveLogAsync("DOWNLOAD_PUBLIC_CERT", hash, "SUCCESS", HttpContext);

                byte[] pdfBytes = _pdfGeneratorService.GenerateAuthenticCertification(pdfData);

                string fileName = $"Certificat_Authenticite_{certification.SerialNumber}.pdf";

                _logger.LogInformation("Envoi du fichier PDF : {FileName}", fileName);

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la génération/téléchargement du PDF pour le hash {Hash}", hash);
                await _auditService.SaveLogAsync("DOWNLOAD_PUBLIC_CERT", hash, "ERROR_GENERATION", HttpContext);
                return StatusCode(500, "Erreur lors de la génération du document.");
            }
        }
        public IActionResult Privacy()
        {
            _logger.LogDebug("Consultation de la page Privacy.");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Une erreur d'application a été déclenchée. RequestID: {RequestId}", requestId);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
