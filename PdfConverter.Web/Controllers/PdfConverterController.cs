using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using iTextSharpPdf = iText.Kernel.Pdf; // Alias for iTextSharp PDF classes
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace PdfConverter.Web.Controllers
{
    public class PdfConverterController : Controller
    {
        private readonly ILogger<PdfConverterController> _logger;

        public PdfConverterController(ILogger<PdfConverterController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Convert(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return BadRequest("Please upload a file.");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (fileExtension == ".pdf")
            {
                return await ConvertPdf(file);
            }
            else if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png")
            {
                return await ConvertImage(file);
            }
            else
            {
                _logger.LogWarning("Unsupported file type.");
                return BadRequest("Unsupported file type.");
            }
        }

        private async Task<IActionResult> ConvertPdf(IFormFile file)
        {
            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            var filePath = Path.Combine(uploadsDirectory, file.FileName);
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving the file.");
                return StatusCode(500, "Internal server error while saving the file.");
            }

            string extractedText;
            var sb = new StringBuilder();

            try
            {
                using (var reader = new iTextSharpPdf.PdfReader(filePath))
                {
                    using (var pdfDocument = new iTextSharpPdf.PdfDocument(reader))
                    {
                        for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                        {
                            var page = pdfDocument.GetPage(i);
                            var pageText = iTextSharpPdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(page);
                            sb.Append(pageText);
                            sb.Append("\n"); // Add a newline between pages
                        }
                    }
                }

                extractedText = sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from the PDF.");
                return StatusCode(500, "Internal server error while extracting text.");
            }

            if (string.IsNullOrEmpty(extractedText))
            {
                return Content("No text extracted from the PDF.", "text/plain");
            }

            return Content(extractedText, "text/plain");
        }

        private async Task<IActionResult> ConvertImage(IFormFile file)
        {
            var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            var filePath = Path.Combine(uploadsDirectory, file.FileName);
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving the file.");
                return StatusCode(500, "Internal server error while saving the file.");
            }

            string extractedText;
            var sb = new StringBuilder();

            try
            {
                // Use the absolute path to tessdata
                var tessdataPath = @"C:\Program Files\Tesseract-OCR\tessdata";
                if (!Directory.Exists(tessdataPath))
                {
                    throw new DirectoryNotFoundException($"Tessdata directory not found: {tessdataPath}");
                }

                using (var ocrEngine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(filePath))
                    {
                        using (var page = ocrEngine.Process(img))
                        {
                            extractedText = page.GetText();
                            sb.Append(extractedText);
                        }
                    }
                }
            }
            catch (TesseractException tesseractEx)
            {
                _logger.LogError(tesseractEx, "Tesseract OCR error.");
                return StatusCode(500, "Tesseract OCR error.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from the image.");
                return StatusCode(500, "Internal server error while extracting text.");
            }

            if (string.IsNullOrEmpty(sb.ToString()))
            {
                return Content("No text extracted from the image.", "text/plain");
            }

            return Content(sb.ToString(), "text/plain");
        }


    }
}
