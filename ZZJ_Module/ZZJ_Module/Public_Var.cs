using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZZJ_Module
{
	public static class Public_Var
	{
		public static ZZJSubModuleInterface.ReadCardResultInfoCls RCR = new ZZJSubModuleInterface.ReadCardResultInfoCls();
		public static decimal OldAmount = 0;//原金额
		public static decimal NewAmount = 0;//充值后金额
		public static PlatformInFo PF = new PlatformInFo();//平台患者信息
		public static string orderNo = "", sFlowId = "";

		/// <summary>
		/// 区域卡有效标志(有效区域卡才可以充值)
		/// </summary>
		public static bool IsRealPlatformCrad = true;

		/// <summary>
		/// 区域卡身份证信息齐全标志
		/// </summary>
		public static bool IsHaveIDNo = true;

		//区分港内卡和附属卡以及院内卡标志
		public static string zhjb = "";
	}
}
