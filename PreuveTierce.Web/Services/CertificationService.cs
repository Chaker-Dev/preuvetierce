using Google.Cloud.Firestore;
using PreuveTierce.Web.Models;
using PreuveTierce.Web.Services.Interfaces;

namespace PreuveTierce.Web.Services
{
    public class CertificationService : ICertificationService
    {
        private readonly FirestoreDb _db;
        private const string CollectionName = "Certifications";
        public CertificationService(FirestoreDb db)
        {
            _db = db;
        }
        public async Task<List<CertifiedDocument>> GetUserHistoryAsync(string userId)
        {
            Query query = _db.Collection(CollectionName)
                              .WhereEqualTo("ownerId", userId);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.Select(MapToModel).ToList();
        }

        public async Task<CertifiedDocument> GetByHashAsync(string hash)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(hash);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists) return null;
            var cc = MapToModel(snapshot); 
            return MapToModel(snapshot);
        }

        public async Task<bool> RegisterCertificationAsync(CertifiedDocument document)
        {
            try
            {
                DocumentReference docRef = _db.Collection(CollectionName).Document(document.Hash);

                var data = new Dictionary<string, object>
                {
                    { "ownerId", document.OwnerId },
                    { "fileName", document.FileName },
                    { "fileSize", document.FileSize },
                    { "certificate_serial", document.SerialNumber },
                    { "reference ", document.Reference },
                    { "status", document.Status },
                    { "createdAt", Timestamp.FromDateTime(document.CertifiedAt.ToUniversalTime()) }
                };

                await docRef.SetAsync(data);
                return true;
            }
            catch (Exception ex)
            {
                // TO DO injecter ILogger pour tracer l'erreur
                Console.WriteLine($"Erreur Firestore : {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string hash)
        {
            DocumentReference docRef = _db.Collection(CollectionName).Document(hash);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            return snapshot.Exists;
        }
        public async Task<CertifiedDocument?> GetBySerialAsync(string certificateSerial)
        {
            var query = _db.Collection("Certifications")
                   .WhereEqualTo("certificate_serial", certificateSerial)
                   .Limit(1);

            var snapshot = await query.GetSnapshotAsync();
            if (!snapshot.Documents.Any()) return null;

            return MapToModel(snapshot.Documents.First());
        }

        private CertifiedDocument MapToModel(DocumentSnapshot doc)
        {
            return new CertifiedDocument
            {
                Hash = doc.Id,
                SerialNumber = doc.GetValue<string>("certificate_serial"),
                FileName = doc.GetValue<string>("fileName"),
                FileSize = doc.GetValue<long>("fileSize"),
                Reference = doc.GetValue<string>("reference "),
                Status = doc.GetValue<string>("status"),
                OwnerId = doc.GetValue<string>("ownerId"),
                CertifiedAt = doc.GetValue<Timestamp>("createdAt").ToDateTime()
            };
        }
       
    }
}
