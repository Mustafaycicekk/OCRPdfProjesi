﻿using System.ComponentModel.DataAnnotations.Schema;

namespace OCRPdf.Data.Entities;
[Table("TBL_USER")]
public class User {
	public int ID { get; set; }
	public string KULLANICI_ADI { get; set; }
	public string PASSWORD { get; set; }
}