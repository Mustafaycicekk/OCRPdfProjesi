using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OCRPdf.Data.Entities;
using OCRPdf.Helpers;
using OCRPdf.Service.Services;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;
using static OCRPdf.Helpers.PdfExtractionHelper;
namespace OCRPdf.Api.Controllers;

[ApiController, Route("[controller]/[action]")]
public class PdfController(IServiceProvider serviceProvider) : ControllerBase {
	private readonly IServiceProvider ServiceProvider = serviceProvider;

	[HttpPost]
	public ServiceResponse<object> ReadToPdfFile(IFormFile file) {
		if(file == null || file.Length == 0) { return ServiceResponse<object>.ErrorResponse("Dosya yüklenmedi"); }
		if(file.ContentType != "application/pdf") { return ServiceResponse<object>.ErrorResponse("Lütfen PDF dosyası yükleyin"); }

		using MemoryStream memoryStream = new();
		file.CopyTo(memoryStream);
		PdfDocument pdfDocument = new();

		try { pdfDocument.LoadFromStream(memoryStream); } catch(Exception ex) { return ServiceResponse<object>.ErrorResponse($"Error loading PDF: {ex.Message}"); }

		string outputDirectory = "Images";
		if(!Directory.Exists(outputDirectory)) { Directory.CreateDirectory(outputDirectory); }

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
			int pageIndex = 0;
			using Image image = pdfDocument.SaveAsImage(pageIndex, PdfImageType.Bitmap, 500, 500);
			using Bitmap bitmap = image as Bitmap;
			if(bitmap != null) {
				string imagePath = Path.Combine(outputDirectory, $"Sayfa-{pageIndex}.png");
				bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
				Optimizasyon extractedData = ReadTextFromImage(imagePath, cropAreas);
				List<Optimizasyon_Satirlari> tableData = ExtractTextFromTableRows(imagePath, crop);
				ServiceProvider.GetService<OptimizasyonService>().SaveToDatabase(extractedData, tableData);
			} else {
				return ServiceResponse<object>.ErrorResponse($"Sayfa {pageIndex} için dönüşüm başarısız.");
			}
		} catch(Exception ex) {
			return ServiceResponse<object>.ErrorResponse($"Sayfa 0 için hata oluştu: {ex.Message}");
		}

		return ServiceResponse<object>.SuccessResponse("Kayıt işlemi başarılı");
	}



//	private static void SaveToDatabase(Optimizasyon extractedData, List<Optimizasyon_Satirlari> tableData) {
//		using var connection = new SqlConnection("Server=DESKTOP-4C96TO4\\SQLEXPRESS; Database=PDF; integrated security=true;TrustServerCertificate=True;");
//		connection.Open();

//		using SqlTransaction transaction = connection.BeginTransaction();

//		try {
//			string insertOptimizasyonSql = @"INSERT INTO [TBL_OPTIMIZASYON] (REF, MIKTAR, [IS], CNC, TARIH, MALZEME, MAKINA, TOPLAM_SURE, AGIRLIK, X, Y, KULLANILAN, T_SAC_K_SAC, K_SAC_T_SAC, KULLANICI_VERISI_1, KULLANICI_VERISI_2, KULLANICI_VERISI_3)
//    VALUES (@REF, @MIKTAR, @IS, @CNC, @TARIH, @MALZEME, @MAKINA, @TOPLAM_SURE, @AGIRLIK, @X, @Y, @KULLANILAN, @T_SAC_K_SAC, @K_SAC_T_SAC, @KULLANICI_VERISI_1, @KULLANICI_VERISI_2, @KULLANICI_VERISI_3); SELECT CAST(SCOPE_IDENTITY() AS INT);";

//			int optimizasyonId = connection.ExecuteScalar<int>(
//	insertOptimizasyonSql,
//	new {
//		extractedData.REF,
//		extractedData.MIKTAR,
//		extractedData.IS,
//		extractedData.CNC,
//		extractedData.TARIH,
//		extractedData.MALZEME,
//		extractedData.MAKINA,
//		extractedData.TOPLAM_SURE,
//		extractedData.AGIRLIK,
//		extractedData.X,
//		extractedData.Y,
//		extractedData.KULLANILAN,
//		extractedData.T_SAC_K_SAC,
//		extractedData.K_SAC_T_SAC,
//		extractedData.KULLANICI_VERISI_1,
//		extractedData.KULLANICI_VERISI_2,
//		extractedData.KULLANICI_VERISI_3
//	},
//	transaction
//);
//			string insertSatirSql = @"INSERT INTO [TBL_OPTIMIZASYON_SATIRLARI] (OPTIMIZASYON_ID, REFERANS, SIRA, SAC, TOPLAM, KAYIP, AGIRLIK, OLCULER) VALUES  (@OPTIMIZASYON_ID, @REFERANS, @SIRA, @SAC, @TOPLAM, @KAYIP, @AGIRLIK, @OLCULER);";
//			foreach(Optimizasyon_Satirlari row in tableData) {
//				connection.Execute(insertSatirSql,
//					new {
//						OPTIMIZASYON_ID = optimizasyonId,
//						row.REFERANS,
//						row.SIRA,
//						row.SAC,
//						row.TOPLAM,
//						row.KAYIP,
//						row.AGIRLIK,
//						row.OLCULER
//					},
//					transaction
//				);
//			}
//			transaction.Commit();
//		} catch(Exception ex) {
//			transaction.Rollback();
//			ServiceResponse<object>.ErrorResponse(ex.Message);
//			throw new Exception("Veritabanına kayıt sırasında hata oluştu.", ex);
//		}
//	}



}







//private List<OptimizasyonSatirlariModel> ExtractTextFromTableRows(string imagePath, List<Rectangle> crop) {
//	List<OptimizasyonSatirlariModel> extractedRows = [];
//	int rowCount = 25; // Kaç satır olacağını varsayalım
//	int yIncrement = 80; // Y koordinat artışı

//	for(int i = 0; i < rowCount; i++) {
//		List<Rectangle> currentRow = [];
//		foreach(Rectangle rect in crop) {
//			Rectangle newRect = new(rect.X, rect.Y + (i * yIncrement), rect.Width, rect.Height);
//			currentRow.Add(newRect);
//		}
//		OptimizasyonSatirlariModel rowData = ReadTextFromTable(imagePath, currentRow);
//		extractedRows.Add(rowData);
//	}

//	return extractedRows;
//}
//private static OptimizasyonModel ReadTextFromImage(string imagePath, List<Rectangle> cropAreas) {
//	using Bitmap bitmap = new(imagePath);
//	OptimizasyonModel extractedData = new();
//	Dictionary<int, Action<OptimizasyonModel, string>> extractionActions = new() {
//			{ 0, (data, text) => data.REF = text },
//			{ 1, (data, text) => data.MIKTAR = text},
//			{ 2, (data, text) => data.IS = text},
//			{ 3, (data, text) => data.TARIH = text },
//			{ 4, (data, text) => data.CNC = text},
//			{ 5, (data, text) => data.MALZEME = text },
//			{ 6, (data, text) => data.MAKINA = text },
//			{ 7, (data, text) => data.TOPLAM_SURE = text },
//			{ 8, (data, text) => data.AGIRLIK = text },
//			{ 9, (data, text) => data.X = text },
//			{ 10, (data, text) => data.Y = text },
//			{ 11, (data, text) => data.KULLANILAN = text },
//			{ 12, (data, text) => data.T_SAC_K_SAC = text },
//			{ 13, (data, text) => data.K_SAC_T_SAC = text },
//	};

//	for(int i = 0; i < cropAreas.Count; i++) {
//		Rectangle cropArea = cropAreas[i];
//		using Bitmap croppedBitmap = bitmap.Clone(cropArea, bitmap.PixelFormat);
//		using Pix pix = PixConverter.ToPix(croppedBitmap);
//		using TesseractEngine engine = new(@"./tessdata", "tur", EngineMode.Default);
//		using Page page = engine.Process(pix);

//		string extractedText = StringHelper.CleanText(page.GetText());

//		if(extractionActions.ContainsKey(i)) {
//			extractionActions[i](extractedData, extractedText);
//		}
//	}

//	return extractedData;
//}
//private static OptimizasyonSatirlariModel ReadTextFromTable(string imagePath, List<Rectangle> crop) {
//	using Bitmap bitmap = new(imagePath);
//	OptimizasyonSatirlariModel extractedData = new();
//	Dictionary<int, Action<OptimizasyonSatirlariModel, string>> extractionActions = new() {
//			{ 0, (data, text) => data.SIRA = text },
//			{ 1, (data, text) => data.REFERANS = text },
//			{ 2, (data, text) => data.SAC = text },
//			{ 3, (data, text) => data.TOPLAM = text },
//			{ 4, (data, text) => data.KAYIP = text },
//			{ 5, (data, text) => data.AGIRLIK = text },
//			{ 6, (data, text) => data.OLCULER = text }
//	};

//	for(int i = 0; i < crop.Count; i++) {
//		Rectangle cropArea = crop[i];
//		using Bitmap croppedBitmap = bitmap.Clone(cropArea, bitmap.PixelFormat);
//		using Pix pix = PixConverter.ToPix(croppedBitmap);
//		using TesseractEngine engine = new(@"./tessdata", "tur", EngineMode.Default);
//		using Page page = engine.Process(pix);

//		string extractedText = StringHelper.CleanText(page.GetText());
//		if(extractionActions.ContainsKey(i)) {
//			extractionActions[i](extractedData, extractedText);
//		}
//	}
//	return extractedData;
//}







//public class OptimizasyonModel {
//	public string REF { get; set; }
//	public string MIKTAR { get; set; }
//	public string IS { get; set; }
//	public string CNC { get; set; }
//	public string TARIH { get; set; }
//	public string MALZEME { get; set; }
//	public string MAKINA { get; set; }
//	public string TOPLAM_SURE { get; set; }
//	public string AGIRLIK { get; set; }
//	public string X { get; set; }
//	public string Y { get; set; }
//	public string KULLANILAN { get; set; }
//	public string T_SAC_K_SAC { get; set; }
//	public string K_SAC_T_SAC { get; set; }
//	public string KULLANICI_VERISI_1 { get; set; }
//	public string KULLANICI_VERISI_2 { get; set; }
//	public string KULLANICI_VERISI_3 { get; set; }
//}
//public class OptimizasyonSatirlariModel {
//	public string SIRA { get; set; }
//	public string REFERANS { get; set; }
//	public string SAC { get; set; }
//	public string TOPLAM { get; set; }
//	public string KAYIP { get; set; }
//	public string AGIRLIK { get; set; }
//	public string OLCULER { get; set; }
//	public override string ToString() {
//		return $"SIRA: {SIRA}, REFERANS: {REFERANS}, SAC: {SAC}, TOPLAM: {TOPLAM}, KAYIP: {KAYIP}, AGIRLIK: {AGIRLIK}, OLCULER: {OLCULER}";
//	}
//}
