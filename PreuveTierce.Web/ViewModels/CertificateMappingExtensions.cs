namespace PreuveTierce.Web.ViewModels
{
    public static class CertificateMappingExtensions
    {
        public static CertificateData ToCertificateData(
        this CertifiedDocument doc,
        string verificationBaseUrl)
        {
            return new CertificateData
            {
                SerialNumber = doc.SerialNumber,
                IssueDate = doc.CertifiedAt,
                DepositDateUtc = doc.CertifiedAt,
                FileName = doc.FileName,
                FileSizeBytes = doc.FileSize,
                FileSizeFormatted = FormatFileSize(doc.FileSize),
                ClientReference = doc.Reference,
                FileHash = doc.Hash,
                VerificationUrl = $"{verificationBaseUrl}/verify/{doc.Hash}"
            };
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} o";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} Ko";
            return $"{bytes / (1024.0 * 1024.0):F2} Mo";
        }
    }
}
