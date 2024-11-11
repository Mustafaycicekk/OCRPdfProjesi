using OCRPdf.Data.Entities;
using OCRPdf.Helpers;
using OCRPdf.Repository.Concrete;
using OCRPdf.Service.Abstract;

namespace OCRPdf.Service.Services;
public class OptimizasyonService(OptimizasyonRepository repository) : BaseService<OptimizasyonRepository, Optimizasyon>(repository) {
	public void SaveToDatabase(Optimizasyon optimizasyon, List<Optimizasyon_Satirlari> optimizasyonSatirlari) {
		try {
			Repository.SaveToDatabase(optimizasyon, optimizasyonSatirlari);
			ServiceResponse<Optimizasyon>.SuccessResponse(optimizasyon, "Veri Kaydetme işlemi Başarılı.");
		}
		catch (Exception ex) {
			ServiceResponse<Optimizasyon>.ErrorResponse(ex.Message);
		}
	}
	public ServiceResponse<IEnumerable<Optimizasyon>> GetOptimizasyonByAgirlik(decimal agirlik) {
		try {
			IEnumerable<Optimizasyon> result = Repository.GetOptimizasyonByAgirlik(agirlik);
			if (result != null && result.Any()) { return ServiceResponse<IEnumerable<Optimizasyon>>.SuccessResponse(result); }
			else { return ServiceResponse<IEnumerable<Optimizasyon>>.ErrorResponse("Girilen ağırlığa eşit ve daha yüksek bir kayıt bulunamadı."); }
		}
		catch (Exception ex) {
			return ServiceResponse<IEnumerable<Optimizasyon>>.ErrorResponse(ex.Message);
		}
	}


}