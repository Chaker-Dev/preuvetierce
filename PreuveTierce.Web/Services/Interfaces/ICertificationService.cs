using PreuveTierce.Web.Models;

namespace PreuveTierce.Web.Services.Interfaces
{
    public interface ICertificationService
    {
        Task<List<CertifiedDocument>> GetUserHistoryAsync(string userId);
        Task<CertifiedDocument> GetByHashAsync(string hash);
        Task<CertifiedDocument?> GetBySerialAsync(string certificateSerial);
        Task<bool> RegisterCertificationAsync(CertifiedDocument document);
        Task<bool> ExistsAsync(string hash);
    }
}
