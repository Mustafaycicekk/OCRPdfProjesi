namespace OCRPdf.Data.Entities;
public class Optimizasyon_Satirlari {
	public int ID { get; set; }
	public int OPTIMIZASYON_ID { get; set; }
	public int SIRA { get; set; }
	public int MALZEME { get; set; }
	public int MALZEME_ACIKLAMA { get; set; }
	public int SAC { get; set; }
	public int TOPLAM { get; set; }
	public int KAYIP { get; set; }
	public decimal AGIRLIK { get; set; }
	public int OLCULER { get; set; }
}
