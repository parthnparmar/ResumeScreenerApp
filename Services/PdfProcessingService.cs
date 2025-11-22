// Services/PdfProcessingService.cs
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text;

namespace ResumeScreenerApp.Services
{
    public class PdfProcessingService
    {
        public async Task<string> ExtractTextFromPdfAsync(IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
                throw new ArgumentException("PDF file is required");

            if (!pdfFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("File must be a PDF");

            using var stream = new MemoryStream();
            await pdfFile.CopyToAsync(stream);
            stream.Position = 0;

            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new PdfDocument(pdfReader);
            
            var text = new StringBuilder();
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var pageText = PdfTextExtractor.GetTextFromPage(page);
                text.AppendLine(pageText);
            }

            return text.ToString();
        }
    }
}