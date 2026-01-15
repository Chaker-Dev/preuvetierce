using PreuveTierce.Web.Models;
using PreuveTierce.Web.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PreuveTierce.Web.Services
{
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private readonly IQrCodeService _qrCodeService;
        // --- PALETTE DE COULEURS ---
        private static readonly Color BleuPreuve = Color.FromHex("#000091");
        private static readonly Color OrAuthentique = Color.FromHex("#D4AF37");
        private static readonly Color OrClairFond = Color.FromHex("#FEFCE8");
        private static readonly Color OrFonce = Color.FromHex("#854d0e");
        private static readonly Color GrisTexte = Colors.Grey.Darken2;
        private static readonly Color GrisClairLabel = Colors.Grey.Medium;
        private static readonly Color GrisFondTableau = Colors.Grey.Lighten4;
        public PdfGeneratorService(IQrCodeService qrCodeService)
        {
            _qrCodeService = qrCodeService;
        }

        public byte[] GenerateAttestation(CertificateData data)
        {
            return CreatePdfDocument(
                data, 
                "ATTESTATION DE DÉPÔT", 
                "Preuve d'enregistrement numérique", 
                Colors.Black,
                isOfficialCertificate: false
                );
        }

        public byte[] GenerateAuthenticCertification(CertificateData data)
        {
            return CreatePdfDocument(
                data, 
                "CERTIFICAT D'AUTHENTICITÉ", 
                "Document légal certifié conforme", 
                OrAuthentique,
                isOfficialCertificate: true);
        }

        private byte[] CreatePdfDocument(CertificateData data, string title, string subtitle, Color themeColor, bool isOfficialCertificate)
        {
            var qrImage = _qrCodeService.GeneratePng(data.VerificationUrl);
            var watermarkText = isOfficialCertificate ? "CERTIFIÉ CONFORME" : "CERTIFIÉ";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial).FontColor(GrisTexte));
                    page.PageColor(Colors.White);

                    // header
                    page.Header().Element(head => ComposeHeader(head, data, themeColor, isOfficialCertificate));

                    // Content 
                    page.Content().Element(body => ComposeContent(body, data, title, subtitle, themeColor, isOfficialCertificate));

                    // Footer
                    page.Footer().Element(foot => ComposeFooter(foot, qrImage, data.FileHash, themeColor));

                    // Filigrane (Watermark)
                    page.Foreground().Element(fg => ComposeWatermark(fg, watermarkText, themeColor));

                });
            }).GeneratePdf();
        }

        private void ComposeHeader(IContainer container, CertificateData data, Color themeColor, bool isOfficial)
        {
            container.BorderBottom(3).BorderColor(themeColor).PaddingBottom(20).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(t =>
                    {
                        t.Span("PREUVE").FontSize(28).Bold().FontColor(BleuPreuve);
                        t.Span("TIERCE").FontSize(28).Bold().FontColor(themeColor);
                    });
                    c.Item().Text("Tiers de Confiance Numérique").FontSize(12).FontColor(GrisClairLabel).LetterSpacing(0.1f);
                });

                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text(t => { t.Span("N° : ").Bold(); t.Span(data.SerialNumber).FontFamily(Fonts.Courier).Bold(); });
                    c.Item().Text($"Date : {DateTime.Now:dd/MM/yyyy}");
                });
            });
        }

        private void ComposeContent(IContainer container, CertificateData data, string title, string subtitle, Color themeColor, bool isOfficial)
        {
            container.Column(col =>
            {
                col.Item().PaddingBottom(10).PaddingTop(15).Column(titles =>
                {
                    titles.Spacing(5);
                    titles.Item().AlignCenter().Text(title).Bold().FontSize(isOfficial ? 32 : 26).FontColor(Colors.Black);
                    titles.Item().AlignCenter().Text(subtitle).FontSize(14).FontColor(GrisClairLabel);
                });

                col.Item().PaddingBottom(30);

                if (isOfficial)
                {
                    col.Item().PaddingBottom(40).Background(OrClairFond)
                        .Border(2).BorderColor(themeColor).BorderLeft(8)
                        .Padding(20)
                        .Row(row =>
                        {
                            row.ConstantItem(60).AlignCenter().AlignMiddle().Text("✓").FontSize(50).FontColor(themeColor);
                            row.RelativeItem().PaddingLeft(20).Column(c =>
                            {
                                c.Item().Text("Document Authentique & Intègre").FontSize(22).Bold().FontColor(OrFonce);
                                c.Item().Text(t => {
                                    t.Span("L'analyse cryptographique confirme que le document n'a subi ");
                                    t.Span("aucune altération").Bold();
                                    t.Span(" depuis son dépôt.").FontColor(OrFonce);
                                });
                            });
                        });
                }
                else
                {
                    col.Item().PaddingBottom(30).Text(txt =>
                            {
                                txt.ParagraphSpacing(1.5f);
                                txt.Span("Il est certifié par la présente que le fichier numérique décrit ci-dessous a été déposé, analysé et horodaté électroniquement sur la plateforme sécurisée ").FontSize(12);
                                txt.Span("PreuveTierce.fr").FontSize(12).Bold();
                                txt.Span(". L'empreinte cryptographique unique de ce document a été enregistrée de manière inaltérable.").FontSize(12);
                            });
                }

                // 3. Section HASH (Commune)
                col.Item().PaddingBottom(10).Text("Empreinte Numérique (SHA-256)").FontSize(14).FontColor(themeColor).Bold();                 

                col.Item().PaddingTop(10)
                       .Background(GrisFondTableau)
                       .BorderLeft(4).BorderColor(BleuPreuve)
                       .Padding(10)
                       .Text(data.FileHash)
                       .FontFamily(Fonts.Consolas).FontSize(12);

                col.Item().PaddingTop(5).PaddingBottom(30).Text("* ce code est unique au monde. la moindre modification d'un pixel ou d'une virgule dans le document original modifierait radicalement cette empreinte.")
                       .FontSize(9).FontColor(GrisClairLabel);

                // 4. Tableau de Données (Commun)
                col.Item().PaddingBottom(10).Text("Identification du Fichier").FontSize(14).FontColor(themeColor).Bold();
                  
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cd => { cd.RelativeColumn(2); cd.RelativeColumn(3); });

                    // Fonction locale pour styliser les lignes
                    void Row(string label, string value)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(10).Text(label).FontColor(GrisClairLabel);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(10).Text(value).Bold().FontColor(Colors.Black);
                    }

                    Row("Nom du fichier :", data.FileName);
                    Row("Taille :", $"{data.FileSizeFormatted:N0} octets");
                    Row("Date de dépôt :", $"{data.IssueDate:dd/MM/yyyy à HH:mm} UTC");
                    Row("Référence :", data.ClientReference);
                });
            });
        }

        private void ComposeFooter(IContainer container, byte[] qrImage, string hash, Color themeColor)
        {
            container.PaddingTop(20).BorderTop(1).BorderColor(Colors.Grey.Lighten3).Row(row =>
            {
                row.ConstantItem(80).Height(80).Image(qrImage);
                row.RelativeItem().PaddingLeft(20).Column(c =>
                {
                    c.Item().PaddingBottom(10);
                    c.Item().Text("Vérification en ligne :").Bold().FontColor(themeColor);
                    c.Item().Text($"https://preuvetierce.fr/verify/{hash}").FontSize(10).Underline().FontColor(BleuPreuve);
                    c.Item().PaddingTop(5).Text("Ce document est généré automatiquement. PreuveTierce agit en tant que tiers de confiance technique").FontSize(9).FontColor(GrisClairLabel);
                });
            });
        }
        private void ComposeWatermark(IContainer container, string text, Color color)
        {
            var transparenceTampon = color.WithAlpha(0.15f); 
                container.PaddingBottom(260).PaddingRight(150).AlignBottom().AlignRight().Rotate(-25)
                .Border(4).BorderColor(transparenceTampon)
                .PaddingVertical(10).PaddingHorizontal(30)
                .Text(text).FontSize(40).Bold().FontColor(transparenceTampon);
        }
    }
}
