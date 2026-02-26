using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PreuveTierce.Web.Models;
using PreuveTierce.Web.Services.Interfaces;
using System.Security.Claims;

namespace PreuveTierce.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ICertificationService _certificationService;
        private readonly IPdfGeneratorService _pdfGeneratorService;
        private readonly IFileHasherService _fileHasherService;
        private readonly ITimestampService _stampService;
        private readonly ILogger<DashboardController> _logger;
        private readonly IAuditService _auditService;

        public DashboardController(
            ICertificationService certificationService,
            IPdfGeneratorService pdfGeneratorService,
            IFileHasherService fileHasherService,
            ILogger<DashboardController> logger,
            IAuditService auditService,
            ITimestampService stampService)
        {
            _certificationService = certificationService;
            _pdfGeneratorService = pdfGeneratorService;
            _fileHasherService = fileHasherService;
            _logger = logger;
            _auditService = auditService;
            _stampService = stampService;
        }
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _logger.LogInformation("Consultation du dashboard par l'utilisateur {UserId}", userId);

            await _auditService.SaveLogAsync("DASHBOARD_VIEW", "N/A", "SUCCESS", HttpContext);

            try
            {
                var history = await _certificationService.GetUserHistoryAsync(userId);
                _logger.LogDebug("Historique récupéré : {Count} documents pour l'utilisateur {UserId}", history.Count(), userId);
                return View(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'historique pour {UserId}", userId);
                return View(new List<CertifiedDocument>());
            }
        }
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(string hash)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (string.IsNullOrWhiteSpace(hash))
            {
                _logger.LogWarning("Tentative de téléchargement sans hash par {UserId}", userId);
                return BadRequest("Hash manquant.");
            }

            try
            {
                var certification = await _certificationService.GetByHashAsync(hash);

                if (certification == null)
                {
                    _logger.LogWarning("Téléchargement échoué : Hash {Hash} introuvable (demandé par {UserId})", hash, userId);
                    await _auditService.SaveLogAsync("DOWNLOAD_ATTESTATION", hash, "NOT_FOUND", HttpContext);
                    return NotFound("Certification introuvable.");
                }
                if (certification.OwnerId != userId)
                {
                    _logger.LogCritical("ALERTE SÉCURITÉ : L'utilisateur {UserId} a tenté de télécharger un document appartenant à {OwnerId}. Hash: {Hash}",
                        userId, certification.OwnerId, hash);
                    await _auditService.SaveLogAsync("DOWNLOAD_ATTESTATION_UNAUTHORIZED", hash, "FORBIDDEN", HttpContext);
                    return Forbid();
                }
                _logger.LogInformation("Génération de l'attestation de dépôt pour le document {SerialNumber}", certification.SerialNumber);
                CertificateData pdfData = certification.ToCertificateData("https://preuvetierce.fr");
                await _auditService.SaveLogAsync("DOWNLOAD_ATTESTATION", hash, "SUCCESS", HttpContext);
                byte[] pdfBytes = _pdfGeneratorService.GenerateAttestation(pdfData);

                string fileName = $"Attestation_{certification.SerialNumber}.pdf";

                return File(
                    pdfBytes,
                    "application/pdf",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du téléchargement PDF pour le hash {Hash}", hash);
                return StatusCode(500);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile? file, string reference)
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Upload annulé : Fichier vide ou nul par {UserId}", userId);
                ModelState.AddModelError("", "Veuillez sélectionner un fichier valide.");
                return await Index();
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                _logger.LogWarning("Fichier refusé : Taille trop grande ({Size} octets)", file.Length);
                ModelState.AddModelError("", "Le fichier dépasse la taille limite de 10 Mo.");
                return await Index();
            }

            try
            {
                string fileHash;
                byte[] hashBytes;
                using (var stream = file.OpenReadStream())
                {
                    hashBytes = await _fileHasherService.ComputeSha256BytesAsync(stream);
                    fileHash = await _fileHasherService.ComputeSha256Async(stream);
                }

                _logger.LogInformation("Fichier haché avec succès : {Hash}", fileHash);
                var existingDoc = await _certificationService.GetByHashAsync(fileHash);
                if (existingDoc != null)
                {
                    _logger.LogInformation("Tentative de double certification pour le hash {Hash}. Action bloquée.", fileHash);
                    await _auditService.SaveLogAsync("UPLOAD_CERTIFICATION", fileHash, "DUPLICATE", HttpContext);
                    TempData["Warning"] = "Ce document a déjà été certifié !";
                    return RedirectToAction(nameof(Index));
                }
                // 3. Appel au service d'horodatage (TSA)
                _logger.LogInformation("Appel au service TSA pour le hash {Hash}", fileHash);
                byte[] tsrToken = await _stampService.GetTimestampTokenAsync(hashBytes);

                var newCertif = new CertifiedDocument
                {
                    Hash = fileHash,
                    OwnerId = userId,
                    FileName = Path.GetFileName(file.FileName),
                    FileSize = file.Length,
                    CertifiedAt = DateTime.UtcNow,
                    Status = "Certified",
                    TimestampToken = tsrToken,
                    Reference = string.IsNullOrWhiteSpace(reference) ? $"DOC-{DateTime.UtcNow:yyyyMMdd-HHmmss}" : reference,
                    SerialNumber = $"PT-{DateTime.UtcNow.Year}-{fileHash.Substring(0, 8).ToUpper()}"
                };

                _logger.LogInformation("Enregistrement de la nouvelle certification : {SerialNumber}", newCertif.SerialNumber);
                bool success = await _certificationService.RegisterCertificationAsync(newCertif);
                await _auditService.SaveLogAsync("UPLOAD_CERTIFICATION", fileHash, success ? "SUCCESS" : "FAILURE", HttpContext);
                
                if (success)
                {
                    _logger.LogInformation("Certification réussie et enregistrée en base pour {SerialNumber}", newCertif.SerialNumber);
                    TempData["Success"] = "Document certifié et horodaté avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogError("Échec de l'enregistrement en base de données pour le hash {Hash}", fileHash);
                    ModelState.AddModelError("", "Erreur lors de l'enregistrement de la certification.");
                    return await Index();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur technique lors de l'upload du fichier {FileName}", file.FileName);
                await _auditService.SaveLogAsync("UPLOAD_CERTIFICATION", "UNKNOWN", "CRASH", HttpContext);
                ModelState.AddModelError("", "Une erreur technique est survenue.");
                return await Index();
            }
        }
    }
}
