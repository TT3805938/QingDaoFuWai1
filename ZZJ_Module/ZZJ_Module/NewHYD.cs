using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;//Image
using System.Runtime.InteropServices;
using System.Data;
using System.Data.SqlClient;
using Mw_MSSQL;
using System.Collections;
using System.Threading;//多线程
using System.Diagnostics;
using System.Threading.Tasks;
using ZZJCore.ZZJStruct;
using ZZJCore;

public static class NewHYD
{
	[DllImport("winspool.drv")]
	public static extern bool SetDefaultPrinter(String Name); //调用win api将指定名称的打印机设置为默认打印机
	public static bool HYDMain()
	{
		#region 初始化
		//ZZJCore.Public_Var.patientInfo.Password="";
		ZZJCore.FormSkin.UseRetCard = true;
		ZZJCore.Public_Var.ModuleName = "报告单打印";
		ZZJCore.BackForm.ShowForm("正在查询,请稍候...", false);
		Application.DoEvents();

		ZZJCore.Initial.Read();
		ZZJCore.SuanFa.Proc.Log("加载配置成功!");
		string Msg = "";

		//XMLCore.GetUserInfo(out Msg);
		#endregion

		#region 查询化验单
		//TableData[] TDA = null;
		//try
		//{
		//  TDA = GetData();
		//  ZZJCore.SuanFa.Proc.Log("调用存储过程成功!");
		//}
		//catch (Exception e)
		//{
		//  ZZJCore.SuanFa.Proc.Log(e);
		//  ZZJCore.SuanFa.Proc.Log("调用存储过程失败!" + e.Message + Environment.NewLine + e.StackTrace);
		//  ZZJCore.BackForm.ShowForm("查询不到可打印的报告单!", true);
		//  ZZJCore.BackForm.CloseForm();
		//  return true;
		//}
		#endregion
		#region

		SetDefaultPrinter("Brother HL-5450DN series");
		string HYDPath = "E:/LIS/LIS5.0/UnionReport.exe";


		int ScanWDRet = ScanWD();
		ZZJCore.SuanFa.Proc.Log("首页的返回值:"+ScanWDRet.ToString());
		if(ScanWDRet==-2)
		{
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		if (ScanWDRet != 0)
		{
			ZZJCore.ShowMessage ErrM = new ZZJCore.ShowMessage("扫描错误，请稍后再试!", true, true, 2);
			ErrM.ShowDialog(ZZJCore.BackForm.BF);
			ErrM.Dispose();
			ZZJCore.BackForm.CloseForm();
		}
		else
		{


		System.Diagnostics.Process process = new System.Diagnostics.Process();
		process.StartInfo.FileName = "" + HYDPath + "";
		//  process.StartInfo.Arguments = " " + personinfo.PatientName + " " + Public_Var.ShortCardNo + " " + personinfo.ImgPath + " ";//姓名 卡号 照片地址
		//process.StartInfo.Arguments = "" + ZZJCore.Public_Var.cardInfo.CardNo + "";// 卡号 
		process.StartInfo.Arguments = "" + ZZJCore.Public_Var.patientInfo.HospitalIzationID + "";// 输入的条形码序列号


		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardInput = true;
		Application.DoEvents();
		//    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		process.Start();
		ZZJCore.SuanFa.Proc.Log("66666666666666666666666666666666=:::" + ZZJCore.Public_Var.patientInfo.HospitalIzationID);
		}





		#region  //输入方式打印化验单
		//SetDefaultPrinter("Brother HL-5450DN series");
		//string HYDPath = "E:/LIS/LIS5.0/UnionReport.exe";
		//ZZJCore.InputForm.InputPasswordParameter OIPP = new ZZJCore.InputForm.InputPasswordParameter();
		////OIPP.Caption = "请输入条形码序列号";
		//OIPP.LabelText = "请输入条形码序列号";
		//OIPP.XButton = false;
		//OIPP.RegMode = false;
		//OIPP.MAX=20;
		//OIPP.Buttons = new ZZJButton[] { ZZJControl.Button_Close };
		//if (ZZJCore.InputForm.InputPassword(OIPP) != 0)
		//{
		//  ZZJCore.BackForm.CloseForm();
		//  return true;
		//}
		//Application.DoEvents();

		
		
		//System.Diagnostics.Process process = new System.Diagnostics.Process();
		//process.StartInfo.FileName = "" + HYDPath + "";
		////  process.StartInfo.Arguments = " " + personinfo.PatientName + " " + Public_Var.ShortCardNo + " " + personinfo.ImgPath + " ";//姓名 卡号 照片地址
		////process.StartInfo.Arguments = "" + ZZJCore.Public_Var.cardInfo.CardNo + "";// 卡号 
		//process.StartInfo.Arguments = "" + ZZJCore.Public_Var.patientInfo.Password + "";// 输入的条形码序列号


		//process.StartInfo.UseShellExecute = false;
		//process.StartInfo.CreateNoWindow = true;
		//process.StartInfo.RedirectStandardOutput = true;
		//process.StartInfo.RedirectStandardInput = true;
		//Application.DoEvents();
		////    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		//process.Start();
		#endregion

		//ZZJCore.ShowMessage ErrM = new ZZJCore.ShowMessage("请稍等!", true, true, 2);
		//ErrM.ShowDialog(ZZJCore.BackForm.BF);
		//ErrM.Dispose();

	//	ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "请稍等!", true);
		ZZJCore.BackForm.CloseForm();
		return true;
		#endregion



	




		#region 判断是否为空
		//if (TDA == null)
		//{
		//  ZZJCore.SuanFa.Proc.Log("没有查到报告单!");
		//  ZZJCore.BackForm.ShowForm("您当前无可打印的报告单!", true);
		//  return true;
		//}
		//if (TDA.Length == 0)
		//{
		//  ZZJCore.SuanFa.Proc.Log("没有查到报告单!");
		//  ZZJCore.BackForm.ShowForm("您当前无可打印的报告单!", true);
		//  return true;
		//}
		#endregion

		//Thread T = new Thread(new ParameterizedThreadStart(Run));
		//T.Start(TDA);//开个线程去打印
		//ZZJCore.SuanFa.Proc.Log("建立打印线程成功!");

		//int printCnt = 0;//统计可打印化验单数量
		//ZZJCore.SuanFa.Proc.Log("统计可打印化验单数量");
		//foreach (TableData TD in TDA)
		//{
		//  if (TD.JYTime <= DateTime.Now) printCnt++;
		//}
		//if (printCnt > 0) ZZJCore.SuanFa.Proc.MsgSend(0xFFA4, printCnt, true);

		#region 显示打印界面
		//ZZJCore.SuanFa.Proc.Log("可打印化验单数量=" + printCnt.ToString());
		//if (printCnt > 0)
		//{
		//  ZZJCore.SuanFa.Proc.Log("初始化打印界面!");
		//  PrintHYDForm PF = new PrintHYDForm(printCnt);
		//  ZZJCore.SuanFa.Proc.Log("显示打印界面!");
		//  PF.ShowDialog(ZZJCore.BackForm.BF);
		//  ZZJCore.SuanFa.Proc.Log("释放打印界面!");
		//  PF.Dispose();
		//  Application.DoEvents();
		//  ZZJCore.SuanFa.Proc.Log("离开打印界面!");
		//}
		//T.DisableComObjectEagerCleanup();
		#endregion

		#region 显示未能打印的化验单
		//if (TDA.Length > printCnt)
		//{
		//  int NoPrintCount = TDA.Length - printCnt;
		//  ZZJCore.SuanFa.Proc.Log(string.Format("显示未能打印的化验单,共{0}张", NoPrintCount.ToString()));
		//  #region 填充数据
		//  string[] LB = new string[] { "单号", "化验项目", "状态" };
		//  byte[] Sizes = new byte[] { 40, 40, 20 };
		//  string[,] Data = new string[3, NoPrintCount];
		//  int i = 0;
		//  foreach (TableData TD in TDA)
		//  {
		//    if (TD.JYTime < DateTime.Now) continue;
		//    Data[0, i] = TD.ID.ToString();
		//    Data[1, i] = TD.HYXM;
		//    Data[2, i] = "正在检验";
		//    i++;
		//  }
		//  #endregion
		//  ZZJCore.SuanFa.Proc.Log("显示不可打印列表!");
		//  ZZJCore.DataGridViewForm2.ShowForm(new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close }, "", Sizes, LB, Data);
		//  Application.DoEvents();
		//  ZZJCore.BackForm.CloseForm();
		//  ZZJCore.SuanFa.Proc.Log("显示不可打印列表完成,关闭!");
		//  return true;
		//}
		#endregion 显示未能打印的化验单

		//if (printCnt > 0)
		//{
		//  ZZJCore.SuanFa.Proc.Log("显示打印完成");
		//  ZZJCore.BackForm.ShowForm("打印完成!", true);
		//  ZZJCore.SuanFa.Proc.MsgSend(0xC3, 0, true);
		//  Application.DoEvents();
		//  ZZJCore.SuanFa.Proc.Log("打印完成返回!");
		//  return true;
		//}

		//ZZJCore.BackForm.CloseForm();
		//ZZJCore.SuanFa.Proc.Log("所有信息完成关闭!");
		//return true;

	}//End Main

	public struct TableData
	{
		public decimal ID;
		public DateTime JYTime;
		public string HYXM;
		//public int PrintCount;
	}

	public static TableData[] GetData()
	{
		SqlConnection con = new SqlConnection("Password=ruimeilis6.0;Persist Security Info=True;User ID=sa; Initial Catalog=rmlis6;Data Source=192.168.100.12");
		SqlCommand cmd = new SqlCommand("sp_inter_getoutpreportlist", con);
		cmd.CommandType = CommandType.StoredProcedure;
		cmd.Parameters.Add("@cardno", SqlDbType.VarChar, 32).Value = ZZJCore.Public_Var.patientInfo.PatientID;
		//cmd.Parameters.Add("@cardno", SqlDbType.VarChar, 32).Value = "6101152788";
		con.Open();
		SqlDataReader sdr = cmd.ExecuteReader();
		cmd.Dispose();

		List<TableData> TDl = new List<TableData>();
		DateTime StartDT = DateTime.Now.AddDays(-int.Parse(ZZJCore.SuanFa.Proc.ReadPublicINI("HYDDAY", "15")));
		while (sdr.Read())
		{
			//sdr.GetString(10);//禁止打印标志 sdr.GetInt32(8);//总打印次数 sdr.GetDateTime(4)//检验时间
			if (sdr.GetString(10) == "1") continue;
			if (sdr[8] != System.DBNull.Value) if (sdr.GetInt32(8) != 0) continue;
			if (sdr[4] != System.DBNull.Value) if (sdr.GetDateTime(4) < StartDT) continue;

			TableData TD = new TableData();
			TD.ID = sdr.GetDecimal(0);//报告ID
			if (sdr[4] != System.DBNull.Value)
			{
				TD.JYTime = sdr.GetDateTime(4);//审核时间
			}
			else
			{
				TD.JYTime = DateTime.Now.AddDays(1);
			}
			TD.HYXM = sdr[12] != System.DBNull.Value ? sdr.GetString(12) : "";//化验项目
			//TD.PrintCount = sdr.GetInt32(8);//总打印次数

			//TD.JYTime = DateTime.Now.AddDays(1);

			/*
			sdr.GetString(1);//病历号
			sdr.GetString(2);//卡号
			sdr.GetString(3);//姓名(患者的?)
			sdr.GetDateTime(6);//最后一次打印的时间
			sdr.GetDateTime(7);//报告时间
			sdr.GetString(11);//禁止打印原因
			// */
			TDl.Add(TD);
		}
		con.Close();
		con.Dispose();
		sdr.Dispose();
		return TDl.ToArray();
	}

	public static void Run(object oTDA)
	{
		//return;
		//if (string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.PatientID)) return;
		SetDefaultPrinter(ZZJCore.Public_Var.ZZJ_Config.HYD_Dev);
		TableData[] TDA = oTDA as TableData[];
		try
		{
			int err = ZZJ_Module.Lisreportdll.f_lisinit("Provider=SQLOLEDB.1;Password=ruimeilis6.0;Persist Security Info=True;User ID=sa;Initial Catalog=rmlis6;Data Source=192.168.100.12",
			string.Empty, ZZJCore.Public_Var.ZZJ_Config.ExtUserID);
			if (err != 0) return;

			ZZJ_Module.Lisreportdll.pro_changereportstyle("2");
			ZZJCore.SuanFa.Proc.Log("进入打印报告单循环" + TDA.Length.ToString());
			foreach (TableData TD in TDA)
			{
				if (TD.JYTime > DateTime.Now) continue;
				ZZJCore.SuanFa.Proc.Log("打印报告单" + TD.ID.ToString());
				ZZJ_Module.Lisreportdll.f_rmlisreport_ext(TD.ID.ToString(), 1, "AUTOPRINT", "ZZJ");
				Thread.Sleep(2000);
			}
			ZZJ_Module.Lisreportdll.f_lisunint();
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("打印化验单异常!" + e.Message);
		}
	}//End Run


	/// <summary>
	/// 扫描腕带
	/// </summary>
	public static int ScanWD()
	{
		bool State = true;
		//	if (!string.IsNullOrEmpty(ZZJCore.Public_Var.cardInfo.CardNo)) return 0;

		ZZJCore.Public_Var.patientInfo.HospitalIzationID = "";
		QRCode.OpenCOM(ZZJCore.Public_Var.ZZJ_Config.QRCode_Dev, ZZJCore.Public_Var.ZZJ_Config.QRCode_Port,
			(MSG, sData, bData) =>
			{
				//if (sData.Length < 5)
				//{
				//  ZZJCore.Public_Var.patientInfo.HospitalIzationID = sData;
				//  State = false;
				//  return;
				//}
				//string ZYH = sData.Substring(3);
				//int offset = ZYH.IndexOf("|");
				//if (offset <= 0)
				//{
				//  ZZJCore.Public_Var.patientInfo.HospitalIzationID = sData;
				//  State = false;
				//  return;
				//}
				//ZYH = ZYH.Substring(0, offset);
				ZZJCore.Public_Var.patientInfo.HospitalIzationID = sData;
				State = false;
				return;
			}
		);
		//ZZJCore.ImageMessage IM = FormStyle.ZYModuleStyle.OpenScan();
		ZZJCore.ImageMessage  IM=NewHYD.OpenScan();
		while (State)
		{
			Application.DoEvents();
			if (IM.Visible) continue;
			break;
		}
		QRCode.CloseCOM();
		NewHYD.CloseScan(IM);
		if (!string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.HospitalIzationID)) return 0;
		return -2;
	}



	#region 二维码扫描画面
	private static Font TsFont = new Font(new FontFamily("黑体"), 42, FontStyle.Bold);//字体
	private static Image IMAGE = Image.FromFile("D:/ZZJ/Module/Image/bb.png");//txm.jpg
	//private static Image IMAGE = Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/TXM.png");
	public static ZZJCore.ImageMessage OpenScan()
	{
		//先显示个请插卡提示
		#region 图片
		List<ZZJCore.ZZJStruct.AImage> IL = new List<ZZJCore.ZZJStruct.AImage>();
		ZZJCore.ZZJStruct.AImage IA = new ZZJCore.ZZJStruct.AImage();
		IA.W = IMAGE.Size.Width;
		IA.H = IMAGE.Size.Height;
		IA.X = (1280 - IA.W) / 2;
		IA.Y = 162;
		IA.image = IMAGE;
		IL.Add(IA);
		#endregion
		#region 添加按钮
		List<ZZJCore.ZZJStruct.ZZJButton> BL = new List<ZZJCore.ZZJStruct.ZZJButton>();
		{
			ZZJCore.ZZJStruct.ZZJButton zzjbutt = new ZZJCore.ZZJStruct.ZZJButton();
			zzjbutt.Rect = new Rectangle(400, 100, 500, 64);//这是设定按钮的位置大小
			zzjbutt.BGIMAGE = null;//ZZJCore.FormSkin.TYButtonImage2;//这里设置按钮的背景图
			zzjbutt.Text = "请扫描条形码";//按钮上的字 
			zzjbutt.TextFont = TsFont;
			zzjbutt.TextColor = ZZJCore.FormSkin.TSColor;
			zzjbutt.RetData = 1;//返回值自己定一个(必须是负数,正数则按钮无效)
			BL.Add(zzjbutt);
			//BL.Add(ZZJCore.ZZJControl.Button_Ret);
			BL.Add(ZZJCore.ZZJControl.Button_Close);
		}
		#endregion
		ZZJCore.ImageMessage IM = new ZZJCore.ImageMessage(IL.ToArray(), BL.ToArray(), "请扫描条形码", 30);
		IM.Show2();
		Application.DoEvents();
		return IM;
	}

	public static int CloseScan(ZZJCore.ImageMessage IM)
	{
		if (IM == null) return -1;
		try
		{
			IM.Close2();
			Application.DoEvents();
		}
		catch
		{
			return -2;
		}
		return 0;
	}
	#endregion



};