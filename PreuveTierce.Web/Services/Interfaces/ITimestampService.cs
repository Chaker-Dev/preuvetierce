namespace PreuveTierce.Web.Services.Interfaces
{
    public interface ITimestampService
    {
        Task<byte[]> GetTimestampTokenAsync(byte[] hash);
    }
}
