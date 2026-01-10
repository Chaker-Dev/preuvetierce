namespace PreuveTierce.Web.Services.Interfaces
{
    public interface IQrCodeService
    {
        byte[] GeneratePng(string content);
    }
}
