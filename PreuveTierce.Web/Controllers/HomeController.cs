using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PreuveTierce.Web.Services.Interfaces;
using PreuveTierce.Web.ViewModels;
using System.Diagnostics;

namespace PreuveTierce.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICertificationService _certificationService;

        public HomeController(ILogger<HomeController> logger, ICertificationService certificationService)
        {
            _certificationService = certificationService;
            _logger = logger;
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
