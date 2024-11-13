using OCRPdf.Data.Entities;
using System.Drawing;
using System.Globalization;
using Tesseract;

namespace OCRPdf.Helpers;
public static class PdfExtractionHelper {
	private static TesseractEngine GetTesseractEngine() { 
		return new TesseractEngine(@"./tessdata", "tur", EngineMode.Default); 
	}

	public static Optimizasyon ReadTextFromImage(string imagePath, List<Rectangle> cropAreas) {
		using Bitmap bitmap = new(imagePath);
		Optimizasyon extractedData = new();
		Dictionary<int, Action<Optimizasyon, string>> extractionActions = new() {
			{ 0, (data, text) => data.REF = text },
			{ 1, (data, text) => data.MIKTAR = string.IsNullOrEmpty(text) ? 0 : Convert.ToInt32(text) },
			{ 2, (data, text) => data.IS = text },
			{ 3, (data, text) => data.TARIH = text },
			{ 4, (data, text) => data.CNC = text },
			{ 5, (data, text) => data.MALZEME = text },
			{ 6, (data, text) => data.MAKINA = text },
			{ 7, (data, text) => data.TOPLAM_SURE = text },
			{ 8, (data, text) => data.AGIRLIK = string.IsNullOrEmpty(text) ? 0.0m : Convert.ToDecimal(text, CultureInfo.InvariantCulture) },
			{ 9, (data, text) => data.X = string.IsNullOrEmpty(text) ? 0.0m : Convert.ToDecimal(text,CultureInfo.InvariantCulture) },
			{ 10, (data, text) => data.Y = string.IsNullOrEmpty(text) ? 0.0m : Convert.ToDecimal(text,CultureInfo.InvariantCulture) },
			{ 11, (data, text) => data.KULLANILAN = string.IsNullOrEmpty(text) ? 0.0m : Convert.ToDecimal(text,CultureInfo.InvariantCulture) },
			{ 12, (data, text) => data.T_SAC_K_SAC = string.IsNullOrEmpty(text) ? 0.0m : Convert.ToDecimal(text,CultureInfo.InvariantCulture) },
			{ 13, (data, text) => data.K_SAC_T_SAC = string.IsNullOrEmpty(text) ? 0.0m : Convert.ToDecimal(text,CultureInfo.InvariantCulture) },
		};


		for (int i = 0; i < cropAreas.Count; i++) {
			Rectangle cropArea = cropAreas[i];
			using Bitmap croppedBitmap = bitmap.Clone(cropArea, bitmap.PixelFormat);
			using Pix pix = PixConverter.ToPix(croppedBitmap);
			using TesseractEngine engine = GetTesseractEngine();
			using Page page = engine.Process(pix);

			string extractedText = StringHelper.CleanText(page.GetText());

			if (extractionActions.ContainsKey(i)) {
				extractionActions[i](extractedData, extractedText);
			}
		}

		return extractedData;
	}
	public static List<Optimizasyon_Satirlari> ReadTextFromImageTableRows(string imagePath, List<Rectangle> crop) {
		using Bitmap bitmap = new(imagePath);
		List<Optimizasyon_Satirlari> extractedRows = [];
		int rowCount = 25;
		int yIncrement = 65;

		Dictionary<int, Action<Optimizasyon_Satirlari, string>> extractionActions = new() {
			{ 0, (data, text) => data.SIRA = string.IsNullOrEmpty(text) ? 0 : Convert.ToInt32(text) },
			{ 1, (data, text) => data.REFERANS = text },
			{ 2, (data, text) => data.SAC = string.IsNullOrEmpty(text) ? 0 : Convert.ToInt32(text) },
			{ 3, (data, text) => data.TOPLAM = string.IsNullOrEmpty(text) ? 0 : Convert.ToInt32(text) },
			{ 4, (data, text) => data.KAYIP = string.IsNullOrEmpty(text) ? 0 : Convert.ToInt32(text) },
			{ 5, (data, text) => data.AGIRLIK = string.IsNullOrEmpty(text) ?  0.0m: Convert.ToDecimal(text, CultureInfo.InvariantCulture)},
			{ 6, (data, text) => data.OLCULER = text }
		};

		for (int i = 0; i < rowCount; i++) {
			List<Rectangle> currentRow = [];
			foreach (Rectangle rect in crop) {
				Rectangle newRect = new(rect.X, rect.Y + (i * yIncrement), rect.Width, rect.Height);
				currentRow.Add(newRect);
			}
			Optimizasyon_Satirlari extractedData = new();
			for (int j = 0; j < currentRow.Count; j++) {
				Rectangle cropArea = currentRow[j];
				using Bitmap croppedBitmap = bitmap.Clone(cropArea, bitmap.PixelFormat);
				using Pix pix = PixConverter.ToPix(croppedBitmap);
				using TesseractEngine engine = GetTesseractEngine();
				using Page page = engine.Process(pix);

				string extractedText = StringHelper.CleanText(page.GetText());
				if (extractionActions.ContainsKey(j)) {
					extractionActions[j](extractedData, extractedText);
				}
			}
			if (string.IsNullOrEmpty(extractedData.REFERANS)) break;
			extractedRows.Add(extractedData);
		}
		return extractedRows;
	}



}