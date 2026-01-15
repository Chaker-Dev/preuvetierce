namespace PreuveTierce.Web.Models
{
    public class CertificateData
    {
        public string SerialNumber { get; set; } = "";
        public DateTime IssueDate { get; set; }
        public string FileName { get; set; } = "";
        public string FileSizeFormatted { get; set; } = "";
        public long FileSizeBytes { get; set; }
        public string ClientReference { get; set; } = "";
        public DateTime DepositDateUtc { get; set; }
        public string FileHash { get; set; } = "";
        public string VerificationUrl { get; set; } = "";
    }
}
