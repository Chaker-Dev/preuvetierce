using Google.Cloud.Firestore;
using PreuveTierce.Web.Models;
using PreuveTierce.Web.Services.Interfaces;

namespace PreuveTierce.Web.Services
{
    public class CertificationService : ICertificationService
    {
        private readonly FirestoreDb _db;
        private const string CollectionName = "Certifications";
        private readonly ILogger<CertificationService> _logger;

        public CertificationService(FirestoreDb db, ILogger<CertificationService> logger)
        {
            _db = db;
            _logger = logger;
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
                    { "TimestampToken", Blob.CopyFrom(document.TimestampToken) },
                    { "createdAt", Timestamp.FromDateTime(document.CertifiedAt.ToUniversalTime()) }
                };

                await docRef.SetAsync(data);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement Firestore pour le document {Hash}", document.Hash);
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
                CertifiedAt = doc.GetValue<Timestamp>("createdAt").ToDateTime(),
                TimestampToken = doc.TryGetValue("TimestampToken", out Google.Protobuf.ByteString byteString) ? byteString.ToByteArray() : null
            };
        }
       
    }
}
