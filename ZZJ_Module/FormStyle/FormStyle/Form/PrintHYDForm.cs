using System;//EventArgs(窗体事件参数)
using System.Windows.Forms;//Form
using System.Drawing;//Image
using System.Threading;//多线程
using System.Xml;//解析XML
using System.Collections;//ArrayList
//using Mw_Public;//汇利斯通:一些公共变量
using Mw_Voice;//汇利斯通:TTS
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
//using Mw_MSSQL;
using System.Collections.Generic;

public partial class PrintHYDForm : CoreTYForm.TYForm
{
	int DJS = 0;

	public PrintHYDForm(int printCnt): base(new CoreTYForm.TYForm.TYParameter())
	{
		InitializeComponent();
		DJS = 7 + (printCnt * 7);
		TYP.DJSTime=DJS;
		Caption.Text = "正在打印,共 " + printCnt + " 张";
		DJSTime=TYP.DJSTime;
	}


	private void PrintHYDForm_Load(object sender, EventArgs e)
	{
		this.Width = ZZJCore.FormSkin.Form1Rect.Width;
		this.Height = ZZJCore.FormSkin.Form1Rect.Height;
		this.Top = ZZJCore.FormSkin.Form1Rect.Top;
		this.Left = ZZJCore.FormSkin.Form1Rect.Left;
		this.BackgroundImage = ZZJCore.FormSkin.BGImage;
		ZZJCore.FormSkin.SetImage(Button_Close, ZZJCore.FormSkin.TYButtonImage2, ZZJCore.FormSkin.TY2ButtonTextColor);

		//this.Button_Close.Image = ZZJCore.FormSkin.TYButtonImage2;
		Talker.Speak(Caption.Text);
		progressBar1.Maximum = DJS * 10;
		progressBar1.Value = 0;
	}

	private void timer1_Tick(object sender, EventArgs e)
	{
		progressBar1.Value++;
		if (progressBar1.Value == (int)(progressBar1.Maximum / 2)) Talker.Speak(Caption.Text);
		if (progressBar1.Value != progressBar1.Maximum) return;
		timer1.Enabled = false;
		iRet = 0;
		Talker.Stop();
		this.Close();
	}

	private void Button_Close_Click(object sender, EventArgs e)
	{
		iRet = -2;
		Talker.Stop();
		this.Close();
	}

	private void RetCard_Click(object sender, EventArgs e)
	{
		timer1.Stop();
		timer1.Dispose();
		iRet = -2;
		Talker.Stop();
		this.Close();
	}

	private void groupBox1_Enter(object sender, EventArgs e)
	{

	}

	private void Caption_Click(object sender, EventArgs e)
	{

	}//End Proc
}//End Class
