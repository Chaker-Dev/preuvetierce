
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Tsp;

namespace PreuveTierce.Web.Services.helpers
{
    public static class TsrParser
    {
        public static (string TsaName, DateTime TsaTimestamp) GetTsaInfo(byte[] tsrBytes)
        {
            try
            {
                var token = new TimeStampToken(ContentInfo.GetInstance(tsrBytes));

                // 1. Date (inchangé)
                DateTime tsaTimestamp = token.TimeStampInfo.GenTime;

                // 2. Récupération du Certificat (Version 2.6.2)
                // GetCertificates() retourne un IStore<X509CertificateHolder>
                var store = token.GetCertificates();

                // On récupère les correspondances. 
                // Note: SignerID implémente ISelector<X509CertificateHolder>
                //var matches =  store.GetMatches(token.SignerID);

                // On utilise Linq pour récupérer le premier certificat
                // Attention : matches est un IEnumerable (ou ICollection) de X509CertificateHolder
                var certHolder = matches.Cast<X509CertificateHolder>().FirstOrDefault();

                string tsaName = "Autorité d'horodatage inconnue";

                if (certHolder != null)
                {
                    // Dans BC 2.x, on accède au sujet directement via la propriété Subject
                    var subject = certHolder.Subject;
                    tsaName = subject.ToString();

                    // Extraction propre du CN (Common Name)
                    if (tsaName.Contains("CN="))
                    {
                        tsaName = tsaName.Split(',')
                                         .Select(part => part.Trim())
                                         .FirstOrDefault(part => part.StartsWith("CN="))
                                         ?.Replace("CN=", "") ?? tsaName;
                    }
                }

                return (tsaName, tsaTimestamp);
            }
            catch (Exception ex)
            {
                return ($"Erreur décodage : {ex.Message}", DateTime.MinValue);
            }
        }
    }
}
