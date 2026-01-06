namespace PreuveTierce.Web.ViewModels
{
    public class CertificationHistoryViewModel
    {
        public string CertificateSerial { get; set; } = "";
        public string FileName { get; set; } = "";
        public string DocumentHash { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string Reference { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
