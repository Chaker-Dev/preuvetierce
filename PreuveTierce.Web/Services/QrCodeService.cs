using PreuveTierce.Web.Services.Interfaces;
using QRCoder;

namespace PreuveTierce.Web.Services
{
    public class QrCodeService : IQrCodeService
    {
        public byte[] GeneratePng(string content)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(20);
                }
            }
        }
    }
}
