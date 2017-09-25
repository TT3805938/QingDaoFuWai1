using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using Mw_Public;
using Mw_Voice;
using ZZJCore;

public partial class ShowData : CoreTYForm.TYForm
{
	public static int ShowForm()
	{
		ShowData SD = new ShowData();
		SD.ShowDialog2();
		int FormRet = SD.iRet;
		SD.Dispose();
		return FormRet;
	}

	public ShowData()	: base(new CoreTYForm.TYForm.TYParameter())
	{
		InitializeComponent();
	}

	private void ShowData_Load(object sender, EventArgs e)
	{
		DJSTime = 60;
		ZZJCore.FormSkin.SetImage(Button_Close, ZZJCore.FormSkin.TYButtonImage2, ZZJCore.FormSkin.TY2ButtonTextColor);
		Namelab.Text = ZZJCore.Public_Var.patientInfo.PatientName;
		Cardlab.Text = ZZJCore.Public_Var.cardInfo.CardNo;
		yelab.Text = Convert.ToDecimal(ZZJCore.Public_Var.patientInfo.DepositAmount).ToString("C");
	}

	private void Button_Close_Click(object sender, EventArgs e)
	{
		ZZJCore.SuanFa.Proc.MsgSend(0xB9, 0);
		iRet = -2;
		Talker.Stop();
		this.Close();
	}

}//End Close