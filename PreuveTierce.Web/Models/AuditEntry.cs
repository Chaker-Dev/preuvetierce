using Google.Cloud.Firestore;

namespace PreuveTierce.Web.Models
{
    [FirestoreData]
    public class AuditEntry
    {
        [FirestoreProperty]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [FirestoreProperty]
        public Timestamp OccurredAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);

        [FirestoreProperty]
        public string Action { get; set; } = "" ; // UPLOAD, VERIFY, DOWNLOAD

        [FirestoreProperty]
        public string UserId { get; set; } = "anonymous"; // "anonymous" ou l'ID de l'utilisateur

        [FirestoreProperty]
        public string DocumentHash { get; set; } = ""; // Le hash du document concerné

        [FirestoreProperty]
        public string IpAddress { get; set; } = "";

        [FirestoreProperty]
        public string Status { get; set; } = ""; // Success / Failure

        [FirestoreProperty]
        public string PreviousLogHash { get; set; } = "";// Pour le chaînage (Immuabilité)

        [FirestoreProperty]
        public string EntryHash { get; set; } = "";// Hash de cette ligne précise
    }
}
