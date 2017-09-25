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
using ZZJCore.ZZJStruct;
using ZZJCore;
using HL.Devices.DKQ;

namespace ZZJ_Module
{
	public static class GangCardPay
	{
		/// <summary>
		/// 卡号 
		/// </summary>
		public static string GangCardNo = "";
		/// <summary>
		/// 金额
		/// </summary>
		public static decimal DC = 0;
		
		/// <summary>
		/// 密码
		/// </summary>
		public static string Psd="";
		/// <summary>
		/// 支付级别
		/// </summary>
		public static string Zfjb="";
		/// <summary>
		/// 港内卡对象
		/// </summary>
		public static GangCard gangCard=new GangCard();

		/// <summary>
		///  字体
		/// </summary>
		public static Font TSTEXTFont = new Font(new FontFamily("黑体"), 42, FontStyle.Bold);//字体
		/// <summary>
		/// 港内卡支付类
		/// </summary>
		public class GangCard
		{
			/// <summary>
			/// 卡号
			/// </summary>
			public string GangCardNo { get; set; }
			/// <summary>
			/// 姓名
			/// </summary>
			public string GangCardName { get; set; }//姓名
			/// <summary>
			/// 账户号
			/// </summary>
			public string GangCardAccoutnID { get; set; }//账户号
			/// <summary>
			/// 支付级别
			/// </summary>
			public string payLevel { get; set; }//支付级别 zfjb
			/// <summary>
			/// 余额
			/// </summary>
			public decimal YE { get; set; }
			/// <summary>
			/// 密码
			/// </summary>
			public string psd { get; set; }
		}

		/// <summary>
		/// 等待进卡界面
		/// </summary>
		/// <param name="CardType">卡片类型</param>
		/// <param name="IMAGE">图片</param>
		/// <param name="TSTEXT">提示内容</param>
		/// <returns></returns>
		public static int WaitCard(out int CardType, Image IMAGE, string TSTEXT = "请插入可用的医疗卡")
		{
			CardType = 0;
			QDYBCard.Mode = 0;

			ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
			for (int i = 0; i < 300; i++)
			{
				Thread.Sleep(1);
				Application.DoEvents();
			}
			//打开读卡器
			if (DKQDev.OpenDevice(ZZJCore.Public_Var.ZZJ_Config.DKQ_Dev, (ZZJCore.Public_Var.ZZJ_Config.DKQ_Port.Substring(3, ZZJCore.Public_Var.ZZJ_Config.DKQ_Port.Length - 3)), 9600) != 0)
			{
				ZZJCore.SuanFa.Proc.Log("打开读卡器失败!");
				ZZJCore.SuanFa.Proc.Log("设备:" + ZZJCore.Public_Var.ZZJ_Config.DKQ_Dev);
				ZZJCore.SuanFa.Proc.Log("端口:" + ZZJCore.Public_Var.ZZJ_Config.DKQ_Port);
				ZZJCore.BackForm.ShowForm("读卡器打开失败！", true);
				return -5;
			}

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
		

		//调用港内卡支付 
		//返回值: 0:成功 -1:用户返回 -2:用户关闭 -4:业务异常 -5:设备异常
		/// <summary>
		/// 调用港内卡支付
		/// </summary>
		/// <returns> 0:成功 -1:用户返回 -2:用户关闭 -4:业务异常 -5:设备异常</returns>
		public static int Payment()
		{
			gangCard=new GangCard();//港内卡对象
			int TestNum = 0;//重试次数
			int CardType = 0;
			if (DC <= 0) return -4;
			GangCard gc=new GangCard();
			try
			{
						WAITCARD:
				int HaveCard = WaitCard(out CardType, Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/gangCard.gif"));
							if (HaveCard == 3) goto ReadCardOK;
							if (HaveCard != 0) return HaveCard;
							string TS = (TestNum < 2) ? string.Format("您还可以试{0}次", (2 - TestNum).ToString()) : "";
							#region 读磁条卡
							if (CardType != 0x20)//磁条卡
							{
									string[] Buff=new string[] { "", "", "" };
									if(CardType== 0x10) //S50卡
									{

										byte[] s50bytes = new byte[512];
										byte[] Key_Code = new byte[] { 0XA1, 0XA3, 0XA2, 0XA4, 0XA6, 0XA5 };
										//先读取医院院内卡
										string msg="";
										int iRet = DKQDev.M1_ReadCard(1, 0, Key_Code, ref s50bytes, ref msg);
										if (iRet != 0 || string.IsNullOrEmpty(Encoding.ASCII.GetString(s50bytes).TrimEnd('\0')))
										{
											ZZJCore.SuanFa.Proc.Log("进入了失败模式,卡号" + Buff[1]);
											DKQDev.MoveCard(1);
											//ZZJCore.BackForm.ShowForm("读卡失败！", true);
											ZZJCore.SuanFa.Proc.ZZJMessageBox("", "读卡失败!" + TS, true);
											ZZJCore.SuanFa.Proc.Log("磁条港内卡读卡失败!读卡器返回:" + iRet.ToString());
											DKQDev.CloseDevice();
											TestNum++;
											if (TestNum < 3) goto WAITCARD;
											return -4;
										}
									
										Buff[1]= Encoding.ASCII.GetString(s50bytes).TrimEnd('\0');
										ZZJCore.SuanFa.Proc.Log("磁条港内卡读卡成功,卡号" + Buff[1]);
										GangCardNo = Buff[1].ToString();
									}
									else
									{
										 
											int DKQiRet = DKQDev.ReadTracksACSII(4, ref Buff);
											//卡插反了也是返回0的,所以要判断一下到底读没读出卡号
											if (DKQiRet != 0||string.IsNullOrEmpty(Buff[1]))
											{
												ZZJCore.SuanFa.Proc.Log("进入了失败模式,卡号" + Buff[1]);
												DKQDev.MoveCard(1);
												//ZZJCore.BackForm.ShowForm("读卡失败！", true);
												ZZJCore.SuanFa.Proc.ZZJMessageBox("", "读卡失败!" + TS, true);
												ZZJCore.SuanFa.Proc.Log("磁条港内卡读卡失败!读卡器返回:" + DKQiRet.ToString());
												DKQDev.CloseDevice();
												TestNum++;
												if (TestNum < 3) goto WAITCARD;
												return -4;
											}
											ZZJCore.SuanFa.Proc.Log("磁条港内卡读卡成功,卡号" + Buff[1]);
											GangCardNo = Buff[1].ToString();
									}
								
								//查询港内卡信息
								ZZJCore.SuanFa.Proc.Log("开始查询港内卡信息通过HIS,卡号" + Buff[1]);
								if (QueryGangCardInfo(GangCardNo,out gc)!=0)
								{
									DKQDev.MoveCard(1);
									//ZZJCore.BackForm.ShowForm("读卡信息失败！", true);
									ZZJCore.SuanFa.Proc.ZZJMessageBox("", "查询港内卡信息通过HIS失败", true);
									DKQDev.CloseDevice();
									ZZJCore.SuanFa.Proc.Log("查询港内卡信息通过HIS失败");
									return -4;
								}
								gangCard=gc;
								ZZJCore.SuanFa.Proc.Log("读卡信息成功!");
								goto ReadCardOK;
							}
							else
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
							#endregion

						ReadCardOK:
							DKQDev.MoveCard(1);
							DKQDev.ICEnableExtIO(false);
							DKQDev.CloseDevice();

							Thread.Sleep(200);
							Application.DoEvents();

							//输入一下密码

							ZZJCore.InputForm.InputPasswordParameter IP = new ZZJCore.InputForm.InputPasswordParameter();
							IP.MAX = 6;
							IP.MIN = 6;
							IP.DJSTime = 120;
							IP.Caption = "请输入您的密码";
							IP.LabelText = "";
							IP.Voice = "";
							List<ZZJCore.ZZJStruct.ZZJButton> ALS = new List<ZZJCore.ZZJStruct.ZZJButton>();
							ALS.Add(ZZJCore.ZZJControl.Button_Close);
							IP.Buttons = ALS.ToArray();
							IP.XButton = false;
							//IP.RegMode = false;
							string ID = "";//////////////////////
							int iRett = ZZJCore.InputForm.InputPassword(IP);
							ID=ZZJCore.Public_Var.patientInfo.Password;
							ZZJCore.Public_Var.patientInfo.Password="";
							ZZJCore.SuanFa.Proc.Log("输入的密码为:" + ID);
							if (iRett != 0)
							{
								//密码输入错误.
								return -4;
							}
							gc.psd = ID;//港内卡密码
							Psd=gc.psd;

							//查询一下港内卡是否合法 并做好支付准备（即创建支付对象） 
							#region 判断港内卡是否合法和余额是否充足
							decimal YE=-1;
							//卡对象为空 说明获取卡信息失败或者不是港内卡
							if(gc==null)
							{
								ZZJCore.SuanFa.Proc.Log("获取卡信息失败，结束");
								ZZJCore.BackForm.ShowForm("获取卡信息失败！", true);
								return -4;
							}
							YE=gc.YE;
							//然后看看余额够不够
							if (YE < DC)
							{
								ZZJCore.SuanFa.Proc.Log("卡余额不足，结束");
								ZZJCore.BackForm.ShowForm("港内卡余额不足！", true);
								return -2;
							}
							#endregion
							ZZJCore.SuanFa.Proc.Log("查询成功，满足支付条件，返回");
							return 0;
			}
			catch(Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex.ToString());
				ZZJCore.BackForm.ShowForm("支付失败",true);
				return -99;
			}
			
		
		}
		
		/// <summary>
		/// 打印港内卡缴费凭条
		/// </summary>
		public static void PrintGKPT()
		{
			string[] NR1 = new string[] { "港内卡号",  "交易金额", "交易时间" };
			string[] NR2 = new string[] { GangCardNo.ToString(),DC.ToString("C"),DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") };
			ZZJCore.SuanFa.PrintCall.PrintPT("港内卡支付凭证", NR1, NR2, "", false);
		}


		/// <summary>
		/// 查询港内卡信息
		/// </summary>
		/// <param name="GangCardNo">卡号</param>
		/// <param name="GC">港内卡</param>
		/// <returns>gc 港内卡类，0:成功 其他:失败</returns>
		private static int QueryGangCardInfo(string GangCardNo,out GangCard GC)
		{
			try
			{
				ZZJCore.SuanFa.Proc.Log("开始查询港内卡信息");
				XmlMaker Table = new XmlMaker(false);
				Table.Add("czyh", ZZJCore.Public_Var.ZZJ_Config.ExtUserID);//His操作员号   ZZJCore.Public_Var.ZZJ_Config.ExtUserID
				Table.Add("cardno", GangCardNo);//卡号
				Table.Add("cardtype", 1);//卡类型
				Table.Add("patid", "");//账户号
				XmlDocument XD =XMLCore.InvokeHIS("查询患者信息", "101", Table);
				if (XD == null)
				{
					ZZJCore.SuanFa.Proc.Log("港内卡信息查询失败，返回");
					//ZZJCore.ShowMessage.ShowForm("读取港内卡信息失败", "读取港内卡信息失败",true);
					GC=null;
					return -1;
				}
				if (XD.SelectSingleNode("zzxt/result/retcode").InnerText.Trim() != "0")//返回失败
				{
					ZZJCore.SuanFa.Proc.Log("港内卡信息查询失败，返回");
					//ZZJCore.ShowMessage.ShowForm("读取港内卡信息失败", "读取港内卡信息失败", true);
					GC=null;
					return -1;
				}
				ZZJCore.SuanFa.Proc.Log("港内卡信息查询成功,开始创建卡对象");
				GangCard gangcard=new GangCard();
				gangcard.GangCardNo=GangCardNo;//卡号
				gangcard.GangCardName = XD.SelectSingleNode("zzxt/table/hzxm").InnerText.Trim();//姓名
				gangcard.GangCardAccoutnID =  XD.SelectSingleNode("zzxt/table/patid").InnerText.Trim();//账户号
				if(XD.SelectSingleNode("zzxt/table/zhjb").InnerText.Trim() != "")
				{
					gangcard.payLevel = XD.SelectSingleNode("zzxt/table/zhjb").InnerText.Trim();//支付级别
					ZZJCore.SuanFa.Proc.Log("支付级别1.就诊卡 2.港内卡 3.附属卡    此卡为:"+gangcard.payLevel);
					if(gangcard.payLevel!="2"&&gangcard.payLevel!="3")
					{
						ZZJCore.SuanFa.Proc.Log("卡片不是港内卡,返回");
						GC=null;
						return -1;
					}
				}
				
				if(!string.IsNullOrEmpty(XD.SelectSingleNode("zzxt/table/zhje").InnerText.Trim()))
				{
					gangcard.YE=Convert.ToDecimal(XD.SelectSingleNode("zzxt/table/zhje").InnerText.Trim());//余额
				}
				GC=gangcard;
				ZZJCore.SuanFa.Proc.Log("港内卡信息查询成功，创建卡对象成功，返回成功");
				return 0;
			}
			catch(Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex.ToString());
				GC=null;
				return -1;
			}
		}
		//查询港内卡余额  //可能用不到了
		private static int QueryGangCardYE(string GangCardNo,out decimal YE)
		{
			YE=121.13M;
			return 0;
		}

		//港内卡交易 his暂时未开放
		private static int MakeOrderByGangCard(GangCard gc,decimal je)
		{
			return 0;
		}
		
	}//End Class



	
}
