using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PreuveTierce.Web.Services.Interfaces;
using PreuveTierce.Web.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PreuveTierce.Web.Services
{
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private readonly IQrCodeService _qrCodeService;
        // Couleurs de la charte
        private static readonly Color BleuPreuve = Color.FromHex("#000091");
        private static readonly Color OrAuthentique = Color.FromHex("#D4AF37"); // Nouvelle couleur pour le certif authentique
        private static readonly Color GrisTexte = Colors.Grey.Darken2;
        private static readonly Color GrisClairLabel = Colors.Grey.Medium;
        private static readonly Color GrisFondTableau = Colors.Grey.Lighten4;

        // Injection du service QR Code (Constructor Injection)
        public PdfGeneratorService(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        public byte[] GenerateAttestation(CertificateData data)
        {
            return CreatePdfDocument(data, "ATTESTATION DE DÉPÔT", "Preuve d'enregistrement numérique", BleuPreuve);
        }

        public byte[] GenerateAuthenticCertification(CertificateData data)
        {
            return CreatePdfDocument(data, "CERTIFICAT D'AUTHENTICITÉ", "Document légal certifié conforme", OrAuthentique);
        }

        private byte[] CreatePdfDocument(CertificateData data, string title, string subtitle, Color couleurPrincipale)
        {
            var qrImage = _qrCodeService.GeneratePng(data.VerificationUrl);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial).FontColor(GrisTexte));

                    page.Content().Column(col =>
                    {
                        col.Spacing(20);

                        // ===== HEADER =====
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(t =>
                                {
                                    t.Span("PREUVE").FontSize(24).Bold().FontColor(couleurPrincipale);
                                    t.Span("TIERCE").FontSize(24).Bold().FontColor(Colors.Grey.Darken3);
                                });
                            });

                            row.ConstantItem(200).AlignRight().Column(c =>
                            {
                                c.Item().Text(txt =>
                                {
                                    txt.Span("N° de Certificat : ").Bold();
                                    txt.Span(data.SerialNumber);
                                });
                                c.Spacing(1.3f);
                                c.Item().Text(txt =>
                                {
                                    txt.ParagraphSpacing(1.5f);
                                    txt.Span("Date d'émission : ").Bold();
                                    txt.Span(data.IssueDate.ToString("dd MMMM yyyy"));
                                });
                            });
                        });
                        col.Item().PaddingBottom(16).BorderBottom(2).BorderColor(BleuPreuve);
                        // ===== TITRE =====

                        col.Item().PaddingBottom(10).Column(titlesCol =>
                        {
                            titlesCol.Spacing(2);
                            titlesCol.Item().AlignCenter().Text(title)
                                .Bold().FontSize(26).FontColor(Colors.Grey.Darken3);

                            titlesCol.Item().AlignCenter().Text(subtitle)
                                .FontSize(14).FontColor(GrisClairLabel);
                        });

                        // ===== TEXTE LEGAL =====
                        col.Item().PaddingBottom(20).Text(txt =>
                        {
                            txt.ParagraphSpacing(1.5f);
                            txt.Span("Il est certifié par la présente que le fichier numérique décrit ci-dessous a été déposé, analysé et horodaté électroniquement sur la plateforme sécurisée ").FontSize(12);
                            txt.Span("PreuveTierce.fr").FontSize(12).Bold();
                            txt.Span(". L'empreinte cryptographique unique de ce document a été enregistrée de manière inaltérable.").FontSize(12);
                        });

                        // ===== TABLEAU INFOS =====
                        col.Item().PaddingBottom(30).Background(GrisFondTableau).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(20)
                .Table(table =>
                {
                    // Définition des colonnes (Labels 40%, Valeurs 60%)
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(180);
                        cols.RelativeColumn();
                    });

                    // Ligne 1
                    table.Cell().PaddingBottom(8).Text("Nom du fichier original :").FontColor(GrisClairLabel);
                    table.Cell().PaddingBottom(8).Text(data.FileName).Bold().FontColor(Colors.Black);
                    // Ligne 2
                    table.Cell().PaddingBottom(8).Text("Taille du fichier :").FontColor(GrisClairLabel);
                    table.Cell().PaddingBottom(8).Text($"{data.FileSizeFormatted} ({data.FileSizeBytes:N0} octets)").Bold().FontColor(Colors.Black);
                    // Ligne 3
                    table.Cell().PaddingBottom(8).Text("Référence Client :").FontColor(GrisClairLabel);
                    table.Cell().PaddingBottom(8).Text(data.ClientReference).Bold().FontColor(Colors.Black);
                    // Ligne 4
                    table.Cell().Text("Date de dépôt (UTC) :").FontColor(GrisClairLabel);
                    table.Cell().Text(data.DepositDateUtc.ToString("dd MMM yyyy à HH:mm:ss")).Bold().FontColor(Colors.Black);
                });

                        // Section du Hash
                        col.Item().PaddingBottom(10).Text("Empreinte Numérique (SHA-256)")
                            .FontColor(BleuPreuve).Bold().FontSize(14);

                        // Le bloc du Hash avec la barre bleue à gauche
                        col.Item().PaddingTop(10)
                            .Background(GrisFondTableau)
                            .BorderLeft(4).BorderColor(BleuPreuve)
                            .Padding(10)
                            .Text(data.FileHash)
                            .FontFamily(Fonts.Consolas).FontSize(12);

                        col.Item().PaddingTop(5).Text("* Ce code est unique au monde. La moindre modification d'un pixel ou d'une virgule dans le document original modifierait radicalement cette empreinte.")
                            .FontSize(9).FontColor(GrisClairLabel);
                        // ===== FOOTER QR =====
                        col.Item().Row(row =>
                            {
                                row.ConstantItem(90).Height(90).Image(qrImage);

                                row.RelativeItem().PaddingLeft(20).Column(c =>
                                {
                                    c.Item().Text("Vérification de l’authenticité :").Bold().FontSize(10);
                                    c.Spacing(1.5f);
                                    c.Item().Text($"Scannez le QR Code ou visitez {data.VerificationUrl}").FontSize(10).FontColor(BleuPreuve).Underline();
                                    c.Spacing(1.5f);
                                    c.Item().Text("Puis saisissez le numéro de certificat.")
                                        .FontSize(10);
                                    c.Spacing(1.5f);
                                    c.Item().PaddingTop(10).Text(
                                        "Ce document est généré automatiquement. PreuveTierce agit en tant que tiers de confiance technique."
                                    ).FontSize(8).FontColor(Colors.Grey.Darken1);
                                });
                            });
                    });

                    // ===== WATERMARK =====
                    page.Foreground().Element(container =>
                    {
                        container
                            .PaddingBottom(350) 
                            .PaddingRight(150) 
                            .AlignBottom()
                            .AlignRight()
                            .Rotate(-25)      
                            .Border(3)     
                            .BorderColor(Color.FromHex("#A6A6D1"))
                            .PaddingVertical(5)
                            .PaddingHorizontal(15)
                            .Text("CERTIFIÉ")
                            .FontSize(24)
                            .Bold()
                            .FontColor(Color.FromHex("#A6A6D1"));
                    });
                });
            }).GeneratePdf();
        }
    }
}
