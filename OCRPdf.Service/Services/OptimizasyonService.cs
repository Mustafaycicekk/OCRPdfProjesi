using OCRPdf.Data.Entities;
using OCRPdf.Repository.Concrete;
using OCRPdf.Service.Abstract;

namespace OCRPdf.Service.Services;
public class OptimizasyonService(OptimizasyonRepository repository) : BaseService<OptimizasyonRepository, Optimizasyon>(repository) {
	public void SaveToDatabase(Optimizasyon optimizasyon, List<Optimizasyon_Satirlari> optimizasyonSatirlari) {
		Repository.SaveToDatabase(optimizasyon, optimizasyonSatirlari);
	}
}