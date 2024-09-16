using Microsoft.Extensions.DependencyInjection;
using SoftwaredeveloperDotAt.Infrastructure.Core.Sections.Pdf;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Sections.BinaryContentSection
{
    public interface ITextExtractor
    {
        string ExtractText(byte[] content);
    }
}
