using IronOcr;
using iText.Layout;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using SkiaSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase {
	[HttpPost]
	public IActionResult UploadPdf(IFormFile file) {
		if (file == null || file.Length == 0)
			return BadRequest("No file uploaded.");

		try {
			// PDF dosyasını geçici bir dosyaya kaydediyoruz
			string tempPdfPath = Path.GetTempFileName();
			using (FileStream fileStream = new(tempPdfPath, FileMode.Create)) {
				file.CopyTo(fileStream);
			}

			// PDF dosyasını görsele dönüştür
			string imagePath = Path.Combine(Path.GetTempPath(), "output.png");
			ConvertPdfToImage(tempPdfPath, imagePath);

			// OCR işlemi yap
			string ocrResult = PerformOcrOnImage(imagePath);

			// Geçici dosyaları temizle
			System.IO.File.Delete(tempPdfPath);
			System.IO.File.Delete(imagePath);

			return Ok(new { Text = ocrResult });
		}
		catch (Exception ex) {
			return StatusCode(500, $"Internal server error: {ex.Message}");
		}
	}

	public void ConvertPdfToImage(string pdfFilePath, string outputImagePath) {
		// SkiaSharp ile PDF'i açma işlemi
		using (var document = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Import)) {
			var page = document.Pages[0];

			double width = page.Width.Point;
			double height = page.Height.Point;

			// SkiaSharp bitmap oluşturma
			using (SKBitmap bitmap = new SKBitmap((int)width, (int)height)) {
				using (SKCanvas canvas = new SKCanvas(bitmap)) {
					canvas.Clear(SKColors.White); // Beyaz zemin
												  // Burada SkiaSharp ile render işlemi yapılır.
												  // SkiaSharp doğrudan PDF render etmiyor, bunun için ekstra bir kütüphane (örn. PdfiumSharp) gerekebilir.
				}

				// Görseli kaydetme
				using (var image = SKImage.FromBitmap(bitmap)) {
					using (var data = image.Encode()) {
						using (var stream = File.OpenWrite(outputImagePath)) {
							data.SaveTo(stream);
						}
					}
				}
			}
		}
	}

	private string PerformOcrOnImage(string imagePath) {
		IronTesseract ocr = new();

		using OcrInput input = new();
		input.LoadImage(imagePath);

		// OCR işlemi yap
		OcrResult result = ocr.Read(input);

		return result.Text;
	}
}






//[HttpPost]
//public ServiceResponse<object> ExtractTextFromPdf(IFormFile file) {
//	if (file == null || file.Length == 0) { return ServiceResponse<object>.ErrorResponse("No file uploaded."); }

//	// PDF dosyasını geçici olarak kaydedelim
//	string tempPdfPath = Path.Combine(Path.GetTempPath(), "input.pdf");
//	using (FileStream fileStream = new(tempPdfPath, FileMode.Create)) {
//		file.CopyTo(fileStream);
//	}

//	// PDF'den metin çıkarma
//	StringBuilder extractedText = new StringBuilder();
//	try {
//		using (PdfReader reader = new PdfReader(tempPdfPath)) {
//			PdfDocument pdfDoc = new PdfDocument(reader);
//			for (int pageNum = 1; pageNum <= pdfDoc.GetNumberOfPages(); pageNum++) {
//				var strategy = new SimpleTextExtractionStrategy();
//				var pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(pageNum), strategy);
//				extractedText.Append(pageText);
//			}
//		}

//		System.IO.File.Delete(tempPdfPath);

//		return ServiceResponse<object>.SuccessResponse(data: extractedText.ToString());
//	}
//	catch (Exception ex) {
//		return ServiceResponse<object>.ErrorResponse("No file uploaded.");
//	}
//}
