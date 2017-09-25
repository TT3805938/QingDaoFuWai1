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
using System.Text;
using System.Threading.Tasks;

public static class BK
{
	static bool ZBCardOK = false;//准备卡OK标志位
	static bool BKBZ = false;

	public static Image[] GNList = null;

	public static bool BKInitial()
	{
		#region 加载资源
		GNList = new Image[] {	Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/挂号1.png"), 
									Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/取号.png"), 
									Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/现金充值.png"), 
									Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/银联卡充值.png") };
		return true;
		#endregion
	}

	public static bool BKMain(int ModuleID)
	{
		bool bRet = false;
		#region 初始化
		if (ModuleID == 0) ZZJCore.Public_Var.ModuleName = "自助办卡";
		if (ModuleID == 1) ZZJCore.Public_Var.ModuleName = "儿童无证办卡";
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.Public_Var.patientInfo.PatientName = "";
		ZZJCore.Public_Var.cardInfo.CardNo = "";
		ZZJCore.Public_Var.patientInfo.DepositAmount = "";
		ZZJCore.Public_Var.cardInfo.CardType = "0";
		ZBCardOK=false;
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.SuanFa.Proc.Log("打开办卡");
		ZZJCore.Initial.Read();
		#endregion

		#region 准备就诊卡
		if (ZZJCore.SuanFa.Proc.ReadPublicINI("VirtualCardNO") == "1")
		{
			ZZJCore.Public_Var.cardInfo.CardNo = DateTime.Now.ToString("dhhmmssfff");
		}
		else
		{
			//ZBCard();
			//if (!ZBCardOK)
			//{
			//  ZZJCore.SuanFa.Proc.Log("准备就诊卡失败退出");
			//  ZZJCore.BackForm.ShowForm("准备就诊卡失败,请联系管理员.", true);
			//  Application.DoEvents();
			//  return false;
			//}
		}
		#endregion

		#region 获取患者信息
		string Msg = "";
		if (ModuleID == 0) bRet = ZZBKMain(out Msg);
		if (ModuleID == 1) bRet = QYKBK.ETBKMain(true);
		if (!bRet)
		{
			if (!string.IsNullOrEmpty(Msg)) ZZJCore.BackForm.ShowForm("系统错误,请联系管理员!", true);
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		#endregion
		//准备就诊卡
		ZBCard();
		if (!ZBCardOK)
		{
			ZZJCore.SuanFa.Proc.Log("准备就诊卡失败退出");
			ZZJCore.BackForm.ShowForm("准备就诊卡失败,请联系管理员.", true);
			Application.DoEvents();
			return false;
		}
		#region 调用接口
		if (ModuleID != 1 && ModuleID != 0)
		{
			ZZJCore.BackForm.ShowForm("系统错误,请联系管理员!", true);
			ZZJCore.BackForm.CloseForm();
			return false;
		}


		#region 判断是否能发卡
		string flag = "";
		int isFKFlag=-1;
		isFKFlag = isFK();
		if(isFKFlag==0)
		{
			BKBZ = XMLCore.BK(ModuleID, out FKMS, out Msg) == 0;
		}
		else
		{
		  ZZJCore.BackForm.ShowForm("您一小时内已经办过卡了，再次办卡需等一小时!", true);
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		#endregion

		
		#region 办卡记录写入数据库
		try
		{
			SavelocalAndServerData();
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("办卡完成,写入数据库失败!" + e.Message);
		}
		#endregion
		if (!BKBZ)
		{
			ZZJCore.BackForm.ShowForm(Msg, true);
			return true;
		}
		#endregion

		ZZJCore.SuanFa.Proc.Log("办卡完成,调用其它功能!");
		try
		{
			CallOtherModule();
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("调用附加功能异常!" + e.Message);
		}
		ZZJCore.SuanFa.Proc.Log("调用附加功能返回!");

		#region 打印卡片
		if (ZZJCore.SuanFa.Proc.ReadPublicINI("BKDoNotPrintCard") == "1") goto OutCardOK;

		
		try
		{
			if (ZZJCore.SuanFa.PrintCall.Print(ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev, PC, 500, 318, false) != 0)
			{
				ZKJ_Dev.Out_Card(ref Msg);
				ZZJCore.SuanFa.Proc.Log("XIN打印卡片失败,已尝试强行出卡");
			}
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log("!!!打印卡片出现异常!证卡机:" + ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev + "异常提示:" + e.Message);
			ZKJ_Dev.Out_Card(ref Msg);
		}
	// 

		//ZZJCore.SuanFa.Proc.RunEXE(
		//  ZZJCore.Public_Var.ModulePath + "ZZJCoreTest.exe",
		//  string.Format("PrintCard {0} {1} {2}",
		//    @"""" + ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev + @"""",
		//    @"""" + ZZJCore.Public_Var.patientInfo.PatientName + @"""",
		//    ZZJCore.Public_Var.cardInfo.CardNo
		//  )
		//);
	//*/

	OutCardOK:
		ZZJCore.SuanFa.Proc.MsgSend(0x21, 0x00);//办卡成功
		if (ModuleID == 0) ZZJCore.Public_Var.ModuleName = "自助办卡";
		if (ModuleID == 1) ZZJCore.Public_Var.ModuleName = "儿童无证办卡";
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.BackForm.ShowForm("办卡成功!请收好您的就诊卡!", true);
		ZZJCore.SuanFa.Proc.Log("办卡完成,关闭办卡模块!");
		#endregion
		ZZJCore.BackForm.CloseForm();
		return true;
	}

	public static void CallOtherModule()
	{
		if (ZZJ_Module.Public_Proc.ReadZZJConfig("BKNextFunction") != "1") return;

		#region 选择功能
		int GN = 0;
		{

			List<Image> IL = new List<Image>();
			int[] GNIDs = new int[4];//给功能分配IP
			int GNi = 0;
			if (ZZJ_Module.Public_Proc.TestModule("DRGH"))
			{
				GNIDs[GNi] = 0;
				IL.Add(GNList[0]);
				GNi++;
			}
			if (ZZJ_Module.Public_Proc.TestModule("QH"))
			{
				GNIDs[GNi] = 1;
				IL.Add(GNList[1]);
				GNi++;
			}
			if (ZZJ_Module.Public_Proc.TestModule("XJCZ"))
			{
				GNIDs[GNi] = 2;
				IL.Add(GNList[2]);
				GNi++;
			}
			if (ZZJ_Module.Public_Proc.TestModule("YLKCX"))
			{
				GNIDs[GNi] = 3;
				IL.Add(GNList[3]);
				GNi++;
			}

			GN = FormStyle.BKStyle.SelectProc(IL.ToArray());
			ZZJCore.SuanFa.Proc.Log("选择功能完成,转换前的功能号:" + GN.ToString());
			if (GN < 0) return;
			try
			{
				GN = GNIDs[GN];
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.Log("转换功能号异常!" + e.Message);
				return;
			}
			ZZJCore.SuanFa.Proc.Log("转换功能号完成,转换后的功能号:" + GN.ToString());
			//返回值: -1=返回按钮 -2=关闭按钮 -3=超时 0=挂号 1=取号 2=现金充值 3=银联充值
			Application.DoEvents();
		}
		#endregion

		#region 调用其它模块

		try
		{
			ZZJCore.SuanFa.Proc.Log("办卡成功!" + ZZJCore.Public_Var.cardInfo.CardNo + " " + GN.ToString());
			ZZJ_Module.Public_Var.RCR = new ZZJSubModuleInterface.ReadCardResultInfoCls();
			ZZJ_Module.Public_Var.RCR.KPLX = 0;
			ZZJ_Module.Public_Var.RCR.KH = ZZJCore.Public_Var.cardInfo.CardNo;
		}
		catch (Exception ex)
		{
			ZZJCore.SuanFa.Proc.Log(ex);
			ZZJCore.SuanFa.Proc.Log("写卡号失败!" + ex.Message);
		}
		#region 当日挂号
		if (GN == 0)
		{
			try
			{
				ZZJSubModuleInterface.SubModuleLib M = new DRGH.DRGHMEF();
				M.InitSubModule("DRGH", null);
				string msg = "";
				M.StartSubModule("DRGH", new object[] { ZZJ_Module.Public_Var.RCR }, out msg);
			}
			catch (Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log("打开当日挂号失败!" + ex.Message);
			}
		}
		#endregion
		#region 预约取号
		if (GN == 1)
		{
			try
			{
				YYQH.QHMain();
			}
			catch (Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log("打开预约取号失败!" + ex.Message);
			}
		}
		#endregion
		#region 现金充值
		if (GN == 2)
		{
			try
			{
				XJCZ.XJCZMain();
				ZZJCore.SuanFa.Proc.Log("打开现金充值成功!");
			}
			catch (Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log("打开现金充值失败!" + ex.Message);
			}
		}
		#endregion
		#region 银联充值
		if (GN == 3)
		{
			try
			{
				YLCZ.YLCZMain();
			}
			catch (Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log("打开银联充值失败!" + ex.Message);
			}
		}
		#endregion
		#endregion
	}

	public static bool ZZBKMain(out string Msg)
	{
		Msg = "";
	/*
	ZZJCore.Public_Var.patientInfo.IDNo = "370283790911703";
	ZZJCore.Public_Var.patientInfo.PatientName = "XX";
	ZZJCore.Public_Var.patientInfo.Sex="男";
	ZZJCore.Public_Var.patientInfo.Address="0";
	ZZJCore.Public_Var.patientInfo.Age=32;
	ZZJCore.Public_Var.patientInfo.DOB="2012-01-02";
	goto DebugFlag1;
	// */
		#region 获取用户信息
	GetInfo:
		//int InfoSource = FormStyle.BKStyle.SelectBKMode();
		//if (InfoSource < 0) return false;
		//if (InfoSource == 0)
		//{
			#region 获取身份证信息
			int GetSFZiRet = FormStyle.BKStyle.GetIDNo002();
			//if (GetSFZiRet == -1) goto GetInfo;
			if (GetSFZiRet != 0) return false;
			Application.DoEvents();
			#endregion
		//}
        //if (InfoSource == 1)
        //{
        //    #region 获取医保卡信息
        //    ZZJCore.SuanFa.Proc.MsgSend(0xB6, 0);//申请使用读卡器
        //    int CardType = 0;
        //    int iRet = ZZJ_Module.YBPay.WaitCard(out CardType, QYKBK.SBK, "请插入一代或二代社保卡");

        //    iRet = QYKBK.ReadCard(CardType, iRet);
        //    DKQDev.ICEnableExtIO(false);
        //    DKQDev.MoveCard(1);
        //    DKQDev.CloseDevice();
        //    ZZJCore.SuanFa.Proc.MsgSend(0xB7, 0);//归还读卡器
        //    //if (iRet < 0) return false;
        //    if (iRet != 0) return false;

        //    ZZJCore.Public_Var.patientInfo.IDNo = QDYBCard.SFZH;
        //    ZZJCore.Public_Var.patientInfo.PatientName = QDYBCard.HZName;
        //    ZZJCore.Public_Var.patientInfo.Sex = QDYBCard.Sex;
        //    ZZJCore.Public_Var.patientInfo.DOB = ZZJCore.SuanFa.Proc.IDNoGetDOB(QDYBCard.SFZH).ToString("yyyy-MM-dd");
        //    #endregion
        //}
	DebugFlag1:
		{//调用接口检测是否可以办卡
			int iRet = XMLCore.QIDCard(out Msg);
			if (iRet != 0) return false;
		}
	InputPhone:
		#region 获取电话号码
		{
			int iRet = FormStyle.BKStyle.GetPhone002();
			Application.DoEvents();
			if (iRet == -1) goto GetInfo;
			if (iRet != 0) return false;
		}
		#endregion

		ZZJCore.Public_Var.patientInfo.ParentName = "";
		ZZJCore.Public_Var.patientInfo.ParentRelationship = "";
		#endregion
		return true;
	}

	//判断是否允许发卡
	public static int isFK(){
	int iRet=-1;
	string InXML="";
	XmlDocument XD = new XmlDocument();
	try
	{
		 InXML = ZZJCore.SuanFa.POSTInvoke.Invoke("http://10.17.133.1:8080/QueryBK.php?IDNo="+ZZJCore.Public_Var.patientInfo.IDNo+"", "");
		XD.LoadXml(InXML);
		iRet = int.Parse(XD.SelectSingleNode("Root/Code").InnerText.Trim());
	}
	catch (Exception e)
	{
		ZZJCore.SuanFa.Proc.Log(e);
		return -1;
	}
	return iRet;
	}


	private static void PC(object sender, PrintPageEventArgs e)
	{
		try
		{//ZZJCore.Public_Var.patientInfo.PatientName
			e.Graphics.DrawString(ZZJCore.Public_Var.patientInfo.PatientName, new Font(new FontFamily("黑体"), 15, FontStyle.Bold), System.Drawing.Brushes.Black, 55, 33);
			ZZJCore.SuanFa.Proc.Log("打印姓名" + ZZJCore.Public_Var.patientInfo.PatientName);
		}
		catch (Exception ex)
		{
			ZZJCore.SuanFa.Proc.Log(ex);
			ZZJCore.SuanFa.Proc.Log("打印姓名异常" + ex.Message);
		}
		//try
		//{
		//  e.Graphics.DrawString(ZZJCore.Public_Var.cardInfo.CardNo, new Font(new FontFamily("黑体"), 12, FontStyle.Regular), System.Drawing.Brushes.Black, 120, 50);
		//  ZZJCore.SuanFa.Proc.Log("打印卡号" + ZZJCore.Public_Var.cardInfo.CardNo);
		//}
		//catch (Exception ex)
		//{
		//  ZZJCore.SuanFa.Proc.Log(ex);
		//  ZZJCore.SuanFa.Proc.Log("打印卡号异常" + ex.Message);
		//}
	}

	private static void ZBCard()//准备就诊卡
	{
		try{
	  List<Task> listT=new List<Task>();
		Task tk=Task.Factory.StartNew(()=>
		{

			//if (ZZJ_Module.EasyHiTi.ReadCard() == 0)
			//{
			//  ZBCardOK = true;
			//  return;
			//}


			//if (ZZJ_Module.EasyHiTi.ReadCard() == 0)//GetHiTiCard
			//{
			//  ZBCardOK = true;
			//  return;
			//}
			////ZZJCore.SuanFa.Proc.sleep(300);
			//ZBCardOK = ZZJ_Module.EasyHiTi.ReadCard() == 0;
			//return;
			ZBCardOK = CardRW.FKQReadCard();
			if (!ZBCardOK) ZBCardOK = CardRW.FKQReadCard();
		});
		listT.Add(tk);
		Task.WaitAll(listT.ToArray(),30000);
		}
		catch(Exception ex)
		{
			ZZJCore.SuanFa.Proc.Log("读卡过程中产生了异常:"+ ex.ToString());
			return;
		}
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

	private static string FKMS = "";
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
		catch (Exception e)
		{ ZZJCore.SuanFa.Proc.Log(e); return; }
		try
		{
			string msg = "";
			MSSQL_Operate mssql_operate = new MSSQL_Operate();
			List<SqlParameter> AL = new List<SqlParameter>();
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

			if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_FK_MX_ForZZJ_V3]", AL.ToArray(), out msg) > 0)
			{
				ZZJCore.SuanFa.Proc.Log("记录写入服务器成功");
				return;
			}
			ZZJCore.SuanFa.Proc.Log("记录写入服务器失败" + msg);
		}
		catch (Exception Ex)
		{
			ZZJCore.SuanFa.Proc.Log(Ex);
			ZZJCore.SuanFa.Proc.Log("记录写入服务器异常，" + Ex.Message);
		}// */
	}
	#endregion
	#endregion
}//End Class