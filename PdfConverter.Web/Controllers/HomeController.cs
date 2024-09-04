using Microsoft.AspNetCore.Mvc;
using PdfConverter.Web.Models;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using PdfConverter.Web.Services;

namespace PdfConverter.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOcrService _ocrService;

        // Inject the OCR service into the controller
        public HomeController(ILogger<HomeController> logger, IOcrService ocrService)
        {
            _logger = logger;
            _ocrService = ocrService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PerformOcr(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Text = "No file selected.";
                return View("Index");
            }

            // Save the file to a temporary location
            var tempFilePath = Path.GetTempFileName();
            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Perform OCR using the service
            var result = _ocrService.PerformOcr(tempFilePath);

            // Clean up temporary file
            System.IO.File.Delete(tempFilePath);

            // Display the result
            ViewBag.Text = result;
            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
