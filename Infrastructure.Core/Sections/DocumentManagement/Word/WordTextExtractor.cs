using System.Text;

using DocumentFormat.OpenXml.Packaging;

using Microsoft.Extensions.Logging;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.DocumentManagement.Word;

[SingletonDependency<ITextExtractor>(Key = "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
public class WordTextExtractor : ITextExtractor
{
    protected readonly ILogger<WordTextExtractor> _logger;

    public WordTextExtractor(ILogger<WordTextExtractor> logger)
    {
        _logger = logger;
    }

    public string ExtractText(byte[] content)
    {
        try
        {
            var fullText = new StringBuilder();

            using (var memoryStream = new MemoryStream(content))
            using (var wordDoc = WordprocessingDocument.Open(memoryStream, false))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                fullText.Append(body.InnerText);

                var headers = wordDoc.MainDocumentPart.HeaderParts;
                foreach (var header in headers)
                {
                    fullText.Append(header.Header.InnerText);
                }

                var footers = wordDoc.MainDocumentPart.FooterParts;
                foreach (var footer in footers)
                {
                    fullText.Append(footer.Footer.InnerText);
                }
            }

            return fullText.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            return null;
        }
    }
}