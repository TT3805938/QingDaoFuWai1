using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Windows.Forms;
using System.Xml;
using Mw_Public;
using Mw_MSSQL;
using System.Data.SqlClient;
using System.Threading;
using System.Collections;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using HL.Devices.DKQ;
using Mw_Voice;

namespace ZZJ_Module
{
	public static class YBPay
	{
		public static string name;
		public static decimal DC = 0;
		public static string KSBM = "0300";
		public static Font TSTEXTFont = new Font(new FontFamily("黑体"), 42, FontStyle.Bold);//字体
		public static int WaitCard(out int CardType, Image IMAGE, string TSTEXT = "请插入一代,二代已激活社保卡")
		{
			CardType = 0;
			QDYBCard.Mode = 0;

			ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
			for (int i = 0; i < 300; i++)
			{
				Thread.Sleep(1);
				Application.DoEvents();
			}

			if (DKQDev.OpenDevice(ZZJCore.Public_Var.ZZJ_Config.DKQ_Dev, (ZZJCore.Public_Var.ZZJ_Config.DKQ_Port.Substring(3, ZZJCore.Public_Var.ZZJ_Config.DKQ_Port.Length - 3)), 9600) != 0)
			{
				ZZJCore.SuanFa.Proc.Log("打开读卡器失败!");
				ZZJCore.SuanFa.Proc.Log("设备:" + ZZJCore.Public_Var.ZZJ_Config.DKQ_Dev);
				ZZJCore.SuanFa.Proc.Log("端口:" + ZZJCore.Public_Var.ZZJ_Config.DKQ_Port);
				ZZJCore.BackForm.ShowForm("读卡器打开失败！", true);
				return -5;
			}

			//如果患者插入的卡就是社保卡,返回3
			//if (ZZJCore.Public_Var.cardInfo.CardType == "2") return 3;

			DKQDev.SetCardIn(3, 1);
			DKQDev.SetCardPostion(4);
			DKQDev.MoveCard(1);

			DKQDev.ICEnableExtIO(false);

			//先显示个请插卡提示
			#region 图片
			List<ZZJCore.ZZJStruct.AImage> IL = new List<ZZJCore.ZZJStruct.AImage>();
			ZZJCore.ZZJStruct.AImage IA = new ZZJCore.ZZJStruct.AImage();

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

				int CaptionWidth = (int)ZZJCore.SuanFa.Proc.GetTextSizeF(TSTEXTFont, TSTEXT + "空").Width;

				zzjbutt.Rect = new Rectangle((1280 - CaptionWidth) / 2, 100, CaptionWidth, 64);//这是设定按钮的位置大小
				zzjbutt.BGIMAGE = null;//ZZJCore.FormSkin.TYButtonImage2;//这里设置按钮的背景图
				zzjbutt.Text = TSTEXT;//按钮上的字
				zzjbutt.TextFont = TSTEXTFont;
				zzjbutt.TextColor = ZZJCore.FormSkin.TSColor;
				zzjbutt.RetData = 1;//返回值自己定一个(必须是负数)
				BL.Add(zzjbutt);
				//BL.Add(ZZJCore.ZZJControl.Button_Ret);
				BL.Add(ZZJCore.ZZJControl.Button_Close);
			}
			#endregion

			ZZJCore.ImageMessage IM = new ZZJCore.ImageMessage(IL.ToArray(), BL.ToArray(), TSTEXT, 30);
			IM.Show(ZZJCore.BackForm.BF);
			
			Application.DoEvents();
			int iRet = 0;
			#region 循环等卡
			while (true)
			{
				int status = 0;//是否有卡标志
				iRet = DKQDev.GetStatus(ref status);//获取读卡器内是否有卡 1 有卡，0 无卡
				Application.DoEvents();
				if (iRet == 0 && status == 1) break;
				if (IM.Visible) continue;
				//如果窗体被关闭,则进入这里
				DKQDev.MoveCard(1);
				DKQDev.CloseDevice();
				ZZJCore.BackForm.CloseForm();
				if (IM.iRet == -1) return -4;
				return -4;
			}
			#endregion 循环等卡
			IM.Close();
			if (iRet != 0) return -4;

			#region 判断卡类型
			string sRet = "";
			int CardTypeRet = DKQDev.DetectICType(out CardType, out sRet);
			if (CardTypeRet != 0)
			{
				ZZJCore.BackForm.ShowForm("读卡失败,请稍后再试！", true);
				DKQDev.MoveCard(1);
				DKQDev.CloseDevice();
				return -4;
			}
			#endregion

			return 0;
		}

		//调用医保支付
		//返回值: 0:成功 -1:用户返回 -2:用户关闭 -4:业务异常 -5:设备异常
		public static int Payment()
		{
			int TestNum = 0;//重试次数
			int CardType = 0;
			if (DC <= 0) return -4;

			if (ZZJCore.SuanFa.Proc.ReadPublicINI("YBDebugMode", "0") == "1") return 0;
		WAITCARD:
			int HaveCard = WaitCard(out CardType, Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/SBK.gif"));
			if (HaveCard == 3) goto ReadCardOK;
			if (HaveCard != 0) return HaveCard;
			string TS = (TestNum < 2) ? string.Format("您还可以试{0}次", (2 - TestNum).ToString()) : "";
			#region 读磁条卡
			if (CardType != 0x20)//磁条卡
			{
				string[] Buff = new string[] { "", "", "" };
				int DKQiRet = DKQDev.ReadTracksACSII(4, ref Buff);
				if (DKQiRet != 0)
				{
					DKQDev.MoveCard(1);
					//ZZJCore.BackForm.ShowForm("读卡失败！", true);
					ZZJCore.SuanFa.Proc.ZZJMessageBox("", "读卡失败!" + TS, true);
					ZZJCore.SuanFa.Proc.Log("磁条医保卡读卡失败!读卡器返回:" + DKQiRet.ToString());
					DKQDev.CloseDevice();
					TestNum++;
					if (TestNum < 3) goto WAITCARD;
					return -4;
				}
				ZZJCore.SuanFa.Proc.Log("磁条医保卡读卡成功,卡号" + Buff[1]);
				if (QDYBCard.YBKHGetHZInfo(Buff[1]) != 0)
				{
					DKQDev.MoveCard(1);
					//ZZJCore.BackForm.ShowForm("读卡信息失败！", true);
					ZZJCore.SuanFa.Proc.ZZJMessageBox("", "读卡信息失败!" + TS, true);
					DKQDev.CloseDevice();
					TestNum++;
					if (TestNum < 3) goto WAITCARD;
					ZZJCore.SuanFa.Proc.Log("读卡信息失败");
					return -4;
				}
				ZZJCore.SuanFa.Proc.Log("读卡信息成功!");
				goto ReadCardOK;
			}
			#endregion

			#region 读IC卡
			DKQDev.MoveCard(4);
			DKQDev.ICEnableExtIO(true);
			for (int i = 0; i < 300; i++)
			{
				Application.DoEvents();
				Thread.Sleep(1);
			}
			QDYBCard.SFXX S = new QDYBCard.SFXX();


			S.SFZ = new StringBuilder(19);
			S.XM = new StringBuilder(31);
			if (QDYBCard.ReadCard(ref S) != 0)
			{
				DKQDev.MoveCard(1);//把卡走出去还给患者
				DKQDev.ICEnableExtIO(false);//切回读头
				//ZZJCore.BackForm.ShowForm("读卡失败!", true);//提示消息
				ZZJCore.SuanFa.Proc.ZZJMessageBox("", "读卡失败!" + TS, true);
				DKQDev.CloseDevice();//关读卡器
				TestNum++;
				if (TestNum < 3) goto WAITCARD;
				return -4;
			}
			name = S.XM.ToString();
			ZZJCore.SuanFa.Proc.Log(string.Format("医保卡读卡成功!身份证:{0}姓名:{1}", S.SFZ, S.XM));

			QDYBCard.Mode = 1;

			if (ZZJCore.SuanFa.Proc.ReadPublicINI("YBNotActivation") != "1")
			{
				if (QDYBCard.YBKHGetHZInfo("") != 0)
				{
					DKQDev.MoveCard(1);
					DKQDev.ICEnableExtIO(false);
					//ZZJCore.BackForm.ShowForm("与中心交易失败!", true);
					ZZJCore.SuanFa.Proc.ZZJMessageBox("", "与中心交易失败!", true);
					DKQDev.CloseDevice();
					return -4;
				}
			}
			else
			{
				QDYBCard.Mode = 0;
				DKQDev.CloseDevice();
				return -4;
				/*
				if (QDYBCard.SFZGetYBCardType(S.SFZ.ToString()) != 0)
				{
					DKQDev.MoveCard(1);
					//ZZJCore.BackForm.ShowForm("与中心交易失败,请稍后再试！", true);
					ZZJCore.SuanFa.Proc.ZZJMessageBox("", "与中心交易失败!", true);
					DKQDev.CloseDevice();
					return -4;
				}
				// */
			}
			#endregion

		ReadCardOK:
			DKQDev.MoveCard(1);
			DKQDev.ICEnableExtIO(false);
			DKQDev.CloseDevice();

			Thread.Sleep(200);
			Application.DoEvents();
			QDYBCard.Mode = 1;

			#region 社保卡卡金缴费
			if (QDYBCard.GRBHGetYBYLYE(QDYBCard.GRBH) != 0)
			{
				ZZJCore.BackForm.ShowForm("查询余额失败!" + QDYBCard.Err, true);
				return -4;
			}

			//然后看看余额够不够
			if (QDYBCard.YBYE < DC)
			{
				ZZJCore.BackForm.ShowForm("社保卡余额不足！", true);
				return -2;
			}// */
			//YL001 三个金额要传一样的
			if (QDYBCard.GRBHZF(QDYBCard.GRBH, KSBM, "", ZZJCore.Public_Var.patientInfo.PatientID, "交易", DC, DC, DC) != 0)
			{
				ZZJCore.BackForm.ShowForm("支付不成功！" + QDYBCard.Err, true);

				return -4;
			}// */

			PirntYLPT();

			#endregion
			return 0;
		}

		private static void PirntYLPT()
		{
			string[] NR1 = new string[] { "个人编号", "批次号", "POS号", "参考号", "交易金额", "医保账户余额", "交易时间" };
			string[] NR2 = new string[] { QDYBCard.GRBH, QDYBCard.PCH, QDYBCard.POSH, QDYBCard.YLJYCKH, DC.ToString("C").ToString(), QDYBCard.YBYE.ToString("C"), QDYBCard.JYSJ.ToString("yyyy年MM月dd日 HH:mm:ss") };
			ZZJCore.SuanFa.PrintCall.PrintPT("医保支付凭证", NR1, NR2, "", false);
		}
	}//End Class
}