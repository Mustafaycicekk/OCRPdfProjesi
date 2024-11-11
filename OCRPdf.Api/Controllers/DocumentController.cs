using Microsoft.AspNetCore.Mvc;
using OCRPdf.Data.Entities;
using OCRPdf.Helpers;
using OCRPdf.Service.Services;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;
using static OCRPdf.Helpers.PdfExtractionHelper;
namespace OCRPdf.Api.Controllers;

[ApiController, Route("[controller]/[action]")]
//[Authorize]
public class PdfController(IServiceProvider serviceProvider) : ControllerBase {
	private readonly IServiceProvider ServiceProvider = serviceProvider;

	[HttpPost]
	public ServiceResponse<object> ReadToPdfFile(IFormFile file) {
		if (file == null || file.Length == 0) { return ServiceResponse<object>.ErrorResponse("Dosya yüklenmedi"); }
		if (file.ContentType != "application/pdf") { return ServiceResponse<object>.ErrorResponse("Lütfen PDF dosyası yükleyin"); }

		using MemoryStream memoryStream = new();
		file.CopyTo(memoryStream);
		PdfDocument pdfDocument = new();

		try { pdfDocument.LoadFromStream(memoryStream); } catch (Exception ex) { return ServiceResponse<object>.ErrorResponse($"PDF yüklenirken hata oluştu: {ex.Message}"); }

		string outputDirectory = "Images";
		if (!Directory.Exists(outputDirectory)) { Directory.CreateDirectory(outputDirectory); }
		List<Rectangle> CropAreas = ImageCoordinates.CropAreas;
		List<Rectangle> Crop = ImageCoordinates.Crop;

		try {
			int pageIndex = 0;
			using Image image = pdfDocument.SaveAsImage(pageIndex, PdfImageType.Bitmap, 500, 500);
			using Bitmap bitmap = image as Bitmap;
			if (bitmap != null) {
				string imagePath = Path.Combine(outputDirectory, $"Sayfa-{pageIndex}.png");
				bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
				Optimizasyon extractedData = ReadTextFromImage(imagePath, CropAreas);
				List<Optimizasyon_Satirlari> tableData = ReadTextFromImageTableRows(imagePath, Crop);
				ServiceProvider.GetService<OptimizasyonService>().SaveToDatabase(extractedData, tableData);
			}
			else {
				return ServiceResponse<object>.ErrorResponse($"Sayfa {pageIndex} için dönüşüm başarısız.");
			}
		}
		catch (Exception ex) {
			return ServiceResponse<object>.ErrorResponse($"Sayfa 0 için hata oluştu: {ex.Message}");
		}
		return ServiceResponse<object>.SuccessResponse("Kayıt işlemi başarılı");
	}
	[HttpGet]
	public ServiceResponse<IEnumerable<string>> GetByWorkAndDate() {
		List<string> columns = ["IS", "TARIH"];
		ServiceResponse<IEnumerable<Optimizasyon>> workAndDate = ServiceProvider.GetService<OptimizasyonService>().GetColumns(columns);
		if (workAndDate.Success) {
			IEnumerable<string> result = workAndDate.Data.Select(x => $"İş: {x.IS} - Tarih: {x.TARIH}");
			return ServiceResponse<IEnumerable<string>>.SuccessResponse(result);
		}
		else {
			return ServiceResponse<IEnumerable<string>>.ErrorResponse(workAndDate.Message);
		}
	}
	[HttpGet]
	public IEnumerable<Optimizasyon> GetOptimizasyonByWeight(decimal weight) {
		ServiceResponse<IEnumerable<Optimizasyon>> getForWeight = ServiceProvider.GetService<OptimizasyonService>().GetOptimizasyonByAgirlik(weight);
		if (getForWeight.Success) { return getForWeight.Data; }
		else { return null; }
	}



}