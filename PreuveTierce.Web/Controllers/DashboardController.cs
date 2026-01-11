using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public DashboardController(ICertificationService certificationService,
            IPdfGeneratorService pdfGeneratorService)
        {
            _certificationService = certificationService;
            _pdfGeneratorService = pdfGeneratorService;
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
    }
}
