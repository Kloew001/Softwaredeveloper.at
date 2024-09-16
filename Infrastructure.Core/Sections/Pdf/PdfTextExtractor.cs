using iText.Kernel.Pdf;

using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Pdf
{
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
            try
            {
                using (var memoryStream = new MemoryStream(content))
                using (
                    var pdfReader = new PdfReader(memoryStream))
                using (var pdfDoc = new PdfDocument(pdfReader))
                {
                    string extractedText = "";
                    for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
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
}
