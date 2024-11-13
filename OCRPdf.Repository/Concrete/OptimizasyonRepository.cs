using Dapper;
using OCRPdf.Data.Entities;
using OCRPdf.Repository.Abstract;
using System.Data;

namespace OCRPdf.Repository.Concrete;
public class OptimizasyonRepository : DapperRepository<Optimizasyon> {
	public OptimizasyonRepository() { }
	public OptimizasyonRepository(IDbConnection dbConnection) : base(dbConnection) { }

	public void SaveToDatabase(Optimizasyon optimizasyon, List<Optimizasyon_Satirlari> optimizasyonSatirlari) {
		int optimizasyonAdded = Add(optimizasyon);
		foreach (Optimizasyon_Satirlari satir in optimizasyonSatirlari) {
			satir.OPTIMIZASYON_ID = optimizasyonAdded;
			Add(satir);
		}
	}

	public IEnumerable<object> GetColumns(List<string> columns) {
		string query = $"SELECT {string.Join(",", columns)} FROM [TBL_OPTIMIZASYON] o RIGHT JOIN [TBL_OPTIMIZASYON_SATIRLARI] os on o.ID = os.OPTIMIZASYON_ID";
		return DbConnection.Query<object>(query);
	}
	public IEnumerable<Optimizasyon> GetOptimizasyonByWeight(decimal weightThreshold) {
		string query = @"WITH AgirlikToplamlari AS (SELECT SUM(os.AGIRLIK) AS [agirlik toplam], o.ID FROM TBL_OPTIMIZASYON_SATIRLARI os JOIN TBL_OPTIMIZASYON o ON o.ID = os.OPTIMIZASYON_ID GROUP BY o.ID )
				  SELECT o.*
				  FROM TBL_OPTIMIZASYON o
				  JOIN AgirlikToplamlari a ON o.ID = a.ID
				  WHERE a.[agirlik toplam] > @WeightThreshold; ";
		return DbConnection.Query<Optimizasyon>(query, new { WeightThreshold = weightThreshold });
	}
}
