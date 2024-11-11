using System.Drawing;

namespace OCRPdf.Helpers;
public static class ImageCoordinates {
	public static List<Rectangle> CropAreas { get; } = [
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
	public static List<Rectangle> Crop { get; } = [
			new Rectangle(64, 3549, 102, 59), // SIRA
			new Rectangle(176, 3555, 1500, 68), // REFERANS
			new Rectangle(1814, 3555, 250, 68), // SAC
			new Rectangle(2162, 3555, 250, 68), // TOPLAM
			new Rectangle(2507, 3555, 250, 68), // KAYIP
			new Rectangle(2855, 3555, 360, 68), // AGIRLIK
			new Rectangle(3250, 3555, 850, 68)  // OLCULER
	];
}
