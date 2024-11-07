//using Microsoft.Extensions.Configuration;

//namespace Dersigo.Helper;
//public enum Sections {
//	ConnectionStrings,
//	Smtp,
//	PayTR,
//	Vimeo,
//}
//public class AppConfig {
//	public readonly struct Static {
//#if DEBUG
//		private static string SettingPath => @"C:\Projects\Dersigo.App\Infrastructure";
//		public static string ImagePath => "https://localhost:44350/";
//		public static string RootPathAsked => @"C:\\Projects\\Dersigo.App\\Service\\Dersigo.Service\\wwwroot\\uploads\\askedQuestions";
//		public static string RootPathSolution => @"C:\\Projects\\Dersigo.App\\Service\\Dersigo.Service\\wwwroot\\uploads\\solutionQuestions";
//#else
//	public static string RootPathAsked => @"E:\Inetpub\vhosts\edutes.net.tr\api.edutes.net.tr\wwwroot\uploads\askedQuestions";
//	public static string RootPathSolution => @"E:\Inetpub\vhosts\edutes.net.tr\api.edutes.net.tr\wwwroot\uploads\solutionQuestions";
//	public static string ImagePath => "https://api.edutes.net.tr";

//	//private static string SettingPath => @"E:\Inetpub\vhosts\dersigo.com\manage.dersigo.com";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\edutes.net.tr\api.edutes.net.tr";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\edutes.net.tr\app.edutes.net.tr";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\dersigo.com\app.dersigo.com";	
//	//private static string SettingPath => @"C:\HostedWebs\api.dersigo.test";	
//	//private static string SettingPath => @"C:\inetpub\vhosts\geleceksensinelazig.com";	
//	//private static string SettingPath => @"C:\Inetpub\vhosts\dersigo.com\sincanakademi.dersigo.com";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\dersigo.com\efechp.dersigo.com";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\dersigo.com\tepav.dersigo.com";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\ibek.com.tr\ogrenci.ibek.com.tr";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\dersigo.com\kurumsal.dersigo.com";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\basarirortaokulu.com\ogrenci.basarirortaokulu.com";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\oyaakinyildiz.k12.tr\ogrenci.oyaakinyildiz.k12.tr";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\dersigo.com\ogrenci.dersigo.com";	
//	private static string SettingPath => @"E:\Inetpub\vhosts\gencegitim.com.tr\ogrenci.gencegitim.com.tr";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\dersigo.com\sorcozogretmen.dersigo.com";	
//	//private static string SettingPath => @"E:\Inetpub\vhosts\mustafakemal.com.tr\ogrenci.mustafakemal.com.tr";	

//#endif
//		public static readonly IConfigurationRoot ConfigurationRoot = new ConfigurationBuilder().SetBasePath(SettingPath).AddJsonFile("appsettings.json").Build();
//		public static string JwtSecretKey => GetSection(nameof(JwtSecretKey));
//		public static string HomeworkPath => GetSection(nameof(HomeworkPath));

//		#region ConnectionStrings
//		public static string CS_EntityFramework => GetSection("EntityFramework", Sections.ConnectionStrings);
//		public static string CS_MongoDB => GetSection("MongoDB", Sections.ConnectionStrings);
//		#endregion

//		#region SMTP
//		public static string Smtp_SenderEmail => GetSection("SenderEmail", Sections.Smtp);
//		public static string Smtp_AppPassword => GetSection("AppPassword", Sections.Smtp);
//		public static string Smtp_Host => GetSection("Host", Sections.Smtp);
//		#endregion

//		#region Vimeo
//		public static string Vimeo_AccessToken => $"Bearer {GetSection("AccessToken", Sections.Vimeo)}";
//		#endregion

//	}


//	#region PayTR
//	public static int PayTR_MerchantId => Convert.ToInt32(GetSection("MerchantId", Sections.PayTR));
//	public static string PayTR_MerchantKey => GetSection("MerchantKey", Sections.PayTR);
//	public static string PayTR_MerchantSalt => GetSection("MerchantSalt", Sections.PayTR);
//	#endregion

//	private static string GetSection(string value, Sections? section = null) {
//		if (section.HasValue) return Static.ConfigurationRoot.GetSection(section.ToString()).GetSection(value).Value;
//		return Static.ConfigurationRoot.GetSection(value).Value;
//	}
//}