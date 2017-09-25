using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using Mw_MSSQL;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

public static class Login
{
	public static bool LoginMain()
	{
		string Msg = "";
		ZZJCore.Public_Var.ModuleName = "登录";
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.Public_Var.patientInfo.PatientName = "";
		ZZJCore.Public_Var.patientInfo.DepositAmount = "";
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		ZZJCore.Initial.Read();
		ZZJ_Module.Public_Var.RCR.LoginFlag = false;
		ZZJCore.SuanFa.Proc.Log("打开登录");
		//读取患者信息
        ZZJCore.SuanFa.Proc.Log("RCR.KPLX:" + ZZJ_Module.Public_Var.RCR.KPLX);
        if (ZZJ_Module.Public_Var.RCR.KPLX == 2)
        {
            ZZJCore.SuanFa.Proc.Log("检测到医保卡数据了");
            QDYBCard.Mode = 1;
						//调用医保接口 :CX001 通过卡号查获取病人信息
						ZZJCore.SuanFa.Proc.Log("卡号:"+ZZJCore.Public_Var.cardInfo.CardNo+" 持卡人姓名:"+ZZJCore.Public_Var.patientInfo.PatientID+" "+ZZJCore.Public_Var.patientInfo.PatientName);
            
						if (QDYBCard.YBKHGetHZInfo("") != 0)//""
            {
                ZZJCore.SuanFa.Proc.Log("在医保中心没有查询到该社保卡数据！");
								ZZJ_Module.Public_Var.RCR.LoginFlag = false;
			          ZZJ_Module.Public_Var.RCR.LoginErrorFlag = -6;
								return true;

            }
            else
            {
                ZZJCore.SuanFa.Proc.Log("在医保中心查询社保卡数据成功！");
            }
        }
				//银联卡
        if (ZZJ_Module.Public_Var.RCR.KPLX == 4)
        {
            ZZJCore.SuanFa.Proc.Log("检测到银联卡，卡号:"+ZZJ_Module.Public_Var.RCR.KH+"CardNo:"+ZZJCore.Public_Var.cardInfo.CardNo);
         ////调用银联接口获取信息==关键参数：姓名，电话，身份证

        }
				////去his查询患者信息
			
				if (XMLCore.GetUserInfo(out Msg) != 0)//
				{
					if (Msg.Length > 0) ZZJCore.SuanFa.Proc.Log(Msg);
					ZZJCore.BackForm.CloseForm();
					return true;
				}

		//如果在获取卡号的过程中已经进行了密码验证,则此处不再验证
		if (!ZZJ_Module.Public_Var.RCR.LoginFlag)
		{
			//20160402修改:不再验证密码
			int TestiRet = 0;//Test(out Msg);
			ZZJ_Module.Public_Var.RCR.LoginFlag = TestiRet == 0;
			ZZJ_Module.Public_Var.RCR.LoginErrorFlag = TestiRet;
			if (TestiRet != 0)
			{
				if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true);
				ZZJCore.BackForm.CloseForm();
				return true;
			}

			//如果不是院内卡,直接作为登录成功处理
			if (ZZJ_Module.Public_Var.RCR.KPLX != 0)
			{
				ZZJCore.BackForm.CloseForm();
				return true;
			}

			#region 换卡标志判定
			bool HKBZ = ZZJ_Module.Public_Proc.ReadZZJConfig("HKBZ") == "1";//如果换卡标志没开
			HKBZ &= !string.IsNullOrEmpty(ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev);//如果没有发卡器
			//HKBZ &= !string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.IDNo);//如果信息不全

			#region 询问用户
			if (HKBZ)
			{
				int CreateNewCard = FormStyle.LoginStyle.HKYesNo();
				if (CreateNewCard == -3) CreateNewCard = 0;
				HKBZ &= CreateNewCard == 0;
			}
			#endregion
			#endregion

			#region 如果还支持院内卡,并且不换卡,则返回
			int iRet = 0;
			int JZK_YNK = int.Parse(ZZJCore.SuanFa.Proc.ReadIniData("Base_Cfg", "JZK_YNK", "0", @"D:\ZZJ\Hospital\Config\BaseConfig.ini", ref iRet));
			if (JZK_YNK == 1 && !HKBZ)
			{
				ZZJCore.BackForm.CloseForm();
				return true;
			}// */
			#endregion

			ZZJ_Module.Public_Var.RCR.LoginFlag = false;
			ZZJ_Module.Public_Var.RCR.LoginErrorFlag = -6;
			if (!HKBZ)
			{
				ZZJCore.SuanFa.Proc.ZZJMessageBox("换卡条件:1:换卡标志为1   2:机器有发卡器   3:患者同意换卡 其中之一不满足,无法换卡", "您的卡已无效,请换新卡片", true);
				ZZJCore.BackForm.CloseForm();
				return true;
			}

			//ZZJCore.SuanFa.Proc.ZZJMessageBox("该卡为院内卡,开始换卡", "您的卡已无效,将为您换卡", true);
			ZZJCore.BackForm.ShowForm("正在为您换卡,请稍候...");
			Application.DoEvents();
			QYKBK.ChangeCard();
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		ZZJCore.BackForm.CloseForm();
		return true;
	}

	/// <summary>
	/// 验证密码,对于已经签约过的医保卡,或者有效就诊卡,在查出病历号后,就调用这个函数,进行验证密码
	/// </summary>
	/// <param name="Msg"></param>
	/// <returns></returns>
	public static int Test(out string Msg)
	{
		int Err = 0;
		string OutXML = "";
		string InXML = "";
	inputPW:
		#region 输入密码
		ZZJCore.InputForm.InputPasswordParameter IP = new ZZJCore.InputForm.InputPasswordParameter();
		IP.Caption = "请输入就诊卡密码";
		IP.XButton = false;
		IP.RegMode = false;
		int iRet = ZZJCore.InputForm.InputPassword(IP);
		Application.DoEvents();
		if (iRet != 0)
		{
			Msg = "";
			return (Err > 0) ? -2 : -1;
		}
		#endregion

		#region 整理报文并查询
		if (ZZJCore.Public_Var.debug && ZZJCore.Public_Var.patientInfo.Password == "805612") goto OK;
		try
		{
			OutXML = "<Request>";
			OutXML += "<CardNo>" + ZZJCore.Public_Var.cardInfo.CardNo + "</CardNo>";//卡号
			OutXML += "<SecrityNo>666</SecrityNo><CardSerNo></CardSerNo>";
			OutXML += "<UserId>" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "</UserId>";//HIS账户
			OutXML += "<PassWord>" + ZZJCore.SuanFa.Proc.MD5(ZZJCore.Public_Var.patientInfo.Password) + "</PassWord>";
			OutXML += "</Request>";
			XmlDocument XD = new XmlDocument();
			ZZJCore.SuanFa.Proc.BW(OutXML, "登录");
			InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "GetPassWord", new object[] { OutXML }) as string;
			ZZJCore.SuanFa.Proc.BW(InXML, "登录");
			XD.LoadXml(InXML);

			//if (ZZJCore.Public_Var.debug) goto OK;
			if (XD.SelectSingleNode("Response/ResultCode").InnerText.Trim() != "0")
			{
				ZZJCore.ShowMessage.ShowForm(null, "密码错误!", true);
				if (Err == 2)
				{
					Msg = "";
					return -3;
				}
				Err++;
				goto inputPW;
			}
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			Msg = "网络错误,请稍后再试!";
			return -5;
		}
		#endregion
	OK:
		Msg = "";
		return 0;
	}

	#region 保存数据表中
	static int QYBZ = 0;
	static string QYMsg = "";
	public static void SavelocalAndServerData(int JGBZ, string MSG)
	{
		QYBZ = JGBZ;
		QYMsg = MSG;
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
			AL.Add(AddSP(SqlDbType.VarChar, "@XM", ZZJCore.Public_Var.patientInfo.PatientName));//姓名
			AL.Add(AddSP(SqlDbType.VarChar, "@XB", ZZJCore.Public_Var.patientInfo.Sex));//性别
			AL.Add(AddSP(SqlDbType.VarChar, "@SJHM", ZZJCore.Public_Var.patientInfo.Mobile));//手机号码
			AL.Add(AddSP(SqlDbType.VarChar, "@ZJHM", ZZJCore.Public_Var.patientInfo.IDNo));//证件号
			AL.Add(AddSP(SqlDbType.VarChar, "@ZJLX", "身份证"));//证件类型
			AL.Add(AddSP(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.patientInfo.PatientID));//登记号
			AL.Add(AddSP(SqlDbType.VarChar, "@YBKH", ""));//卡号
			AL.Add(AddSP(SqlDbType.VarChar, "@YBLX", "青岛医保"));//医保类型
			AL.Add(AddSP(SqlDbType.VarChar, "@GRBH", ZZJCore.Public_Var.cardInfo.CardNo));//个人编号
			AL.Add(AddSP(SqlDbType.Int, "@BZ", QYBZ));//标志
			AL.Add(AddSP(SqlDbType.VarChar, "@HISMS", QYMsg));//描述
			AL.Add(AddSP(SqlDbType.Int, "@QYBZ", 0));//区域标志
			AL.Add(AddSP(SqlDbType.VarChar, "@QYMS", ""));//区域描述
			AL.Add(AddSP(SqlDbType.DateTime, "@SJ", DateTime.Now));//时间
			AL.Add(AddSP(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));//自助机编号
			AL.Add(AddSP(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//HIS操作员
			AL.Add(AddSP(SqlDbType.VarChar, "@BL1", ""));
			AL.Add(AddSP(SqlDbType.VarChar, "@ZZJLSH", ZZJCore.Public_Var.SerialNumber));//流水号
			if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_YBQY_MX_V2]", AL.ToArray(), out msg) > 0)
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
}//End class