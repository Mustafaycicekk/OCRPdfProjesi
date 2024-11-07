using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace OCRPdf.Helpers;
public static class TokenHelper {
	public static string GenerateToken(string email) {
		SymmetricSecurityKey symmetricSecurityKey = new(Encoding.UTF8.GetBytes("fgai2cWFRU7lyuWSl9ZKHHCS6vWDJuBIcexzgl4lRz2wsbiIEG3Ks9fYjbK888Yl"));
		JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
		SecurityTokenDescriptor securityTokenDescriptor = new() {
			Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, email) }),
			Expires = DateTime.Now.AddDays(30),
			SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256Signature)
		};
		SecurityToken token = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
		return jwtSecurityTokenHandler.WriteToken(token);
	}
}