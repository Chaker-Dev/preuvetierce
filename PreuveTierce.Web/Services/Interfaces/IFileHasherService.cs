namespace PreuveTierce.Web.Services.Interfaces
{
    public interface IFileHasherService
    {
        Task<string> ComputeSha256Async(Stream fileStream);
        Task<byte[]> ComputeSha256BytesAsync(Stream stream);
    }
}
