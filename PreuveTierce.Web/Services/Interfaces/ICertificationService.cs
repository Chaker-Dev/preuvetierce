using PreuveTierce.Web.ViewModels;

namespace PreuveTierce.Web.Services.Interfaces
{
    public interface ICertificationService
    {
        Task<List<CertifiedDocument>> GetUserHistoryAsync(string userId);
        Task<CertifiedDocument> GetByHashAsync(string hash);
        Task<bool> RegisterCertificationAsync(CertifiedDocument document);
        Task<bool> ExistsAsync(string hash);
    }
}
