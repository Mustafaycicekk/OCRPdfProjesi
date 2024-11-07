using Crypt = BCrypt.Net.BCrypt;
namespace OCRPdf.Helpers;
public static class BcryptHasher {
	public static string HashPassword(string password) => Crypt.HashPassword(password);
	public static bool VerifyPassword(string password, string hashedPassword) => Crypt.Verify(password, hashedPassword);
}