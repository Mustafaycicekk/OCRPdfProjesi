using Ghostscript.NET.Rasterizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OCRPdf.Helpers;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Drawing;
using System.IO;
using Tesseract;

[ApiController]
[Route("[controller]/[action]")]
public class PdfController : ControllerBase {
	private readonly string tessDataPath;

	public PdfController(string tessDataPath) {
		this.tessDataPath = tessDataPath;
	}
	[HttpOptions]
	public ServiceResponse<object> UploadPdf(IFormFile file) {
		if(file == null || file.Length == 0) {
			return ServiceResponse<object>.ErrorResponse("No file uploaded.");
		}

		try {
			string tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
			//string extractedText = PerformOcrOnImage(file.OpenReadStream(), tessDataPath);
			string extractedText = ProcessPdf(file.OpenReadStream());
			return ServiceResponse<object>.SuccessResponse(new { Text = extractedText });
		} catch(Exception ex) {
			return ServiceResponse<object>.ErrorResponse("Error processing the file: " + ex.Message);
		}
	}

	private string PerformOcrOnImage(MemoryStream imageStream, string tessDataPath) {
		using TesseractEngine engine = new(tessDataPath, "tur", EngineMode.Default);
		// MemoryStream'den Pix nesnesi oluştur
		using Pix pix = Pix.LoadFromMemory(imageStream.ToArray());
		Page result = engine.Process(pix);
		return result.GetText();
	}

	private byte[] StreamToByteArray(Stream inputStream) {
		using MemoryStream memoryStream = new();
		inputStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}
	public string ProcessPdf(Stream pdfStream) {
		string extractedText = string.Empty;

		// PDF dosyasını PdfSharp ile yükleyin
		using(PdfDocument pdfDocument = PdfReader.Open(pdfStream, PdfDocumentOpenMode.ReadOnly)) {
			int pageCount = pdfDocument.PageCount;

			// Her sayfa için Ghostscript ile resmi render edin
			for(int i = 0; i < pageCount; i++) {
				using(var pageImage = RenderPageToImage(pdfStream, i + 1)) {
					// Her sayfa resmini dört alana bölerek OCR işlemi yapın
					extractedText += PerformOcrOnImageSections((Bitmap)pageImage);
				}
			}
		}

		return extractedText;
	}
	private Image RenderPageToImage(Stream pdfStream, int pageNumber) {
		// Ghostscript.NET ile PDF sayfasını resme dönüştürme
		using(var rasterizer = new GhostscriptRasterizer()) {
			rasterizer.Open(pdfStream);

			// 300 DPI çözünürlükte resmi render edin
			Image pageImage = rasterizer.GetPage(300, pageNumber);
			return pageImage;
		}
	}
	private string PerformOcrOnImageSections(Bitmap pageImage) {
		string pageText = string.Empty;

		// Görüntüyü dört eşit alana bölmek için boyutları hesapla
		int width = pageImage.Width / 2;
		int height = pageImage.Height / 2;

		// Dört alanı tanımla ve OCR işlemi yap
		Rectangle[] sections = new Rectangle[]
		{
			new(0, 0, width, height),                           // Sol üst
            new Rectangle(width, 0, width, height),                      // Sağ üst
            new Rectangle(0, height, width, height),                     // Sol alt
            new Rectangle(width, height, width, height)                  // Sağ alt
        };

		foreach(var section in sections) {
			using(Bitmap croppedImage = CropImage(pageImage, section))
			using(MemoryStream ms = new MemoryStream()) {
				croppedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
				ms.Seek(0, SeekOrigin.Begin);
				pageText += PerformOcrOnImage(ms);
			}
		}

		return pageText;
	}
	private Bitmap CropImage(Bitmap source, Rectangle section) {
		// Belirtilen alanı kırp ve yeni bir Bitmap döndür
		Bitmap croppedImage = new Bitmap(section.Width, section.Height);
		using(Graphics g = Graphics.FromImage(croppedImage)) {
			g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
		}
		return croppedImage;
	}
	private string PerformOcrOnImage(Stream imageStream) {
		// Tesseract OCR işlemi
		using(var engine = new TesseractEngine(tessDataPath, "tur", EngineMode.Default)) {
			using(var pix = Pix.LoadFromMemory(StreamToByteArray(imageStream))) {
				Page result = engine.Process(pix);
				return result.GetText();
			}
		}
	}




}














//	[HttpPost]
//	public ServiceResponse<object> UploadPdf(IFormFile file) {
//		if(file == null || file.Length == 0) {
//			return ServiceResponse<object>.ErrorResponse("No file uploaded.");
//		}

//		try {
//			// Geçici dosya yolları oluştur
//			string tempPdfPath = Path.GetTempFileName();
//			string imagePath = Path.Combine(Path.GetTempPath(), "output.png");

//			// PDF dosyasını geçici dosyaya kaydet
//			using(FileStream fileStream = new(tempPdfPath, FileMode.Create)) {
//				file.CopyTo(fileStream);
//			}

//			// PDF dosyasını görsele dönüştür
//			ConvertPdfToImage(tempPdfPath, imagePath);

//			// Görsel üzerinde OCR işlemi yap
//			string ocrResult = PerformOcrOnImage(imagePath);

//			// Geçici dosyaları temizle
//			System.IO.File.Delete(tempPdfPath);
//			System.IO.File.Delete(imagePath);

//			// OCR sonucu ve görselin byte[] içeriğini ServiceResponse ile döndür
//			return ServiceResponse<object>.SuccessResponse(new { Text = ocrResult, ImageBytes = System.IO.File.ReadAllBytes(imagePath) });
//		} catch(Exception ex) {
//			return ServiceResponse<object>.ErrorResponse("Error processing the file: " + ex.Message);
//		}
//	}



//	public void ConvertPdfToImage(string pdfFilePath, string outputImagePath) {
//		// PdfSharp ile PDF'i açma işlemi
//		using(var document = PdfReader.Open(pdfFilePath, PdfDocumentOpenMode.Import)) {
//			var page = document.Pages[0];

//			double width = page.Width.Point;
//			double height = page.Height.Point;

//			// SkiaSharp ile bitmap oluşturma
//			using(SKBitmap bitmap = new SKBitmap((int)width, (int)height)) {
//				using(SKCanvas canvas = new SKCanvas(bitmap)) {
//					canvas.Clear(SKColors.White); // Beyaz zemin

//					// SkiaSharp ile PDF sayfasını çizme işlemi (PdfiumSharp gibi bir kütüphane gerekebilir)
//					// Eğer PDF render etmek gerekiyorsa PdfiumSharp gibi bir kütüphane eklenmelidir
//				}

//				// Bitmap'i görsel olarak kaydetme
//				using SKImage image = SKImage.FromBitmap(bitmap);
//				using SKData data = image.Encode();

//				// Çıkış dizinini kontrol et ve oluştur
//				string directoryPath = Path.GetDirectoryName(outputImagePath);
//				if(!Directory.Exists(directoryPath)) {
//					Directory.CreateDirectory(directoryPath);
//				}

//				using(var stream = new FileStream(outputImagePath, FileMode.Create, FileAccess.Write, FileShare.None)) {
//					data.SaveTo(stream);
//				}
//			}
//		}
//	}


//	private string PerformOcrOnImage(string imagePath) {
//		IronTesseract ocr = new IronTesseract();

//		using(OcrInput input = new OcrInput()) {
//			input.AddImage(imagePath);

//			// OCR işlemi yapma
//			OcrResult result = ocr.Read(input);

//			return result.Text;
//		}
//	}
//}






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
