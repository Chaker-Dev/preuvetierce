using PreuveTierce.Web.Models;

namespace PreuveTierce.Web.Services.Interfaces
{
    public interface IPdfGeneratorService
    {
        byte[] GenerateAttestation(CertificateData data);
        byte[] GenerateAuthenticCertification(CertificateData data);
    }
}
