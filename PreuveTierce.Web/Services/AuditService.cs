using Google.Cloud.Firestore;
using PreuveTierce.Web.Models;
using PreuveTierce.Web.Services.Interfaces;

namespace PreuveTierce.Web.Services
{
    public class AuditService : IAuditService
    {
        private readonly FirestoreDb _db;
        private const string CollectionName = "AuditLogs";
        public AuditService(FirestoreDb db) => _db = db;

        public async Task SaveLogAsync(string action, string docHash, string status, HttpContext context)
        {
            // 1. Récupérer l'ID de l'utilisateur ou "Anonymous"
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            // 2. Récupérer le hash du dernier log (pour le chaînage)
            var lastLogQuery = await _db.Collection(CollectionName)
                                        .OrderByDescending("OccurredAt")
                                        .Limit(1)
                                        .GetSnapshotAsync();

            string lastHash = lastLogQuery.Documents.Any()
                ? lastLogQuery.Documents[0].GetValue<string>("EntryHash")
                : "GENESIS_BLOCK";

            var entry = new AuditEntry
            {
                Action = action,
                DocumentHash = docHash,
                UserId = userId,
                Status = status,
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                PreviousLogHash = lastHash
            };

            // 3. Calculer le hash de CETTE entrée (La preuve d'immuabilité)
            entry.EntryHash = ComputeSHA256($"{entry.OccurredAt}|{entry.Action}|{entry.DocumentHash}|{entry.PreviousLogHash}");

            // 4. Enregistrer dans Firestore
            await _db.Collection(CollectionName).Document(entry.Id).SetAsync(entry);
        }
        private string ComputeSHA256(string rawData)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
