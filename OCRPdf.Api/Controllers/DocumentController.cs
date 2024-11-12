using Microsoft.AspNetCore.Authorization;
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
[Authorize]
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
				string imagePath = Path.Combine(outputDirectory, $"Pdf-{pageIndex}.png");
				bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
				Optimizasyon extractedData = ReadTextFromImage(imagePath, CropAreas);
				List<Optimizasyon_Satirlari> tableData = ReadTextFromImageTableRows(imagePath, Crop);
				ServiceProvider.GetService<OptimizasyonService>().SaveToDatabase(extractedData, tableData);
			}
			else {
				return ServiceResponse<object>.ErrorResponse($"Dönüşüm başarısız.");
			}
		}
		catch (Exception ex) {
			return ServiceResponse<object>.ErrorResponse(ex.Message);
		}
		return ServiceResponse<object>.SuccessResponse("Kayıt işlemi başarılı");
	}
	[HttpGet]
	public ServiceResponse<IEnumerable<object>> GetByWorkAndDate() {
		List<string> columns = ["[IS]", "TARIH", "os.SIRA", "os.REFERANS", "os.SAC", "os.TOPLAM", "os.KAYIP", "os.AGIRLIK", "os.OLCULER"];
		ServiceResponse<IEnumerable<object>> workAndDate = ServiceProvider.GetService<OptimizasyonService>().GetColumns(columns);
		return workAndDate;
	}
	[HttpGet]
	public ServiceResponse<IEnumerable<Optimizasyon>> GetOptimizasyonByWeight(decimal weight) {
		ServiceResponse<IEnumerable<Optimizasyon>> getForWeight = ServiceProvider.GetService<OptimizasyonService>().GetOptimizasyonByWeight(weight);
		return getForWeight;
	}



}