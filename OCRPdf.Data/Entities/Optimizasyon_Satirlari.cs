using System.ComponentModel.DataAnnotations.Schema;

namespace OCRPdf.Data.Entities;
[Table("TBL_OPTIMIZASYON_SATIRLARI")]
public class Optimizasyon_Satirlari {
	public int ID { get; set; }
	public int OPTIMIZASYON_ID { get; set; }
	public int SIRA { get; set; }
	public string REFERANS { get; set; }
	public int SAC { get; set; }
	public int TOPLAM { get; set; }
	public int KAYIP { get; set; }
	public decimal AGIRLIK { get; set; }
	public string OLCULER { get; set; }
}
