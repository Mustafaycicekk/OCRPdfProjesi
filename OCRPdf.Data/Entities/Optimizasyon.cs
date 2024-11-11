using System.ComponentModel.DataAnnotations.Schema;

namespace OCRPdf.Data.Entities;
[Table("TBL_OPTIMIZASYON")]
public class Optimizasyon {
	public int ID { get; set; }
	public string REF { get; set; }
	public int MIKTAR { get; set; }
	public string IS { get; set; }
	public string CNC { get; set; }
	public string TARIH { get; set; }
	public string MAKINA { get; set; }
	public string MALZEME { get; set; }
	public string TOPLAM_SURE { get; set; }
	public decimal AGIRLIK { get; set; }
	public decimal X { get; set; }
	public decimal Y { get; set; }
	public decimal KULLANILAN { get; set; }
	public decimal T_SAC_K_SAC { get; set; }
	public decimal K_SAC_T_SAC { get; set; }
	public string KULLANICI_VERISI_1 { get; set; }
	public string KULLANICI_VERISI_2 { get; set; }
	public string KULLANICI_VERISI_3 { get; set; }
}