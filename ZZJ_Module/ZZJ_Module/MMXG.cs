using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using ZZJCore;
using ZZJCore.ZZJStruct;
public static class MMXG
{
	public static bool MMXGMain()
	{
		string Msg = "";
		#region 初始化
		ZZJCore.FormSkin.UseRetCard = true;
		ZZJCore.Public_Var.ModuleName = "密码修改";
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.Initial.Read();
		if (XMLCore.GetUserInfo(out Msg) != 0)
		{
			if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true);
			ZZJCore.BackForm.CloseForm();
			return false;
		}
		#endregion

	InputOldPW:

		//goto PassWordOK;
		ZZJCore.InputForm.InputPasswordParameter OIPP = new ZZJCore.InputForm.InputPasswordParameter();
		OIPP.Caption = "请输入6位旧密码";
		OIPP.XButton = false;
		OIPP.RegMode = false;
		OIPP.Buttons = new ZZJButton[] { ZZJControl.Button_Close };

		if (ZZJCore.InputForm.InputPassword(OIPP) != 0)
		{
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		Application.DoEvents();
		ZZJCore.Public_Var.patientInfo.Password = ZZJCore.SuanFa.Proc.MD5(ZZJCore.Public_Var.patientInfo.Password);
		#region 整理报文并查询
		try
		{
			string OutXML = "<Request>";
			OutXML += "<CardNo>" + ZZJCore.Public_Var.cardInfo.CardNo + "</CardNo>";//卡号
			OutXML += "<SecrityNo>666</SecrityNo><CardSerNo></CardSerNo>";//"+(ZZJCore.Public_Var.cardInfo.CardType=="0"?"666":"")+"
			OutXML += "<UserId>" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "</UserId>";//HIS账户
			OutXML += "<PassWord>" + ZZJCore.Public_Var.patientInfo.Password + "</PassWord>";
			OutXML += "</Request>";
			XmlDocument XD = new XmlDocument();
			XD.LoadXml(ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "GetPassWord", new object[] { OutXML }) as string);
			if (XD.SelectSingleNode("Response/ResultCode").InnerText.Trim() != "0")
			{
				ZZJCore.BackForm.ShowForm("密码错误!", true);
				return true;
			}
		}
		catch(Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.BackForm.ShowForm("调用接口失败,请稍后再试!", true);
			return true;
		}
		#endregion

		//PassWordOK:
		ZZJCore.InputForm.InputPasswordParameter IPP = new ZZJCore.InputForm.InputPasswordParameter();
		IPP.Caption = "请设置6位新密码";
		IPP.XButton = false;
		IPP.RegMode = true;
		IPP.Buttons = new ZZJButton[] { ZZJControl.Button_Close, ZZJControl.Button_Ret };
		int iRet = ZZJCore.InputForm.InputPassword(IPP);
		Application.DoEvents();
		if (iRet == -1) goto InputOldPW;
		if (iRet == -2 || iRet == -3)
		{
			ZZJCore.BackForm.CloseForm();
			return true;
		}

		try
		{
			XmlDocument XD = new XmlDocument();
			ZZJCore.Public_Var.patientInfo.Password = ZZJCore.SuanFa.Proc.MD5(ZZJCore.Public_Var.patientInfo.Password);
			string OutXML = "<Request>";
			OutXML += "<CardNo>" + ZZJCore.Public_Var.cardInfo.CardNo + "</CardNo>";
			OutXML += "<PassWord>" + ZZJCore.Public_Var.patientInfo.Password + "</PassWord>";
			OutXML += "</Request>";
			XD.LoadXml(ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "UpdatePassWord", new object[] { OutXML }) as string);
			if (XD.SelectSingleNode("Response/ResultCode").InnerText.Trim() != "0")
			{
				ZZJCore.BackForm.ShowForm("修改密码失败,请稍后再试!", true);
				return true;
			}
		}
		catch(Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.BackForm.ShowForm("调用接口失败,请稍后再试!", true);
			return false;
		}
		ZZJCore.BackForm.ShowForm("修改密码成功!", true);
		return true;

	}
}//End Class