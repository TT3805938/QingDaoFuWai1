using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ZZJCore;
using Mw_Public;
using System.Data;
using Mw_Voice;
using System.Threading;
using System.Xml;
using System.IO;
using System.Drawing;
using Mw_MSSQL;
using System.Data.SqlClient;
using System.Collections;
using System.Drawing.Printing;
using HL.Devices.DKQ;
using HL.Devices.ZKJ;
using System.Text;//证件卡

public static class TESTQYKBK
{
	static bool ZBCardOK = false;//准备卡OK标志位
	static bool BKBZ = false;

	public static string FKMS = "";
	/// <summary>
	/// 办卡
	/// </summary>
	/// <param name="ModuleID">0为有证  1为无证</param>
	/// <returns></returns>
	public static bool BKMain(int ModuleID)
	{
		#region 初始化
		if (ModuleID == 0) ZZJCore.Public_Var.ModuleName = "自助办卡";
		if (ModuleID == 1) ZZJCore.Public_Var.ModuleName = "儿童无证办卡";
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.Public_Var.patientInfo = new PatientInfo();
		ZZJCore.Public_Var.cardInfo.CardNo = "";
		ZZJCore.Public_Var.cardInfo.CardType = "1";
		//ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.SuanFa.Proc.Log("打开办卡");
		ZZJCore.Initial.Read();
		#endregion

    
		ZZJCore.Public_Var.patientInfo.PatientName = "测试";
		ZZJCore.Public_Var.patientInfo.Sex = "男";
		ZZJCore.Public_Var.patientInfo.Nation = "民族";
		ZZJCore.Public_Var.patientInfo.Address = "地狱";
		ZZJCore.Public_Var.patientInfo.DOB = "1992-12-10";
		ZZJCore.Public_Var.patientInfo.IDNo = "511702197409284963";
		ZZJCore.Public_Var.patientInfo.Mobile="13112123133";
		ZZJ_Module.Public_Var.PF.patientCard = "511702197409284963";
		//平台流水ID
		ZZJ_Module.Public_Var.PF.patientId="1234554321";
    // */

		#region 准备就诊卡
		ZBCard();
		if (!ZBCardOK)
		{
			ZZJCore.SuanFa.Proc.Log("准备就诊卡失败退出");
			ZZJCore.BackForm.ShowForm("准备就诊卡失败,请联系管理员.", true);
			Application.DoEvents();
			return false;
		}
		#endregion

		return BKEnd(ModuleID);
	}

	public static bool BKEnd(int ModuleID)
	{
		ZZJ_Module.PingTaiCreateCard.WriterCard();

		ZZJCore.SuanFa.Proc.Log("办卡完成,调用其它功能!");
		ZZJCore.SuanFa.Proc.Log("调用附加功能返回!");
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");

		//PrintCard();//打印卡片
		ZZJCore.SuanFa.Proc.RunEXE(
			ZZJCore.Public_Var.ModulePath + "ZZJCoreTest.exe",
			string.Format("PrintCard {0} {1} {2}",
				@"""" + ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev + @"""",
				@"""" + ZZJCore.Public_Var.patientInfo.PatientName + @"""",
				ZZJ_Module.Public_Var.PF.patientCard
			)
		);

		if (ModuleID == 0) ZZJCore.Public_Var.ModuleName = "自助办卡";
		if (ModuleID == 1) ZZJCore.Public_Var.ModuleName = "儿童无证办卡";
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.BackForm.ShowForm("请收好您的就诊卡!", true);
		ZZJCore.SuanFa.Proc.Log("办卡完成,关闭办卡模块!");

		ZZJCore.BackForm.CloseForm();
		return true;
	}


	public static void PrintCard()
	{
		ZZJCore.SuanFa.PrintCall.Print(ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev, PC, 500, 318, false);
		ZZJCore.SuanFa.Proc.MsgSend(0x21, 0x00);//办卡成功
	}

	private static void PC(object sender, PrintPageEventArgs e)
	{
		try
		{//姓名
			e.Graphics.DrawString(ZZJCore.Public_Var.patientInfo.PatientName, new Font(new FontFamily("黑体"), 12, FontStyle.Bold), System.Drawing.Brushes.Black, 55, 25);
		}
		catch
		{ }

		try
		{//患者ID
			e.Graphics.DrawString(ZZJ_Module.Public_Var.PF.patientCard, new Font(new FontFamily("黑体"), 12, FontStyle.Bold), System.Drawing.Brushes.Black, 175, 25);
		}
		catch
		{ }
	}

	private static void ZBCard()//准备就诊卡
	{
		if (ZZJCore.SuanFa.Proc.ReadPublicINI("VirtualCardNO") == "1")
		{
			ZZJCore.Public_Var.cardInfo.CardNo = DateTime.Now.ToString("0hhmmssfff");
			ZBCardOK = true;
			return;
		}

		if (ZZJ_Module.EasyHiTi.ReadCard() == 0)
		{
			ZBCardOK = true;
			return;
		}

		ZZJCore.SuanFa.Proc.sleep(300);

		ZBCardOK = ZZJ_Module.EasyHiTi.ReadCard() == 0;
		return;
	}

	#region 保存数据表中
	private static void SavelocalAndServerData()
	{
		Thread threadServer = new Thread(SaveServerData);//添加到远程数据表中
		threadServer.IsBackground = true;
		threadServer.Start();
	}

	#region 添加远程数据记录
	private static SqlParameter AddSP(SqlDbType SDT, string PName, object Value)
	{
		SqlParameter parameter = new SqlParameter();
		parameter.SqlDbType = SDT;
		parameter.ParameterName = PName;
		parameter.Value = Value;
		return (parameter);
	}

	private static void SaveServerData()
	{
		try
		{
			if (MSSQL_Init.Init_DBConfig() != 0)
			{
				ZZJCore.SuanFa.Proc.Log("加载数据库配置文件失败");
				return;
			}
			ZZJCore.SuanFa.Proc.Log("加载配置文件成功");
			if (MSSQL_Operate.Test_MSSQL_Conn() != 0)
			{//数据库测试连接失败
				ZZJCore.SuanFa.Proc.Log("数据库测试连接失败");
				return;
			}
			ZZJCore.SuanFa.Proc.Log("数据库测试连接成功");
		}
		catch
		{ return; }
		try
		{
			string msg = "";
			MSSQL_Operate mssql_operate = new MSSQL_Operate();
			ArrayList AL = new ArrayList();
			//发卡描述 
			AL.Add(AddSP(SqlDbType.VarChar, "@LSH", ZZJCore.Public_Var.SerialNumber));//流水号
			AL.Add(AddSP(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.cardInfo.CardNo));//登记号
			AL.Add(AddSP(SqlDbType.VarChar, "@KH", ZZJCore.Public_Var.cardInfo.CardNo));//卡号
			AL.Add(AddSP(SqlDbType.Int, "@KPLX", int.Parse(ZZJCore.Public_Var.cardInfo.CardType)));//卡类型
			AL.Add(AddSP(SqlDbType.VarChar, "@XM", ZZJCore.Public_Var.patientInfo.PatientName));//姓名
			AL.Add(AddSP(SqlDbType.VarChar, "@XB", ZZJCore.Public_Var.patientInfo.Sex));//性别
			AL.Add(AddSP(SqlDbType.VarChar, "@SJHM", ZZJCore.Public_Var.patientInfo.Mobile));//手机号码
			AL.Add(AddSP(SqlDbType.Date, "@CSNY", DateTime.Parse(ZZJCore.Public_Var.patientInfo.DOB)));//出生日期
			AL.Add(AddSP(SqlDbType.VarChar, "@ZJHM", ZZJCore.Public_Var.patientInfo.IDNo));//证件号
			AL.Add(AddSP(SqlDbType.VarChar, "@ZJLX", ZZJCore.Public_Var.patientInfo.IDType));//证件类型
			AL.Add(AddSP(SqlDbType.VarChar, "@YWLX", "新办卡"));//业务类型
			AL.Add(AddSP(SqlDbType.Int, "@SL", 1));//数量
			AL.Add(AddSP(SqlDbType.Decimal, "@JE", (decimal)0));//金额
			AL.Add(AddSP(SqlDbType.DateTime, "@SJ", DateTime.Now));//时间
			AL.Add(AddSP(SqlDbType.VarChar, "@SFY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//收费员
			AL.Add(AddSP(SqlDbType.VarChar, "@KFLX", "无"));//病人扣费类型
			AL.Add(AddSP(SqlDbType.Int, "@FKBZ", BKBZ ? 0 : 1));//发卡标志
			AL.Add(AddSP(SqlDbType.VarChar, "@FKMS", FKMS));//发卡描述
			AL.Add(AddSP(SqlDbType.VarChar, "@CZFS", 0));//发卡充值描述
			AL.Add(AddSP(SqlDbType.Decimal, "@CZJE", (decimal)0));//充值金额
			AL.Add(AddSP(SqlDbType.Int, "@CZBZ", 0));//充值成功标志
			AL.Add(AddSP(SqlDbType.VarChar, "@CZMS", "没有充值"));//充值描述
			AL.Add(AddSP(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));//自助机编号
			AL.Add(AddSP(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//HIS操作员
			AL.Add(AddSP(SqlDbType.VarChar, "@BL1", ZZJ_Module.Public_Var.PF.patientCard));//区域卡面号
			if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_FK_MX_ForZZJ_V4]", (SqlParameter[])AL.ToArray(typeof(SqlParameter)), out msg) > 0)
			{
				ZZJCore.SuanFa.Proc.Log("记录写入服务器成功");
				return;
			}
			ZZJCore.SuanFa.Proc.Log("记录写入服务器失败" + msg);
		}
		catch (Exception Ex)
		{
			ZZJCore.SuanFa.Proc.Log("记录写入服务器异常，" + Ex.Message);
		}// */
	}
	#endregion
	#endregion
}//End Class