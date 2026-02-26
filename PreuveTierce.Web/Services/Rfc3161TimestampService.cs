using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Tsp;
using PreuveTierce.Web.Services.Interfaces;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace PreuveTierce.Web.Services
{
    public class Rfc3161TimestampService : ITimestampService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Rfc3161TimestampService> _logger;

        public Rfc3161TimestampService(HttpClient httpClient, IConfiguration configuration, ILogger<Rfc3161TimestampService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<byte[]> GetTimestampTokenAsync(byte[] hash)
        {
            try
            {
                // 1. Récupération de l'URL du TSA (Autorité d'Horodatage)
                var tsaUrl = _configuration["Tsa:Url"];
                var tsaUsername = _configuration["Tsa:Username"];
                var tsaPassword = _configuration["Tsa:Password"];

                if (string.IsNullOrEmpty(tsaUrl))
                    throw new InvalidOperationException("L'URL du TSA n'est pas configurée.");

                // 2. Préparation de la requête RFC 3161
                var reqGen = new TimeStampRequestGenerator();

                // On demande explicitement le certificat du signataire dans la réponse (certReq = true)
                reqGen.SetCertReq(true);

                // On utilise SHA-256 (Standard actuel)
                var oid = NistObjectIdentifiers.IdSha256.Id;

                // On ajoute un "nonce" (nombre aléatoire) pour éviter les attaques par rejeu
                using var rng = RandomNumberGenerator.Create();
                byte[] nonceBytes = new byte[16];
                rng.GetBytes(nonceBytes);
                var nonce = new BigInteger(1, nonceBytes);

                // Génération de la requête
                var request = reqGen.Generate(oid, hash, nonce);
                byte[] requestBytes = request.GetEncoded();

                // 3. Envoi de la requête HTTP
                var content = new ByteArrayContent(requestBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/timestamp-query");

                // Gestion de l'authentification Basic 
                if (!string.IsNullOrEmpty(tsaUsername) && !string.IsNullOrEmpty(tsaPassword))
                {
                    var authBytes = System.Text.Encoding.ASCII.GetBytes($"{tsaUsername}:{tsaPassword}");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
                }

                var response = await _httpClient.PostAsync(tsaUrl, content);
                response.EnsureSuccessStatusCode();

                // 4. Traitement de la réponse
                if (response.Content.Headers.ContentType?.MediaType != "application/timestamp-reply")
                {
                    throw new InvalidOperationException("Le type de contenu de la réponse TSA est invalide.");
                }

                byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();

                // 5. Validation BouncyCastle
                var tsResponse = new TimeStampResponse(responseBytes);
                tsResponse.Validate(request); // Vérifie que la réponse correspond à notre requête (nonce, hash, algo)

                if (tsResponse.Status != 0 && tsResponse.Status != 1) // 0 = Granted, 1 = GrantedWithMods
                {
                    throw new Exception($"Erreur TSA : {tsResponse.GetStatusString()}");
                }

                // 6. Extraction du Token pur (le fichier .tsr)
                var token = tsResponse.TimeStampToken;
                return token.GetEncoded();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'horodatage RFC 3161.");
                throw;
            }
        }
    }
}
