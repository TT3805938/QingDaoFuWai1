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

public static class QYKBK
{
	static bool ZBCardOK = false;//准备卡OK标志位
	static bool BKBZ = false;

	public static string FKMS = "";

	public static int GetBK(int Mode, string CardType)
	{
		ZZJ_Module.UserData.Cardlist CL = null;
		if (ZZJ_Module.UserData.QueryUserInfo(Mode, CardType, out CL) != 0)
		{
			ZZJCore.SuanFa.Proc.Log("查询办卡记录失败!" + ZZJ_Module.UserData.Msg);
			ZZJCore.BackForm.ShowForm("查询办卡记录失败!", true);
			ZZJCore.BackForm.CloseForm();
			return -1;
		}
		if (CL.clist == null) return 0;
		if (CL.clist.Length == 0) return 0;

		foreach (ZZJ_Module.UserData.Carditem Ci in CL.clist)
		{
			if (Ci.cardStatus == "1" || Ci.cardStatus == "-1") return 1;
		}
		return 0;
	}

	public static int ChackReCreateCard(int Mode)
	{
		if (ZZJCore.SuanFa.Proc.ReadPublicINI("LocalInfo", "0") == "1") return 0;
		decimal CardCash = 0;
		ZZJ_Module.UserData.Cardlist CL = null;
		if (ZZJ_Module.UserData.QueryUserInfo(Mode, "2", out CL) != 0)
		{
			ZZJCore.SuanFa.Proc.Log("查询办卡记录失败!" + ZZJ_Module.UserData.Msg);
			ZZJCore.BackForm.ShowForm("查询办卡记录失败!", true);
			ZZJCore.BackForm.CloseForm();
			return -2;
		}
		if (CL.clist == null) return 0;
		if (CL.clist.Length == 0) return 0;

		#region 枚举找可以补的卡
		ZZJ_Module.UserData.Carditem CI = null;
		foreach (ZZJ_Module.UserData.Carditem Ci in CL.clist)
		{
			if (Ci.cardStatus == "1" || Ci.cardStatus == "-1")
			{
				CI = Ci;
				break;
			}
		}
		if (CI == null) return 0;
		#endregion

		if (ZZJ_Module.Public_Proc.ReadZZJConfig("BKBZ") != "1")
		{
			ZZJCore.SuanFa.Proc.ZZJMessageBox("进入补卡模式", "您已经办过卡,请到人工窗口补卡", false);
			return -2;
		}

		int SFM = FormStyle.BKStyle.BKYesNo();//询问用户是否补卡
		if (SFM != 0)
		{
			ZZJCore.BackForm.CloseForm();
			return -2;
		}

		ZZJ_Module.Public_Var.PF.patientId = CL.patientId;
		ZZJ_Module.Public_Var.PF.guardianNo = ZZJCore.Public_Var.patientInfo.IDNo;

		/*
		#region 收补卡费
		if (CL.accBalance < CardCash)
		{
			ZZJ_Module.ChargeClass.ChargeParameter CP = new ZZJ_Module.ChargeClass.ChargeParameter();
			CP.Caption = string.Format("您需要交{0}元补卡费,请选择交费方式", CardCash.ToString("C"));
			CP.YBCard = false;
			CP.BankCard = true;
			CP.Cash = false;
			CP.DC = CardCash;
			if (ZZJ_Module.ChargeClass.Charge(CP) != 0) return -2;
		}
		#endregion
		// */

		if (ZZJ_Module.UserData.ReCreateCard("OC", CI) != 0)
		{
			ZZJCore.SuanFa.Proc.Log(ZZJ_Module.UserData.Msg);
			ZZJCore.BackForm.ShowForm("补卡失败!" + ZZJ_Module.UserData.Msg, true);
			return -2;
		}
		string Msg = "";

		#region 签约
		try
		{

			if (XMLCore.QY(out Msg) != 0) ZZJCore.SuanFa.Proc.Log("签约失败:" + Msg);
			ZZJCore.BackForm.CloseForm();
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("*签约时异常:" + e.Message);
		}
		#endregion

		//if (CL.accBalance < CardCash) XMLCore.CZ(CardCash, out Msg, 1, "银联", "消费");
		//ZZJ_Module.UserData.yjjxf(CardCash);

		return 1;
	}

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
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.SuanFa.Proc.Log("打开办卡");
		ZZJCore.Initial.Read();
		#endregion

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

		#region 获取患者信息
		string Msg = "";
		bool bRet = false;
		if (ModuleID == 0) bRet = ZZBKMain(out Msg);
		if (ModuleID == 1) bRet = ETBKMain();
		if (!bRet)
		{
			if (!string.IsNullOrEmpty(Msg)) ZZJCore.BackForm.ShowForm("系统错误,请联系管理员!", true);
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		#endregion

		#region 调用接口
		if (ModuleID != 1 && ModuleID != 0)
		{//首先检查一下.
			ZZJCore.BackForm.ShowForm("系统错误,请联系管理员!", true);
			ZZJCore.BackForm.CloseForm();
			return false;
		}

		//BKBZ = XMLCore.BK(ModuleID, out FKMS, out Msg) == 0;
		BKBZ = ZZJ_Module.PingTaiCreateCard.CreateCard(ModuleID) == 0;

		if (!BKBZ)
		{
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
			ZZJCore.BackForm.ShowForm("办卡失败!" + ZZJ_Module.RegionalPlatform.Err, true);
			return true;
		}
		#endregion

		#region HIS签约
		try
		{
			if (XMLCore.QY(out Msg) != 0) ZZJCore.SuanFa.Proc.Log("签约失败:" + Msg);
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("*签约时异常:" + e.Message);
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
		return BKEnd(ModuleID);
	}

	public static bool BKEnd(int ModuleID)
	{
		ZZJ_Module.PingTaiCreateCard.WriterCard();

		ZZJCore.SuanFa.Proc.Log("办卡完成,调用其它功能!");
		try
		{
			CallOtherModule();
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("*调用附加功能异常!" + e.Message);
		}
		ZZJCore.SuanFa.Proc.Log("调用附加功能返回!");
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");

		PrintCard();//打印卡片

		if (ModuleID == 0) ZZJCore.Public_Var.ModuleName = "自助办卡";
		if (ModuleID == 1) ZZJCore.Public_Var.ModuleName = "儿童无证办卡";
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.BackForm.ShowForm("请收好您的就诊卡!", true);
		ZZJCore.SuanFa.Proc.Log("办卡完成,关闭办卡模块!");

		ZZJCore.BackForm.CloseForm();
		return true;
	}

	public static void CallOtherModule()
	{
		if (ZZJ_Module.Public_Proc.ReadZZJConfig("BKNextFunction") != "1") return;

		#region 选择功能
		int GN = 0;
		{
			ZZJCore.SelectForm2.SelectFormParameter SFP = new SelectForm2.SelectFormParameter();
			List<Image> IL = new List<Image>();
			int[] GNIDs = new int[4];//给功能分配IP
			int GNi = 0;
			if (ZZJ_Module.Public_Proc.TestModule("DRGH"))
			{
				GNIDs[GNi] = 0;
				IL.Add(BK.GNList[0]);
				GNi++;
			}
			if (ZZJ_Module.Public_Proc.TestModule("QH"))
			{
				GNIDs[GNi] = 1;
				IL.Add(BK.GNList[1]);
				GNi++;
			}
			if (ZZJ_Module.Public_Proc.TestModule("XJCZ"))
			{
				GNIDs[GNi] = 2;
				IL.Add(BK.GNList[2]);
				GNi++;
			}
			if (ZZJ_Module.Public_Proc.TestModule("YLKCX"))
			{
				GNIDs[GNi] = 3;
				IL.Add(BK.GNList[3]);
				GNi++;
			}

			SFP.Images = IL.ToArray();
			SFP.Caption = "办卡成功,您还可以选择以下功能:";
			SFP.ContentAlignment = ContentAlignment.MiddleCenter;
			SFP.VDistance = 10;//垂直间距
			SFP.HDistance = 10;//水平间距
			ZZJCore.ZZJStruct.ZZJButton zzjbutt = ZZJControl.NewButton_Close();
			zzjbutt.Text = "出卡";
			SFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { zzjbutt };
			SFP.DJSTime = 15;//倒计时时间
			GN = ZZJCore.SelectForm2.ShowForm(SFP);
			Application.DoEvents();
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
			ZZJ_Module.Public_Var.RCR.KPLX = 1;
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

	public static int ReadCard(int CardType, int WaitRet)
	{
		if (WaitRet != 0) return -1;
		if (CardType == 0x20)
		{
			QDYBCard.SFXX S = new QDYBCard.SFXX();
			S.SFZ = new StringBuilder(19);
			S.XM = new StringBuilder(31);
			DKQDev.MoveCard(4);
			DKQDev.ICEnableExtIO(true);
			ZZJCore.SuanFa.Proc.sleep(200);
			int iRet = QDYBCard.ReadCard(ref S);
			if (iRet != 0)
			{
				ZZJCore.BackForm.ShowForm("读卡失败,请稍后再试！", true);
				return -1;
			}

			QDYBCard.Mode = 1;
			if (QDYBCard.YBKHGetHZInfo("") != 0)
			{
				QDYBCard.Mode = 0;
				ZZJCore.BackForm.ShowForm("社保卡认证失败！", true);
				return -1;
			}
			QDYBCard.Mode = 0;
			return 0;
		}

		string[] Buff = new string[] { "", "", "" };
		int DKQiRet = DKQDev.ReadTracksACSII(4, ref Buff);
		if (DKQiRet != 0)
		{
			ZZJCore.BackForm.ShowForm("读卡失败,请稍后再试！", true);
			return -1;
		}

		//Buff[1] = "0029167915191112";

		QDYBCard.Mode = 1;
		if (QDYBCard.YBKHGetHZInfo(Buff[1]) != 0)
		{
			QDYBCard.Mode = 0;
			ZZJCore.BackForm.ShowForm("社保卡认证失败！", true);
			return -1;
		}
		QDYBCard.Mode = 0;
		return 0;
	}

	public static Image SBK = Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/SBK.gif");
	public static bool ZZBKMain(out string Msg)
	{
		Msg = "";
		#region 获取用户信息
	GetInfo:
		int InfoSource = FormStyle.BKStyle.SelectBKMode();
		if (InfoSource < 0) return false;
		if (InfoSource == 0)
		{
			#region 获取身份证信息
			int GetSFZiRet = FormStyle.BKStyle.GetIDNo002();
			if (GetSFZiRet == -1) goto GetInfo;
			if (GetSFZiRet != 0) return false;
			Application.DoEvents();
			#endregion
		}
		if (InfoSource == 1)
		{
			#region 获取医保卡信息
			ZZJCore.SuanFa.Proc.MsgSend(0xB6, 0);//申请使用读卡器
			int CardType = 0;
			int iRet = ZZJ_Module.YBPay.WaitCard(out CardType, SBK, "请插入一代或二代社保卡");

			iRet = ReadCard(CardType, iRet);
			DKQDev.ICEnableExtIO(false);
			DKQDev.MoveCard(1);
			DKQDev.CloseDevice();
			ZZJCore.SuanFa.Proc.MsgSend(0xB7, 0);//归还读卡器
			//if (iRet < 0) return false;
			if (iRet != 0) return false;

			ZZJCore.Public_Var.patientInfo.IDNo = QDYBCard.SFZH;
			ZZJCore.Public_Var.patientInfo.PatientName = QDYBCard.HZName;
			ZZJCore.Public_Var.patientInfo.Sex = QDYBCard.Sex;
			ZZJCore.Public_Var.patientInfo.DOB = ZZJCore.SuanFa.Proc.IDNoGetDOB(QDYBCard.SFZH).ToString("yyyy-MM-dd");
			#endregion
		}

		{//调用接口检测是否可以办卡
			int iRet = XMLCore.QIDCard(out Msg);
			if (iRet != 0) return false;

			#region 检测是否需要补卡
			iRet = ChackReCreateCard(0);
			//返回值: -2:业务失败关闭 0:不用补卡 1:补卡成功
			if (iRet == -2)
			{
				ZZJCore.BackForm.CloseForm();
				return false;
			}
			if (iRet == 1)
			{
				BKEnd(0);
				return false;
			}
			#endregion
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

		//InputPW: 
		#region 输入卡密码
		{
			int iRet = FormStyle.BKStyle.GetPassword002();
			if (iRet == -1) goto InputPhone;
			if (iRet < 0) return false;
			ZZJCore.Public_Var.patientInfo.Password = ZZJCore.SuanFa.Proc.MD5(ZZJCore.Public_Var.patientInfo.Password);
		}
		#endregion
		ZZJCore.Public_Var.patientInfo.ParentName = "";
		ZZJCore.Public_Var.patientInfo.ParentRelationship = "";
		#endregion
		return true;
	}

	public static bool ETBKMain(bool NotCheckBK = false)
	{
		int GX = 0, iRet = 0;
		DateTime DOB = DateTime.Now;
		#region 获取用户信息
	GetInfo://获取家属身份证号与姓名
		if (FormStyle.BKStyle.GetIDNo001() != 0) return false;
		ZZJCore.Public_Var.patientInfo.ParentName = ZZJCore.Public_Var.patientInfo.PatientName;

	InputName://获取孩子姓名
		//Talker.Speak("请输入小孩姓名");
		iRet = ZZJCore.ExChineseInput.ShowForm("请输入小孩姓名", ref ZZJCore.Public_Var.patientInfo.PatientName, true);
		//int iRet = ZZJCore.CNInputForm.ShowForm("请输入小孩姓名", ref ZZJCore.Public_Var.patientInfo.PatientName, true);
		Application.DoEvents();
		if (iRet == -1) goto GetInfo;
		if (iRet != 0) return false;

		if (NotCheckBK) goto InputPhone;
		#region 查询是否办过卡
		iRet = ChackReCreateCard(1);
		//返回值: -2:业务失败关闭 0:不用补卡 1:补卡成功
		if (iRet == -2)
		{
			ZZJCore.BackForm.CloseForm();
			return false;
		}
		if (iRet == 1)
		{
			BKEnd(0);
			return false;
		}
		#endregion

	InputPhone://获取电话号码
		iRet = FormStyle.BKStyle.GetPhone001();
		if (iRet == -1) goto InputName;
		if (iRet != 0) return false;

	SelectGX://选择关系
		iRet = FormStyle.BKStyle.GetGuanXi();
		if (iRet == -1) goto InputPhone;
		if (iRet < 0) return false;
		GX = (new int[] { 17, 18, 15, 16, 10 })[iRet];
		ZZJCore.Public_Var.patientInfo.ParentRelationship = GX.ToString();

	SelectXHSex://选择小孩性别
		iRet = FormStyle.BKStyle.SelectSex();
		if (iRet == -1) goto SelectGX;
		if (iRet < 0) return false;
		ZZJCore.Public_Var.patientInfo.Sex = iRet == 0 ? "男" : "女";

	SelectDT://输入小孩出生日期
		iRet = FormStyle.BKStyle.GetDOB001(ref DOB);
		if (iRet == -1) goto SelectXHSex;
		if (iRet < 0) return false;//如果是 关闭或者倒计时结束,就直接退出程序
		ZZJCore.Public_Var.patientInfo.DOB = DOB.ToString("yyyy-MM-dd");

		//InputPW://输入卡密码
		iRet = FormStyle.BKStyle.GetPassword001();
		if (iRet == -1) goto SelectDT;
		if (iRet < 0) return false;
		ZZJCore.Public_Var.patientInfo.Password = ZZJCore.SuanFa.Proc.MD5(ZZJCore.Public_Var.patientInfo.Password);
		#endregion

		return true;
	}

	public static int ChangeCard()
	{
		ZZJCore.SuanFa.Proc.Log("打开换卡");

		#region 准备就诊卡
		decimal DC = decimal.Parse(ZZJCore.Public_Var.patientInfo.DepositAmount);
		string OldCardNo = ZZJCore.Public_Var.cardInfo.CardNo;
		try
		{
			ZBCard();
		}
		catch (Exception EEE)
		{
			ZZJCore.SuanFa.Proc.Log(EEE);
			ZBCardOK = false;
		}
		string NewCardNo = ZZJCore.Public_Var.cardInfo.CardNo;

		if (!ZBCardOK)
		{
			ZZJCore.SuanFa.Proc.Log("准备就诊卡失败退出");
			ZZJCore.BackForm.ShowForm("准备就诊卡失败,请稍后再试.", true);
			Application.DoEvents();
			return -1;
		}

		//if (ZZJCore.Public_Var.debug) ZZJCore.ShowMessage.ShowForm(null, ZZJCore.Public_Var.cardInfo.CardNo, false);
		#endregion

		//ZZJCore.Public_Var.patientInfo.IDNo="";

		bool NeedPhone = false, NeedIDNo = false, NeedDob = false;
		NeedPhone = !PhoneOK(ZZJCore.Public_Var.patientInfo.Mobile);
		NeedIDNo = ZZJCore.SuanFa.Proc.strlen(ZZJCore.Public_Var.patientInfo.IDNo) != 18;
		NeedDob = string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.DOB);
		bool HardNeedIDNo = ZZJCore.SuanFa.Proc.ReadPublicINI("HardNeedIDNo", "0") == "1";
		if (HardNeedIDNo) NeedIDNo = true;

		ZZJCore.SuanFa.Proc.Log(string.Format("NeedPhone:{0} NeedIDNo:{1}", NeedPhone.ToString(), NeedIDNo.ToString()));

		int CreateCardMode = int.Parse(ZZJCore.SuanFa.Proc.ReadPublicINI("CreateCardMode", "0"));
		#region 选择身份证归属
		if (ZZJCore.SuanFa.Proc.ReadPublicINI("UserSelectCreateCardMode", "0") == "1")
		{
			CreateCardMode = FormStyle.BKStyle.SelectIDNo(NeedIDNo);
			if (CreateCardMode < 0) return -1;
			if (CreateCardMode != 0 && CreateCardMode != 1) CreateCardMode = 1;
			NeedDob = CreateCardMode != 0;//如果身份证号为本人则无需输入出生日期了
		}
		#endregion

	GetIDNo:
		if (!NeedIDNo) goto InputPhone;
		#region 输入身份证号
		{
			int iRet = FormStyle.BKStyle.GetIDNo003(CreateCardMode);
			if (iRet < 0) return -1;
			ZZJCore.SuanFa.Proc.Log("身份证号:" + ZZJCore.Public_Var.patientInfo.IDNo);
		}
		#endregion

	InputPhone:
		if (!NeedPhone) goto InputDob;
		#region 输入手机号
		{
			//如果患者信息里手机号为空,则请TA输入
			int iRet = FormStyle.BKStyle.GetPhone003(NeedIDNo);
			Application.DoEvents();
			if (iRet == -1) goto GetIDNo;
			if (iRet < 0) return -1;
			ZZJCore.SuanFa.Proc.Log("手机号:" + ZZJCore.Public_Var.patientInfo.Mobile);
		}
		#endregion

	InputDob:
		if (CreateCardMode == 0) ZZJCore.Public_Var.patientInfo.DOB = ZZJCore.SuanFa.Proc.IDNoGetDOB(ZZJCore.Public_Var.patientInfo.IDNo).ToString("yyyy-MM-dd");
		if (!NeedDob) goto SaveInfo;
		#region 输入出生日期
		{
			if (CreateCardMode == 0)
			{
				ZZJCore.Public_Var.patientInfo.DOB = ZZJCore.SuanFa.Proc.IDNoGetDOB(ZZJCore.Public_Var.patientInfo.IDNo).ToString("yyyy-MM-dd");
				goto SaveInfo;
			}
			DateTime DOB = DateTime.Now;
			int iRet = FormStyle.BKStyle.GetDOB002(ref DOB);
			if (iRet == -1 && NeedIDNo) goto GetIDNo;
			if (iRet == -1 && NeedPhone) goto InputPhone;
			if (iRet != 0) return -1;//如果是 关闭或者倒计时结束,就直接退出程序
			ZZJCore.Public_Var.patientInfo.DOB = DOB.ToString("yyyy-MM-dd");
			ZZJCore.SuanFa.Proc.Log("出生日期:" + ZZJCore.Public_Var.patientInfo.DOB);
		}
		#endregion

	SaveInfo:
		if (GetBK(0, "2") != 0)
		{
			ZZJCore.BackForm.ShowForm("您已经办理过区域卡了,请到窗口更换!", true);
			Application.DoEvents();
			return -1;
		}// */

		//if (ZZJCore.Public_Var.debug) ZZJCore.Public_Var.patientInfo.PatientName = DateTime.Now.ToString("DHHmmssfff");

		#region 平台建档
		ZZJCore.SuanFa.Proc.Log(CreateCardMode.ToString());

		if (ZZJ_Module.PingTaiCreateCard.CreateCard(CreateCardMode) != 0)
		{
			ZZJCore.SuanFa.Proc.Log("在平台建立账户失败," + ZZJ_Module.RegionalPlatform.Err);
			ZZJCore.BackForm.ShowForm("在平台建立账户失败," + ZZJ_Module.RegionalPlatform.Err, true);
			Application.DoEvents();
			return -1;
		}
		#endregion
		BKBZ = true;
		#region 写卡
		if (ZZJ_Module.PingTaiCreateCard.WriterCard() == 0)
		{
			ZZJCore.SuanFa.Proc.Log("写卡成功");
		}
		else
		{
			ZZJCore.SuanFa.Proc.Log("写卡失败");
		}
		#endregion

		ZZJCore.SuanFa.Proc.sleep(100);

		PrintCard();//打印卡片
		bool CZJG = true;
		string Msg = "";
		ZZJCore.Public_Var.cardInfo.CardType = "1";
		if (XMLCore.BindingCard(ZZJCore.Public_Var.cardInfo.CardNo, ZZJCore.Public_Var.patientInfo.PatientID, out Msg) != 0)
		{
			CZJG = false;
			ZZJCore.SuanFa.Proc.Log("绑定卡失败," + Msg);
			goto RET;
		}

		if (DC <= 0) goto RET;

		//从院内卡退费
		ZZJCore.Public_Var.cardInfo.CardType = "0";
		ZZJCore.Public_Var.cardInfo.CardNo = OldCardNo;
		if (XMLCore.CZ(-DC, out Msg, out InvokeRet, 0) != 0)
		{
			CZJG = false;
			goto RET;
		}

		//充到区域卡
		ZZJCore.Public_Var.cardInfo.CardType = "1";
		ZZJCore.Public_Var.cardInfo.CardNo = NewCardNo;
		if (XMLCore.CZ(DC, out Msg, out InvokeRet, 0) != 0)
		{
			CZJG = false;
			//如果充值区域失败,则把钱充回旧卡
			ZZJCore.Public_Var.cardInfo.CardType = "0";
			ZZJCore.Public_Var.cardInfo.CardNo = OldCardNo;
			XMLCore.CZ(DC, out Msg, out InvokeRet, 0);
		}

	RET:
		ZZJCore.SuanFa.Proc.Log(string.Format("充值结果:{0} 金额:{1} 描述:{2}", CZJG.ToString(), DC, Msg));
		HKSaveServerData(CZJG, DC, OldCardNo, Msg);
		ZZJCore.SuanFa.Proc.ZZJMessageBox("换卡成功!", "换卡成功!请收好区域卡!", false);
		return 0;
	}

	public static bool PhoneOK(string Phone)
	{
		if (ZZJCore.SuanFa.Proc.strlen(Phone) != 11) return false;
		if (!new string[] { "13", "14", "15", "17", "18" }.Contains(Phone.Substring(0, 2))) return false;
		return true;
	}

	public static void PrintCard()
	{
		ZZJCore.SuanFa.Proc.MsgSend(0x21, 0x00);//办卡成功
		ZZJCore.SuanFa.Proc.RunEXE(
		ZZJCore.Public_Var.ModulePath + "ZZJCoreTest.exe",
		string.Format(@"PrintCard ""{0}"" ""{1}"" {2}",
			ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev,
			ZZJCore.Public_Var.patientInfo.PatientName,
			ZZJ_Module.Public_Var.PF.patientCard
		));
		return;

		#region 打印卡片
		if (ZZJCore.SuanFa.Proc.ReadPublicINI("BKDoNotPrintCard") != "1")
		{
			try
			{
				ZZJCore.SuanFa.PrintCall.Print(ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev, PC, 500, 318, false);
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.Log("打印卡片出现异常!" + e.Message);
				string Msg = "";
				ZKJ_Dev.Out_Card(ref Msg);
			}
		}// */
		#endregion
	}

	private static void PC(object sender, PrintPageEventArgs e)
	{
		try
		{//姓名
			e.Graphics.DrawString(ZZJCore.Public_Var.patientInfo.PatientName, new Font(new FontFamily("黑体"), 12, FontStyle.Bold), System.Drawing.Brushes.Black, 55, 25);
		}
		catch (Exception ex)
		{ ZZJCore.SuanFa.Proc.Log(ex); }

		try
		{//患者ID
			e.Graphics.DrawString(ZZJ_Module.Public_Var.PF.patientCard, new Font(new FontFamily("黑体"), 12, FontStyle.Bold), System.Drawing.Brushes.Black, 175, 25);
		}
		catch (Exception ex)
		{
			ZZJCore.SuanFa.Proc.Log(ex);
		}
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
		catch (Exception e)
		{ ZZJCore.SuanFa.Proc.Log(e); return; }
		try
		{
			string msg = "";
			MSSQL_Operate mssql_operate = new MSSQL_Operate();
			List<SqlParameter> AL = new List<SqlParameter>();
			//发卡描述 
			AL.Add(AddSP(SqlDbType.VarChar, "@LSH", ZZJCore.Public_Var.SerialNumber));//流水号
			AL.Add(AddSP(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.patientInfo.PatientID));//登记号
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
			if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_FK_MX_ForZZJ_V4]", AL.ToArray(), out msg) > 0)
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

	#region 添加远程数据记录
	private static int InvokeRet = 0;
	private static void HKSaveServerData(bool CZJG, decimal DC, string OldCardNo, string CZMS)
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
			AL.Add(AddSP(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.patientInfo.PatientID));//登记号
			AL.Add(AddSP(SqlDbType.VarChar, "@KH", ZZJCore.Public_Var.cardInfo.CardNo));//卡号
			AL.Add(AddSP(SqlDbType.Int, "@KPLX", int.Parse(ZZJCore.Public_Var.cardInfo.CardType)));//卡类型
			AL.Add(AddSP(SqlDbType.VarChar, "@XM", ZZJCore.Public_Var.patientInfo.PatientName));//姓名
			AL.Add(AddSP(SqlDbType.VarChar, "@XB", ZZJCore.Public_Var.patientInfo.Sex));//性别
			AL.Add(AddSP(SqlDbType.VarChar, "@SJHM", ZZJCore.Public_Var.patientInfo.Mobile));//手机号码
			AL.Add(AddSP(SqlDbType.Date, "@CSNY", DateTime.Parse(ZZJCore.Public_Var.patientInfo.DOB)));//出生日期
			AL.Add(AddSP(SqlDbType.VarChar, "@ZJHM", ZZJCore.Public_Var.patientInfo.IDNo));//证件号
			AL.Add(AddSP(SqlDbType.VarChar, "@ZJLX", ZZJCore.Public_Var.patientInfo.IDType));//证件类型
			AL.Add(AddSP(SqlDbType.VarChar, "@YWLX", "换卡"));//业务类型
			AL.Add(AddSP(SqlDbType.Int, "@SL", 1));//数量
			AL.Add(AddSP(SqlDbType.Decimal, "@JE", (decimal)0));//金额
			AL.Add(AddSP(SqlDbType.DateTime, "@SJ", DateTime.Now));//时间
			AL.Add(AddSP(SqlDbType.VarChar, "@SFY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//收费员
			AL.Add(AddSP(SqlDbType.VarChar, "@KFLX", "无"));//病人扣费类型
			AL.Add(AddSP(SqlDbType.Int, "@FKBZ", 0));//发卡标志
			AL.Add(AddSP(SqlDbType.VarChar, "@FKMS", FKMS));//发卡描述
			AL.Add(AddSP(SqlDbType.VarChar, "@CZFS", CZMS));//发卡充值描述
			AL.Add(AddSP(SqlDbType.Decimal, "@CZJE", DC));//充值金额
			AL.Add(AddSP(SqlDbType.Int, "@CZBZ", CZJG ? 0 : -1));//充值成功标志
			AL.Add(AddSP(SqlDbType.VarChar, "@CZMS", ZZJ_Module.Public_Var.sFlowId));//充值描述
			AL.Add(AddSP(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));//自助机编号
			AL.Add(AddSP(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//HIS操作员
			AL.Add(AddSP(SqlDbType.VarChar, "@BL1", ZZJ_Module.Public_Var.PF.patientCard));//区域卡面号
			AL.Add(AddSP(SqlDbType.VarChar, "@BL2", OldCardNo));//旧卡号
			if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_FK_MX_ForZZJ_V5]", AL.ToArray(), out msg) > 0)
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