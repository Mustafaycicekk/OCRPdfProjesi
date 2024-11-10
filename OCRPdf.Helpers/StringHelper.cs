using System.Text.RegularExpressions;

namespace OCRPdf.Helpers;
public static class StringHelper {
	public static string CleanText(string inputText) {
		if(string.IsNullOrEmpty(inputText)) return inputText;
		string cleanedText = inputText.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
		cleanedText = Regex.Replace(cleanedText, @"[\u0000-\u001F\u007F-\u009F]+", " ");
		cleanedText = Regex.Replace(cleanedText, @"\s+", " ").Trim();
		return cleanedText;
	}
}