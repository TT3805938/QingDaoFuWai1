using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;


namespace FormStyle
{
	public static class BKStyle
	{
		#region 儿童办卡
		public static Image[] SexSelect = new Image[] { 
			ZZJCore.SuanFa.Proc.ResetImageSize(Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/男孩.png"),300,280),
			ZZJCore.SuanFa.Proc.ResetImageSize(Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/女孩.png"), 300, 280),
		};

		public static int GetIDNo001()
		{

			ZZJCore.GetSFZ.GetSFZParameter GP = new ZZJCore.GetSFZ.GetSFZParameter();
			GP.CaptionText = "请刷或手工输入监护人身份证";
			GP.CaptionText2 = "请输入监护人身份证";
			GP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
			int iRet = ZZJCore.GetSFZ.ShowForm(GP);
			Application.DoEvents();
			return iRet;
		}

		public static int GetPhone001()
		{
			ZZJCore.InputForm.InputPhoneParameter IP = new ZZJCore.InputForm.InputPhoneParameter();
			IP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
			int iRet = ZZJCore.InputForm.InputPhone(IP);
			Application.DoEvents();
			return iRet;
		}

		public static int GetGuanXi()
		{
			ZZJCore.SelectForm2.SelectFormParameter SFP = new ZZJCore.SelectForm2.SelectFormParameter();
			SFP.ButtonTexts = new string[] { "父亲", "母亲", "爷爷", "奶奶", "其他" };
			SFP.Caption = "请选择关系";
			SFP.ContentAlignment = ContentAlignment.MiddleCenter;
			SFP.VDistance = 10;//垂直间距
			SFP.HDistance = 10;//水平间距
			SFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
			int iRet = ZZJCore.SelectForm2.ShowForm(SFP);
			Application.DoEvents();
			return iRet;
		}

		public static int SelectSex()
		{
			//iRet = ZZJCore.Select2in1.ShowForm("请选择小孩性别", BK.SexSelect[0], BK.SexSelect[1]);
			//if (iRet > 0) iRet--;
			ZZJCore.SelectForm2.SelectFormParameter SFP = new ZZJCore.SelectForm2.SelectFormParameter();
			ZZJCore.ZZJStruct.FontText FT = new ZZJCore.ZZJStruct.FontText();
			SFP.Images = SexSelect;
			SFP.Texts = new ZZJCore.ZZJStruct.FontText[] { FT };
			SFP.ContentAlignment = ContentAlignment.MiddleCenter;
			SFP.VDistance = 10;//垂直间距
			SFP.HDistance = 100;//水平间距
			SFP.Voice = FT.Text;
			SFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
			unchecked
			{
				FT.FontColor = Color.FromArgb((int)0xFFFF7000);
			}
			SFP.Caption = "";
			FT.Text = "请选择小孩性别";
			SFP.Voice = FT.Text;
			FT.X = 450;
			FT.Y = 100;
			FT.W = 400;
			int iRet = ZZJCore.SelectForm2.ShowForm(SFP);
			Application.DoEvents();
			return iRet;
		}

		public static int GetDOB001(ref DateTime DOB)
		{
			ZZJCore.DateSelect2.DateSelectParameter DSP = new ZZJCore.DateSelect2.DateSelectParameter();
			DSP.CaptionString = "请输入出生日期:";
			DSP.defDT = DOB;
			DSP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret, ZZJCore.ZZJControl.Button_OK };
			DSP.StartDT = DateTime.Now.AddYears(-50);
			DSP.EndDT = DateTime.Now;
			int iRet = ZZJCore.DateSelect2.ShowForm(DSP, ref DOB);
			Application.DoEvents();
			return iRet;
		}

		public static int GetPassword001()
		{
			ZZJCore.InputForm.InputPasswordParameter IPP = new ZZJCore.InputForm.InputPasswordParameter();
			IPP.RegMode = true;
			IPP.Caption = "请设置6位卡密码";
			IPP.XButton = false;
			IPP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
			int iRet = ZZJCore.InputForm.InputPassword(IPP);
			Application.DoEvents();
			return iRet;
		}
		#endregion 儿童办卡

		#region 自助办卡
		public static int SelectBKMode()
		{
			ZZJCore.SelectForm2.SelectFormParameter SFP = new ZZJCore.SelectForm2.SelectFormParameter();
			SFP.Caption = "请选择办卡方式";
			SFP.ButtonTexts = new string[] { "使用身份证", "使用社保卡" };
			SFP.HDistance = 100;
			SFP.TextButtonWidth = 300;
			SFP.TextButtonHeight = 200;
			SFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
			int iRet = ZZJCore.SelectForm2.ShowForm(SFP);
			Application.DoEvents();
			return iRet;
		}

		public static int BKYesNo()
		{
			ZZJCore.YesNoForm.YesNoFormParameter YNFPM = new ZZJCore.YesNoForm.YesNoFormParameter();
			YNFPM.Caption = "您已经办过卡,是否补卡?";
			#region 添加按钮
			ZZJCore.ZZJStruct.ZZJButton Button_F = ZZJCore.ZZJControl.NewButton_Ret();
			Button_F.Text = "否";
			Button_F.RetData = -5;
			Button_F.Rect.X = 800;
			ZZJCore.ZZJStruct.ZZJButton Button_S = ZZJCore.ZZJControl.NewButton_OK();
			Button_S.Text = "是";
			YNFPM.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, Button_S, Button_F };
			#endregion
			int iRet = ZZJCore.YesNoForm.ShowForm(YNFPM);
			Application.DoEvents();
			return iRet;
		}

		public static int GetIDNo002()
		{
			ZZJCore.GetSFZ.GetSFZParameter GP = new ZZJCore.GetSFZ.GetSFZParameter();
			GP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
        //    GP.NoInput = true;
            int iRet = ZZJCore.GetSFZ.ShowForm(GP);
			Application.DoEvents();
			return iRet;
		}

		public static int GetPassword002()
		{//这个界面和儿童办卡一样,所以直接套用
			return GetPassword001();
		}

		public static int GetPhone002()
		{
			return GetPhone001();
		}

		public static int SelectProc(Image[] IL)
		{
			ZZJCore.SelectForm2.SelectFormParameter SFP = new ZZJCore.SelectForm2.SelectFormParameter();
			SFP.Images = IL;
			SFP.Caption = "办卡成功,您还可以选择以下功能:";
			SFP.ContentAlignment = ContentAlignment.MiddleCenter;
			SFP.VDistance = 10;//垂直间距
			SFP.HDistance = 10;//水平间距
			ZZJCore.ZZJStruct.ZZJButton zzjbutt = ZZJCore.ZZJControl.NewButton_Close();
			zzjbutt.Text = "出卡";
			SFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { zzjbutt };
			SFP.DJSTime = 15;//倒计时时间
			int iRet = ZZJCore.SelectForm2.ShowForm(SFP);
			Application.DoEvents();
			return iRet;
		}
		#endregion

		#region 换卡
		public static int SelectIDNo(bool NeedIDNo)
		{
			ZZJCore.SelectForm2.SelectFormParameter SFP = new ZZJCore.SelectForm2.SelectFormParameter();
			ZZJCore.ZZJStruct.FontText FT = new ZZJCore.ZZJStruct.FontText();
			SFP.ButtonTexts = new string[] { "本人身份证", "监护人身份证" };
			SFP.Texts = new ZZJCore.ZZJStruct.FontText[] { FT };
			SFP.ContentAlignment = ContentAlignment.MiddleCenter;
			SFP.VDistance = 10;//垂直间距
			SFP.HDistance = 300;//水平间距
			SFP.Voice = FT.Text;
			SFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
			unchecked
			{
				FT.FontColor = Color.FromArgb((int)0xFFFF7000);
			}
			if (NeedIDNo)
			{
				SFP.Caption = "";
				FT.Text = "　　这张就诊卡无身份证信息,需要提供,请选择身份证来源";
				FT.X = 250;
				FT.Y = 100;
				FT.W = 800;
				FT.H = 180;
				SFP.Voice = FT.Text;
			}
			else
			{
				SFP.Caption = "以下身份证号是您办这张卡时所填写,请选择身份证来源";
				FT.Text = "身份证号:" + ZZJCore.SuanFa.Proc.GetMaskIDNo();
				FT.X = 300;
				FT.Y = 180;
				FT.W = 800;
			}

			int iRet = ZZJCore.SelectForm2.ShowForm(SFP);
			Application.DoEvents();
			return iRet;
		}

		public static int GetIDNo003(int CreateCardMode)
		{
			//如果患者信息里身份证号为空,则请TA输入
			ZZJCore.GetSFZ.GetSFZParameter GSP = new ZZJCore.GetSFZ.GetSFZParameter();
			GSP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
			GSP.NeedName = false;
			if (CreateCardMode == 1)
			{
				GSP.CaptionText = "请刷或手工输入监护人身份证";
				GSP.CaptionText2 = "请输入监护人身份证号码";
			}
			ZZJCore.PatientInfo pi = ZZJCore.Public_Var.patientInfo;
			int iRet = ZZJCore.GetSFZ.ShowForm(GSP);
			pi.IDNo = ZZJCore.Public_Var.patientInfo.IDNo;
			ZZJCore.Public_Var.patientInfo = pi;
			Application.DoEvents();
			return iRet;
		}

		public static int GetPhone003(bool NeedIDNo)
		{
			//如果患者信息里手机号为空,则请TA输入
			ZZJCore.InputForm.InputPhoneParameter IP = new ZZJCore.InputForm.InputPhoneParameter();
			if (!NeedIDNo) IP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
			if (NeedIDNo) IP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
			int iRet = ZZJCore.InputForm.InputPhone(IP);
			Application.DoEvents();
			return iRet;
		}

		public static int GetDOB002(ref DateTime DOB)
		{
			ZZJCore.DateSelect2.DateSelectParameter DSP = new ZZJCore.DateSelect2.DateSelectParameter();
			DSP.CaptionString = "请输入小孩出生日期:";
			DSP.defDT = DOB;
			DSP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_OK };
			DSP.StartDT = DateTime.Now.AddYears(-120);
			DSP.EndDT = DateTime.Now;
			int iRet = ZZJCore.DateSelect2.ShowForm(DSP, ref DOB);
			Application.DoEvents();
			return iRet;
		}

		#endregion
	}//End Class
}