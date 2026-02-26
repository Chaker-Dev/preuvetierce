namespace PreuveTierce.Web.Services.Interfaces
{
    public interface ITimestampService
    {
        /// <summary>
        /// Envoie un hash à l'autorité d'horodatage (TSA) et récupère le jeton signé.
        /// </summary>
        /// <param name="hash">Le hash du document (SHA-256)</param>
        /// <returns>Le jeton d'horodatage (TimeStepToken) au format binaire (.tsr)</returns>
        Task<byte[]> GetTimestampTokenAsync(byte[] hash);
    }
}
