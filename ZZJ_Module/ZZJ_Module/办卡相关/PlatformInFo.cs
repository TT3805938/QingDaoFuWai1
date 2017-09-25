using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZZJ_Module
{
	//写卡信息
	public class CardInfo
	{
		//证件类型
		public string Patient_CardType = "1";
		//病人Id
		public string Patient_Id { get; set; }
		//平台卡号
		public string PT_CradId { get; set; }
		//发行日期
		public string FX_Date = DateTime.Now.ToString("yyyy-MM-dd");
		//有效日期
		public string YX_Date = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");
		//启用日期
		public string QY_Date = DateTime.Now.ToString("yyyy-MM-dd");
		//院区代码
		public string Hospit_Code = "3702010324";
		//平台患者流水号
		public string PTLS_Id { get; set; }
	}

	//建档信息
	public class PlatformInFo
	{
		/// <summary>
		/// 门诊,病例号
		/// </summary>
		public string patientId = "";
		/// <summary>
		/// 卡面号
		/// </summary>
		public string patientCard = "";
		/// <summary>
		/// 病人类型 自费,医保
		/// </summary>
		public string patientType = "自费";
		/// <summary>
		/// 监护人身份证
		/// </summary>
		public string guardianNo = "";
		/// <summary>
		/// 卡内 余额
		/// </summary>
		public string accBalance = "";
		/// <summary>
		/// 状态
		/// </summary>
		public byte state=0;
	}
}
