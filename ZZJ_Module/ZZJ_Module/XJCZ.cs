using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Windows.Forms;
using System.Xml;
using Mw_Public;
using Mw_MSSQL;
using System.Data.SqlClient;
using System.Threading;
using System.Collections;
using System.Drawing;

public static class XJCZ
{
	static ZZJCore.xjycProcessCls DB = new ZZJCore.xjycProcessCls(); //现金充值过程类
	static decimal DC = 0;
	static decimal OldAmount = 0;//原金额
	static decimal NewAmount = 0;//充值后金额
	public static bool XJCZMain()
	{
		#region 初始化
		string Msg = "";
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.Public_Var.ModuleName = "现金充值";
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.Initial.Read();
		if (XMLCore.GetUserInfo(out Msg) != 0)
		{
			if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true);
			ZZJCore.SuanFa.Proc.Log("查询卡信息失败!" + ZZJCore.Public_Var.cardInfo.CardNo);
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		#endregion
		//除院内卡以外的卡不能充值
		//if (ZZJCore.Public_Var.cardInfo.CardType.ToString() != "0")
		//{
		//  ZZJCore.BackForm.ShowForm("该卡不支持现金充值,请换院内卡！", true);
		//  ZZJCore.SuanFa.Proc.Log("该卡不支持现金充值,请换院内卡!" + ZZJCore.Public_Var.cardInfo.CardNo);
		//  ZZJCore.BackForm.CloseForm();
		//  return true;
		//}
		if (ZZJCore.Public_Var.cardInfo.CardType == "1")
		{
			ZZJCore.SuanFa.Proc.ZZJMessageBox("港内卡与附属卡现金充值未开启", "港内卡与附属卡现金充值功能当前禁止使用!", true);
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		
		int iRet = XJCZModule();
		if (iRet == 0)
		{
			ZZJCore.FormSkin.UseRetCard = true;
			ZZJCore.BackForm.ShowForm("充值成功!", true);
		}
		else
		{
			ZZJCore.BackForm.CloseForm();
		}
		return (iRet != -5);
	}

	public static int XJCZModule(bool NeedRetButt = false)
	{
		ZZJ_Module.Public_Var.sFlowId = "";//清空区域流水号
		ZZJ_Module.Public_Var.orderNo = "";

		

		//if (ZZJCore.Public_Var.cardInfo.CardType == "1" && !ZZJ_Module.Public_Var.IsRealPlatformCrad)
		//{
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox("区域平台查询不到该卡信息", "查询区域卡信息失败,您当前无法使用预交金功能!", true);
		//  return -1;
		//}

		//if (ZZJCore.Public_Var.cardInfo.CardType == "0" && ZZJ_Module.Public_Proc.ReadZZJConfig("YNK_XJCZ") == "0")
		//{
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox("院内卡现金充值未开启", "院内卡现金充值功能当前禁止使用!", true);
		//  return -1;
		//}

		//if (ZZJCore.Public_Var.cardInfo.CardType == "1" && ZZJ_Module.Public_Proc.ReadZZJConfig("QYK_XJCZ") == "0")
		//{
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox("区域卡现金充值未开启", "区域卡现金充值功能当前禁止使用", true);
		//  return -1;
		//}

		//if (ZZJCore.Public_Var.cardInfo.CardType == "1" && !ZZJ_Module.Public_Var.IsHaveIDNo)
		//{
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox("区域卡身份信息不全", "该卡查询不到身份证号,无法充值!", true);
		//  return -1;
		//}

		//if (ZZJCore.Public_Var.cardInfo.CardType == "2" && ZZJ_Module.Public_Proc.ReadZZJConfig("SBK_XJCZ") == "0")
		//{
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox("医保卡现金充值未开启", "医保卡现金充值功能当前禁止使用", true);
		//  return -1;
		//}

		int iRet = FormStyle.XJCZStyle.XJSFForm(NeedRetButt,ref DC);
		if (iRet != 0) return iRet;
		if (DC == 0) return -1;

		#region 充值
		ZZJCore.BackForm.SetFormText("正在充值,请稍候...");
		string YCMS = "";
		OldAmount = decimal.Parse(ZZJCore.Public_Var.patientInfo.DepositAmount);
		iRet = XMLCore.CZ(DC, out YCMS, out InvokeRet);
		ZZJCore.BackForm.SetFormText("正在准备,请稍候...");
		/* 刷新余额方式1
		NewAmount = (iRet == 0) ? (OldAmount + DC) : OldAmount;
		ZZJCore.Public_Var.patientInfo.DepositAmount = NewAmount.ToString();
		// */

		string Msg = "";
		XMLCore.GetUserInfo(out Msg);
		NewAmount = decimal.Parse(ZZJCore.Public_Var.patientInfo.DepositAmount);

		DB.YCMS = YCMS;
		#endregion

		#region 数据库记录
		DB.CGBZ = iRet == 0 ? 0 : -1;//成功标志
		Thread T = new Thread(SaveData);
		T.IsBackground = true;
		T.Start();
		#endregion

		if (ZZJCore.SuanFa.Proc.ReadPublicINI("CZDoNotTicket", "0") == "1") return 0;
		#region 打印凭条
		List<ZZJCore.ZZJStruct.DataLine> AL = new List<ZZJCore.ZZJStruct.DataLine>();
		AL.Add(new ZZJCore.ZZJStruct.DataLine("金额", DC.ToString("C"), null, new Font(new FontFamily("黑体"), 20, FontStyle.Bold)));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("充值结果", (iRet == 0) ? "【成功】" : "【失败】", null, new Font(new FontFamily("黑体"), 20, FontStyle.Bold)));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("余额", NewAmount.ToString("C"), null, null));
		if (iRet != 0)
		{
			ZZJCore.SuanFa.PrintCall.PrintPT("现金充值退费凭证", AL.ToArray(), "如果充值失败请持本凭证至窗口!", false);
			ZZJCore.BackForm.ShowForm("充值失败!" + YCMS, true);
			return -3;
		}
		ZZJCore.SuanFa.PrintCall.PrintPT("自助机充值凭证", AL.ToArray(), "如果充值失败请持本凭证至窗口!", false);
		#endregion

		return 0;
	}

	#region 数据库操作
	private static void Init_DBConfig()//加载数据库
	{
		try
		{
			if (MSSQL_Init.Init_DBConfig("Config.db") != 0)
			{
				ZZJCore.SuanFa.Proc.Log("加载数据库配置文件失败");//加载配置文件失败
				return;
			}
			ZZJCore.SuanFa.Proc.Log("加载配置文件成功");//加载配置文件成功
			if (MSSQL_Operate.Test_MSSQL_Conn() != 0)
			{
				//数据库测试连接失败
				ZZJCore.SuanFa.Proc.Log("数据库测试连接失败");
				return;
			}
			ZZJCore.SuanFa.Proc.Log("数据库测试连接成功");//数据库测试连接成功
		}
		catch (Exception e)
		{ ZZJCore.SuanFa.Proc.Log(e); }
	}

	private static SqlParameter AddSP(SqlDbType SDT, string PName, object Value)
	{
		SqlParameter parameter = new SqlParameter();
		parameter.SqlDbType = SDT;
		parameter.ParameterName = PName;
		parameter.Value = Value;
		return (parameter);
	}

	private static int InvokeRet = 0;
	public static void SaveData()
	{
		string msg = "";
		#region 网络数据库
		Init_DBConfig();
		try
		{
			MSSQL_Operate mssql_operate = new MSSQL_Operate();
			SqlParameter[] parameter = null;
			List<SqlParameter> AL = new List<SqlParameter>();
			AL.Add(AddSP(SqlDbType.VarChar, "@LSH", ZZJCore.Public_Var.SerialNumber));//流水号
			AL.Add(AddSP(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.patientInfo.PatientID));//登记号
			AL.Add(AddSP(SqlDbType.VarChar, "@KH", ZZJCore.Public_Var.cardInfo.CardNo));//卡号
			if (string.IsNullOrEmpty(ZZJCore.Public_Var.cardInfo.CardType)) ZZJCore.Public_Var.cardInfo.CardType = "0";
			AL.Add(AddSP(SqlDbType.Int, "@KPLX", int.Parse(ZZJCore.Public_Var.cardInfo.CardType)));//卡类型
			AL.Add(AddSP(SqlDbType.VarChar, "@XM", ZZJCore.Public_Var.patientInfo.PatientName));//姓名
			AL.Add(AddSP(SqlDbType.Decimal, "@JE", DC));//充值金额
			AL.Add(AddSP(SqlDbType.DateTime, "@SJ", DateTime.Now.ToString()));//充值时间
			AL.Add(AddSP(SqlDbType.VarChar, "@SFY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//收费员

			int BZ = DB.CGBZ;
			try
			{
				if (ZZJCore.Public_Var.cardInfo.CardType == "1") BZ = new int[] { 0, 1, -1 }[InvokeRet];
			}
			catch (Exception E)
			{
				ZZJCore.SuanFa.Proc.Log(E);
			}
			AL.Add(AddSP(SqlDbType.Int, "@CGBZ", BZ));//操作标识 0成功 其他失败
			AL.Add(AddSP(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));//自助机编号
			AL.Add(AddSP(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//HIS操作员
			AL.Add(AddSP(SqlDbType.VarChar, "@BRLX", "自费"));//病人类型
			AL.Add(AddSP(SqlDbType.VarChar, "@YWLX", "现金充值"));//业务类型
			AL.Add(AddSP(SqlDbType.Decimal, "@YCQJE", OldAmount));//充值前金额
			AL.Add(AddSP(SqlDbType.Decimal, "@YCHJE", NewAmount));//充值后金额
			AL.Add(AddSP(SqlDbType.VarChar, "@YCMS", DB.YCMS));//HIS充值描述
			AL.Add(AddSP(SqlDbType.Decimal, "@SSXJ", DC));//钞箱实收现金
			AL.Add(AddSP(SqlDbType.VarChar, "@BL1", ZZJ_Module.Public_Var.sFlowId));
			AL.Add(AddSP(SqlDbType.VarChar, "@BL2", ZZJ_Module.Public_Var.orderNo));
			AL.Add(AddSP(SqlDbType.VarChar, "@BL3", ""));

			parameter = AL.ToArray();
			if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_MZ_XJYC_MX_ForZZJ_V5]", parameter, out msg) > 0)
			{
				ZZJCore.SuanFa.Proc.Log("现金预存记录写入服务器成功");
			}
			else
			{
				ZZJCore.SuanFa.Proc.Log("现金预存记录写入服务器失败" + msg);
			}
		}
		catch (Exception Ex)
		{
			ZZJCore.SuanFa.Proc.Log(Ex);
			ZZJCore.SuanFa.Proc.Log("现金预存记录写入服务器失败，可能是网络中断或远程自助机服务器未开启，" + Ex.Message);
		}
		#endregion

		#region 本地数据库
		int result = 0;
		msg = "";
		try
		{
			Mw_Access.Access_DB AD = new Mw_Access.Access_DB(ZZJCore.Public_Var.LocalDataPath);
			string sql = "insert into Tb_His_MZ_XJYC_MX(XJYCGuid,LSH,DJH,KH,KPLX,XM,JE,SJ,ZJHM,SFY,CGBZ,JYLX,ZZJBH,HisCZY,BL1) values"
					 + "('" + Guid.NewGuid().ToString() + "','"//XJYCGuid
					 + ZZJCore.Public_Var.SerialNumber + "','"//LSH
					+ ZZJCore.Public_Var.patientInfo.PatientID + "','"//DJH
					 + ZZJCore.Public_Var.cardInfo.CardNo + "',"//KH
					 + ZZJCore.Public_Var.cardInfo.CardType + ",'"//KPLX
					 + ZZJCore.Public_Var.patientInfo.PatientName + "',"//XM
					 + DC + ",'"//JE
					 + DateTime.Now.ToString() + "','"//SJ
					 + ZZJCore.Public_Var.patientInfo.IDNo + "','"//ZJHM
					 + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "',"//SFY
					 + DB.CGBZ + ",'"//CGBZ
					 + "现金充值" + "','"//交易(业务）类型
					 + ZZJCore.Public_Var.ZZJ_Config.TerminalID + "','"
					 + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "','"
					 + DB.YCMS + "')";//充值描述
			result = AD.ExecuteSQLNonquery(sql, out msg);
			AD.Close();
			//result = ZZJCore.Public_AccessHelper.Access_ExecuteNonQuery(DB, out msg);
		}
		catch (Exception e) { ZZJCore.SuanFa.Proc.Log(e); }
		ZZJCore.SuanFa.Proc.Log(result > 0 ? "[现金充值]现金预存记录写入本机成功" : ("现金预存记录写入本机失败" + msg));
		#endregion
	}
	#endregion
}//End Class
