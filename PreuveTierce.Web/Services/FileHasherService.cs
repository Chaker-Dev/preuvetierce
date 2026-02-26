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

        public async Task<byte[]> ComputeSha256BytesAsync(Stream stream)
        {
            using var sha256 = SHA256.Create();
            return await sha256.ComputeHashAsync(stream);
        }
    }
}
