using Dapper;
using OCRPdf.Data.Entities;
using OCRPdf.Repository.Abstract;
using System.Data;

namespace OCRPdf.Repository.Concrete;
public class OptimizasyonRepository : DapperRepository<Optimizasyon> {
	public OptimizasyonRepository(IDbConnection dbConnection) : base(dbConnection) { }
	public OptimizasyonRepository() : base(null) { }

	public void SaveToDatabase(Optimizasyon optimizasyon, List<Optimizasyon_Satirlari> optimizasyonSatirlari) {
		int optimizasyonAdded = Add(optimizasyon);
		foreach (Optimizasyon_Satirlari satir in optimizasyonSatirlari) {
			satir.OPTIMIZASYON_ID = optimizasyonAdded;
			Add(satir);
		}
	}
	public IEnumerable<Optimizasyon> GetOptimizasyonByAgirlik(decimal agirlik) {
		string query = "SELECT * FROM [TBL_OPTIMIZASYON] WHERE AGIRLIK >= @Agirlik";
		return DbConnection.Query<Optimizasyon>(query, new { Agirlik = agirlik });
	}

}
