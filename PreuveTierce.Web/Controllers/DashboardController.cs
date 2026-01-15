using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PreuveTierce.Web.Models;
using PreuveTierce.Web.Services.Interfaces;
using PreuveTierce.Web.ViewModels;
using System.Security.Claims;

namespace PreuveTierce.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ICertificationService _certificationService;
        private readonly IPdfGeneratorService _pdfGeneratorService;
        private readonly IFileHasherService _fileHasherService;
        public DashboardController(
            ICertificationService certificationService,
            IPdfGeneratorService  pdfGeneratorService,
            IFileHasherService    fileHasherService)
        {
            _certificationService = certificationService;
            _pdfGeneratorService = pdfGeneratorService;
            _fileHasherService = fileHasherService;
        }
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            var history = await _certificationService.GetUserHistoryAsync(userId);

            return View(history);
        }
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                return BadRequest("Hash manquant.");

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var certification = await _certificationService.GetByHashAsync(hash);

            if (certification == null)
                return NotFound("Certification introuvable.");

            if (certification.OwnerId != userId)
                return Forbid();
            CertificateData pdfData = certification.ToCertificateData("https://preuvetierce.fr");

            byte[] pdfBytes = _pdfGeneratorService.GenerateAttestation(pdfData);

            string fileName = $"Attestation_{certification.SerialNumber}.pdf";

            return File(
                pdfBytes,
                "application/pdf",
                fileName
            );
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile? file, string reference)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Veuillez sélectionner un fichier valide.");
                return await Index();
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError("", "Le fichier dépasse la taille limite de 10 Mo.");
                return await Index();
            }

            try
            {
                string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

                string fileHash;
                using (var stream = file.OpenReadStream())
                {
                    fileHash = await _fileHasherService.ComputeSha256Async(stream);
                }

                var existingDoc = await _certificationService.GetByHashAsync(fileHash);
                if (existingDoc != null)
                {
                    TempData["Warning"] = "Ce document a déjà été certifié !";
                    return RedirectToAction(nameof(Index));
                }

                var newCertif = new CertifiedDocument
                {
                    Hash = fileHash,
                    OwnerId = userId,
                    FileName = Path.GetFileName(file.FileName),
                    FileSize = file.Length,
                    CertifiedAt = DateTime.UtcNow,
                    Status = "Certified",
                    Reference = string.IsNullOrWhiteSpace(reference) ? $"DOC-{DateTime.UtcNow:yyyyMMdd-HHmmss}" : reference,
                    SerialNumber = $"PT-{DateTime.UtcNow.Year}-{fileHash.Substring(0, 8).ToUpper()}"
                };

                bool success = await _certificationService.RegisterCertificationAsync(newCertif);

                if (success)
                {
                    TempData["Success"] = "Document certifié et horodaté avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "Erreur lors de l'enregistrement de la certification.");
                    return await Index();
                }
            }
            catch (Exception ex)
            {
                // Log l'erreur réelle pour toi, mais affiche un message générique à l'utilisateur
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError("", "Une erreur technique est survenue.");
                return await Index();
            }
        }
    }
}
