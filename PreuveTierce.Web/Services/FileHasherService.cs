using PreuveTierce.Web.Services.Interfaces;
using System.Security.Cryptography;

namespace PreuveTierce.Web.Services
{
    public class FileHasherService : IFileHasherService
    {
        public async Task<string> ComputeSha256Async(Stream fileStream)
        {
            if (fileStream.Position != 0 && fileStream.CanSeek)
                fileStream.Position = 0;

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = await sha256.ComputeHashAsync(fileStream);
                return Convert.ToHexString(hashBytes).ToLowerInvariant();
            }
        }
    }
}
