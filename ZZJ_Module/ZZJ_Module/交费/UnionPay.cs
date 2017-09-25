using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Windows.Forms;
using System.Xml;
using Mw_Public;
using Mw_MSSQL;
using Mw_Voice;
using System.Data.SqlClient;
using System.Threading;
using System.Collections;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ZZJ_Module
{
	public static class UnionPay
	{
		public static string UnionPayCardNo = "";
		public static QDYLLib.READSTRIINGSTRURT RETDATA = new QDYLLib.READSTRIINGSTRURT();
		public static byte Mode = 0;
		public static decimal DC = 0;
		//调用银联支付 
		//返回值: 0:成功 -1:用户返回 -2:用户关闭 -4:业务异常 -5:设备异常
		public static int Payment()
		{
			if (DC <= 0) return -4;

			string YLDebugTXTPath = ZZJCore.SuanFa.Proc.ReadPublicINI("YLDebugTXTPath", "");
			if (ZZJCore.Public_Var.debug && YLDebugTXTPath == "1") return 0;
			/*
			if (!string.IsNullOrEmpty(YLDebugTXTPath))
			{
				try
				{
					QDYLLib.CardTransfer.UnPackData(File.ReadAllText(YLDebugTXTPath, Encoding.Default), out RETDATA);
					return 0;
				}
				catch
				{ }
			}// */
			QDYLLib.CardTransfer.MODE = Mode;
			ZZJCore.BackForm.ShowForm("正在进入银联模式,请稍候...\n请退卡后按图示插入银联卡");
			for (int i = 0; i < 360; i++)
			{
				Thread.Sleep(1);
				Application.DoEvents();
			}

			//要用多线程,不然会界面假死
			int OpenYLRet=-1;
			try
			{
				List<Task> listTask=new List<Task>();
				Task T=Task.Factory.StartNew(()=>
				{
					QDYLLib.QDYL.Initial();
					OpenYLRet = QDYLLib.QDYL.CardOpen();
				});
				listTask.Add(T);
				ZZJCore.BackForm.SetFormText("银联初始化,请稍候...");
				Application.DoEvents();
				Task.WaitAll(listTask.ToArray(),5000);
			}
			catch(Exception ex)
			{
				ZZJCore.BackForm.ShowForm("银联初始化错误",true);
				ZZJCore.BackForm.CloseForm();
				ZZJCore.SuanFa.Proc.Log("银联初始化失败," + QDYLLib.QDYL.s_err);
				return -5;
			}

			//QDYLLib.QDYL.Initial();
			//if (QDYLLib.QDYL.CardOpen() != 0)
			if (OpenYLRet != 0)
			{
				ZZJCore.BackForm.ShowForm("进入银联模式失败!", true);
				ZZJCore.BackForm.CloseForm();
				ZZJCore.SuanFa.Proc.Log("进入银联模式失败," + QDYLLib.QDYL.s_err);
				return -5;
			}
			// */
			ZZJCore.BackForm.SetFormText("正在准备,请稍候...");
			Application.DoEvents();

			//goto inputPWD;

			//先显示个请插卡提示
			#region 图片
			List<ZZJCore.ZZJStruct.AImage> IL = new List<ZZJCore.ZZJStruct.AImage>();
			ZZJCore.ZZJStruct.AImage IA = new ZZJCore.ZZJStruct.AImage();
			Image IMAGE = Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/bankcard.gif");

			IA.W = IMAGE.Size.Width;
			IA.H = IMAGE.Size.Height;
			IA.X = (1280 - IA.W) / 2;
			IA.Y = 150;
			IA.image = IMAGE;
			IL.Add(IA);
			#endregion
			#region 添加按钮
			List<ZZJCore.ZZJStruct.ZZJButton> BL = new List<ZZJCore.ZZJStruct.ZZJButton>();
			{
				ZZJCore.ZZJStruct.ZZJButton zzjbutt = new ZZJCore.ZZJStruct.ZZJButton();
				zzjbutt.Rect = new Rectangle(400, 100, 500, 64);//这是设定按钮的位置大小
				zzjbutt.BGIMAGE = null;//ZZJCore.FormSkin.TYButtonImage2;//这里设置按钮的背景图
				zzjbutt.Text = "请插入银联卡";//按钮上的字
				zzjbutt.TextFont = new Font(new FontFamily("黑体"), 42, FontStyle.Bold);//字体
				zzjbutt.TextColor = ZZJCore.FormSkin.TSColor;
				zzjbutt.RetData = 1;//返回值自己定一个(必须是负数)
				BL.Add(zzjbutt);
				//BL.Add(ZZJCore.ZZJControl.Button_Ret);
				BL.Add(ZZJCore.ZZJControl.Button_Close);
			}
			#endregion
			//Talker.Init_Voice(100, 1);
			ZZJCore.ImageMessage IM = new ZZJCore.ImageMessage(IL.ToArray(), BL.ToArray(), "请插入银联卡", 30);
			IM.Show(ZZJCore.BackForm.BF);
			Application.DoEvents();
			ZZJCore.SuanFa.Proc.Log("[银联模式]开始读卡！");
			#region 循环读卡
			int iRet = 0;
			while (true)
			{

				for (int i = 0; i < 1000; i++)
				{
					Application.DoEvents();
				}
				//读卡号
				try
				{
					List<Task> listTask = new List<Task>();
					Task T2=Task.Factory.StartNew(()=>
					{
						iRet = QDYLLib.QDYL.CardRead(out UnionPayCardNo);
					});
				
					listTask.Add(T2);
					Task.WaitAll(listTask.ToArray(), 15000);
				}
				catch(Exception ex)
				{
					
					ZZJCore.BackForm.ShowForm("读卡失败", true);
					ZZJCore.BackForm.CloseForm();
					ZZJCore.SuanFa.Proc.Log("读银行卡失败," + QDYLLib.QDYL.s_err);
					return -5;
				}
				//iRet = QDYLLib.QDYL.CardRead(out UnionPayCardNo);

				//if (string.IsNullOrEmpty(UnionPayCardNo)) continue;

				UnionPayCardNo = UnionPayCardNo.Trim();
				if (iRet < 0)
				{
					ZZJCore.SuanFa.Proc.Log(QDYLLib.QDYL.s_err);
					//IM.ReDJS();
					IM.Close();
					ZZJCore.BackForm.ShowForm("读卡失败,请稍后再试!", true);
				}
				if (iRet == 0)
				{
					IM.Close();
					break;
				}
				if (IM.Visible) continue;

				//如果窗体被关闭,则进入这里
				QDYLLib.QDYL.CardClose();
				ZZJCore.BackForm.CloseForm();
				if (IM.iRet == -1) return -1;
				return -2;
			}
			#endregion 循环读卡

			ZZJCore.SuanFa.Proc.Log("[银联模式]读卡完成,卡号:" + UnionPayCardNo);

		inputPWD:

			#region 打开密码键盘
			int PinState = 1;//储存密码键盘状态
			string PW = "";
			iRet = QDYLLib.QDYL.PinPadOpen(
					(int state, string password) =>
					{
						PinState = state;
						PW = password;
						if (PW.Length >= 6) Talker.Speak("请按确认继续");
					});
			if (iRet != 0)
			{
				ZZJCore.BackForm.ShowForm("打开密码键盘失败!请联系管理员!", true);
				ZZJCore.SuanFa.Proc.Log("打开密码键盘失败!" + QDYLLib.QDYL.s_err);
				return -5;
			}
			#endregion

			ZZJCore.SuanFa.Proc.Log("[银联模式]打开密码键盘成功,开始输入密码");

			#region 循环等待输入密码
			//IM = new ZZJCore.ImageMessage(null, null, "", 0);

			ZZJCore.ShowControlForm.ShowControlFormPatameter SCFP = new ZZJCore.ShowControlForm.ShowControlFormPatameter();
			SCFP.Caption = "请在屏幕下方金属键盘输入密码";
			SCFP.Voice="请在屏幕下方金属键盘输入密码";
			SCFP.DJSTime=0;
			ZZJCore.SubWindow.MaskTextBox MTB = new ZZJCore.SubWindow.MaskTextBox();
			MTB.AutoSize = false;
			MTB.Width = 400;
			MTB.Height = 90;
			MTB.Font = new Font("黑体", 48, FontStyle.Bold);
			MTB.Text = "";
			SCFP.ControlTemp = MTB;
			ZZJCore.ShowControlForm.ShowForm(SCFP);

			while (true)
			{
				for (int i = 0; i < 100; i++)
				{
					Application.DoEvents();
				}

				MTB.Text = PW;

				if (PinState > 0) continue;
				if (PinState == 0) break;
				QDYLLib.QDYL.CardOut();
				QDYLLib.QDYL.CardClose();
				//ZZJCore.BackForm.ShowForm("请收好您的卡!", true);
				MTB.Dispose();
				ZZJCore.ShowControlForm.CloseForm();
				ZZJCore.SuanFa.Proc.Log("输入密码超时!");
				return -1;
			}
			#endregion
			MTB.Dispose();
			ZZJCore.ShowControlForm.CloseForm();
			ZZJCore.SuanFa.Proc.Log("[银联模式]输入密码完成,开始扣费:" + DC.ToString("C"));

			#region 转账
			if (ZZJCore.Public_Var.IsVoice)
			{
				//Talker.Stop();
				//2016-8-14 注释语音	Talker.Speak("正在扣费,请稍候...");
			}
			ZZJCore.BackForm.SetFormText("正在扣费,请稍候...");
			Application.DoEvents();
			//款台号
			QDYLLib.QDYL.OutData.strCounterId = ZZJCore.SuanFa.Proc.ReadPublicINI("strCounterId", "ABCDEF");
			//操作员号
			QDYLLib.QDYL.OutData.strOperId = ZZJCore.SuanFa.Proc.ReadPublicINI("strOperId", "ABCDEF");
			//交易
			ZZJCore.SuanFa.Proc.Log("[银联模式]进入交易前");

			try
			{
				//DC = decimal.Parse("0.01");
				iRet = QDYLLib.QDYL.Consumption(DC, out RETDATA);
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.Log("[银联模式]交易时异常!" + e.Message);
				ZZJCore.BackForm.ShowForm("交易失败!", true);
				ZZJCore.SuanFa.Proc.Log(e.Message);
			}
			ZZJCore.SuanFa.Proc.Log("[银联模式]交易完成!");
			if (iRet != 0)
			{
				ZZJCore.SuanFa.Proc.Log("交易失败!");
				try
				{
					string strRespCode = RETDATA.strRespCode.Replace("\0", "").Trim();
					string strRespInfo = RETDATA.strRespInfo.Replace("\0", "").Trim();
					ZZJCore.SuanFa.Proc.Log(strRespCode + "\n" + strRespInfo);
					ZZJCore.BackForm.ShowForm("交易失败!" + strRespInfo, true);
				}
				catch (Exception e)
				{
					ZZJCore.SuanFa.Proc.Log(e);
					ZZJCore.BackForm.ShowForm("交易失败!", true);
					ZZJCore.SuanFa.Proc.Log(e.Message);
				}
				QDYLLib.QDYL.CloseYL();
				ZZJCore.BackForm.CloseForm();
				return -4;
			}
			ZZJCore.SuanFa.Proc.Log("[银联模式]交易成功!");
			QDYLLib.QDYL.CloseYL();
			ZZJCore.SuanFa.PrintCall.Print(ZZJCore.Public_Var.ZZJ_Config.PT_Dev, PirntYLPT, 300, 430, false);
			ZZJCore.SuanFa.Proc.Log("[银联模式]凭条打印完毕!");

			#endregion

			return 0;
		}

		private static void PirntYLPT(object sender, PrintPageEventArgs e)
		{
			string S = "";
			for (int i = 0; i < 100; i++)
			{
				Application.DoEvents();
				Thread.Sleep(1);
			}
			try
			{
				if (QDYLLib.CardTransfer.MODE == 0) S = File.ReadAllText("D:/umsips/cup1/receipt.txt", Encoding.GetEncoding("GB2312"));
				if (QDYLLib.CardTransfer.MODE == 1) S = File.ReadAllText("D:/umsips/cup2/receipt.txt", Encoding.GetEncoding("GB2312"));
				ZZJCore.SuanFa.Proc.BW("\n" + S, "银联凭条");
			}
			catch (Exception ex) { ZZJCore.SuanFa.Proc.Log(ex); }
			e.Graphics.DrawString(S, new Font("黑体", 12, FontStyle.Bold), System.Drawing.Brushes.Black, 15, 0);

		}
	}//End Class
}
