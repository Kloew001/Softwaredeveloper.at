using iText.Kernel.Pdf;

using Microsoft.Extensions.Logging;

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
        if (content == null)
            return null;

        try
        {
            using (var memoryStream = new MemoryStream(content))
            using (var pdfReader = new PdfReader(memoryStream))
            using (var pdfDoc = new PdfDocument(pdfReader))
            {
                var extractedText = "";
                for (var page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                {
                    extractedText += iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor
                        .GetTextFromPage(pdfDoc.GetPage(page));
                }
                return extractedText;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return null;
        }
    }
}