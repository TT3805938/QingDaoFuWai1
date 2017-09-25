using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Mw_Public;
using Mw_MSSQL;
using System.Data.SqlClient;
using System.Threading;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml;
using System.IO;

public static class GMBL
{
	static Font font = new Font("微软雅黑", 34, FontStyle.Regular);
	static Bitmap Img = new System.Drawing.Bitmap(new Bitmap(1496, 866));
	static Graphics DrawingBoard = System.Drawing.Graphics.FromImage(Img);
	static int FFBZ = 0;
	public static bool GMBLMain()
	{
		string Msg = "";
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.Public_Var.ModuleName = "病历本发放";
		ZZJCore.BackForm.ShowForm("正在准备病历本,请稍候...", false);
		Application.DoEvents();
		ZZJCore.Initial.Read();
		if (XMLCore.GetUserInfo(out Msg) != 0)
		{
			if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true);
			ZZJCore.BackForm.CloseForm();
			return false;
		}

		#region 初始化资源
		DrawingBoard.Clear(Color.White);
		DrawingBoard.SmoothingMode = SmoothingMode.HighQuality;
		DrawingBoard.CompositingQuality = CompositingQuality.HighQuality;
		DrawingBoard.InterpolationMode = InterpolationMode.High;
		#endregion

		int state = 0;
		try
		{
			state = ZZJ_Module.Msprint.Open();//打开USB打印设备
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("打开打印机异常!" + e.Message);
			ZZJCore.BackForm.CloseForm();
			return false;
		}

		if (state != 0)
		{//打开设备出现异常
			ZZJCore.SuanFa.Proc.Log("初始化打印机失败,请联系管理员!错误码" + state.ToString());
			ZZJCore.BackForm.ShowForm("初始化打印机失败!请联系管理员!", true);
			return false;
		}

		#region 获取设备的状态
		state = ZZJ_Module.Msprint.GetState();
		if (state != 0)
		{
			if (state == 1) ShowMsgBox("打印机未连接,或未初始化");//......\n错误码[001]
			if (state == 2) ShowMsgBox("色带已用完");//......\n错误码[002]
			if (state == 3) ShowMsgBox("本机病历本已发完");//......\n错误码[003]
			if (state == 4) ShowMsgBox("病历本堵塞");//......\n错误码[004]
			if (state == 5) ShowMsgBox("本机病历本已经发完");//......\n错误码[005]
			if (state == 6) ShowMsgBox("打印头温度异常");//......\n错误码[006]
			if (state == 7) ShowMsgBox("抬压头异常");//......\n错误码[007]
			if (state == 8) ShowMsgBox("打印机正在忙,请等待");//......\n错误码[008]
			ZZJ_Module.Msprint.Close();
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		#endregion
		// */

		#region 询问用户
		int SFM = FormStyle.GMBLStyle.DYBLYesNo();
		if (SFM != 0)
		{
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		#endregion

		#region 查询是否可发病历本

		string InXML = "";
		string OutXML = "<zzxt>";
		OutXML += "<transcode>108</transcode>";
		OutXML += "<table>";
		OutXML += "<patid>" + ZZJCore.Public_Var.patientInfo.PatientID +"</patid>";
		OutXML += "</table>";
		OutXML += "</zzxt>";


		try
		{
			ZZJCore.SuanFa.Proc.BW(OutXML, "购买病历");
			//InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "MedicalCard", new string[] { OutXML }) as string;
			InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { OutXML }) as string;
			//if (ZZJCore.SuanFa.Proc.ReadPublicINI("GMBLDebugData", "0") == "1")
			//{
			//  InXML = "<Response><Address>山东青岛</Address><Nation>汉</Nation><Dob>2001-05-14</Dob><ErrorMsg>当天已经超过病历本发放数量</ErrorMsg><ResultCode>0</ResultCode></Response>";
			//}// */
			ZZJCore.SuanFa.Proc.BW(InXML, "购买病历");
			XmlDocument DocXMLs = new XmlDocument();
			DocXMLs.LoadXml(InXML);

		//	<zzxt><result><retcode>-1</retcode><retmsg>

			if (DocXMLs.SelectSingleNode("zzxt/result/retcode").InnerText.Trim() != "0")
			{
				ZZJCore.BackForm.ShowForm(DocXMLs.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim(), true);
				return true;
			}

			try
			{
				ZZJCore.Public_Var.patientInfo.DOB = DocXMLs.SelectSingleNode("zzxt/table/birth").InnerText.Trim();
				ZZJCore.Public_Var.patientInfo.Sex = DocXMLs.SelectSingleNode("zzxt/table/sex").InnerText.Trim();
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
			}
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.BackForm.ShowForm("调用接口失败,请稍候再试!", true);
			ZZJCore.SuanFa.Proc.Log("调接口时异常!" + e.Message);
			return true;
		}
		// */
		#endregion

		#region 画图
		int WP = 0, HP = 0;
		if (ZZJCore.Public_Var.ZZJ_Config.YQGUID == "3B34C527-E6DB-4BE3-ADAC-34FFA199AFDD")
		{
			WP = 10;
			HP = 130;
		}

		//姓名
		DrawingBoard.DrawString(ZZJCore.Public_Var.patientInfo.PatientName, font, Brushes.Black, new PointF(WP + 166, HP - 112));

		//性别
		if (ZZJCore.Public_Var.patientInfo.Sex == "男") DrawingBoard.FillRectangle(Brushes.Black, WP + 531, HP-82, 27, 27);
		if (ZZJCore.Public_Var.patientInfo.Sex == "女") DrawingBoard.FillRectangle(Brushes.Black, WP + 654, HP-82, 27, 27);

		//出生日期
		if (!string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.DOB))
		{
			try
			{
				//DateTime DOB = DateTime.Parse(ZZJCore.Public_Var.patientInfo.DOB);
				DrawingBoard.DrawString(ZZJCore.Public_Var.patientInfo.DOB.Substring(0,4), font, Brushes.Black, new PointF(937, HP -94));//年
				DrawingBoard.DrawString(ZZJCore.Public_Var.patientInfo.DOB.Substring(4,2), font, Brushes.Black, new PointF(1068, HP -94));//月
				DrawingBoard.DrawString(ZZJCore.Public_Var.patientInfo.DOB.Substring(6,2), font, Brushes.Black, new PointF(1148, HP - 94));//日
			//	if (DOB > DateTime.Now) goto ends;
				//DrawingBoard.DrawString(DOB.Year.ToString(), font, Brushes.Black, new PointF(975, HP + 1));//年
				//DrawingBoard.DrawString(DOB.Month.ToString(), font, Brushes.Black, new PointF(1130, HP + 1));//月
				//DrawingBoard.DrawString(DOB.Day.ToString(), font, Brushes.Black, new PointF(1220, HP + 1));//日
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
			}
		}

		//地址
		  if (string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.Address)) ZZJCore.Public_Var.patientInfo.Address = "";
			DrawingBoard.DrawString(ZZJCore.Public_Var.patientInfo.Address, font, Brushes.Black, new PointF(WP + 490, HP + 175));

	//ends:
	//  //地址
	//  if (string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.Address)) ZZJCore.Public_Var.patientInfo.Address = "";
	//  if (ZZJCore.Public_Var.ZZJ_Config.YQGUID == "F84E90BD-FA18-4B5F-AC67-4AB16301503B")
	//  {//如果是东院
	//    DrawingBoard.DrawString(ZZJCore.Public_Var.patientInfo.Address, font, Brushes.Black, new PointF(WP + 70, HP + 470));
	//  }
	//  else
	//  {
	//    DrawingBoard.DrawString(ZZJCore.Public_Var.patientInfo.Address, font, Brushes.Black, new PointF(WP + 70, HP + 410));
	//  }
			ZZJCore.SuanFa.Proc.BW(ZZJCore.Public_Var.patientInfo.DOB + "-" + ZZJCore.Public_Var.patientInfo.DOB.Substring(0, 4) + "-" + ZZJCore.Public_Var.patientInfo.DOB.Substring(4, 2) + "-" + ZZJCore.Public_Var.patientInfo.DOB.Substring(6, 2), "记录港内卡病例出生日期");
	ZZJCore.Public_Var.SerialNumber=DateTime.Now.ToString("yyyyMMddHHmmss");
		string BMPPath = ZZJCore.Public_Var.ModulePath + "cache/";
		string BMPPathAndFileName = string.Format("{0}{1}.BMP", BMPPath, ZZJCore.Public_Var.SerialNumber);

		try
		{
			Directory.CreateDirectory(BMPPath);
			using (Bitmap bm = Convert24bppTo1bpp(Img.Clone() as Bitmap))
			{
				bm.RotateFlip(RotateFlipType.Rotate90FlipNone);
				bm.Save(BMPPathAndFileName, ImageFormat.Bmp);
			}
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("转格式时异常!" + e.Message);
			ZZJCore.BackForm.ShowForm("系统错误!请联系管理员!", true);
			return true;
		}
		#endregion

		FFBZ = ZZJ_Module.Msprint.PrintBmp8(BMPPathAndFileName);//开始打印

	//	Thread T = new Thread(SaveData);
		//T.IsBackground = false;
		//T.Start();

		if (FFBZ != 0)
		{
			ZZJCore.BackForm.ShowForm("病历本打印失败!", true);
			try
			{
				ZZJ_Module.Msprint.Close();
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.Log("关闭打印机异常!" + e.Message);
			}
			ZZJCore.BackForm.CloseForm();
			return false;
		}

		Application.DoEvents();
		System.Threading.Thread.Sleep(5000);
		ZZJCore.BackForm.ShowForm("病历本打印完毕,请收好您的病历本 !", true);
		ZZJCore.SuanFa.Proc.MsgSend(0x10, 0x00, true);

		try
		{
			ZZJ_Module.Msprint.Close();
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("关闭打印机异常!" + e.Message);
		}
		return true;
	}

//	#region 数据库操作
	
/*	private static SqlParameter AddSP(SqlDbType SDT, string PName, object Value)
	{
		SqlParameter parameter = new SqlParameter();
		parameter.SqlDbType = SDT;
		parameter.ParameterName = PName;
		parameter.Value = Value;
		return (parameter);
	}

	public static void SaveData()
	{
		string msg = "";
		#region 网络数据库
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
		{
			ZZJCore.SuanFa.Proc.Log(e);
			return;
		}
		try
		{

			MSSQL_Operate mssql_operate = new MSSQL_Operate();
			SqlParameter[] parameter = null;
			ArrayList AL = new ArrayList();
			//AL.Add(AddSP(SqlDbType.VarChar, "@LSH", ZZJCore.Public_Var.SerialNumber));//流水号
			AL.Add(AddSP(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.cardInfo.CardNo));//登记号
			AL.Add(AddSP(SqlDbType.VarChar, "@KH", ZZJCore.Public_Var.cardInfo.CardNo));//卡号
			if (string.IsNullOrEmpty(ZZJCore.Public_Var.cardInfo.CardType)) ZZJCore.Public_Var.cardInfo.CardType = "0";
			AL.Add(AddSP(SqlDbType.Int, "@KPLX", int.Parse(ZZJCore.Public_Var.cardInfo.CardType)));//卡类型
			AL.Add(AddSP(SqlDbType.VarChar, "@XM", ZZJCore.Public_Var.patientInfo.PatientName));//姓名
			AL.Add(AddSP(SqlDbType.Int, "@SL", 1));//数量
			AL.Add(AddSP(SqlDbType.Decimal, "@JE", 0m));//金额
			AL.Add(AddSP(SqlDbType.DateTime, "@SJ", DateTime.Now.ToString()));//充值时间
			AL.Add(AddSP(SqlDbType.VarChar, "@SFY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//收费员
			AL.Add(AddSP(SqlDbType.Int, "@CGBZ", 0));//操作标识 0成功 其他失败
			AL.Add(AddSP(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));//自助机编号
			AL.Add(AddSP(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//HIS操作员
			AL.Add(AddSP(SqlDbType.VarChar, "@BRLX", "自费"));//病人类型
			AL.Add(AddSP(SqlDbType.VarChar, "@KFMS", ""));//扣费描述
			AL.Add(AddSP(SqlDbType.Int, "@FFBZ", FFBZ));//发放标志
			AL.Add(AddSP(SqlDbType.VarChar, "@FFMS", ""));//发放描述
			parameter = (SqlParameter[])AL.ToArray(typeof(SqlParameter));
			if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_BL_MX_ForZZJ_V2]", parameter, out msg) > 0)
			{
				ZZJCore.SuanFa.Proc.Log("购买病历记录写入服务器成功");
				return;
			}
			ZZJCore.SuanFa.Proc.Log("购买病历记录写入服务器失败" + msg);
		}
		catch (Exception Ex)
		{
			ZZJCore.SuanFa.Proc.Log(Ex);
			ZZJCore.SuanFa.Proc.Log("购买病历记录写入服务器失败，可能是网络中断或远程自助机服务器未开启，" + Ex.Message);
		}
		#endregion
	}
	#endregion*/

	private static void ShowMsgBox(string sText, int iTime = 5, bool showbutton = true, bool WriteLog = true)
	{
		if (WriteLog) ZZJCore.SuanFa.Proc.Log(sText);//写入日志
		ZZJCore.BackForm.ShowForm(sText, true);
	}

	private static Bitmap Convert24bppTo1bpp(Bitmap SrcImg)
	{
		unsafe
		{
			byte* SrcPointer, DestPointer;
			int Width, Height, SrcStride, DestStride;
			int X, Y, Index, Sum;
			;
			Bitmap DestImg = new Bitmap(SrcImg.Width, SrcImg.Height, PixelFormat.Format1bppIndexed);
			BitmapData SrcData = new BitmapData();
			SrcImg.LockBits(new Rectangle(0, 0, SrcImg.Width, SrcImg.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb, SrcData);
			BitmapData DestData = new BitmapData();
			DestImg.LockBits(new Rectangle(0, 0, SrcImg.Width, SrcImg.Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed, DestData);
			Width = SrcImg.Width;
			Height = SrcImg.Height;
			SrcStride = SrcData.Stride;
			DestStride = DestData.Stride;
			for (Y = 0; Y < Height; Y++)
			{
				SrcPointer = (byte*)SrcData.Scan0 + Y * SrcStride;
				DestPointer = (byte*)DestData.Scan0 + Y * DestStride;
				Index = 7;
				Sum = 0;
				for (X = 0; X < Width; X++)
				{
					if (*SrcPointer + (*(SrcPointer + 1) << 1) + *(SrcPointer + 2) >= 512)
						Sum += (1 << Index);
					if (Index == 0)
					{
						*DestPointer = (byte)Sum;
						Sum = 0;
						Index = 7;
						DestPointer++;
					}
					else
						Index--;
					SrcPointer += 3;
				}
				if (Index != 7)
					*DestPointer = (byte)Sum;
			}
			SrcImg.UnlockBits(SrcData);
			DestImg.UnlockBits(DestData);
			return DestImg;
		}
	}
}
