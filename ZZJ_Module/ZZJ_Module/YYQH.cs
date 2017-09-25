using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Mw_Public;
using System.Xml;
using Mw_Voice;
using System.Drawing;
using Mw_MSSQL;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using HL.Devices.DKQ;
using System.Threading;
using System.Text;
using System.Drawing.Printing;
using System.IO;
using ZZJ_Module;
using ZZJCore.ZZJStruct;
using ZZJCore;

public static class YYQH
{
	/// <summary>
	/// 预约信息
	/// </summary>
  public class YYInfo
	{
		public string xh{get;set;}//预约序号
		public string pbmxxh{get;set;}//排版明细号序
		public string yylb{get;set;}//预约类别
		public string yylsh{get;set;}//预约流水号
		public string doctorName{get;set;}//医生姓名
		public string doctorID { get; set; }//医生代码
		public string deptName { get; set; }//科室名称
		public string deptID { get; set; }//科室代码
		public string jzsj { get; set; }//就诊时间
		public string ghf { get; set; }//挂号费
		public string zlf { get; set; }//诊疗费
		public string sjd { get; set; }//就诊时间
		public string isqh{get;set;}//是否已取号
		public string sjh{get;set;}//收据号
		public string zhzf{get;set;}//账户支付
		public string jlxh{get;set;}//记录序号
	}
	public static string zhye="0";
	public static string errorTxt = "";
	public static string YB_Flag = "-1";
	public static bool QHMain()
	{
		string MSG = "";
	
		#region 初始化
		ZZJ_Module.ChargeClass.ChargePayMode = -1;
		ZZJCore.FormSkin.UseRetCard = true;
		ZZJCore.Public_Var.ModuleName = "取预约号";
		ZZJCore.BackForm.ShowForm("正在查询,请稍候...");
		ZZJCore.SuanFa.Proc.Log("开始取预约号");
		XmlMaker Table = XmlMaker.NewTabel();
		Application.DoEvents();
		ZZJCore.SuanFa.Proc.Log("读取配置文件");
		ZZJCore.Initial.Read();
		ZZJCore.SuanFa.Proc.Log("查询患者信息");
		if (XMLCore.GetUserInfo(out MSG) != 0)
		{
			ZZJCore.SuanFa.Proc.Log("查询患者信息失败!结束");
			if (!string.IsNullOrEmpty(MSG)) ZZJCore.BackForm.ShowForm(MSG, true);
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		#endregion

		try
		{
			#region 1.查询已预约信息
			YYXX:
				ZZJCore.SuanFa.Proc.Log("开始查询已预约信息");
				List<YYInfo> listYY=new List<YYInfo>();
				listYY.Clear();
				listYY=GetHisYYInfo(ZZJCore.Public_Var.patientInfo.PatientID);
				if(listYY==null)
				{
					ZZJCore.SuanFa.Proc.Log("查询预约信息失败");
					ZZJCore.BackForm.ShowForm("查询预约信息失败");
					return true;
				}
				ZZJCore.SuanFa.Proc.Log("查询预约信息成功");
			#endregion

			#region 2.选择要取的预约号
			XZQH:
				ZZJCore.SuanFa.Proc.Log("开始选择要取的预约号");
				ZZJCore.SelectDoct.SelectDoctParameter sdp=new ZZJCore.SelectDoct.SelectDoctParameter();
				sdp.Head=null;
				sdp.VDistance = 10;
				sdp.HDistance = 10;
				sdp.DefPage = 0;
				sdp.ContentAlignment=ContentAlignment.MiddleCenter;
				sdp.EndLineAlignment=HorizontalAlignment.Left;
				sdp.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
				List<ZZJCore.ZZJStruct.Doct> listDoct=new List<ZZJCore.ZZJStruct.Doct>();
				foreach(YYInfo y in listYY)
				{
					ZZJCore.ZZJStruct.Doct doct=new ZZJCore.ZZJStruct.Doct();
					if(y.isqh=="0")
					{
						doct.GHButt=true;
						doct.GHbuttString="取号";
					}
					else
					{
						doct.GHButt=false;
						doct.GHbuttString="已取号";
					}
					doct.LineSpacing=15;
					doct.PIC=null;
					doct.Data1=new string[]{"预约科室","预约医生","取号金额","就诊时间"};
					doct.Data2=new string[]{y.deptName,y.doctorName,Convert.ToDecimal(y.ghf).ToString("C"),y.jzsj+" "+y.sjd};

					listDoct.Add(doct);
				}
				sdp.Docts=listDoct.ToArray();
				int iRet=ZZJCore.SelectDoct.ShowForm(sdp);
				if(iRet<0)
				{
					ZZJCore.SuanFa.Proc.Log("未选择要取的预约号");
					ZZJCore.BackForm.CloseForm();
					return true;
				}
				ZZJCore.SuanFa.Proc.Log("选择预约号成功!");
			#endregion
				
			#region 3.扣费
				YYInfo yf=listYY[iRet];
				if(yf.jzsj!= DateTime.Now.ToString("yyyyMMdd"))
				{
					ZZJCore.BackForm.ShowForm("只能当日取号!",true);
					return true;
				}
				decimal ghf=Convert.ToDecimal(yf.ghf);


				ZZJCore.SuanFa.Proc.Log("开始202挂号预算");
				//挂号预算
				Table = XmlMaker.NewTabel(ZZJCore.Public_Var.patientInfo.PatientID);
				//0挂号预算 1正式挂号
				Table.Add("flag", "0");
				//挂号类别：0普通  1急诊  2专家
				// Table.Add("ghlb", SelRegType.ToString());
				Table.Add("ghlb", "9");
				Table.Add("yyxh", yf.yylsh);



				//科室代码
				Table.Add("ksdm", yf.deptID);
				//医生代码  普通时 医生代码为空
				Table.Add("ysdm", yf.doctorID);
				//下列为空
				//Table.Add("ksmc", yf.deptName);//科室名称(不传)
				Table.Add("pbmxxh", yf.pbmxxh);//排班明细序号(不传)
				//Table.Add("ghksdm", yf.deptID);//挂号科室代码(不传)
				Table.Add("ybksdm", "");//医保科室代码 
				Table.Add("ybksmc", "");//医保科室名称(不传)
				
				
				//RecvData.Text = InvokeWeb(NewXML("202", Table));
				string XmlData="";
				XmlDocument XmlDoc;
				
				try
				{

					string DataIn = XmlMaker.NewXML("202", Table).ToString();
					ZZJCore.SuanFa.Proc.BW(DataIn, "Service.202");
					XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
					ZZJCore.SuanFa.Proc.BW(XmlData, "Service.202");
					XmlDoc = new XmlDocument();
					XmlDoc.LoadXml(XmlData);
					string ResultCode = XmlDoc.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();
					string  Msg = XmlDoc.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
					if (ResultCode != "0")
					{
						ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "预算接口调用失败!\r\n" + Msg, true);
						ZZJCore.BackForm.CloseForm();
						//	ZZJCore.BackForm.ShowForm("预算接口调用失败!\r\n" + Msg, true);
						return true;
					}
				}
				catch (System.Exception ex)
				{
					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "挂号接口通讯错误!\r\n" + ex.Message, true);
					ZZJCore.BackForm.CloseForm();
					//	ZZJCore.BackForm.ShowForm("挂号接口通讯错误!\r\n" + ex.Message, true);
					return true;
				}
				//预算成功,开始走挂号接口
				
				Console.WriteLine("预算成功:" + XmlData);
				try
				{
					yf.sjh = XmlDoc.SelectSingleNode("zzxt/table/sjh").InnerText.Trim();
					yf.ghf = XmlDoc.SelectSingleNode("zzxt/table/ysje").InnerText.Trim();
					yf.zhzf = XmlDoc.SelectSingleNode("zzxt/table/zhzf").InnerText.Trim();
					yf.yylb = XmlDoc.SelectSingleNode("zzxt/table/ghlb").InnerText.Trim();
					yf.jlxh = XmlDoc.SelectSingleNode("zzxt/table/jlxh").InnerText.Trim();

				}
				catch (System.Exception ex)
				{
					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "预算接口返回数据错误!\r\n" + ex.Message, true);
					ZZJCore.BackForm.CloseForm();
					//ZZJCore.BackForm.ShowForm("预算接口返回数据错误!\r\n" + ex.Message, true);
					return true;
				}
				ZZJCore.SuanFa.Proc.Log("202接口预算成功");
		#endregion

			#region 3.5 如果是港内卡或者附属卡就弹出输入密码的界面
				
				if (ZZJ_Module.Public_Var.zhjb.ToString() == "2" || ZZJ_Module.Public_Var.zhjb.ToString() == "3")
				{
					ZZJCore.SuanFa.Proc.Log("港内卡输入密码");
					ZZJCore.InputForm.InputPasswordParameter OIPP = new ZZJCore.InputForm.InputPasswordParameter();
					OIPP.Caption = "请输入就诊卡密码";
					OIPP.XButton = false;
					OIPP.RegMode = false;
					OIPP.Buttons = new ZZJButton[] { ZZJControl.Button_Close };
					if (ZZJCore.InputForm.InputPassword(OIPP) != 0)
					{
						ZZJCore.SuanFa.Proc.Log("港内卡未输入密码");
						ZZJCore.BackForm.CloseForm();
						return true;
					}
					Application.DoEvents();
					ZZJCore.SuanFa.Proc.Log("港内卡输入密码成功");
					//	ZZJCore.Public_Var.patientInfo.Password = ZZJCore.SuanFa.Proc.MD5(ZZJCore.Public_Var.patientInfo.Password);
				}
				else
				{
					ZZJCore.Public_Var.patientInfo.Password = "";
				}
				#endregion

			#region  4.挂号缴费（余额足用余额，不足选择其他支付方式）
			StartGHJF:
				ZZJCore.SuanFa.Proc.Log("开始挂号缴费");
				decimal KYE = Convert.ToDecimal(ZZJCore.Public_Var.patientInfo.DepositAmount);
				if (Convert.ToDecimal(ghf) <= KYE)//
				{
					//重置挂号 gh_flag
					ZZJCore.SuanFa.Proc.Log("默认预交金缴费");
					YB_Flag = "0";
					if (Pay(yf,"预交金") != 0)//缴费
					{
						ZZJCore.SuanFa.Proc.Log("挂号扣费失败");
						ZZJCore.BackForm.ShowForm("挂号扣费失败,请稍后再试!" + errorTxt.ToString(), true);
						return true;
					}
					//ZZJCore.SuanFa.Proc.MsgSend(0xB5, 0, true);//向主控发出退卡指令
					//ZZJCore.BackForm.ShowForm("挂号成功!", true);
					//Application.DoEvents();
					//return true;

					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "挂号成功!", true);
					ZZJCore.BackForm.CloseForm();
					if (int.Parse((ZZJCore.Public_Var.ZZJ_Config.ExtUserID).Substring(3, 2)) % 2 != 0)
					{
						if (Convert.ToDecimal(ghf) >= 7)
						{
							GMBL.GMBLMain();
						}
					}
					return true;
				}
				else
				{
					//ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "余额不足，请充值，港内卡不支持充值!", true);
					//ZZJCore.BackForm.CloseForm();
					//return true;
					YB_Flag = "0";
					if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "1")
					{
						ZZJCore.SuanFa.Proc.Log("港内卡余额不足,但不允许充值,取预约号功能结束");
						ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "余额不足,请至人工窗口办理!", true);
						ZZJCore.BackForm.CloseForm();
						return true;
					}
					#region   当开通余额不足时，允许其他方式支付时启用
					ZZJCore.SuanFa.Proc.Log("余额不足,显示其它方式供选择");
					//	decimal gh = Convert.ToDecimal(0.02);
					ZZJ_Module.ChargeClass.ChargeParameter CP = new ZZJ_Module.ChargeClass.ChargeParameter();
					CP.Caption = "";
					CP.PayType = "挂号收费";
					//CP.DC = Convert.ToDecimal(gh_ysje);
					CP.DC = Convert.ToDecimal(ghf);
					int SPMiRet = ZZJ_Module.ChargeClass.Charge(CP);
					Application.DoEvents();
					//if (SPMiRet == -1) return -1;
					//if (SPMiRet == -2) return -2;
					//if (SPMiRet < -2) return -2;
					if (SPMiRet < 0)
					{
						ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "交易取消或交易失败,请稍后再试!", true);
						ZZJCore.BackForm.CloseForm();
						return true;
					}

					int PayiRet = -1;
					if (SPMiRet == 0)
					{
						#region   如果银联扣费成功，先进行充值，在进行扣费
						iRet = YLCZ.Pub_CZ(CP.DC, 1);//传1是说明 充值是从其他地方调用银联充值
						#endregion
						if (iRet == 0)
						{
							YB_Flag = "0";//清空掉这个医保标志,前面不知道谁写的不清空.会造成下个人用银联还是当成医保,最好的办法是做传一个标志参数
							PayiRet = Pay(yf,"银联POS", "", ZZJ_Module.UnionPay.UnionPayCardNo, ZZJ_Module.UnionPay.RETDATA.strRef, "", ZZJ_Module.UnionPay.RETDATA.strBatch, ZZJ_Module.UnionPay.RETDATA.strTrace, ZZJ_Module.UnionPay.RETDATA.strRef, ZZJ_Module.UnionPay.RETDATA.strTId);
							//ZZJ_Module.ChargeClass.SaveData((PayiRet == 0) ? 0 : -1);
						}
						else
						{
							ZZJCore.SuanFa.Proc.Log("银联扣费成功但充值失败");
							ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "扣费成功但充值失败,请到人工窗口处理!", true);
							ZZJCore.BackForm.CloseForm();
							return true;
						}
					}
					if (SPMiRet == 1)
					{
						YB_Flag = "1";
						ZZJCore.SuanFa.Proc.Log("医保支付成功!");
						PayiRet = Pay(yf,"社保银联", "", QDYBCard.GRBH, QDYBCard.sJYLSH, QDYBCard.sYYZFLSH, QDYBCard.PCH, QDYBCard.POSH, QDYBCard.YLJYCKH, QDYBCard.POSH);
						//if(PayiRet!=0)//说明his那边失败了
						//{
						//  ZZJCore.SuanFa.Proc.Log("提交his接口交易结果:"+(PayiRet==0?"提交成功":"提交失败")+"   开始冲正");
						//  //冲正这笔交易
						//  if( QDYBCard.GRBHCXZF(QDYBCard.GRBH,QDYBCard.sYYZFLSH,QDYBCard.sJYLSH,ZJE)==0)//说明失败了
						//  {
						//    ZZJCore.SuanFa.Proc.Log("已冲正");
						//    ZZJCore.BackForm.ShowForm("医保金已冲正",true);
						//  }
						//  else
						//  {
						//    ZZJCore.SuanFa.Proc.Log("冲正失败");
						//    ZZJCore.BackForm.ShowForm("医保金冲正失败,请持医保缴费凭证到窗口办理退费",true);
						//  }
						//}

						ZZJ_Module.ChargeClass.SaveData((PayiRet == 0) ? 0 : -1);
					}
					if (SPMiRet == 2) goto StartGHJF;
					//说明是港内卡支付
					if (SPMiRet == 3)
					{
						YB_Flag = "0";//清空掉这个医保标志,前面不知道谁写的不清空.会造成下个人用银联还是当成医保,最好的办法是做传一个标志参数
						PayiRet = Pay(yf,"港内卡G", "", "", "", "", "", "", "", "");

					}
					if (PayiRet != 0)
					{
						//	isSuccess = "失败";
						//	PrintPT();//银联缴费成功,挂号失败,打印凭条,方便窗口退费
						//	ZZJCore.BackForm.ShowForm("扣费成功但挂号失败,请到人工窗口处理!", true);

						if (PayiRet == -99)//说明是医保方式
						{
							ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "医保扣费成功,挂号失败!请去人工窗口处理", true);
							ZZJCore.SuanFa.Proc.Log("医保扣费成功，挂号失败!结束");
							ZZJCore.BackForm.CloseForm();
							return true;
						}

						ZZJCore.SuanFa.Proc.Log("挂号失败,可能是账户和密码不正确.");
						ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "挂号失败!" + errorTxt, true);
						ZZJCore.BackForm.CloseForm();
						return true;
					}
					ZZJCore.SuanFa.Proc.Log("挂号成功");
					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "挂号成功!", true);
					ZZJCore.BackForm.CloseForm();
					if (int.Parse((ZZJCore.Public_Var.ZZJ_Config.ExtUserID).Substring(3, 2)) % 2 != 0)
					{
						if (Convert.ToDecimal(ghf) >= 7)
						{
							GMBL.GMBLMain();
						}
					}
					return true;
					#endregion

				}
				#endregion
			return true;
		}
		catch(Exception ex)
		{
			ZZJCore.SuanFa.Proc.Log("出现了无法挽救的异常:"+ex.ToString());
			ZZJCore.BackForm.ShowForm("异常!请联系管理员!");
			return true;
		}
	}


	#region 挂号缴费XML
	public static int Pay(YYInfo yy, string PayMode = "", string Bankname = "", string TransCode = "",
									string TransSerialNo = "", string PayTransNo = "", string BatchNo = "", string PostransNo = "", string ReferenceNo = "", string TerminalNo = "")
	{
		#region 整理报文
		string PayModeTemp = PayMode;
		XmlMaker Table = XmlMaker.NewTabel();
		string XmlData = "";
		string ResultCode = "";
		string Msg = "";
		errorTxt = "";
		XmlDocument XmlDoc = new XmlDocument();

		Table = XmlMaker.NewTabel(ZZJCore.Public_Var.patientInfo.PatientID);
		Table.Add("flag", "1");
		//收据号  挂号预算时返回
		Table.Add("sjh", yy.sjh);
		//预算金额  挂号预算时返回   如果银行卡支付成功了，挂号费就为0
		//if (PayMode.ToString() == "银联POS")
		//{
		//  Table.Add("ysje", "0");
		//}
		//else
		//{
		Table.Add("ysje", yy.ghf);
		//	}
		//账户支付  挂号预算时返回
		Table.Add("zhzf", yy.zhzf);
		//挂号类别  挂号预算时返回
		Table.Add("ghlb", yy.yylb);
		//预约序号
		Table.Add("yyxh",yy.yylsh);


		if (PayMode == "预交金")
		{
			if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "1")//港内卡为自己支付
			{
				if (ZZJ_Module.Public_Var.zhjb.ToString() == "2")//港内卡
				{
					Table.Add("zffs", "G");
				}
				else//附属卡
				{
					Table.Add("zffs", "F");
				}
			}
			else //就诊卡
			{
				Table.Add("zffs", "J");
			}
		}
		else if (PayMode == "银联POS")
		{
			Table.Add("zffs", "J");
		}
		else if (PayMode == "社保银联")
		{
			Table.Add("zffs", "10");
		}
		else if (PayMode == "港内卡G")
		{
			if (GangCardPay.gangCard.payLevel == "2")//支付级别为2 
			{
				Table.Add("zffs", "G");
			}
			else
			{
				Table.Add("zffs", "F");//附属卡
			}
		}
		else
		{
			return -3;//返回 来路不明
		}

		//港内卡支付专用
		Table.Add("cardno", PayMode == "港内卡G" ? GangCardPay.GangCardNo : "");//港内卡卡号
		Table.Add("passwd", PayMode == "港内卡G" ? GangCardPay.Psd : ZZJCore.Public_Var.patientInfo.Password);//港内卡密码
		//GH_ZDGHJLK记录序号  挂号预算时返回
		Table.Add("jlxh", yy.jlxh);//
		//密码(可选的,如果卡有密码必填)
		//Table.Add("passwd", ZZJCore.Public_Var.patientInfo.Password);
		//如果缴费方式是医保,才会传医保编号等.	不然会造成 其他方式也传了上一笔的医保支付信息	
		if (PayMode == "社保银联")
		{
			Table.Add("yyzflsh", QDYBCard.sYYZFLSH);
			Table.Add("grbh", QDYBCard.GRBH);
			Table.Add("jylsh", QDYBCard.sJYLSH);
			Table.Add("pch", QDYBCard.PCH);
			Table.Add("posh", QDYBCard.POSH);
			Table.Add("yljyckh", QDYBCard.YLJYCKH);
			Table.Add("je", yy.ghf);
		}
		else
		{
			Table.Add("yyzflsh", "");
			Table.Add("grbh", "");
			Table.Add("jylsh", "");
			Table.Add("pch", "");
			Table.Add("posh", "");
			Table.Add("yljyckh", "");
			Table.Add("je", yy.ghf);
		}


		#endregion

		try
		{
			errorTxt = "";
			string DataIn = XmlMaker.NewXML("202", Table).ToString();
			ZZJCore.SuanFa.Proc.BW(DataIn, "Service.挂号");
			XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
			ZZJCore.SuanFa.Proc.BW(XmlData, "Service.挂号");
			//XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(webserviceUrl, "CallService", new object[] { XmlMaker.NewXML("202", Table).ToString() }) as string;
			XmlDoc = new XmlDocument();
			XmlDoc.LoadXml(XmlData);
			ResultCode = XmlDoc.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();
			Msg = XmlDoc.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
			if (ResultCode == "0")
			{
				if (PayMode == "港内卡G" || ZZJ_Module.Public_Var.zhjb.ToString() == "2")
				{
					if (PayMode == "港内卡G")
					{
						GangCardPay.PrintGKPT();
					}
					else if (PayMode == "预交金")
					{
						PrintGKPT(ZZJ_Module.Public_Var.RCR.KH, Convert.ToDecimal(yy.ghf));
					}
				}
				else if (PayMode == "预交金" && ZZJ_Module.Public_Var.zhjb.ToString() == "1")
				{
					PrintGKPT(Convert.ToDecimal(yy.ghf));
				}
				zhye = XmlDoc.SelectSingleNode("zzxt/table/zhje").InnerText.Trim();
			}

			if (ResultCode != "0")
			{

				errorTxt = Msg;
				if (PayMode == "社保银联")
				{
					ZZJCore.SuanFa.Proc.Log("解析挂号扣费报文时出错." + "ResultCode:" + ResultCode + "Msg:" + Msg.ToString());
					return -99;
				}
				//ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "挂号失败!\r\n" + Msg, true);
				//ZZJCore.BackForm.CloseForm();
				//	ZZJCore.BackForm.ShowForm("挂号失败!\r\n" + Msg, true);
				ZZJCore.SuanFa.Proc.Log("解析挂号扣费报文时出错." + "ResultCode:" + ResultCode + "Msg:" + Msg.ToString());
				return -3;
			}

		}
		catch (Exception e)
		{//解析报文异常,网络异常等不可退费,因为可能缴费成功了
			//ZZJCore.SuanFa.Proc.Log("挂号扣费失败,可能是因为网络错误." + e.Message);
			//ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "结算接口通讯错误!\r\n" + e.Message, true);
			//ZZJCore.BackForm.CloseForm();
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("挂号扣费失败,可能是因为网络错误." + e.Message);
			return -3;
		}
		#region 挂号记录写入数据库
		try
		{
			SaveDataQH(yy,Convert.ToInt32(ResultCode), PayMode);
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("挂号完成,写入数据库失败!" + e.Message);
		}
		#endregion
		//if (PayMode == "预交金") ZZJ_Module.Public_Var.NewAmount = ZZJ_Module.Public_Var.NewAmount - Convert.ToDecimal(gh_ysje);
		if (YB_Flag == "1" || PayMode == "港内卡G")
		{

		}
		else
		{
			ZZJCore.Public_Var.patientInfo.DepositAmount = Convert.ToDecimal(zhye).ToString();
		}
		//isSuccess="成功";
		PrintPT(yy,PayMode);
		return 0;
	}
	#endregion 挂号

	#region 打印挂号凭条
	private static void PrintPT(YYInfo yy, string PayMode)
	{
		string PayProc = "";
		//if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "1")
		//{
		//  PayProc = "港内卡";
		//}
		//if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "0")
		//{
		//  PayProc = "预交金";
		//}

		if (ZZJ_Module.Public_Var.zhjb.ToString() == "1")
		{
			PayProc = "预交金";
		}
		else if (ZZJ_Module.Public_Var.zhjb.ToString() == "2")
		{
			PayProc = "港内卡";
		}
		else
		{
			PayProc = "附属卡";
		}

		if (PayMode == "港内卡G")
		{
			if (GangCardPay.gangCard.payLevel == "2")
			{
				PayProc = "港内卡";
			}
			else if (GangCardPay.gangCard.payLevel == "3")
			{
				PayProc = "附属卡";
			}
		}
		if (ZZJ_Module.ChargeClass.ChargePayMode == 0) PayProc = "银联";
		if (ZZJ_Module.ChargeClass.ChargePayMode == 1) PayProc = "医保";


		List<ZZJCore.ZZJStruct.DataLine> AL = new List<ZZJCore.ZZJStruct.DataLine>();
		AL.Add(new ZZJCore.ZZJStruct.DataLine("姓名", ZZJCore.Public_Var.patientInfo.PatientName, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊卡号", ZZJCore.Public_Var.cardInfo.CardNo, null, null));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("登记号", ZZJCore.Public_Var.patientInfo.PatientID, null, null));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊序号", yy.xh, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊科室", yy.deptName, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("医生名称", yy.doctorName, null, null));
		//	AL.Add(new ZZJCore.ZZJStruct.DataLine("队列号", gh_jlxh, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
		//AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊时间", Convert.ToDateTime(selectRegTime).ToString("yyyy-MM-dd"), null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊日期", yy.jzsj + yy.sjd, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊时间段", yy.sjd, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("挂号费", Convert.ToDecimal(yy.ghf).ToString("C"), null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("账户余额", Convert.ToDecimal(ZZJCore.Public_Var.patientInfo.DepositAmount).ToString("C"), null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));//Convert.ToDecimal(zhye).ToString("C")
		AL.Add(new ZZJCore.ZZJStruct.DataLine("支付方式", PayProc, null, null));
		if (PayMode == "港内卡G")
		{

			AL.Add(new ZZJCore.ZZJStruct.DataLine("港内卡号", GangCardPay.gangCard.GangCardNo, null, null));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("港内卡姓名", GangCardPay.gangCard.GangCardName, null, null));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("港内卡余额", zhye, null, null));
		}
		// ZFFS = "银联"
		//if (isSuccess.ToString()!=""||isSuccess.ToString()!=null)
		//{
		//AL.Add(new ZZJCore.ZZJStruct.DataLine("挂号描述", isSuccess, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
		//	}
		AL.Add(new ZZJCore.ZZJStruct.DataLine("流水号", ZZJCore.Public_Var.SerialNumber, null, null));
		AL.Add(new ZZJCore.ZZJStruct.DataLine("打印时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), null, new Font(new FontFamily("黑体"), 13, FontStyle.Bold)));
		bool[] HideTYData = new bool[] { true, true, true, true, true, true };
		if (ZZJCore.SuanFa.PrintCall.PrintPT("门诊挂号凭证", AL.ToArray(), "当日挂号请于当日" + yy.sjd + "时内就诊，就诊时请出示本凭条和病历!", false, HideTYData))
		{
			ZZJCore.SuanFa.Proc.Log("->凭条打印成功");
		}
		else
		{
			ZZJCore.SuanFa.Proc.Log("->凭条打印打印异常");
		}
	}

	#endregion

	#region 打印缴费凭条
	/// <summary>
	/// 打印港内卡缴费凭条
	/// </summary>
	public static void PrintGKPT(string KH, decimal je)
	{
		string[] NR1 = new string[] { "港内卡号", "交易金额", "交易时间" };
		string[] NR2 = new string[] { KH.ToString(), je.ToString("C"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") };
		ZZJCore.SuanFa.PrintCall.PrintPT("港内卡支付凭证", NR1, NR2, "", false);
	}

	/// <summary>
	/// 打印预交金缴费凭条
	/// </summary>
	public static void PrintGKPT(decimal je)
	{
		string[] NR1 = new string[] { "交易金额", "交易时间" };
		string[] NR2 = new string[] { je.ToString("C"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") };
		ZZJCore.SuanFa.PrintCall.PrintPT("预交金支付凭证", NR1, NR2, "", false);
	}
	#endregion

	#region 获取his预约信息列表

	/// <summary>
	/// 获取病号的已预约信息
	/// </summary>
	/// <param name="patid">病人编号</param>
	/// <returns>预约信息列表</returns>
	public static List<YYInfo> GetHisYYInfo(string patid)
	{
		
		List<YYInfo> listYY = new List<YYInfo>();
		XmlMaker Table = XmlMaker.NewTabel();
		Table.Add("patid", patid);// 病人ID
	
		try
		{
			string DataIn = XmlMaker.NewXML("304", Table).ToString();
			ZZJCore.SuanFa.Proc.BW(DataIn, "Service.304");
			string XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
			ZZJCore.SuanFa.Proc.BW(XmlData, "Service.304");
			XmlDocument XmlDoc = new XmlDocument();
			XmlDoc.LoadXml(XmlData);
			XmlDocument XD=new XmlDocument();
			XD.LoadXml(XmlData);
				//<zzxt><result><retcode>-1</retcode><retmsg>未取到数据</retmsg></result></zzxt>
			string retCode=XD.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();//回参  0是查询成功 其他是失败
				if(retCode.Trim()!="0")//说明失败了
				{
						string errorMsg=XD.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
						ZZJCore.SuanFa.Proc.Log("his查询预约记录失败,可能的原因为:" + errorMsg.ToString());
						ZZJCore.SuanFa.Proc.Log("[取预约号功能结束]");
						ZZJCore.BackForm.ShowForm("his查询预约记录失败", true);
						return null;
				}
				else//说明成功了
				{
					if (string.IsNullOrEmpty(XD.SelectSingleNode("zzxt/tablecount").InnerText))//说明没有记录
					{
						ZZJCore.SuanFa.Proc.Log("his没有预约记录");
						ZZJCore.SuanFa.Proc.Log("预约挂号功能结束");
						ZZJCore.BackForm.CloseForm();
						ZZJCore.BackForm.ShowForm("his没有预约记录", true);
						return null;
					}
					int tableCount = Convert.ToInt32(XD.SelectSingleNode("zzxt/tablecount").InnerText);
					if (tableCount<=0)
					{
						return null;
					}
					for(int i=1;i<=tableCount;i++)
					{
						string src = string.Format("zzxt/table{0}", i.ToString());
						YYInfo yy=new YYInfo();
						yy.jzsj=XD.SelectSingleNode(src+"/yyrq").InnerText.Trim();//预约执法犯法 
						yy.xh = XD.SelectSingleNode(src + "/yyhx").InnerText.Trim();//预约号序
						yy.yylsh = XD.SelectSingleNode(src + "/yylsh").InnerText;//预约流水号
						yy.sjd = XD.SelectSingleNode(src + "/sjd").InnerText.Trim();//预约时间段
						yy.deptName = XD.SelectSingleNode(src + "/ksmc").InnerText.Trim();//科室名称
						yy.doctorName = XD.SelectSingleNode(src + "/ysmc").InnerText.Trim();//医生名称
						yy.yylb = XD.SelectSingleNode(src + "/ghlb").InnerText.Trim();//预约 类别 
						yy.deptID = XD.SelectSingleNode(src + "/ksdm").InnerText.Trim();//科室代码 
						yy.doctorID = XD.SelectSingleNode(src + "/ysdm").InnerText.Trim();//医生代码
						yy.pbmxxh = XD.SelectSingleNode(src + "/pbmxxh").InnerText.Trim();//排版明细序号
						yy.isqh = XD.SelectSingleNode(src + "/isqh").InnerText.Trim();//是否取号
						yy.ghf  = XD.SelectSingleNode(src + "/yyje").InnerText.Trim();//挂号费
						listYY.Add(yy);
					}
				}
			}
			catch(Exception ex)
			{
				return null;
			}
		return listYY;
	}
	#endregion

	#region 数据库操作
	private static void Init_DBConfig()//加载数据库
	{
		try
		{
			if (MSSQL_Init.Init_DBConfig() != 0)
			{
				ZZJCore.SuanFa.Proc.Log("加载数据库配置文件失败");//加载配置文件失败
				return;
			}
			ZZJCore.SuanFa.Proc.Log("加载配置文件成功");//加载配置文件成功
			if (MSSQL_Operate.Test_MSSQL_Conn() != 0)
			{//数据库测试连接失败
				ZZJCore.SuanFa.Proc.Log("数据库测试连接失败");
				return;
			}
			ZZJCore.SuanFa.Proc.Log("数据库测试连接成功");//数据库测试连接成功
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
		}
	}

	#region 取号入库
	private static SqlParameter AddGH(SqlDbType SDT, string PName, object Value)
	{
		SqlParameter parameter = new SqlParameter();
		parameter.SqlDbType = SDT;
		parameter.ParameterName = PName;
		parameter.Value = Value;
		return (parameter);
	}
	/// <summary>
	/// 挂号存数据库
	/// </summary>
	/// <param name="yy">预约信息类</param>
	/// <param name="QHBZ">取号成功标志</param>
	/// <param name="QHZFFS">支付方式</param>
	public static void SaveDataQH(YYInfo yy,int QHBZ,string QHZFFS)//挂号入库
	{
		string msg = "";
		#region 网络数据库
		Init_DBConfig();
		try
		{
			MSSQL_Operate mssql_operate = new MSSQL_Operate();
			SqlParameter[] parameter = null;
			List<SqlParameter> AL = new List<SqlParameter>();
			AL.Add(AddGH(SqlDbType.VarChar, "@KH", ZZJCore.Public_Var.cardInfo.CardNo));//--就诊卡号
			AL.Add(AddGH(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.patientInfo.PatientID));//登记号
			AL.Add(AddGH(SqlDbType.VarChar, "@KPLX", ZZJCore.Public_Var.cardInfo.CardType));//卡片类型
			AL.Add(AddGH(SqlDbType.VarChar, "@XM ", ZZJCore.Public_Var.patientInfo.PatientName));//姓名
			AL.Add(AddGH(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//HIS操作员
			AL.Add(AddGH(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));//自助机编号
			AL.Add(AddGH(SqlDbType.DateTime, "@GHSJ", DateTime.Now.ToString()));//挂号时间
			AL.Add(AddGH(SqlDbType.VarChar, "@JZSJ", yy.jzsj+" "+yy.sjd));//就诊时间
			AL.Add(AddGH(SqlDbType.VarChar, "@JE", yy.ghf));//总金额	 本次费用发生总金额
			AL.Add(AddGH(SqlDbType.VarChar, "@YSXM", yy.doctorName));//医生姓名
			AL.Add(AddGH(SqlDbType.VarChar, "@KS", yy.deptName));//科室名称
			AL.Add(AddGH(SqlDbType.VarChar, "@YSJB", ""));//医生级别
			AL.Add(AddGH(SqlDbType.VarChar, "@GHBZ", QHBZ == 0 ? "成功" : "失败"));//挂号标志
			int qhzf=0;
			switch (QHZFFS)
			{//--支付方式 (0：现金； 1 ：预交金； 2：社卡保金； 3： 区域账户； 4：银联卡）
				case "预交金"://
					{
						AL.Add(AddGH(SqlDbType.VarChar, "@LSH", ZZJ_Module.Public_Var.orderNo));//流水号
						AL.Add(AddGH(SqlDbType.VarChar, "@ZFLSH ", ZZJ_Module.Public_Var.sFlowId));//支付流水号
						qhzf=1;
						break;
					}
				case "社保银联":
					{
						AL.Add(AddGH(SqlDbType.VarChar, "@LSH", QDYBCard.sJYLSH));//流水号
						AL.Add(AddGH(SqlDbType.VarChar, "@ZFLSH ", QDYBCard.sYYZFLSH));//支付流水号
						qhzf=2;
						break;
					}
				case "银联POS":
					{
						AL.Add(AddGH(SqlDbType.VarChar, "@LSH", ZZJ_Module.UnionPay.RETDATA.strBatch));//流水号
						AL.Add(AddGH(SqlDbType.VarChar, "@ZFLSH ", ZZJ_Module.UnionPay.RETDATA.strTrace));//支付流水号
						qhzf=4;
						break;
					}
				default:
					{
						AL.Add(AddGH(SqlDbType.VarChar, "@LSH", ZZJ_Module.Public_Var.orderNo));//流水号
						AL.Add(AddGH(SqlDbType.VarChar, "@ZFLSH ", ZZJ_Module.Public_Var.sFlowId));//支付流水号
						qhzf=1;
						break;
					}
			}

			AL.Add(AddGH(SqlDbType.Int, "@ZFFS", qhzf));//支付方式
			AL.Add(AddGH(SqlDbType.Decimal, "@ZFJE", yy.ghf));//支付金额
			AL.Add(AddGH(SqlDbType.Int, "@ZFJG", QHBZ.ToString()));//支付结果
			AL.Add(AddGH(SqlDbType.VarChar, "@ZFJGMS", 0));//支付结果描述
			AL.Add(AddGH(SqlDbType.VarChar, "@BL1", yy.yylsh));//收据号
			AL.Add(AddGH(SqlDbType.VarChar, "@BL2", "取号"));
			AL.Add(AddGH(SqlDbType.VarChar, "@BL3", yy.pbmxxh));
			AL.Add(AddGH(SqlDbType.VarChar, "@ZZJLSH", ZZJCore.Public_Var.SerialNumber));
			parameter = AL.ToArray();
			if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_MZ_DRGH_MX_V4]", parameter, out msg) > 0)
			{
				ZZJCore.SuanFa.Proc.Log("记录写入服务器成功");
				return;
			}
			ZZJCore.SuanFa.Proc.Log("记录写入服务器失败" + msg);
		}
		catch (Exception Ex)
		{
			ZZJCore.SuanFa.Proc.Log(Ex);
			ZZJCore.SuanFa.Proc.Log("取号记录写入服务器失败，可能是网络中断或远程自助机服务器未开启，" + Ex.Message);
		}
		#endregion
	}
	#endregion

	#endregion


}