using Tesseract;

namespace PdfConverter.Web.Services
{
    public class OcrService : IOcrService
    {
        private readonly string _tessdataPath = @"C:\Program Files\Tesseract-OCR\tessdata"; // Update this path

        public string PerformOcr(string imagePath)
        {
            try
            {
                using var engine = new TesseractEngine(_tessdataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(imagePath);
                using var page = engine.Process(img);
                return page.GetText();
            }
            catch (Exception ex)
            {
                // Handle exceptions and provide feedback
                return $"Error: {ex.Message}";
            }
        }
    }
}
