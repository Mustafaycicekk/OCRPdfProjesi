using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OCRPdf.Helpers;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;
using System.Text;
using Tesseract;

[ApiController]
[Route("[controller]/[action]")]
public class PdfController : ControllerBase {


	[HttpPost]
	public ServiceResponse<object> ConvertPdfToPng(IFormFile file) {
		if (file == null || file.Length == 0) {
			return ServiceResponse<object>.ErrorResponse("Dosya yüklenmedi");
		}
		if (file.ContentType != "application/pdf") {
			return ServiceResponse<object>.ErrorResponse("Lütfen PDF dosyası yükleyin");
		}

		using MemoryStream memoryStream = new();
		file.CopyTo(memoryStream);
		PdfDocument pdfDocument = new();

		try {
			pdfDocument.LoadFromStream(memoryStream);
		}
		catch (Exception ex) {
			return ServiceResponse<object>.ErrorResponse($"Error loading PDF: {ex.Message}");
		}

		string outputDirectory = "Images";
		if (!Directory.Exists(outputDirectory)) {
			Directory.CreateDirectory(outputDirectory);
		}

		StringBuilder allExtractedText = new();

		// 10 farklı bölgeyi belirlemek için bir liste (Bu kısımdaki cropAreas bozulmamalıdır)
		List<Rectangle> cropAreas =
		[
			new Rectangle(1739, 80, 1425, 170), //REF
        new Rectangle(3795, 126, 124, 107), // MIKTAR
        new Rectangle(1741, 270, 860, 100), // İŞ
        new Rectangle(3533, 388, 525, 200), // CNC
        new Rectangle(1733, 388, 380, 130), // TARIH
        new Rectangle(1059, 2934, 650, 130), // MALZEME
        new Rectangle(1059, 2830, 650, 130), // MAKINA
        new Rectangle(1059, 3069, 650, 130), // TOPLAM SURE
        new Rectangle(3085, 2833, 200, 130), // AĞIRLIK
        new Rectangle(3085, 2934, 300, 130), // X
        new Rectangle(3085, 3079, 300, 130), // Y 
        new Rectangle(3085, 3185, 300, 130), // KULLANILAN
        new Rectangle(3085, 3300, 300, 130), // ISKARTA T_SAC_K_SAC
        new Rectangle(3500, 3300, 300, 130)  // ISKARTA K_SAC_T_SAC
		];

		// crop tablosunu tanımlıyoruz
		List<Rectangle> crop =
		[
			new Rectangle(70, 3556, 80, 80), // SIRA
        new Rectangle(176, 3556, 1500, 80), // REFERANS
        new Rectangle(1821, 3556, 150, 80), // SAC
        new Rectangle(2167, 3556, 150, 80), // TOPLAM
        new Rectangle(2517, 3556, 150, 80), // KAYIP
        new Rectangle(2855, 3556, 350, 80), // AGIRLIK
        new Rectangle(3250, 3556, 700, 80)  // OLCULER
		];

		try {
			int pageIndex = 0; // Burada sadece 1. sayfa olacak şekilde 0 veriyoruz
			using Image image = pdfDocument.SaveAsImage(pageIndex, PdfImageType.Bitmap, 500, 500);
			using Bitmap bitmap = image as Bitmap;

			if (bitmap != null) {
				string imagePath = Path.Combine(outputDirectory, $"ToImage-{pageIndex}.png");

				bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

				// Text extraction from multiple areas on the image
				PdfExtractedData extractedData = ReadTextFromImage(imagePath, cropAreas);

				// Text extraction from crop table rows (y koordinatlarını arttırarak)
				List<PdfExtractedData1> tableData = ExtractTextFromTableRows(imagePath, crop);

				// Veritabanına kaydetme
				//SaveToDatabase(pageIndex, extractedData);

				// Tüm çıkarılan verileri response'a ekliyoruz
				allExtractedText.AppendLine(
					@$"Sayfa {pageIndex} - REF: {extractedData.REF}, MIKTAR: {extractedData.MIKTAR}, İŞ: {extractedData.IS}, CNC: {extractedData.CNC}, TARIH: {extractedData.TARIH}, MALZEME: {extractedData.MALZEME}, MAKINA: {extractedData.MAKINA}, TOPLAM SURE: {extractedData.TOPLAM_SURE}, AĞIRLIK: {extractedData.AGIRLIK}, X: {extractedData.X}, Y: {extractedData.Y}, KULLANILAN: {extractedData.KULLANILAN}, ISKARTA T_SAC_K_SAC: {extractedData.ISKARTA_T_SAC_K_SAC}, ISKARTA K_SAC_T_SAC: {extractedData.ISKARTA_K_SAC_T_SAC}, ");

				// Ayrıca tablonun metinlerini de ekliyoruz
				foreach (PdfExtractedData1 rowText in tableData) {
					allExtractedText.AppendLine(rowText.ToString()); // Using ToString() here
				}
			}
			else {
				return ServiceResponse<object>.ErrorResponse($"Sayfa {pageIndex} için dönüşüm başarısız.");
			}
		}
		catch (Exception ex) {
			return ServiceResponse<object>.ErrorResponse($"Sayfa 0 için hata oluştu: {ex.Message}");
		}

		return ServiceResponse<object>.SuccessResponse(allExtractedText.ToString());
	}




	private List<PdfExtractedData1> ExtractTextFromTableRows(string imagePath, List<Rectangle> crop) {
		List<PdfExtractedData1> extractedRows = new List<PdfExtractedData1>();
		int rowCount = 25; // Kaç satır olacağını varsayalım
		int yIncrement = 80; // Y koordinat artışı

		// Her satır için crop listesindeki her bir Rectangle'ın Y koordinatını artırarak yeni bir Rectangle oluşturuyoruz
		for (int i = 0; i < rowCount; i++) {
			List<Rectangle> currentRow = new List<Rectangle>();

			// crop listesindeki her bir Rectangle'ı Y koordinatını artırarak yeni bir Rectangle oluşturuyoruz
			foreach (Rectangle rect in crop) {
				Rectangle newRect = new Rectangle(rect.X, rect.Y + (i * yIncrement), rect.Width, rect.Height);
				currentRow.Add(newRect);
			}

			// Yeni satırdan metin çıkarıyoruz
			PdfExtractedData1 rowData = ReadTextFromImage1(imagePath, currentRow);

			// Satırın metnini oluşturup listeye ekliyoruz
			extractedRows.Add(rowData);
		}

		return extractedRows;
	}

	private static PdfExtractedData ReadTextFromImage(string imagePath, List<Rectangle> cropAreas) {
		using Bitmap bitmap = new(imagePath);
		PdfExtractedData extractedData = new();

		// Her Rectangle indeksi için alan eşlemelerini tanımlayın
		Dictionary<int, Action<PdfExtractedData, string>> extractionActions = new()
		{
		{ 0, (data, text) => data.REF = text },
		{ 1, (data, text) => data.MIKTAR = text},
		{ 2, (data, text) => data.IS = text},
		{ 3, (data, text) => data.TARIH = text },
		{ 4, (data, text) => data.CNC = text},
		{ 5, (data, text) => data.MALZEME = text },
		{ 6, (data, text) => data.MAKINA = text },
		{ 7, (data, text) => data.TOPLAM_SURE = text },
		{ 8, (data, text) => data.AGIRLIK = text },
		{ 9, (data, text) => data.X = text },
		{ 10, (data, text) => data.Y = text },
		{ 11, (data, text) => data.KULLANILAN = text },
		{ 12, (data, text) => data.ISKARTA_T_SAC_K_SAC = text },
		{ 13, (data, text) => data.ISKARTA_K_SAC_T_SAC = text },
		{ 14, (data, text) => data.SIRA = text },
		{ 15, (data, text) => data.REFERANS = text },
		{ 16, (data, text) => data.SAC = text },
		{ 17, (data, text) => data.TOPLAM = text },
		{ 18, (data, text) => data.KAYIP = text },
		{ 19, (data, text) => data.AGIRLIKT = text },
		{ 20, (data, text) => data.OLCULER = text }
	};

		for (int i = 0; i < cropAreas.Count; i++) {
			Rectangle cropArea = cropAreas[i];
			using Bitmap croppedBitmap = bitmap.Clone(cropArea, bitmap.PixelFormat);
			using Pix pix = PixConverter.ToPix(croppedBitmap);
			using TesseractEngine engine = new(@"./tessdata", "tur", EngineMode.Default);
			using Page page = engine.Process(pix);

			string extractedText = CleanText(page.GetText());

			// Doğrudan Dictionary ile ilgili alanlara atama yapın
			if (extractionActions.ContainsKey(i)) {
				extractionActions[i](extractedData, extractedText);
			}
		}

		return extractedData;
	}
	private static PdfExtractedData1 ReadTextFromImage1(string imagePath, List<Rectangle> cropAreas) {
		using Bitmap bitmap = new Bitmap(imagePath);
		PdfExtractedData1 extractedData = new PdfExtractedData1();

		// Her Rectangle indeksi için alan eşlemelerini tanımlayın
		Dictionary<int, Action<PdfExtractedData1, string>> extractionActions = new Dictionary<int, Action<PdfExtractedData1, string>>()
		{
		{ 0, (data, text) => data.SIRA = text },
		{ 1, (data, text) => data.REFERANS = text },
		{ 2, (data, text) => data.SAC = text },
		{ 3, (data, text) => data.TOPLAM = text },
		{ 4, (data, text) => data.KAYIP = text },
		{ 5, (data, text) => data.AGIRLIKT = text },
		{ 6, (data, text) => data.OLCULER = text }
	};

		for (int i = 0; i < cropAreas.Count; i++) {
			Rectangle cropArea = cropAreas[i];
			using Bitmap croppedBitmap = bitmap.Clone(cropArea, bitmap.PixelFormat);
			using Pix pix = PixConverter.ToPix(croppedBitmap);
			using TesseractEngine engine = new TesseractEngine(@"./tessdata", "tur", EngineMode.Default);
			using Page page = engine.Process(pix);

			string extractedText = CleanText(page.GetText());

			// Doğrudan Dictionary ile ilgili alanlara atama yapın
			if (extractionActions.ContainsKey(i)) {
				extractionActions[i](extractedData, extractedText);
			}
		}

		return extractedData;
	}







	public class PdfExtractedData {
		public string REF { get; set; }
		public string MIKTAR { get; set; }
		public string IS { get; set; }
		public string CNC { get; set; }
		public string TARIH { get; set; }
		public string MALZEME { get; set; }
		public string MAKINA { get; set; }
		public string TOPLAM_SURE { get; set; }
		public string AGIRLIK { get; set; }
		public string X { get; set; }
		public string Y { get; set; }
		public string KULLANILAN { get; set; }
		public string ISKARTA_T_SAC_K_SAC { get; set; }
		public string ISKARTA_K_SAC_T_SAC { get; set; }
		public string SIRA { get; set; }
		public string REFERANS { get; set; }
		public string SAC { get; set; }
		public string TOPLAM { get; set; }
		public string KAYIP { get; set; }
		public string AGIRLIKT { get; set; }
		public string OLCULER { get; set; }
	}
	public class PdfExtractedData1 {
		public string SIRA { get; set; }
		public string REFERANS { get; set; }
		public string SAC { get; set; }
		public string TOPLAM { get; set; }
		public string KAYIP { get; set; }
		public string AGIRLIKT { get; set; }
		public string OLCULER { get; set; }
		public override string ToString() {
			return $"SIRA: {SIRA}, REFERANS: {REFERANS}, SAC: {SAC}, TOPLAM: {TOPLAM}, KAYIP: {KAYIP}, AGIRLIK: {AGIRLIKT}, OLCULER: {OLCULER}";
		}
	}




	public static class PixConverter {
		public static Pix ToPix(Bitmap bitmap) {
			using MemoryStream memoryStream = new();
			bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
			memoryStream.Seek(0, SeekOrigin.Begin);
			return Pix.LoadFromMemory(memoryStream.ToArray());
		}
	}
	private static string CleanText(string inputText) {
		if (string.IsNullOrEmpty(inputText)) return inputText;
		string cleanedText = inputText.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
		cleanedText = System.Text.RegularExpressions.Regex.Replace(cleanedText, @"[\u0000-\u001F\u007F-\u009F]+", " ");
		cleanedText = System.Text.RegularExpressions.Regex.Replace(cleanedText, @"\s+", " ").Trim();
		return cleanedText;
	}

	private static void SaveToDatabase(int pageNumber, PdfExtractedData extractedData) {
		// Veritabanı bağlantı dizesi
		string connectionString = "YourConnectionString"; // Burada bağlantı dizesini güncellemeyi unutmayın

		// SQL sorgusu
		string query = @"INSERT INTO PdfExtractedDataTable (PageNumber, Id, Ad, Soyad) VALUES (@PageNumber, @Id, @Ad, @Soyad)";
		using SqlConnection connection = new(connectionString);
		connection.Open();

		// Dapper ile SQL sorgusunu çalıştırıyoruz
		connection.Execute(query, new {
			PageNumber = pageNumber,
			extractedData.REF,
			extractedData.MIKTAR,
			extractedData.IS,
			extractedData.CNC,
			extractedData.TARIH
		});
	}

}






//[HttpPost]
//public ServiceResponse<object> UploadPdf(IFormFile file) {
//	if (file == null || file.Length == 0) {
//		return ServiceResponse<object>.ErrorResponse("No file uploaded.");
//	}

//	try {
//		string extractedText = ProcessPdf(file.OpenReadStream());
//		return ServiceResponse<object>.SuccessResponse(new { Text = extractedText });
//	}
//	catch (Exception ex) {
//		return ServiceResponse<object>.ErrorResponse("Error processing the file: " + ex.Message);
//	}
//}

//private string ProcessPdf(Stream pdfStream) {
//	string extractedText = string.Empty;

//	try {
//		// Spire.Pdf ile PDF dosyasını yükleyin
//		PdfDocument pdfDocument = new PdfDocument();
//		pdfDocument.LoadFromStream(pdfStream);

//		int pageCount = pdfDocument.Pages.Count;

//		// PDF sayfalarını döngü ile işlemeye başlıyoruz
//		for (int i = 0; i < pageCount; i++) {
//			// PDF sayfasını bitmap'e dönüştür
//			using var pageImage = RenderPageToImage(pdfDocument, i);

//			// OCR işlemi yap
//			extractedText += PerformOcrOnImageSections(pageImage);
//		}
//	}
//	catch (Exception ex) {
//		extractedText = "Error reading PDF: " + ex.Message;
//	}

//	return extractedText;
//}

//private Bitmap RenderPageToImage(PdfDocument pdfDocument, int pageIndex) {
//	// Spire.Pdf ile PDF sayfasını bitmap'e render et
//	PdfPageBase page = pdfDocument.Pages[pageIndex];

//	// Sayfayı 300 DPI çözünürlük ile bitmap'e dönüştür
//	// SaveToImage metodu ile PDF sayfasını bitmap'e dönüştürme
//	Bitmap pageImage = page.SaveToImage(300);
//	return pageImage;
//}

//private string PerformOcrOnImageSections(Bitmap pageImage) {
//	string pageText = string.Empty;

//	int width = pageImage.Width / 2;
//	int height = pageImage.Height / 2;

//	Rectangle[] sections = new Rectangle[] {
//		new Rectangle(0, 0, width, height),
//		new Rectangle(width, 0, width, height),
//		new Rectangle(0, height, width, height),
//		new Rectangle(width, height, width, height)
//	};

//	foreach (var section in sections) {
//		using (Bitmap croppedImage = CropImage(pageImage, section))
//		using (MemoryStream ms = new MemoryStream()) {
//			croppedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
//			ms.Seek(0, SeekOrigin.Begin);
//			pageText += PerformOcrOnImage(ms);
//		}
//	}

//	return pageText;
//}

//private Bitmap CropImage(Bitmap source, Rectangle section) {
//	Bitmap croppedImage = new Bitmap(section.Width, section.Height);
//	using (Graphics g = Graphics.FromImage(croppedImage)) {
//		g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
//	}
//	return croppedImage;
//}

//private string PerformOcrOnImage(Stream imageStream) {
//	using (var engine = new TesseractEngine(tessDataPath, "tur", EngineMode.Default)) {
//		using (var pix = Pix.LoadFromMemory(StreamToByteArray(imageStream))) {
//			using (Page result = engine.Process(pix)) {
//				return result.GetText();
//			}
//		}
//	}
//}

//private byte[] StreamToByteArray(Stream inputStream) {
//	using MemoryStream memoryStream = new();
//	inputStream.CopyTo(memoryStream);
//	return memoryStream.ToArray();
//}







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
