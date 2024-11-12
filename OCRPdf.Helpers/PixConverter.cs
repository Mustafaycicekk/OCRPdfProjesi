using System.Drawing;
using Tesseract;

namespace OCRPdf.Helpers;
public class PixConverter {
	public static Pix ToPix(Bitmap bitmap) {
		using MemoryStream memoryStream = new();
		bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
		memoryStream.Seek(0, SeekOrigin.Begin);
		return Pix.LoadFromMemory(memoryStream.ToArray());
	}

}