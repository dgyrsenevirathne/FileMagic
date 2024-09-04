namespace PdfConverter.Web.Services
{
    public interface IOcrService
    {
        string PerformOcr(string imagePath);
    }
}
