using Microsoft.Extensions.Logging;

using UglyToad.PdfPig;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.DocumentManagement.Pdf;

[SingletonDependency<ITextExtractor>(Key = "application/pdf")]
public class PdfTextExtractor : ITextExtractor
{
    protected readonly ILogger<PdfTextExtractor> _logger;

    public PdfTextExtractor(ILogger<PdfTextExtractor> logger)
    {
        _logger = logger;
    }

    public string ExtractText(byte[] content)
    {
        using (var memoryStream = new MemoryStream(content))
        using (var pdfDoc = PdfDocument.Open(memoryStream))
        {
            var extractedText = "";
            foreach (var page in pdfDoc.GetPages())
            {
                extractedText += page.Text;
            }
            return extractedText;
        }
    }
}