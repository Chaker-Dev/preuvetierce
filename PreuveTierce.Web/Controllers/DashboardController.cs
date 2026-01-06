using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PreuveTierce.Web.ViewModels;

namespace PreuveTierce.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly FirestoreDb _db;
        public DashboardController(FirestoreDb db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            string userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

            Query query = _db.Collection("Certifications")
                             .WhereEqualTo("ownerId", userId);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            var history = snapshot.Documents.Select(doc => new CertificationHistoryViewModel
            {
                CertificateSerial = doc.GetValue<string>("certificate_serial"),
                FileName = doc.GetValue<string>("fileName"),
                DocumentHash = doc.Id,
                Reference = doc.GetValue<string>("reference "),
                Status = doc.GetValue<string>("status"),
                CreatedAt = doc.GetValue<Timestamp>("createdAt").ToDateTime()
            }).ToList();

            return View(history);
        }
    }
}
