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
using ZZJCore;
using ZZJ_Module;

namespace ZZJ_Module
{
	public static class DoctorPay
	{
		public class FJFData
		{
			public string other;
			public decimal JE;
		}

		public static int PayJF(decimal DC, string RegSerNo, string CLINIC_LABEL, string PayWay, string Bank, string Trancode, string TransserailNo, string TranspayNo, string PosbatchNo, string PostransNo, string ReferenceNo)
		{
	//2016-8-14 注释语音		if (ZZJCore.Public_Var.IsVoice) Talker.Speak("正在缴费,请稍候...");
			ZZJCore.BackForm.ShowForm("正在缴费,请稍候...");
			Application.DoEvents();

			#region  添加绑定的费用到缴费数据
			try
			{
				string OutXML = "<Request><Autobill>";
				OutXML += "<CardNo>" + ZZJCore.Public_Var.patientInfo.PatientID + "</CardNo>";
				OutXML += "<UserID>" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "</UserID>";
				OutXML += "<RegserNO>" + RegSerNo + "</RegserNO>";
				OutXML += "<ClinicLable>" + CLINIC_LABEL + "</ClinicLable>";
				OutXML += "</Autobill></Request>";
				ZZJCore.SuanFa.Proc.BW(OutXML, "添加绑定数据");
				string InXML = "";
				if (ZZJCore.SuanFa.Proc.ReadPublicINI("YYQHJFDebugData", "0") == "0") InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(MEF.URL, "AutoBillBinding", new string[] { OutXML }) as string;
				ZZJCore.SuanFa.Proc.BW(InXML, "添加绑定数据");
			}
			catch (Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log("添加绑定数据时异常," + ex.ToString());
				return -3;
			}
			#endregion
			#region 查询绑定费用
			//string YZID = "";//医嘱集合
			decimal YZJE = 0;
			List<XmlNode> AL = new List<XmlNode>();
			XmlNode[] BDSJ = null;//绑定代缴费的数据
			//附加费医嘱
			List<FJFData> FJFYZ = new List<FJFData>();
			try
			{
				#region 整理报文查询
				string OutXML = "<Request>";
				OutXML += "<CardNo>" + ZZJCore.Public_Var.patientInfo.PatientID + "</CardNo>";
				OutXML += "<UserId>" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "</UserId>";
				OutXML += "<SecrityNo>666</SecrityNo>";
				OutXML += "<CardSerNo></CardSerNo>";
				OutXML += "</Request>";
				ZZJCore.SuanFa.Proc.BW(OutXML, "获取缴费信息");
				string InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(MEF.URL, "GetBillInfo", new string[] { OutXML }) as string;
				ZZJCore.SuanFa.Proc.BW(InXML, "获取缴费信息");
				#endregion
				#region 测试数据
				if (ZZJCore.SuanFa.Proc.ReadPublicINI("YYQHJFDebugData", "0") == "0") goto DebugDataEnd;
				RegSerNo = "68576";
				InXML = @"<Response>
<Item>
	<VisitDate>2016/6/6 0:00:00</VisitDate>
	<ItemId>311201023</ItemId>
	<DeptName>产前检查</DeptName>
	<CateId>D</CateId>
	<CateName>检查</CateName>
	<Price>5</Price>
	<Num>1.0000</Num>
	<CtLoc>东院产科门诊</CtLoc>
	<other>20160321-68576-0-自助-D-D3108</other>
	<Unit>次</Unit>
</Item>
<Item>
	<VisitDate>2016/6/6 13:00:00</VisitDate>
	<ItemId>311201026b</ItemId>
	<DeptName>胎心监护仪下进行胎心监测</DeptName>
	<CateId>D</CateId>
	<CateName>检查</CateName>
	<Price>2</Price>
	<Num>1.0000</Num>
	<CtLoc>东院产科门诊</CtLoc>
	<other>20160321-68576-0-自助-D-D3108</other>
	<Unit>次</Unit>
</Item>
<Item>
<VisitDate>2016/6/6 13:00:00</VisitDate>
<ItemId>311201023</ItemId>
<DeptName>产前检查</DeptName>
<CateId>D</CateId>
<CateName>检查</CateName>
<Price>2</Price>
<Num>1.0000</Num>
<CtLoc>东院产科门诊</CtLoc>
<other>20160322-68576-0-自助-K-D3108</other>
<Unit>次</Unit>
</Item>
<ResultCode>0</ResultCode><ErrorMsg></ErrorMsg></Response>
"; // */
			DebugDataEnd:
				#endregion
				#region 数据解析
				XmlDocument docXML = new XmlDocument();
				docXML.LoadXml(InXML);
				if (docXML.SelectSingleNode("Response/ResultCode").InnerText.Trim() != "0")
				{
					ZZJCore.SuanFa.Proc.Log("查询缴费信息不成功");
					return -3;
				}
				XmlNodeList nodelists = docXML.SelectNodes("Response/Item");
				if (nodelists.Count <= 0)
				{
					ZZJCore.SuanFa.Proc.Log("查询缴费信息不成功");
					return -3;
				}
				List<XmlNode> BDALList = new List<XmlNode>();//绑定的数据集合
				foreach (XmlNode node in nodelists)
				{
					BDALList.Add(node);
				}
				if (BDALList.Count == 0) return -3;
				BDSJ = BDALList.ToArray();
				#endregion
				#region 根据缴费得到相同医嘱和总金额
				//相同医嘱的总金额
				foreach (XmlNode node in BDSJ)//枚举医嘱
				{
					DateTime VisitDate = DateTime.Parse(node.SelectSingleNode("VisitDate").InnerText.Trim());
					decimal Price = Convert.ToDecimal(node.SelectSingleNode("Price").InnerText.Trim());
					string other = node.SelectSingleNode("other").InnerText.Trim();
					if (
						node.SelectSingleNode("other").InnerText.Trim().IndexOf("自助") < 0 ||
						node.SelectSingleNode("other").InnerText.Trim().IndexOf("-" + RegSerNo + "-") < 0 ||
						VisitDate.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd")
						) continue;
					YZJE += Price;
					AL.Add(node);

					FJFData[] FA = (from FJFData FFF in FJFYZ where FFF.other == other select FFF).ToArray();
					if (FA == null) FA = new FJFData[0];
					if (FA.Length > 0)
					{
						FA[0].JE += Price;
						continue;
					}
					FJFData FJF = new FJFData();
					FJF.JE = Price;
					FJF.other = other;
					FJFYZ.Add(FJF);
				}
				if (FJFYZ.Count == 0) return -3;
				#endregion
			}
			catch (Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log("查询绑定费用时异常" + ex.ToString());
				return -3;
			}
			if (DC != YZJE)
			{
				ZZJCore.SuanFa.Proc.Log(string.Format("预交金金额统计结果与挂号信息不符!统计结果:{0} 挂号信息返回:{1}", YZJE, DC));
				return -3;
			}
			#endregion

			foreach (FJFData FD in FJFYZ)
			{
				if (FJFJF(FD.other, PayWay, FD.JE, Trancode, TransserailNo, TranspayNo, PosbatchNo, PostransNo, ReferenceNo) != 0)
				{
					XmlNode[] XNL = (from XmlNode XN in AL where XN.SelectSingleNode("other").InnerText.Trim() == FD.other select XN).ToArray();
					foreach (XmlNode XN in XNL)
					{
						AL.Remove(XN);
					}
				}
			}
			XM = AL.ToArray();
			ZZJCore.SuanFa.PrintCall.Print(ZZJCore.Public_Var.ZZJ_Config.PT_Dev, PC, 300, 300 + (XM.Length * 20), false);
			return 0;
		}

		public static int FJFJF(string YZID, string PayWay, decimal YZJE, string Trancode, string TransserailNo, string TranspayNo, string PosbatchNo, string PostransNo, string ReferenceNo)
		{
			if (PayWay == "区域预交")
			{
				if (ZZJ_Module.UserData.yjjxf(YZJE) != 0)
				{
					ZZJCore.SuanFa.Proc.Log("区域收费失败!" + ZZJ_Module.UserData.Msg);
					return -2;
				}
				TransserailNo = ZZJ_Module.UserData.sFlowId;
				PostransNo = ZZJ_Module.UserData.LSH;
				Trancode = ZZJCore.Public_Var.cardInfo.CardNo;
			}
			#region 缴绑定费用走缴费接口
			try
			{
				#region 整理报文并查询
				string OutXML = "<Request>";
				OutXML += "<CardNo>" + ZZJCore.Public_Var.patientInfo.PatientID + "</CardNo>";
				OutXML += "<RcptGroupID>" + YZID + "</RcptGroupID>";
				OutXML += "<Amt>" + YZJE.ToString() + "</Amt>";
				OutXML += "<SecrityNo>666</SecrityNo>";
				OutXML += "<CardSerNo></CardSerNo>";
				OutXML += "<Moneytype>" + PayWay + "</Moneytype>";
				OutXML += "<TransType>院内自助</TransType>";
				OutXML += "<Bankname>" + ZZJCore.Public_Var.cardInfo.CardType + "</Bankname>";//开户行
				OutXML += "<TransCode>" + Trancode + "</TransCode>";//银行卡号社保个人编号
				OutXML += "<TransSerialNo>" + TransserailNo + "</TransSerialNo>";//交易流水号
				OutXML += "<PayTransNo>" + TranspayNo + "</PayTransNo>";//医院支付流水号
				OutXML += "<BatchNo>" + PosbatchNo + "</BatchNo>";//银联批次号
				OutXML += "<PostransNo>" + PostransNo + "</PostransNo>";//pos流水号
				OutXML += "<ReferenceNo>" + ReferenceNo + "</ReferenceNo>";//参考号
				OutXML += "<TerminalNo></TerminalNo>";
				OutXML += "<UserId>" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "</UserId>";
				OutXML += "</Request>";
				ZZJCore.SuanFa.Proc.BW(OutXML, "取号交附加费");
				string InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(MEF.URL, "AutoOPBillCharge", new string[] { OutXML }) as string;
				if (ZZJCore.SuanFa.Proc.ReadPublicINI("YYQHJFDebugData", "0") == "1")
				{
					InXML = @"<Response>
					<ResultCode>0</ResultCode>
					<RcptNo>12345678</RcptNo>
					</Response>";
				}

				ZZJCore.SuanFa.Proc.BW(InXML, "取号交附加费");
				#endregion
				XmlDocument docXML = new XmlDocument();
				docXML.LoadXml(InXML);
				if (docXML.SelectSingleNode("Response/ResultCode").InnerText.Trim() != "0")
				{
					if (PayWay == "区域预交") ZZJ_Module.UserData.BackCash();//如果有明确的失败提示,则退费
					ZZJCore.SuanFa.Proc.Log("交绑定费用失败");
					return -3;
				}
				ReceiptNo = docXML.SelectSingleNode("Response/RcptNo").InnerText.Trim();
			}
			catch (Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log("交绑定费用异常" + ex.ToString());
				return -5;
			}
			#endregion
			return 0;
		}
		
		#region 打印凭条
		private static XmlNode[] XM = null;
		private static string ReceiptNo = "";//附加费用收费收据号
		private static void PC(object sender, PrintPageEventArgs e)
		{
			decimal JES = 0;//每种费用的总额
			string print_datetime = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss");
			int HP = 0, WP = 20;
			//Graphics graphics = Graphics();
			#region 初始化字体
			Pen pen = new Pen(Color.Black, 1);
			pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
			pen.DashPattern = new float[] { 10, 2 };
			Font NRFont = new Font(new FontFamily("黑体"), 10);//内容字体
			Font BTFont = new Font(new FontFamily("黑体"), 18);//标题字体
			Font LXBTFont = new Font(new FontFamily("黑体"), 15);//类型表头字体
			#endregion
			//抬头
			SizeF sizeF = e.Graphics.MeasureString(ZZJCore.Public_Var.ZZJ_Config.DwMc, BTFont);//获取内容的宽度
			e.Graphics.DrawString(ZZJCore.Public_Var.ZZJ_Config.DwMc, BTFont, System.Drawing.Brushes.Black, WP + ((300 - sizeF.Width) / 2), HP);
			HP += 28;
			e.Graphics.DrawLine(pen, 30, HP, 270, HP);
			HP += 5;
			//sizeF = graphics.MeasureString("医嘱缴费凭证", BTFont);//获取内容的宽度
			e.Graphics.DrawString("医嘱缴费凭证", BTFont, System.Drawing.Brushes.Black, (130 - WP) / 2, HP);
			HP += 30;
			//e.Graphics.DrawString("患者病历号：" + patientInfo.PatientID, NRFont, System.Drawing.Brushes.Black, WP, HP);
			//HP += 16;
			e.Graphics.DrawString("患者姓名：" + ZZJCore.Public_Var.patientInfo.PatientName, NRFont, System.Drawing.Brushes.Black, WP, HP);//
			HP += 16;
			e.Graphics.DrawString("就诊卡号：" + ZZJCore.Public_Var.cardInfo.CardNo, NRFont, System.Drawing.Brushes.Black, WP, HP);//
			HP += 16;
			e.Graphics.DrawString("收据号:" + ReceiptNo, NRFont, System.Drawing.Brushes.Black, WP, HP);//
			HP += 16;
			//e.Graphics.DrawString("账户余额：" + YJJE.ToString("C"), NRFont, System.Drawing.Brushes.Black, WP, HP);//
			//HP += 16;
			e.Graphics.DrawString("自助机编号：" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID, NRFont, System.Drawing.Brushes.Black, WP, HP);
			HP += 16;
			e.Graphics.DrawString("打印时间：" + print_datetime, NRFont, System.Drawing.Brushes.Black, WP, HP);
			HP += 16;
			e.Graphics.DrawString("流水号：" + ZZJCore.Public_Var.SerialNumber, NRFont, System.Drawing.Brushes.Black, WP, HP);
			HP += 16;
			e.Graphics.DrawLine(pen, 30, HP, 270, HP);
			HP += 5;
			e.Graphics.DrawString("项目", NRFont, System.Drawing.Brushes.Black, WP, HP);
			e.Graphics.DrawString("单价", NRFont, System.Drawing.Brushes.Black, 190, HP);
			e.Graphics.DrawString("数量", NRFont, System.Drawing.Brushes.Black, 250, HP);
			HP += 16;
			string DeptName = "";
			foreach (XmlNode nodes in XM)
			{
				#region 打印项目名称,支持换行
				int MeiHangZiShu = 13;//每行字数
				string XMName = nodes.SelectSingleNode("DeptName").InnerText.Trim();//项目名称
				DeptName = nodes.SelectSingleNode("CtLoc").InnerText.Trim();//位置
				while (XMName.Length > MeiHangZiShu)
				{
					e.Graphics.DrawString(XMName.Substring(0, MeiHangZiShu), NRFont, System.Drawing.Brushes.Black, WP, HP);
					HP += 16;
					XMName = XMName.Substring(MeiHangZiShu, XMName.Length - MeiHangZiShu);//去前部
				}
				e.Graphics.DrawString(XMName, new Font(new FontFamily("黑体"), 10), System.Drawing.Brushes.Black, WP, HP);
				#endregion
				JES += Convert.ToDecimal(nodes.SelectSingleNode("Price").InnerText.Trim());
				e.Graphics.DrawString(nodes.SelectSingleNode("Price").InnerText.Trim(), NRFont, System.Drawing.Brushes.Black, 190, HP);
				string Pcs = nodes.SelectSingleNode("Num").InnerText.Trim();
				Pcs = Pcs.Substring(0, Pcs.Length - 2);
				e.Graphics.DrawString(Pcs, NRFont, System.Drawing.Brushes.Black, 250, HP);
				HP += 26;
			}

			e.Graphics.DrawString("已缴金额：" + JES.ToString("C"), LXBTFont, System.Drawing.Brushes.Black, WP, HP);
			HP += 26;
			e.Graphics.DrawString("位置：" + DeptName, LXBTFont, System.Drawing.Brushes.Black, WP, HP);
			JES = 0;
			HP += 25;
			/*
			e.Graphics.DrawLine(pen, 30, HP, 270, HP);
			HP += 5;
			e.Graphics.DrawString("温馨提示：", NRFont, System.Drawing.Brushes.Black, WP, HP);
			HP += 16;
			e.Graphics.DrawString("　　如有药品请到本凭条上标示的窗口领取。", NRFont, System.Drawing.Brushes.Black, WP, HP);
			HP += 16;// */
		}
		#endregion

		
	}
}
