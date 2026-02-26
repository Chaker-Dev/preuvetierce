namespace PreuveTierce.Web.Models
{
    public class CertifiedDocument
    {
        public string Hash { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string FileName { get; set; } = "";
        public long FileSize { get; set; }
        public string FileExtension => Path.GetExtension(FileName).ToLower();
        public DateTime CertifiedAt { get; set; }
        public string Reference { get; set; } = "";
        public string OwnerId { get; set; } = "";
        public string Status { get; set; } = "Certified";
        public byte[]? TimestampToken { get; set; }
    }
}
