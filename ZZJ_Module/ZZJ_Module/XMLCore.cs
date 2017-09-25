using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Mw_Public;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

public static class XMLCore
{
	#region 签约
	public static int QYJD = 0;
	public static int QY(out string Msg)
	{
		ZZJCore.SuanFa.Proc.Log("开始签约,卡号:" + ZZJCore.Public_Var.cardInfo.CardNo);
		string OutXML = "";
		string VCardID = "";//虚拟卡号
		QYJD = 0;//签约进度
		int iRet = 0;
		iRet = QIDCard(out Msg);
		if (iRet != 0) return iRet;

		bool NeedPhone = false, NeedIDNo = false, NeedPassword = false;
		NeedPhone = string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.Mobile);
		NeedIDNo = string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.IDNo);
		NeedPassword = string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.Password);
		ZZJCore.SuanFa.Proc.Log(string.Format("NeedPhone:{0},NeedIDNo:{1},NeedPassword:{2}", NeedPhone.ToString(), NeedIDNo.ToString(), NeedPassword.ToString()));

		QYJD++;//1

	InputPhone:
		if (!NeedPhone) goto GetIDNo;
		#region 输入手机号
		//如果患者信息里手机号为空,则请TA输入
		ZZJCore.InputForm.InputPhoneParameter IP = new ZZJCore.InputForm.InputPhoneParameter();
		IP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
		iRet = ZZJCore.InputForm.InputPhone(IP);
		if (iRet < 0)
		{
			Msg = "";
			return -1;
		}
		ZZJCore.SuanFa.Proc.Log("手机号:" + ZZJCore.Public_Var.patientInfo.Mobile);
		#endregion

	GetIDNo:
		if (!NeedIDNo) goto InputPWD;
		#region 输入身份证号
		//如果患者信息里身份证号为空,则请TA输入
		ZZJCore.GetSFZ.GetSFZParameter GSP = new ZZJCore.GetSFZ.GetSFZParameter();
		if (!NeedPhone) GSP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
		if (NeedPhone) GSP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
		GSP.NeedName = false;
		iRet = ZZJCore.GetSFZ.ShowForm(GSP);
		if (iRet < 0)
		{
			Msg = "";
			return -1;
		}
		ZZJCore.SuanFa.Proc.Log("身份证号:" + ZZJCore.Public_Var.patientInfo.IDNo);
		#endregion

	InputPWD:
		if (!NeedPassword) goto CreateCard;
		#region 设置密码
		ZZJCore.InputForm.InputPasswordParameter IPP = new ZZJCore.InputForm.InputPasswordParameter();
		IPP.Caption = "请为您的卡设置6位密码";
		if (ZZJCore.Public_Var.cardInfo.CardType == "2") IPP.Caption = "请为社保卡设置6位密码";
		if (ZZJCore.Public_Var.cardInfo.CardType == "1") IPP.Caption = "请为区域卡设置6位密码";
		IPP.RegMode = true;
		if (NeedIDNo || NeedPhone) IPP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
		if (!(NeedIDNo || NeedPhone)) IPP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
		iRet = ZZJCore.InputForm.InputPassword(IPP);
		if (iRet == -1 && NeedIDNo) goto GetIDNo;
		if (iRet == -1 && NeedPhone) goto InputPhone;
		if (iRet < 0)
		{
			Msg = "";
			return -1;
		}
		ZZJCore.Public_Var.patientInfo.Password = ZZJCore.SuanFa.Proc.MD5(ZZJCore.Public_Var.patientInfo.Password);
		#endregion

		QYJD++;//2

	CreateCard:
		#region 建立虚拟卡
		OutXML = "<Request>";
		OutXML += "<CardNo></CardNo>";
		OutXML += "<PatientName>" + ZZJCore.Public_Var.patientInfo.PatientName + "</PatientName>";//从社保卡读取
		OutXML += "<Sex>" + ZZJCore.Public_Var.patientInfo.Sex + "</Sex>";//性别从身份证号获取,倒数第二位如果是单数就是男,是双数就是女
		DateTime DOB = DateTime.Parse(ZZJCore.Public_Var.patientInfo.DOB);
		OutXML += "<Birthday>" + DOB.ToString("yyyy-MM-dd") + "</Birthday>";//出生日期从身份证号获取
		int Age = DateTime.Now.Year - DOB.Year;
		OutXML += "<Age>" + Age.ToString() + "</Age>";//年龄从出生日期算
		OutXML += "<IDCardNo>" + ZZJCore.Public_Var.patientInfo.IDNo + "</IDCardNo>";//身份证号可以从医保卡读取
		OutXML += "<SecrityNo>666</SecrityNo>";
		OutXML += "<CardSerNo></CardSerNo>";
		OutXML += "<Amt>0.00</Amt>";
		OutXML += "<Address> </Address>";//地址就为空吧
		OutXML += "<Tel>" + ZZJCore.Public_Var.patientInfo.Mobile + "</Tel>";
		OutXML += "<UserId>" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "</UserId>";
		OutXML += "<ActDate>" + DateTime.Now.ToString("yyyy-MM-dd") + "</ActDate>";
		OutXML += "<ActTime>" + DateTime.Now.ToString("hh-mm-ss") + "</ActTime>";
		OutXML += "<PassWord>" + ZZJCore.Public_Var.patientInfo.Password + "</PassWord>";
		OutXML += "<CardStatus>2</CardStatus>";
		OutXML += "<CardType>本人</CardType>";
		OutXML += "<KinNext></KinNext>";
		OutXML += "<KinRelation></KinRelation>";
		OutXML += "</Request>";
		try
		{
			ZZJCore.SuanFa.Proc.BW(OutXML, "办虚拟卡");
			string InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CreateCardPatInfo", new string[] { OutXML }) as string;
			ZZJCore.SuanFa.Proc.BW(InXML, "办虚拟卡");
			XmlDocument XD = new XmlDocument();
			XD.LoadXml(InXML);
			VCardID = XD.SelectSingleNode("Response/SerID").InnerText.Trim();

			if (string.IsNullOrEmpty(VCardID))
			{
				ZZJCore.SuanFa.Proc.Log("绑定失败,办虚拟卡返回空卡号.");
				Msg = "签约失败,请到人工窗口办理!";
				return -1;
			}

			ZZJCore.SuanFa.Proc.Log("办理虚拟卡成功!卡号:" + VCardID);

		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("办理虚拟卡时出现异常,可能是网络问题" + e.Message);
			Msg = "注册账户失败!";
			return -2;
		}// */
		#endregion

		QYJD++;//3

		if (BindingCard(ZZJCore.Public_Var.cardInfo.CardNo, VCardID, out Msg) != 0)
		{
			XK(VCardID);
			return -1;
		}

		QYJD++;

		if (ZZJ_Module.Public_Var.RCR == null) ZZJ_Module.Public_Var.RCR = new ZZJSubModuleInterface.ReadCardResultInfoCls();
		ZZJ_Module.Public_Var.RCR.LoginFlag = true;
		ZZJCore.SuanFa.Proc.Log("签约成功!姓名:" + ZZJCore.Public_Var.patientInfo.PatientName + " 虚拟卡号:" + VCardID);
		ZZJCore.Public_Var.patientInfo.PatientID = VCardID;
		Msg = "";
		return 0;
	}
	#endregion

	#region 绑定
	public static int BindingCard(string RealCradNo, string VCardNo, out string Msg)
	{
		try
		{
			string OutXML = "<Request>";
			OutXML += "<CardNo>" + VCardNo + "</CardNo>";
			OutXML += "<SecrityNo>666</SecrityNo>";
			OutXML += "<BankCardNo>" + RealCradNo + "</BankCardNo>";
			if (ZZJCore.Public_Var.cardInfo.CardType == "2") OutXML += "<BingType>2</BingType>";//医保卡
			if (ZZJCore.Public_Var.cardInfo.CardType == "1") OutXML += "<BingType>7</BingType>";//区域
			string Word = "";
			if (ZZJCore.Public_Var.cardInfo.CardType == "1") Word = ZZJ_Module.Public_Var.PF.patientCard;//区域卡卡面号
			OutXML += "<Word>" + Word + "</Word>";
			OutXML += "<IDNo>" + ZZJCore.Public_Var.patientInfo.IDNo + "</IDNo>";
			OutXML += "<PhoneNumber>" + ZZJCore.Public_Var.patientInfo.Mobile + "</PhoneNumber>";
			OutXML += "<UserId>" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "</UserId>";
			OutXML += "</Request>";
			ZZJCore.SuanFa.Proc.BW(OutXML, "绑定");
			string InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "BindingBankNo", new string[] { OutXML }) as string;
			ZZJCore.SuanFa.Proc.BW(InXML, "绑定");
			XmlDocument XD = new XmlDocument();
			XD.LoadXml(InXML);
			if (XD.SelectSingleNode("Response/ResultCode").InnerText.Trim() != "0")
			{
				ZZJCore.SuanFa.Proc.Log("绑定失败,需要查看报文.");
				Msg = "签约失败,请到人工窗口办理!";
				return -1;
			}
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("绑定虚拟卡时出现异常,可能是网络问题," + e.Message);
			Msg = "网络异常,签约失败,请到人工窗口查询!";
			return -2;
		}
		ZZJCore.SuanFa.Proc.Log("绑定虚拟卡成功!");
		Msg = "";
		return 0;
	}
	#endregion

	#region 销卡
	public static bool XK(string CardID)
	{
		try
		{
			string OutXML = "<Request><CardNo>" + CardID + "</CardNo></Request>";
			ZZJCore.SuanFa.Proc.BW(OutXML, "销卡");
			string InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "Rush_Patid", new object[] { OutXML }) as string;
			ZZJCore.SuanFa.Proc.BW(InXML, "销卡");
			XmlDocument XD = new XmlDocument();
			XD.LoadXml(InXML);
			return (int.Parse(XD.SelectSingleNode("Response/ResultCode").InnerText.Trim()) == 0);
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			return false;
		}
	}
	#endregion

	public static int QIDCard(out string Msg)
	{
		#region 查询办卡记录
		/*
		try
		{
			string InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(MEF.URL, "IDCardCheck", new string[] { "<Request><IDCardNo>" + ZZJCore.Public_Var.patientInfo.IDNo + "</IDCardNo></Request>" }) as string;
			XmlDocument XD = new XmlDocument();
			XD.LoadXml(InXML);
			if (XD.SelectSingleNode("Response/ResultCode").InnerText.Trim() != "0" || XD.SelectSingleNode("Response/Status").InnerText.Trim() != "0")
			{
				ZZJCore.SuanFa.Proc.Log("在签约时,查询办卡记录发现不可办卡,返回报文:" + InXML);
				Msg = "签约失败,您可能办过就诊卡,请到人工窗口咨询!";
				return -1;
			}
		}
		catch (Exception Ex)
		{
			ZZJCore.SuanFa.Proc.Log("办理虚拟卡前查询时出现异常,可能是网络问题," + Ex.Message);
			Msg = "签约失败!";
			return -2;
		}// */
		#endregion
		Msg = "";
		return 0;//不查询了,集成到办卡接口
	}

	/// <summary>
	/// 通过卡号获取账户号
	/// </summary>
	/// <param name="CardNo"></param>
	/// <param name="CardType"></param>
	/// <param name="Msg"></param>
	/// <returns></returns>
	public static int XMLGetCardNo(string CardNo, string CardType, out string Msg)
	{
		string OutXML = "";
		XmlDocument XD = new XmlDocument();
		OutXML = "<Request><PCardNo>" + CardNo + "</PCardNo><PCardType>" + CardType + "</PCardType></Request>";
		try
		{
			ZZJCore.SuanFa.Proc.BW(OutXML, "根据卡号查询账户号");
			string InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "Get_CardNo", new string[] { OutXML }) as string;
			ZZJCore.SuanFa.Proc.BW(InXML, "根据卡号查询账户号");
			XD.LoadXml(InXML);
			if (XD.SelectSingleNode("Response/ResultCode").InnerText.Trim() == "1")
			{
				ZZJCore.SuanFa.Proc.Log("[读取持卡人信息]返回1:无效卡");
				Msg = "该卡为无效卡";
				return 1;
			}

			if (XD.SelectSingleNode("Response/ResultCode").InnerText.Trim() == "0")
			{
				ZZJCore.Public_Var.patientInfo.PatientID = XD.SelectSingleNode("Response/CardNo").InnerText.Trim();
				ZZJCore.SuanFa.Proc.Log("有效卡,虚拟卡号:" + ZZJCore.Public_Var.patientInfo.PatientID);
				Msg = "";
				return string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.PatientID) ? -1 : 0;
			}
			ZZJCore.SuanFa.Proc.Log(XD.SelectSingleNode("Response/ErrorMsg").InnerText.Trim());
			Msg = "查询信息失败,请到人工窗口处理!";
			ZZJCore.SuanFa.Proc.Log("[读取持卡人信息]返回-1:查询失败");
			return -1;
		}
		catch (Exception Ex)
		{
			ZZJCore.SuanFa.Proc.Log(Ex);
			Msg = "接口异常,请稍后再试!";
			return -2;
		}
	}

	public static int GetCardNO(out string Msg)
	{
		ZZJCore.Public_Var.cardInfo.CardType = ZZJ_Module.Public_Var.RCR.KPLX.ToString();
		//卡片类型卡片类型(0.院内卡,1.区域卡,2.社保卡(IC),3.社保卡(旧),4.青岛银行卡）
		#region 院内卡
		if (ZZJ_Module.Public_Var.RCR.KPLX == 0)
		{
			ZZJCore.SuanFa.Proc.Log("院内卡,卡号:" + ZZJ_Module.Public_Var.RCR.KH);
			ZZJCore.Public_Var.cardInfo.CardNo = ZZJ_Module.Public_Var.RCR.KH;
			int iRet = XMLGetCardNo(ZZJ_Module.Public_Var.RCR.KH, "院内诊疗卡", out Msg);
			ZZJ_Module.Public_Var.RCR.LoginErrorFlag = -4;
			if (iRet == 0) Msg = "";
			return iRet;
		}
		#endregion
		#region 社保卡
		if (ZZJ_Module.Public_Var.RCR.KPLX == 2)
		{
			QDYBCard.Mode = 0;
			ZZJCore.SuanFa.Proc.Log("IC社保卡,证件号:" + ZZJ_Module.Public_Var.RCR.ZJHM);
			if (ZZJCore.Public_Var.debug)
			{
				QDYBCard.GRBH = DateTime.Now.ToString("HHmmss");
				goto ReadSBCardEnd;
			}
			if (QDYBCard.YBKHGetHZInfo("") != 0)
			{
				ZZJCore.SuanFa.Proc.Log("查不到社保卡信息!");
				Msg = "查不到社保卡信息!";
				return -1;
			}// */

			ReadSBCardEnd:
			ZZJCore.SuanFa.Proc.Log("在医保平台查询医保卡信息成功,个人编号:" + QDYBCard.GRBH);

			ZZJCore.Public_Var.patientInfo.Mobile = "";
			ZZJCore.Public_Var.patientInfo.PatientName = QDYBCard.HZName;
			ZZJCore.Public_Var.cardInfo.CardNo = QDYBCard.GRBH;
			ZZJCore.Public_Var.patientInfo.IDNo = QDYBCard.SFZH;
			ZZJCore.Public_Var.patientInfo.Sex = QDYBCard.Sex;
			try
			{
				ZZJCore.Public_Var.patientInfo.DOB = ZZJCore.SuanFa.Proc.IDNoGetDOB(QDYBCard.SFZH).ToString("yyyy-MM-dd");
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
			}
			//返回:
			//0:表示该卡为已经签约社保卡,帐号在病例号变量里放着
			//1:表示该卡为未签约社保卡
			int iRet = XMLGetCardNo(QDYBCard.GRBH, "社保卡", out Msg);
			if (iRet == 1)
			{
				ZZJCore.SuanFa.Proc.Log("未签约社保卡,开始签约!");
				if (ZZJ_Module.Public_Proc.ReadZZJConfig("SBK_QYBD") == "0")
				{//禁止签约模式,直接返回-5
					ZZJ_Module.Public_Var.RCR.LoginFlag = false;
					ZZJ_Module.Public_Var.RCR.LoginErrorFlag = -5;
					Msg = "您的卡未签约!";
					return -2;
				}
				// */
				int QYRet = QY(out Msg);

				if (QYJD > 1) Login.SavelocalAndServerData(QYRet, Msg);

				/*
				if (QYRet == 0)
				{
					try
					{
						if (QYKBK.GetBK(1, "10") == 0) ZZJ_Module.PingTaiCreateCard.CreateCard(9);
					}
					catch (Exception e)
					{
						ZZJCore.SuanFa.Proc.Log(e);
					}
				}
				// */
				return QYRet;
			}
			return iRet;
		}
		#endregion 社保卡
		#region 区域卡
		if (ZZJ_Module.Public_Var.RCR.KPLX == 1)
		{
			ZZJCore.SuanFa.Proc.Log("区域卡,卡号:" + ZZJ_Module.Public_Var.RCR.KH);
			ZZJCore.Public_Var.cardInfo.CardNo = ZZJ_Module.Public_Var.RCR.KH;

			int iRet = XMLGetCardNo(ZZJ_Module.Public_Var.RCR.KH, "区域卡", out Msg);
			if (iRet == 1)
			{
				if (ZZJ_Module.RegionalPlatform.QueryPatient() != 0)
				{
					ZZJCore.SuanFa.Proc.Log("读取区域卡信息失败!" + ZZJ_Module.RegionalPlatform.Err);
					return -1;
				}
				ZZJCore.SuanFa.Proc.Log("未签约区域卡,开始签约!");
				iRet = QY(out Msg);
				return iRet;
			}
			ZZJ_Module.Public_Var.RCR.LoginErrorFlag = -4;
			if (iRet == 0) Msg = "";
			return iRet;
		}
		#endregion 区域卡

		Msg = "不支持的卡";
		ZZJCore.SuanFa.Proc.Log("不支持的卡!");

		return -2;
	}

	public static int QueryQYCardAmount()
	{
		if (ZZJCore.Public_Var.cardInfo.CardType != "1") return 0;

		if (ZZJ_Module.RegionalPlatform.QueryPatient("", false) != 0)
		{
			ZZJCore.Public_Var.patientInfo.DepositAmount = "0";
			ZZJCore.SuanFa.Proc.Log("查询区域卡信息失败!" + ZZJ_Module.RegionalPlatform.Err);
			ZZJ_Module.Public_Var.IsRealPlatformCrad = false;
			return -1;
		}
		ZZJ_Module.Public_Var.IsRealPlatformCrad = true;
		return 0;
	}

	public static int GetUserInfo(out string Msg, bool XXBL = false)
	{
		if (ZZJ_Module.Public_Var.RCR == null)
		{
			ZZJCore.Public_Var.cardInfo.CardNo = "";
			ZZJCore.Public_Var.cardInfo.CardType = "0";
			ZZJCore.Public_Var.patientInfo.PatientName = "";
			ZZJCore.Public_Var.patientInfo.Password = "";
			ZZJCore.Public_Var.patientInfo.DepositAmount = "";
			Msg = "无卡";
			return 1;
		}

		#region 调试信息
		if (ZZJCore.SuanFa.Proc.ReadPublicINI("LocalInfo", "0") == "1")
		{
			ZZJCore.Public_Var.patientInfo.IDNo = "330184198501184115";
			ZZJCore.Public_Var.patientInfo.Mobile = "13023456789";
			ZZJCore.Public_Var.patientInfo.ParentName = "家属姓名";
			ZZJCore.Public_Var.patientInfo.PatientName = "演示卡";
			ZZJCore.Public_Var.patientInfo.Sex = "男";
			ZZJCore.Public_Var.patientInfo.DOB = "2001-01-15";
			ZZJCore.Public_Var.patientInfo.PatientID = "0123456789";
			ZZJCore.Public_Var.cardInfo.CardNo = ZZJ_Module.Public_Var.RCR.KH;
			ZZJCore.Public_Var.patientInfo.DepositAmount = ZZJCore.SuanFa.Proc.ReadPublicINI("LocalYE", "0");
			Msg = "";
			ZZJ_Module.Public_Var.RCR.LoginFlag = true;
			return 0;
		}
		#endregion

		//if (GetCardNO(out Msg) != 0) return -1;

		#region 整理报文并查询

        //读卡逻辑中KPLX：院内卡-0，社保卡-2，港内卡-5 ，区域卡-1，银联卡-4
		// HIS卡类型（CardType）:    0为院内卡(射频卡 按IC)  1为港内卡(港内，附属卡)  2社保卡 
        //接口卡类型：1、磁卡2、保障卡 3、IC卡 4、身份证号
        //读出银联卡==HIS：X？
        if (ZZJ_Module.Public_Var.RCR.KPLX == 4)
        {
            ZZJCore.Public_Var.cardInfo.CardType = "3";
        }       
        //读出院内卡==HIS：3
        if (ZZJ_Module.Public_Var.RCR.KPLX == 0)
        {
            ZZJCore.Public_Var.cardInfo.CardType = "0";
        }
        //读出IC社保卡==HIS：2
        if (ZZJ_Module.Public_Var.RCR.KPLX == 2)
        {
            ZZJCore.Public_Var.cardInfo.CardType = "2";
            ZZJCore.Public_Var.cardInfo.CardNo = ZZJ_Module.Public_Var.RCR.ZJHM;//读卡逻辑中，从医保接口读到的身份证号
            ZZJCore.Public_Var.patientInfo.Mobile = "";
            ZZJCore.Public_Var.patientInfo.PatientName = QDYBCard.HZName;
            ZZJCore.Public_Var.patientInfo.IDNo = QDYBCard.SFZH;
            ZZJCore.Public_Var.patientInfo.Sex = QDYBCard.Sex;
            ZZJCore.Public_Var.patientInfo.Address = "";
            string getYBInfo = string.Format("个人编号{0}，姓名{1}，身份证{2}，性别{3}", QDYBCard.GRBH, QDYBCard.HZName, QDYBCard.SFZH, QDYBCard.Sex);
            ZZJCore.SuanFa.Proc.Log(getYBInfo);
            try
            {
                ZZJCore.Public_Var.patientInfo.DOB = ZZJCore.SuanFa.Proc.IDNoGetDOB(QDYBCard.SFZH).ToString("yyyy-MM-dd");
                ZZJCore.SuanFa.Proc.Log("生日为:" + ZZJCore.Public_Var.patientInfo.DOB);
            }
            catch (Exception e)
            {
                ZZJCore.SuanFa.Proc.Log(e);
            }

        }
        if (ZZJ_Module.Public_Var.RCR.KPLX == 5)
        {
            ZZJCore.Public_Var.cardInfo.CardType = "1";
        }
    SecondQuery:
        XmlMaker XM = NewTabel("", ZZJCore.Public_Var.cardInfo.CardNo, int.Parse(ZZJCore.Public_Var.cardInfo.CardType));
		XmlDocument XD = InvokeHIS("查询患者信息", "101", XM);
		if (XD == null)
		{
			Msg = "调用查询接口失败,请稍后再试!";
			ZZJ_Module.Public_Var.RCR.LoginErrorFlag = -5;
			return -1;
		}
		#endregion
		#region 解析返回的报文
		try
		{
			if (XD.SelectSingleNode("zzxt/result/retcode").InnerText.Trim() != "0")//修改的地方  !=0
			{
                //社保卡类型 在HIS中没有查询到信息， 尝试后台签约。
                ZZJCore.SuanFa.Proc.Log("没有查询到患者信息!");
                if (ZZJ_Module.Public_Var.RCR.KPLX == 2)
                {
                    ZZJCore.SuanFa.Proc.Log("医保卡进行签约!");
                    InputPhone:
                   
                        int iRet = FormStyle.BKStyle.GetPhone002();
												ZZJCore.SuanFa.Proc.Log("输入手机号的回参iRet:"+iRet);
                if (iRet == 0) ZZJCore.SuanFa.Proc.Log("输入的手机号为:"+ZZJCore.Public_Var.patientInfo.Mobile);
                if (iRet != 0) {ZZJCore.SuanFa.Proc.Log("用户取消了医保卡签约!"); Msg="用户取消了医保卡签约!"; return -1; } //goto InputPhone;
                if (!string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.Mobile))
               {   
										//取一下性别
										#region 设置性别
										try
										{
											if(ZZJCore.Public_Var.patientInfo.IDNo.Length==15)//老身份证号 15位 末位为性别
											{
												if (Convert.ToInt32(ZZJCore.Public_Var.patientInfo.IDNo.Substring(ZZJCore.Public_Var.patientInfo.IDNo.Length - 1, 1)) % 2 != 0)//奇数 男
												{
													ZZJCore.Public_Var.patientInfo.Sex="男";
												}
												else
												{
													ZZJCore.Public_Var.patientInfo.Sex = "女";
												}
											}
											else if (ZZJCore.Public_Var.patientInfo.IDNo.Length == 18)//新身份证号
											{
												if (Convert.ToInt32(ZZJCore.Public_Var.patientInfo.IDNo.Substring(ZZJCore.Public_Var.patientInfo.IDNo.Length - 2, 1)) % 2 != 0)//奇数 男
													{
														ZZJCore.Public_Var.patientInfo.Sex = "男";
													}
													else
													{
														ZZJCore.Public_Var.patientInfo.Sex = "女";
													}
											}
										}
										catch(Exception ex)
										{
											ZZJCore.SuanFa.Proc.Log("建档时设置患者性别参上异常:"+ex.ToString());
										}
										#endregion
										ZZJCore.SuanFa.Proc.Log("转换过后的性别:" + ZZJCore.Public_Var.patientInfo.Sex.ToString());
										string FKMS="";
                    string OutMsg="";
										//签约
                  int QYiRet=  BK(0, out OutMsg, out FKMS);
                  if (QYiRet == 0)
                  {
                      ZZJCore.SuanFa.Proc.Log("社保卡与HIS签约成功！");
											//ZZJCore.BackForm.ShowForm("社保卡在医院建档成功!");
                      goto SecondQuery;

                  }
                  else
                  {
                      ZZJCore.SuanFa.Proc.Log("社保卡与HIS签约失败！");
                      

                  }
                }
                 
              }
								if (ZZJ_Module.Public_Var.RCR.KPLX == 4)
								{
									Msg = "暂未开通银联卡签约";
									return -1;
									//ToDo先清除一下病人信息.初始化
									ZZJCore.Public_Var.patientInfo.Age=0;
									ZZJCore.Public_Var.patientInfo.DepositAmount="0";
									ZZJCore.Public_Var.patientInfo.DOB="";
									ZZJCore.Public_Var.patientInfo.IDNo="";
									ZZJCore.Public_Var.patientInfo.PatientName="";
									ZZJCore.Public_Var.patientInfo.Sex="";
									ZZJCore.Public_Var.patientInfo.Address="";
									ZZJCore.Public_Var.patientInfo.Mobile="";

									Msg = "";
									ZZJCore.SuanFa.Proc.Log("银联卡进行签约!");
								
								InputIDNo://输入身份证号
									int iRet=FormStyle.BKStyle.GetIDNo002();
									if(iRet==0) ZZJCore.SuanFa.Proc.Log("输入的身份证号为:"+ZZJCore.Public_Var.patientInfo.IDNo);
									if (iRet != 0) { Msg = "签约取消或失败"; ZZJCore.SuanFa.Proc.Log("银联卡签约被取消或失败!"); return -2; }
									if(!string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.IDNo)) goto GetInfo;
								GetInfo://通过输入身份证号获取相关信息
									//性别
									if(Convert.ToInt32( ZZJCore.Public_Var.patientInfo.IDNo.Substring(16,1))%2==0)
									{
										ZZJCore.Public_Var.patientInfo.Sex="女";
									}
									else
									{
										ZZJCore.Public_Var.patientInfo.Sex="男";
									}
									ZZJCore.SuanFa.Proc.Log("输入的性别为:" + ZZJCore.Public_Var.patientInfo.Sex);
									//出生日期
									ZZJCore.Public_Var.patientInfo.DOB = ZZJCore.Public_Var.patientInfo.IDNo.Substring(6, 4) + "-" + ZZJCore.Public_Var.patientInfo.IDNo.Substring(10, 2) + "-" + ZZJCore.Public_Var.patientInfo.IDNo.Substring(12, 2);
									ZZJCore.SuanFa.Proc.Log("输入的性别为:"+ZZJCore.Public_Var.patientInfo.DOB);
									//年龄
									ZZJCore.Public_Var.patientInfo.Age=Convert.ToInt32(DateTime.Now.Year)-Convert.ToInt32(ZZJCore.Public_Var.patientInfo.DOB.Substring(0,4));
									ZZJCore.SuanFa.Proc.Log("输入的年龄为:"+ZZJCore.Public_Var.patientInfo.Age);
								InputPhone://输入手机号
									iRet = FormStyle.BKStyle.GetPhone002();
									if (iRet == 0) ZZJCore.SuanFa.Proc.Log("输入的手机号为:" + ZZJCore.Public_Var.patientInfo.Mobile);
									if (iRet != 0) goto InputIDNo;
									if (!string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.Mobile))
									{
										string FKMS = "";
										string OutMsg = "";
										//签约
										int QYiRet = BK(0, out OutMsg, out FKMS);
										if (QYiRet == 0)
										{
											//ZZJCore.BackForm.ShowForm("建档成功，请办理业务!", true);
											ZZJCore.SuanFa.Proc.Log("银联卡与HIS签约成功！");
											goto SecondQuery;

										}
										else
										{
											//ZZJCore.BackForm.ShowForm("建档失败,可能的原因:"+OutMsg, true);
											ZZJCore.SuanFa.Proc.Log("银联与HIS签约失败！");

										}
									}
								}
                
				Msg = XD.TrySelectSingleNode("zzxt/result/retmsg");
				ZZJ_Module.Public_Var.RCR.LoginErrorFlag = -4;
				return -2;
			}// */
			/*
				<Response>
					<PatName>田秀明</PatName> 
					<Sex>女</Sex> 
					<Age>33</Age> 
					<ID>372527198211202841</ID> 
					<ChargeType>0100000000</ChargeType> 
					<Balance>11.08</Balance> 
					<PlatformID /> 
					<PhoneNumber>13168146182</PhoneNumber> 
					<ResultCode>0</ResultCode> 
					<ErrorMsg /> 
				</Response>
			// */
			if (XD.SelectSingleNode("zzxt/table/zhjb").InnerText.Trim() != "")
			{
				ZZJ_Module.Public_Var.zhjb = XD.SelectSingleNode("zzxt/table/zhjb").InnerText.Trim();
			}
			ZZJCore.Public_Var.patientInfo.DOB = "";
			ZZJCore.Public_Var.patientInfo.PatientName = XD.SelectSingleNode("zzxt/table/hzxm").InnerText.Trim();//患者姓名
			ZZJCore.Public_Var.patientInfo.DepositAmount = XD.SelectSingleNode("zzxt/table/zhje").InnerText.Trim();//余额
		//	if (ZZJCore.Public_Var.cardInfo.CardType == "1") ZZJCore.Public_Var.patientInfo.DepositAmount = "0";//如果是区域卡,则HIS余额不算
			ZZJCore.Public_Var.patientInfo.IDNo = XD.SelectSingleNode("zzxt/table/sfzh").InnerText.Trim();//身份证号
			ZZJCore.Public_Var.patientInfo.PatientID = XD.SelectSingleNode("zzxt/table/patid").InnerText.Trim();//账户号
	//		if (ZZJCore.SuanFa.Proc.ReadPublicINI("IDNoClear", "0") == "1") ZZJCore.Public_Var.patientInfo.IDNo = "";

			if (!string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.IDNo)) if (ZZJCore.Public_Var.patientInfo.IDNo.Length == 18)
			{
				try
				{
					ZZJCore.Public_Var.patientInfo.DOB = ZZJCore.SuanFa.Proc.IDNoGetDOB(ZZJCore.Public_Var.patientInfo.IDNo).ToString();
				}
				catch (Exception e)
				{
					ZZJCore.SuanFa.Proc.Log(e);
				}
			}//End If


			//ZZJCore.Public_Var.patientInfo.Age = int.Parse(XD.SelectSingleNode("Response/Age").InnerText.Trim());//年龄

			ZZJCore.Public_Var.patientInfo.Sex = XD.SelectSingleNode("zzxt/table/sex").InnerText.Trim();//性别
			ZZJCore.Public_Var.patientInfo.Address=XD.SelectSingleNode("zzxt/table/lxdz").InnerText.Trim();//地址
			XmlNode XN = XD.SelectSingleNode("zzxt/table/lxdh");
			if (XN != null) ZZJCore.Public_Var.patientInfo.Mobile = XN.InnerText.Trim();//手机号码
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("查询患者信息,解析返回报文时异常." + e.Message);
			Msg = "系统错误,查询失败,请稍后再试!";
			ZZJ_Module.Public_Var.RCR.LoginErrorFlag = -5;
			return -3;
		}
		#endregion
		Msg = "";
	//在没有使用区域卡,2016-8-20注释	QueryQYCardAmount();


		return 0;
	}

	public static DateTime ToDateTime(this string S)
	{
		return DateTime.Parse(S);
	}

	public static int BK(int Mode, out string FKMS, out string Msg)
	{
		XmlMaker XM = NewTabel("", ZZJCore.Public_Var.cardInfo.CardNo, int.Parse(ZZJCore.Public_Var.cardInfo.CardType));
		XM.Add("blh", "");//病历号
		XM.Add("hzxm", ZZJCore.Public_Var.patientInfo.PatientName);//患者姓名
		XM.Add("grybh", "");
		XM.Add("dwybh", "");
		XM.Add("sbh", "");
		XM.Add("qtkh", "");
		XM.Add("sfzh", ZZJCore.Public_Var.patientInfo.IDNo);//身份证号
		XM.Add("sex", ZZJCore.Public_Var.patientInfo.Sex);//性别
		XM.Add("birth", ZZJCore.Public_Var.patientInfo.DOB.ToDateTime().ToString("yyyyMMdd"));//生日
		XM.Add("lxdz", ZZJCore.Public_Var.patientInfo.Address);//地址
		XM.Add("lxdh", ZZJCore.Public_Var.patientInfo.Mobile);//联系电话
		XM.Add("yzbm", "");//邮政编码
		XM.Add("lxr", ZZJCore.Public_Var.patientInfo.PatientName);//联系人
		XM.Add("ybdm", "1");//医保代码
		XM.Add("pzh", "");//凭证号
		XM.Add("dwbm", "");//单位代码
		XM.Add("dwmc", "");//单位名称
		XM.Add("qxdm", "");//区县代码
		XM.Add("ksrq", "");//开始日期
		XM.Add("jsrq", "");//结束日期
		XM.Add("ylxm", "");//医疗项目
		XM.Add("zddm", "");//诊断代码
		XM.Add("zhje", 0);//账户金额
		XM.Add("memo", "zzxt");

		int iRet = 0;
		Msg = "";
		try
		{
			//InXML="<Response><ResultCode>0</ResultCode><ErrorMsg>HIS:medical_card_memo中存在该卡号记录</ErrorMsg><SerID></SerID></Response>";
			XmlDocument XD = InvokeHIS("办卡接口", "103", XM);
			iRet = int.Parse(XD.SelectSingleNode("zzxt/result/retcode").InnerText.Trim());
			ZZJCore.SuanFa.Proc.BW(XD.ToString(),"HIS接口返回(办卡)");
			FKMS = "办卡失败";
			if (iRet == 0) FKMS = XD.SelectSingleNode("zzxt/table/patid").InnerText.Trim();
			if (iRet == 1) FKMS = XD.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
			if (iRet != 0) Msg = FKMS;
            if (iRet != 0) return iRet;
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			FKMS = "调用办卡接口失败!";
			Msg = "调用办卡接口失败!";
            return -1;
		}
        try
        {
            XmlMaker Table = NewTabel(FKMS);
            Table.Add("passwd", "");//密码
            //InXML="<Response><ResultCode>0</ResultCode><ErrorMsg>HIS:medical_card_memo中存在该卡号记录</ErrorMsg><SerID></SerID></Response>";
            XmlDocument XD = InvokeHIS("建立预交金账户", "104", Table);
            iRet = int.Parse(XD.SelectSingleNode("zzxt/result/retcode").InnerText.Trim());
            FKMS = "建账户失败";
            if (iRet == 0) FKMS = XD.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
            if (iRet != 0) Msg = FKMS;
        }
        catch (Exception e)
        {
            ZZJCore.SuanFa.Proc.Log(e);
            FKMS = "调用建立预交金账户接口失败!";
            Msg = "调用建立预交金账户接口失败!";
						iRet = -1;
				}
				try
				{
	      	 string JL=ZZJCore.SuanFa.POSTInvoke.Invoke("http://10.17.133.1:8080/SetBK.php?IDNo="+ZZJCore.Public_Var.patientInfo.IDNo+"", "");
    		 }
    		 catch(Exception e)
				{
					ZZJCore.SuanFa.Proc.Log("保存办卡时间出错" + e);
	   	 }
		return iRet;
	}

	//金额 预存描述 充值类型
	//2种返回值:
	//函数直接返回值:表示充值结果 
	//out返回值:表示调接口结果 bit: 000000 平台 HIS
	public static int CZ(decimal DC, out string YCMS, out int InvokeRet, int CXType = 0, string BankName = "", string JYLX = "")
	{
		InvokeRet = 0;
		int zffsFlag=1;
		if(CXType!=0)
		{
		   zffsFlag=5;
		}
		//if (ZZJCore.Public_Var.cardInfo.CardType == "1")
		//{
		//  int iRet = ZZJ_Module.UserData.YJJCZ(DC, (CXType == 0 ? "CA" : "DB"));
		//  YCMS = ZZJ_Module.UserData.Msg;
		//  ZZJCore.SuanFa.Proc.Log(YCMS);
		//  if (iRet != 0)
		//  {
		//    InvokeRet = 2;
		//    return iRet;
		//  }
		//}
		//string OutXML = "<Request>";
		//OutXML += "<CardNo>" + ZZJCore.Public_Var.patientInfo.PatientID + "</CardNo>";//卡号
		//OutXML += "<SecrityNo>333</SecrityNo>";//校验码
		//OutXML += "<CardSerNo></CardSerNo>";//卡序列号
		//OutXML += "<BankName>" + ZZJCore.Public_Var.cardInfo.CardType + "</BankName>"; //银行名称(改传卡类型)
		//OutXML += "<RechargeType>" + ((CXType == 0) ? "现金" : "银联POS") + "</RechargeType>";//充值类型 现金 银联POS
		//OutXML += "<BankCardNo>" + (CXType == 0 ? ZZJCore.Public_Var.cardInfo.CardNo : ZZJ_Module.UnionPay.UnionPayCardNo) + "</BankCardNo>";//银行卡卡号
		//OutXML += "<SysJourNo>" + (CXType == 0 ? ZZJ_Module.Public_Var.sFlowId : ZZJ_Module.UnionPay.RETDATA.strRef) + "</SysJourNo>";//JPRE流水号
		//OutXML += "<PosserialNo>" + (CXType == 0 ? ZZJ_Module.Public_Var.orderNo : QDYLLib.QDYL.InData.strTrace) + "</PosserialNo>";//POS流水号
		//OutXML += "<PosbatchNo>" + (CXType == 0 ? "" : ZZJ_Module.UnionPay.RETDATA.strBatch) + "</PosbatchNo>";//
		//OutXML += "<TransType>" + JYLX + "</TransType>";//交易类型
		//OutXML += "<TerminalNo>" + (CXType == 0 ? "" : QDYLLib.QDYL.InData.strTId) + "</TerminalNo>";//终端号
		//OutXML += "<Amt>" + DC.ToString() + "</Amt>";  //金额
		//OutXML += "<UserId>" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "</UserId>";//操作员
		//OutXML += "</Request>";
		// */

		XmlMaker Table = NewTabel(ZZJCore.Public_Var.patientInfo.PatientID, ZZJCore.Public_Var.cardInfo.CardNo, int.Parse(ZZJCore.Public_Var.cardInfo.CardType));
		//Table.Add("Czlx", CXType);//充值方式,0现金,1银联
		//Table.Add("passwd", "");//密码
		//Table.Add("czje", DC.ToString());//充值金额
		//Table.Add("yhlx", "");//银行类型(0建行，1非建行)
		//Table.Add("yhkh", (CXType == 0 ? ZZJCore.Public_Var.cardInfo.CardNo : ZZJ_Module.UnionPay.UnionPayCardNo));//银行卡号
		//Table.Add("yhlsh", (CXType == 0 ? ZZJ_Module.Public_Var.sFlowId : ZZJ_Module.UnionPay.RETDATA.strRef));//银行流水号
		//Table.Add("klx", "");//卡类型(0不扣卡押金，1可能扣卡押金)
		//Table.Add("bz", "");//备注

		Table.Add("czlx", CXType);//充值方式,0现金,1银联
		Table.Add("passwd", "");//密码
		Table.Add("czje", DC.ToString());//充值金额
		Table.Add("yhlx", "");//银行类型(0建行，1非建行)
		
		Table.Add("yhkh",CXType==1? ZZJ_Module.UnionPay.RETDATA.strCardNo:"");//银行卡号  ""
		Table.Add("yhlsh",CXType==1? ZZJ_Module.UnionPay.RETDATA.strTrace:"");//银行流水号   ""
		Table.Add("klx", "");//卡类型(0不扣卡押金，1可能扣卡押金)
		Table.Add("bz", "");//备注
		Table.Add("zffs", zffsFlag);//支付方式
		

		try
		{
			YCMS = "充值成功";
			XmlDocument XD = InvokeHIS("充值接口", "501", Table);
			if (XD.SelectSingleNode("zzxt/result/retcode").InnerText.Trim() != "0")
			{
				try
				{
					YCMS = XD.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
				}
				catch (Exception e)
				{
					ZZJCore.SuanFa.Proc.Log(e);
				}
				ZZJCore.SuanFa.Proc.Log("充值失败," + YCMS);
				InvokeRet = 1;
				//if (ZZJCore.Public_Var.cardInfo.CardType == "1") return 0;
				return -1;
			}
		}
		catch (Exception Ex)
		{
			ZZJCore.SuanFa.Proc.Log(Ex);
			YCMS = "充值失败!" + Ex.Message;
			ZZJCore.SuanFa.Proc.Log(YCMS);
			InvokeRet = 1;
			//if (ZZJCore.Public_Var.cardInfo.CardType == "1") return 0;
			return -2;
		}
		ZZJCore.SuanFa.Proc.Log("充值成功!" + YCMS);
		return 0;
	}//End Proc

	private static XmlMaker NewTabel(string Patid = "", string CardNo = "", int CardType = -1)
	{
        //院内卡 为3
		if (CardType == 0) CardType = 3;
		//港内卡为1
		if (CardType == 1) CardType = 1;
        //if (CardType == 1) CardType = 3;
		XmlMaker Table = new XmlMaker(false);
		Table.Add("czyh", ZZJCore.Public_Var.ZZJ_Config.ExtUserID);//His操作员号   ZZJCore.Public_Var.ZZJ_Config.ExtUserID

		if (!string.IsNullOrEmpty(CardNo)) Table.Add("cardno", CardNo);//卡号
		if (CardType >= 0) Table.Add("cardtype", CardType);//卡类型
		if (!string.IsNullOrEmpty(Patid)) Table.Add("patid", Patid);//账户号
		return Table;
	}

	/// <summary>
	/// 取XML数据,如果出错则返回空
	/// </summary>
	/// <param name="XD"></param>
	/// <param name="NodeName"></param>
	/// <returns></returns>
	public static string TrySelectSingleNode(this XmlDocument XD, string NodeName)
	{
		try
		{
			XmlNode XN = XD.SelectSingleNode(NodeName);
			if (XN == null) return "";
			return XN.InnerText.Trim();
		}
		catch
		{
			return "";
		}
	}

	public static XmlDocument InvokeHIS(string ProcName, string transcode, XmlMaker XM)
	{
		XmlMaker XML = new XmlMaker();
		XML.Add("transcode", transcode);//交易号
		if (XM != null) XML.AddNode("table", XM.ToString());
		string OutXML = XML.ToString();
		string InXML = "";
		ZZJCore.SuanFa.Proc.BW(OutXML, ProcName); //保存报文
		try
		{
			List<Task> listTask = new List<Task>();
			string retInXML = "";
			Task T = Task.Factory.StartNew(() =>
			{
				retInXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new string[] { OutXML }) as string; 
			});
			listTask.Add(T);
			Application.DoEvents();
			//线程等待15秒 如果没结果 返回空
			Task.WaitAll(listTask.ToArray(),5000);
			
			InXML =retInXML;//ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new string[] { OutXML }) as string;
		}
		catch (Exception E)
		{
			ZZJCore.SuanFa.Proc.BW("调用接口时抛异常了!" + E.Message, ProcName); //保存报文
			return null;
		}
		ZZJCore.SuanFa.Proc.BW(InXML, ProcName); //保存报文
		XmlDocument XD = new XmlDocument();
		try
		{
			XD.LoadXml(InXML);
		}
		catch
		{
			ZZJCore.SuanFa.Proc.Log("装载报文时出现异常!");
			return null;
		}
		return XD;
	}

}//End Class