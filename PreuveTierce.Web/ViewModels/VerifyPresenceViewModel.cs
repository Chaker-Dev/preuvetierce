
namespace PreuveTierce.Web.ViewModels
{
    public class VerifyPresenceViewModel
    {
        public string CertificateSerial { get; set; } = "";
        public DateTime CreatedAtUtc { get; set; }
        public string FileName { get; set; } = "";
        public long FileSizeBytes { get; set; }
        public string FileSizeFormatted { get; set; } = "";
        public string Hash { get; set; } = "";
        public bool HasTimestampToken  {get; set; }
        public bool Exists { get; set; }

    }
}
