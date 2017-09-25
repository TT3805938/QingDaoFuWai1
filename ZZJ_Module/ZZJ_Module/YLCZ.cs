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
using System.Drawing.Printing;
using System.IO;
using System.Text;
using ZZJCore;
using ZZJCore.ZZJStruct;

public static class YLCZ
{
	static ZZJCore.xjycProcessCls DB = new ZZJCore.xjycProcessCls();//现金充值过程类
	private static decimal DC = 0;
	static decimal OldAmount = 0;//原金额
	static decimal NewAmount = 0;//充值后金额
	static int iRet=0;
	public static bool YLCZMain()
	{
		ZZJ_Module.Public_Var.sFlowId = "";//清空区域流水号
		ZZJ_Module.Public_Var.orderNo = "";

		#region 初始化
		string Msg = "";
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.Public_Var.ModuleName = "银联卡充值";
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.Initial.Read();
		if (XMLCore.GetUserInfo(out Msg) != 0)
		{
			if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true);
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		#endregion

		#region 判断该卡是否能充值

		//if (ZZJ_Module.Public_Var.zhjb.ToString() == "2" || ZZJ_Module.Public_Var.zhjb.ToString() == "3")
		if (ZZJCore.Public_Var.cardInfo.CardType=="1")
			{
				ZZJCore.SuanFa.Proc.ZZJMessageBox("港内卡与附属卡银联充值未开启", "港内卡与附属卡银联充值功能当前禁止使用!", true);
				ZZJCore.BackForm.CloseForm();
				return true;
			}

		//if (ZZJCore.Public_Var.cardInfo.CardType == "1" && !ZZJ_Module.Public_Var.IsRealPlatformCrad)
		//{
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox("区域平台查询不到该卡信息", "查询区域卡信息失败,您当前无法使用预交金功能!", true);
		//  ZZJCore.BackForm.CloseForm();
		//  return true;
		//}

		//if (ZZJCore.Public_Var.cardInfo.CardType == "0" && ZZJ_Module.Public_Proc.ReadZZJConfig("YNK_YLKCZ") == "0")
		//{
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox("院内卡银联充值未开启", "院内卡银联充值功能当前禁止使用!", true);
		//  ZZJCore.BackForm.CloseForm();
		//  return true;
		//}

		//if (ZZJCore.Public_Var.cardInfo.CardType == "1" && ZZJ_Module.Public_Proc.ReadZZJConfig("QYK_YLKCZ") == "0")
		//{
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox("区域卡银联充值未开启", "区域卡银联充值功能当前禁止使用", true);
		//  ZZJCore.BackForm.CloseForm();
		//  return true;
		//}

		//if (ZZJCore.Public_Var.cardInfo.CardType == "1" && !ZZJ_Module.Public_Var.IsHaveIDNo)
		//{
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox("区域卡身份信息不全", "该卡查询不到身份证号,无法充值!", true);
		//  return true;
		//}

		//if (ZZJCore.Public_Var.cardInfo.CardType == "2" && ZZJ_Module.Public_Proc.ReadZZJConfig("SBK_YLKCZ") == "0")
		//{
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox("医保卡银联充值未开启", "医保卡银联充值功能当前禁止使用", true);
		//  ZZJCore.BackForm.CloseForm();
		//  return true;
		//}
		#endregion

		#region 输入金额
	InputAmount:
	try{
		 iRet = FormStyle.YLCZStyle.InputAmount(out DC);
		 }
  catch(Exception ex){
		ZZJCore.BackForm.CloseForm();
		return true;
	}

		if (iRet != 0 || DC == 0)
		{
			ZZJCore.BackForm.CloseForm();
			return true;
		}

		iRet = FormStyle.YLCZStyle.Alert(DC);
		if (iRet == -1) goto InputAmount;
		if (iRet != 0)
		{
			ZZJCore.BackForm.CloseForm();
			return true;
		}

		#endregion

		//DC /= 100;

		ZZJCore.SuanFa.Proc.MsgSend(0xB6, 0);//申请使用读卡器
		Application.DoEvents();
		ZZJ_Module.UnionPay.Mode = 1;//收费模式: 1:充值预交金  0:挂号缴费
		ZZJ_Module.UnionPay.DC = DC;//要收的金额
		iRet = ZZJ_Module.UnionPay.Payment();
		ZZJCore.SuanFa.Proc.MsgSend(0xB7, 0);//归还读卡器
		if (iRet != 0)
		{
			ZZJCore.BackForm.CloseForm();
			return iRet != -5;
		}

		#region 充值
		ZZJCore.BackForm.SetFormText("正在充值,请稍候...");
		Application.DoEvents();
		int pub_cz=Pub_CZ(DC,0);

		return true;
	}
	//cz_flag，0是自身调用，1是其他地方调用（用于判断省去弹框提示）
	public static int Pub_CZ(decimal DCJE,int cz_flag=0)
	{
		
		string YCMS = "";
		DC = DCJE;
		OldAmount = decimal.Parse(ZZJCore.Public_Var.patientInfo.DepositAmount);
		iRet = XMLCore.CZ(DC, out YCMS, out InvokeRet, 1, "银联", "消费");

		//刷新余额方式1
		//NewAmount = iRet == 0 ? OldAmount + DC : OldAmount;
		//ZZJCore.Public_Var.patientInfo.DepositAmount=NewAmount.ToString();
		
		string Msg="";
		XMLCore.GetUserInfo(out Msg);
		NewAmount = decimal.Parse(ZZJCore.Public_Var.patientInfo.DepositAmount);

		DB.YCMS = YCMS;
		#endregion

		#region 数据库记录

		DB.CGBZ = iRet == 0 ? 0 : -1;//成功标志

		//QDYLLib.CardTransfer.UnPackData(File.ReadAllText(@"E:\S2\ZZJ_Module\ZZJ_ModuleTest\bin\Release\银联报文\0-20160317-093244-0.TXT"), out ZZJ_Module.UnionPay.RETDATA);

		Thread T = new Thread(SaveData);
		T.IsBackground = true;
		T.Start();
		#endregion

		#region 打印凭条并显示结果
		{
			List<ZZJCore.ZZJStruct.DataLine> AL = new List<ZZJCore.ZZJStruct.DataLine>();
			AL.Add(new ZZJCore.ZZJStruct.DataLine("金额", DC.ToString("C"), null, new Font(new FontFamily("黑体"), 20, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("充值结果", (iRet == 0) ? "【成功】" : "【失败】", null, new Font(new FontFamily("黑体"), 20, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("账户余额", NewAmount.ToString("C"), null, null));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("银联卡号", ZZJ_Module.UnionPay.UnionPayCardNo, null, null));
			//decimal.Parse(ZZJCore.Public_Var.patientInfo.DepositAmount);

			if (iRet != 0)
			{
				ZZJCore.SuanFa.PrintCall.PrintPT("银联充值退费凭证", AL.ToArray(), "如果充值失败请持本凭证至窗口!", false);
				if (cz_flag.ToString()=="0")
					{
			   		ZZJCore.BackForm.ShowForm("充值失败!" + YCMS, true);
					}
				return -1;
			}
			else
			{
				ZZJCore.SuanFa.PrintCall.PrintPT("自助机银联充值凭证", AL.ToArray(), "如果充值失败请持本凭证至窗口!", false);
					if (cz_flag.ToString()=="0")
					{
		     		ZZJCore.BackForm.ShowForm("充值成功!", true);
			   	}
				return 0;
			}
		}
		#endregion
	}

	#region 数据库操作
	private static void Init_DBConfig()//加载数据库
	{
		try
		{
			if (MSSQL_Init.Init_DBConfig() != 0)
			{
				ZZJCore.SuanFa.Proc.Log("加载数据库配置文件失败");//加载配置文件失败
				return;
			}
			ZZJCore.SuanFa.Proc.Log("加载配置文件成功");//加载配置文件成功
			if (MSSQL_Operate.Test_MSSQL_Conn() != 0)
			{//数据库测试连接失败
				ZZJCore.SuanFa.Proc.Log("数据库测试连接失败");
				return;
			}
			ZZJCore.SuanFa.Proc.Log("数据库测试连接成功");//数据库测试连接成功
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
		}
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
			//发卡描述
			AL.Add(AddSP(SqlDbType.VarChar, "@LSH", ZZJCore.Public_Var.SerialNumber));//流水号
			AL.Add(AddSP(SqlDbType.VarChar, "@KH", ZZJCore.Public_Var.cardInfo.CardNo));//卡号
			AL.Add(AddSP(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.patientInfo.PatientID));//登记号
			if (string.IsNullOrEmpty(ZZJCore.Public_Var.cardInfo.CardType)) ZZJCore.Public_Var.cardInfo.CardType = "0";
			AL.Add(AddSP(SqlDbType.Int, "@KPLX", int.Parse(ZZJCore.Public_Var.cardInfo.CardType)));//卡类型
			AL.Add(AddSP(SqlDbType.VarChar, "@XM", ZZJCore.Public_Var.patientInfo.PatientName));//姓名
			AL.Add(AddSP(SqlDbType.VarChar, "@BRLX", "自费"));//病人类型
			AL.Add(AddSP(SqlDbType.VarChar, "@YHKH", ZZJ_Module.UnionPay.UnionPayCardNo));//银行卡号
			AL.Add(AddSP(SqlDbType.Decimal, "@YCQJE", OldAmount));//充值前金额
			AL.Add(AddSP(SqlDbType.Decimal, "@POSJE", DC));//POS金额
			AL.Add(AddSP(SqlDbType.DateTime, "@RQSJ", DateTime.Now));//交易日期
			AL.Add(AddSP(SqlDbType.VarChar, "@JYSJ", ZZJ_Module.UnionPay.RETDATA.strTransDate + " " + ZZJ_Module.UnionPay.RETDATA.strTransTime));//交易时间
			AL.Add(AddSP(SqlDbType.VarChar, "@SQM", ZZJ_Module.UnionPay.RETDATA.strAuth));//授权码
			AL.Add(AddSP(SqlDbType.VarChar, "@CKH", ZZJ_Module.UnionPay.RETDATA.strRef));//参考号
			AL.Add(AddSP(SqlDbType.VarChar, "@POSLSH", ZZJ_Module.UnionPay.RETDATA.strTrace));//POS流水号
			AL.Add(AddSP(SqlDbType.VarChar, "@POSZDH", ZZJ_Module.UnionPay.RETDATA.strTId));//POS终端号
			AL.Add(AddSP(SqlDbType.VarChar, "@YHJYJG", "0"));//银行操作标识 0成功 其他失败
			AL.Add(AddSP(SqlDbType.VarChar, "@YHJYJGMS", ZZJ_Module.UnionPay.RETDATA.strRespInfo));//银行交易描述

			int BZ = DB.CGBZ;
			try
			{
				if (ZZJCore.Public_Var.cardInfo.CardType == "1") BZ = new int[] { 0, 1, -1 }[InvokeRet];
			}
			catch (Exception E)
			{
				ZZJCore.SuanFa.Proc.Log(E);
			}
			AL.Add(AddSP(SqlDbType.Int, "@YCJG", BZ));//预存结果
			AL.Add(AddSP(SqlDbType.VarChar, "@YCJGMS", DB.YCMS));//HIS充值描述
			AL.Add(AddSP(SqlDbType.Decimal, "@YCHJE", NewAmount));//预存后金额
			AL.Add(AddSP(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));//自助机编号
			AL.Add(AddSP(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//HIS操作员
			AL.Add(AddSP(SqlDbType.VarChar, "@SendYHBW", ""));//银行发送报文
			AL.Add(AddSP(SqlDbType.VarChar, "@ReceiveYHBW", ""));//银行返回报文
			AL.Add(AddSP(SqlDbType.VarChar, "@BL1", "门诊预交金充值"));
			AL.Add(AddSP(SqlDbType.VarChar, "@BL2", ZZJ_Module.Public_Var.sFlowId));
			AL.Add(AddSP(SqlDbType.VarChar, "@BL3", ZZJ_Module.Public_Var.orderNo));
			parameter = AL.ToArray();
			if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_MZ_YHKYC_MX_ForZZJ_V5]", parameter, out msg) > 0)
			{
				ZZJCore.SuanFa.Proc.Log("记录写入服务器成功");
			}
			else
			{
				ZZJCore.SuanFa.Proc.Log("记录写入服务器失败" + msg);
			}
		}
		catch (Exception Ex)
		{
			ZZJCore.SuanFa.Proc.Log(Ex);
			ZZJCore.SuanFa.Proc.Log("记录写入服务器失败" + Ex.Message);
		}
		#endregion

		#region 本地数据库
		int result = 0;
		msg = "";
		try
		{
			Mw_Access.Access_DB AD = new Mw_Access.Access_DB(ZZJCore.Public_Var.LocalDataPath);
			string sql = string.Format(@"insert into Tb_His_MZ_YHKYC_MX(YHKYCGuid,LSH,DJH,KH,KPLX,XM,JE,SJ,SFY,YHKH,FKH,POSLSH,POSCKH,CGBZ,ZZJBH,HisCZY,YWLX,BL1)
												values	('{0}','{1}','{2}','{3}',{4},'{5}',{6},'{7}','{8}','{9}','','{10}','{11}',{12},'{13}','{14}','门诊帐户充值','{15}')",
						Guid.NewGuid().ToString(),
						ZZJCore.Public_Var.SerialNumber, //流水号
						ZZJCore.Public_Var.patientInfo.PatientID,//登记号
						ZZJCore.Public_Var.cardInfo.CardNo,//卡号
						ZZJCore.Public_Var.cardInfo.CardType,//卡类型
						ZZJCore.Public_Var.patientInfo.PatientName,//姓名
						DC,//金额
						DateTime.Now.ToString(),//日期时间
						ZZJCore.Public_Var.ZZJ_Config.ExtUserID,//操作员
						ZZJ_Module.UnionPay.UnionPayCardNo,//银联卡号
						ZZJ_Module.UnionPay.RETDATA.strTrace,
						ZZJ_Module.UnionPay.RETDATA.strRef,
						DB.CGBZ,//成功标志
						ZZJCore.Public_Var.ZZJ_Config.TerminalID,//自助机编号
						ZZJCore.Public_Var.ZZJ_Config.ExtUserID,//HIS操作员
						DB.YCMS); //保留1，（充值结果）

			result = AD.ExecuteSQLNonquery(sql, out msg);
			AD.Close();
			//result = ZZJCore.Public_AccessHelper.Access_ExecuteNonQuery(DB, out msg);
		}
		catch (Exception ex)
		{
			ZZJCore.SuanFa.Proc.Log(ex);
		}
		ZZJCore.SuanFa.Proc.Log(result > 0 ? "预存记录写入本机成功" : ("预存记录写入本机失败" + msg));
		#endregion
	}
	#endregion
}//End Class