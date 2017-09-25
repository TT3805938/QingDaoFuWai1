using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ZZJ_Module
{
	public static class UserData
	{
		public static string Msg = "";

		/// <summary>
		/// 预交金充值
		/// </summary>
		/// <param name="platinfo"></param>
		/// <param name="Msg"></param>
		/// <returns></returns>
		public static int YJJCZ(decimal DC = 0, string tradeMode = "CA")
		{
			ItemBox Box = new ItemBox("yuantu.wap.recharge.virtual.settlement");
			string rString = "";
			Box.Add("patientId", Public_Var.PF.patientId);
			//卡号
			Box.Add("cardNo", ZZJCore.Public_Var.cardInfo.CardNo);
			//卡类型
			Box.Add("cardType", "2");
			//交易方式  DB 银联 CA 现金
			Box.Add("tradeMode", tradeMode);

			if (tradeMode == "DB")
			{
				Box.Add("posTransNo", QDYLLib.QDYL.InData.strTrace);//终端流水号
				Box.Add("bankTransNo", QDYLLib.QDYL.InData.strRef);//系统参考号(中心流水号)
				Box.Add("bankCardNo", QDYLLib.QDYL.InData.strCardNo);//银行卡卡号

				try
				{//转日期时间格式
					DateTime DT = DateTime.Now;
					DT = DateTime.Parse(string.Format("{3}-{4}-{5} {0}:{1}:{2}",
						QDYLLib.QDYL.InData.strTransTime.Substring(0, 2),
						QDYLLib.QDYL.InData.strTransTime.Substring(2, 2),
						QDYLLib.QDYL.InData.strTransTime.Substring(4, 2),
						DateTime.Now.Year,
						QDYLLib.QDYL.InData.strTransDate.Substring(0, 2),
						QDYLLib.QDYL.InData.strTransDate.Substring(2, 2)
					));
					Box.Add("bankDate", DT.ToString("yyyy-MM-dd"));//银行交易日期
					Box.Add("bankTime", DT.ToString("HH:mm:ss"));//银行交易时间
				}
				catch (Exception E)
				{//如果出了异常就按照原来的样子上传
					ZZJCore.SuanFa.Proc.Log(E);
					Box.Add("bankDate", QDYLLib.QDYL.InData.strTransDate);//银行交易日期
					Box.Add("bankTime", QDYLLib.QDYL.InData.strTransTime);//银行交易时间
				}

				Box.Add("bankSettlementTime", "23:00:00");//银行结账时间
				Box.Add("deviceInfo", QDYLLib.QDYL.InData.strTId);//终端号
				Box.Add("posIndexNo", QDYLLib.QDYL.InData.strBatch);//批次号
				Box.Add("sellerAccountNo", QDYLLib.QDYL.InData.strMId);//商户号
			}

			//金额 分
			Box.Add("cash", (DC * 100).ToString("0"));
			//1 医院 2 门诊
			Box.Add("inHos", "1");
			//流水号
			Box.Add("flowId", ZZJCore.Public_Var.SerialNumber);
			//交易时间
			Box.Add("tradeTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			//终端号
			Box.Add("terminalNo", ZZJCore.Public_Var.SerialNumber);
			//订单号
			Box.Add("orderNo", ZZJCore.Public_Var.SerialNumber);

			try
			{
				ZZJCore.SuanFa.Proc.BW(Box.GetString(), "区域充值接口"); //保存报文
				rString = YuanTuGatewaydo.Gateway.Query(MEF.GateUrl, Box.GetString());
				ZZJCore.SuanFa.Proc.BW(rString, "区域充值接口"); //保存报文
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.Log("*区域接口调用失败!" + e.Message);
				ZZJCore.SuanFa.Proc.BW(string.Format("{0}\n{1}", e.Message, e.StackTrace), "区域充值接口"); //保存报文
			}

			if (string.IsNullOrEmpty(rString))
			{
				Msg = "服务器错误 !";
				return -1;
			}
			try
			{
				string returns = GetJsonValue(rString, "success");
				if (returns.ToUpper() == "TRUE")
				{
					JObject dataString = JObject.Parse(JObject.Parse(rString)["data"].ToString());
					// data里面没数据
					if (dataString.Count < 1)
					{
						Msg = "没有查到用户信息!";
						return 1;
					}
					//返回
					Public_Var.orderNo = dataString["orderNo"].ToString();
					Public_Var.sFlowId = dataString["sFlowId"].ToString();

					Msg = Public_Var.orderNo + " " + Public_Var.sFlowId;

					ZZJCore.Public_Var.patientInfo.DepositAmount = (decimal.Parse(dataString["cash"].ToString()) / 100).ToString();
					return 0;
				}
				Msg = GetJsonValue(rString, "msg");
				return -1;
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				Msg = ex.ToString();
			}
			if (rString == "") Msg = "服务器返回数据错误";
			return -1;
		}

		public static string GetJsonValue(string sJson, string sname)
		{
			string returns = "";
			try
			{
				JObject jsonObj = JObject.Parse(sJson);
				returns = jsonObj[sname].ToString();
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
			}
			return returns;
		}

		public class PostGuaHao
		{
			//挂号方式 1预约 2挂号
			public byte regModel = 1;
			//上下午标志 1上午  2下午
			public byte medAmPm = 0;
			//就诊时间 HH:mmss
			public DateTime medTime = DateTime.Now;
			//科室名称
			public string deptName = "";
			//科室名称
			public string deptCode = "";
			//医生名称
			public string doctName = "";
			//医生代码
			public string doctCode = "";
			//队列号
			public string appoNo = "";
			//金额
			public decimal cash = 0;
			//交易方式//可以为空
			public string tradeMode = "";
			//号源类别 1普通 2专家 3 名医 4急诊
			public byte regType = 1;

			public string thirdPartyRegId;
		}

		/// <summary>
		/// 三方预约/挂号记录上传接口
		/// </summary>
		/// <param name="INFO"></param>
		/// <param name="Msg"></param>
		/// <returns></returns>
		public static int PostGuaHaoData(PostGuaHao INFO)
		{
			JObject jsonObj;
			ItemBox Box = new ItemBox("yuantu.wap.upload.reg.record");
			//挂号方式 1预约 2挂号
			Box.Add("regModel", INFO.regModel.ToString());
			//证件号码
			Box.Add("idNo", ZZJCore.Public_Var.patientInfo.IDNo);
			Box.Add("name", ZZJCore.Public_Var.patientInfo.PatientName);
			Box.Add("phone", ZZJCore.Public_Var.patientInfo.Mobile);
			Box.Add("medAmPm", INFO.medAmPm);
			Box.Add("medDate", DateTime.Now.ToString("yyyy-MM-dd"));
			Box.Add("medTime", INFO.medTime.ToString("HH:mm:ss"));
			Box.Add("regType", INFO.regType.ToString());
			//市立本部的医院编码:3702010323
			//市立东院的医院编码:3702010324
			//if (INFO.hospCode != "") Box.Add("hospCode", );
			Box.Add("hospName", ZZJCore.Public_Var.ZZJ_Config.DwMc);
			if (INFO.deptName != "") Box.Add("deptName", INFO.deptName);
			if (INFO.deptCode != "") Box.Add("deptCode", INFO.deptCode);
			if (INFO.doctName != "") Box.Add("doctName", INFO.doctName);
			if (INFO.doctCode != "") Box.Add("doctCode", INFO.doctCode);
			if (INFO.appoNo != "") Box.Add("appoNo", INFO.appoNo);
			Box.Add("cash", (INFO.cash * 100).ToString("0"));
			Box.Add("thirdPartyRegId", INFO.thirdPartyRegId);
			Box.Add("guarderIdNo", Public_Var.PF.guardianNo);
			if (INFO.tradeMode != "") Box.Add("tradeMode", INFO.tradeMode);

			try
			{
				ZZJCore.SuanFa.Proc.BW(Box.GetString(), "挂号预约记录");
				string rString = YuanTuGatewaydo.Gateway.Query(MEF.GateUrl, Box.GetString());
				ZZJCore.SuanFa.Proc.BW(rString, "挂号预约记录");
				jsonObj = JObject.Parse(rString);
				string returns = jsonObj["success"].ToString();//获取返回结果
				if (returns.ToUpper() == "TRUE") return 0;
				Msg = jsonObj["msg"].ToString();
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log("调用区域挂号记录接口异常" + ex.Message);
				Msg = ex.Message;
			}
			return -1;
		}

		/// <summary>
		/// 预约记录状态变化接口
		/// </summary>
		/// <returns></returns>
		public static int UpdateRegStatus(string RegId, int State)
		{
			JObject jsonObj;
			ItemBox Box = new ItemBox("yuantu.wap.upload.reg.status");
			string rString = "";
			//预约号
			Box.Add("thirdPartyRegId", RegId);
			//预约状态 1已预约  2以挂号  3已取消  4已过期
			Box.Add("status", State);
			Console.WriteLine(Box.GetString());
			try
			{
				rString = YuanTuGatewaydo.Gateway.Query(MEF.GateUrl, Box.GetString());
				jsonObj = JObject.Parse(rString);
				//获取返回结果
				string returns = jsonObj["success"].ToString();
				if (returns.ToUpper() == "TRUE") return 0;
				Msg = jsonObj["msg"].ToString();
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				Msg = ex.ToString();
			}
			return -1;
		}
		public static decimal JYJE = 0;//上次交易金额

		public static string LSH = "";//上次交易流水号
		public static string sFlowId = "";//源流水号

		public static int yjjxf(decimal DC)
		{
			JYJE = DC;
			LSH = ZZJCore.Public_Var.SerialNumber + DateTime.Now.ToString("HHmmss");

			JObject jsonObj = null;
			ItemBox Box = new ItemBox("yuantu.wap.consume.vs.cash");
			string rString = "";
			//卡内号
			Box.Add("cardNo", ZZJCore.Public_Var.cardInfo.CardNo);
			//卡类型 2 就诊卡
			Box.Add("cardType", "2");
			//交易金额
			Box.Add("cash", (DC * 100).ToString("0"));
			//交易时间 1门诊 2住院
			Box.Add("inHos", "1");
			//交易流水号
			Box.Add("flowId", LSH);
			Box.Add("orderNo", LSH);

			//交易时间
			Box.Add("tradeTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

			Msg = "服务器返回错误";

			try
			{
				ZZJCore.SuanFa.Proc.BW(Box.GetString(), "区域消费接口");
				rString = YuanTuGatewaydo.Gateway.Query(MEF.GateUrl, Box.GetString());
				ZZJCore.SuanFa.Proc.BW(rString, "区域消费接口");
				if (GetJsonValue(rString, "success").ToUpper() != "TRUE") return 1;
				jsonObj = JObject.Parse(rString);
				JObject dataString = JObject.Parse(jsonObj["data"].ToString());
				if (dataString.Count < 1) return 1;// data里面没数据
				sFlowId = dataString["sFlowId"].ToString();//返回流水号
				Msg = "";
				ZZJCore.SuanFa.Proc.Log(string.Format("交易成功!本地流水号:{0} 平台流水号:{1}", LSH, sFlowId));
				return 0;
			}
			catch (Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log("调用平台消费接口异常!" + ex.Message);
				Msg = "连接平台服务器异常!";
			}
			//Console.WriteLine(rString);

			return -1;
		}

		public static int BackCash()
		{
			if (ZZJCore.Public_Var.cardInfo.CardType != "1")
			{
				ZZJCore.SuanFa.Proc.Log(string.Format("区域退费失败!金额:{0} 提示:不是区域卡!", JYJE.ToString("C")));
				return -1;
			}
			ItemBox Box = new ItemBox("yuantu.wap.consume.flushes.vs.cash");
			//源流水号,充值成功返回的流水号
			Box.Add("sFlowId", sFlowId);
			//订单Id
			Box.Add("orderId", LSH);
			//流水Id
			Box.Add("flowId", LSH);
			//交易时间
			Box.Add("tradeTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			//Console.WriteLine(Box.GetString());
			try
			{
				ZZJCore.SuanFa.Proc.BW(Box.GetString(), "区域消费撤销接口");
				string rString = YuanTuGatewaydo.Gateway.Query(MEF.GateUrl, Box.GetString());
				ZZJCore.SuanFa.Proc.BW(rString, "区域消费撤销接口");
				string returns = GetJsonValue(rString, "success");
				if (returns.ToUpper() == "TRUE") return 0;
				ZZJCore.SuanFa.Proc.Log(string.Format("区域退费失败!金额:{0} 提示:{1}", JYJE.ToString("C"), GetJsonValue(rString, "msg")));
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.BW(string.Format("区域退费失败!金额:{0} 提示:{1}", JYJE.ToString("C"), e.Message), "区域消费撤销接口");
				ZZJCore.SuanFa.Proc.Log(string.Format("区域退费失败!金额:{0} 提示:{1}", JYJE.ToString("C"), e.Message));
			}
			return -1;
		}

		public class Carditem
		{
			/// <summary>
			/// 卡面号
			/// </summary>
			public string cardNo = "";
			//卡状态
			public string cardStatus = "";

			/// <summary>
			/// 卡类型
			/// </summary>
			public string CardType = "";
		}

		public class Cardlist
		{
			//平台患者流水
			public string platformId = "";
			//病人Id
			public string patientId = "";
			//姓名
			public string name = "";
			//性别
			public string sex = "";
			//电话
			public string phone = "";
			//余额
			public decimal accBalance = 0;
			//卡列表
			public Carditem[] clist;
		}

		/// <summary>
		/// 查询用户信息 补卡用
		/// </summary>
		/// <param name="Mode">模式0:成人 1:儿童 9:</param>
		/// <param name="cardType"></param>
		/// <param name="sList"></param>
		/// <returns></returns>
		public static int QueryUserInfo(int Mode, string cardType, out Cardlist sList)
		{
			JObject jsonObj;
			ItemBox Box = new ItemBox("yuantu.wap.query.patient.vs.info");
			//cardlist= new string[]{""};
			sList = new Cardlist();
			//身份证
			if (Mode == 0) Box.Add("cardNo", ZZJCore.Public_Var.patientInfo.IDNo);
			if (Mode == 1) Box.Add("guarderId", ZZJCore.Public_Var.patientInfo.IDNo);
			//卡类型
			Box.Add("cardType", "1");
			//姓名
			Box.Add("patientName", ZZJCore.Public_Var.patientInfo.PatientName);

			string rString = "";
			try
			{
				ZZJCore.SuanFa.Proc.BW(Box.GetString(), "查询办卡记录");
				rString = YuanTuGatewaydo.Gateway.Query(MEF.GateUrl, Box.GetString());
				ZZJCore.SuanFa.Proc.BW(rString, "查询办卡记录");
			}
			catch (System.Net.WebException err)
			{
				ZZJCore.SuanFa.Proc.Log(err);
				Msg = err.Message;
				return -1;
			}
			if (string.IsNullOrEmpty(rString))
			{
				Msg = "区域平台返回空数据";
				return -1;
			}
			Console.WriteLine("查询返回:\n" + rString);
			try
			{
				jsonObj = JObject.Parse(rString);
				//jsonObj = JObject.Parse(jsonObj.ToString());
				string returns = jsonObj["success"].ToString();
				if (returns.ToUpper() == "FALSE")
				{
					Msg = jsonObj["msg"].ToString();
					return 0;
				}

				Carditem Item = new Carditem();
				List<Carditem> rlist = new List<Carditem>();
				JArray jsonVal = JArray.Parse(jsonObj["data"].ToString()) as JArray;
				JArray dataString = JArray.Parse(jsonObj["data"].ToString());
				// data里面没数据
				if (dataString.Count <= 0)
				{
					Msg = "没有查到用户信息!";
					return 0;
				}
				//读取返回信息
				sList.accBalance = decimal.Parse(dataString[0]["accBalance"].ToString()) * 100;
				sList.name = dataString[0]["name"].ToString();
				sList.patientId = dataString[0]["patientId"].ToString();
				sList.phone = dataString[0]["phone"].ToString();
				sList.platformId = dataString[0]["platformId"].ToString();
				sList.sex = dataString[0]["sex"].ToString();
				if (jsonVal.Count == 0)
				{
					Msg = "没有查到用户信息!";
					return 0;
				}
				dynamic datas = jsonVal;
				foreach (dynamic album in datas)
				{
					foreach (dynamic song in album.cardItems)
					{
						if (song.cardType.ToString() != cardType) continue;
						Item = new Carditem();
						Item.cardNo = song.cardNo.ToString();
						Item.cardStatus = song.cardStatus.ToString();
						rlist.Add(Item);
					}
				}
				sList.clist = rlist.ToArray();
				//data里面没数据
				if (sList.clist.Length < 1)
				{
					Msg = "没有查到用户信息!";
					return 0;
				}
				return 0;
			}
			catch (Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				Msg = "平台服务器返回数据错误!";
				return -1;
			}
		}

		public static int GuaShi(string FlowID, Carditem Ci)
		{
			if (Ci.cardStatus == "-1") return 0;
			ItemBox Box = new ItemBox("yuantu.wap.lost.patient.card");
			//流水号
			Box.Add("flowId", FlowID);
			//订单号 用 flowid
			Box.Add("orderNo", FlowID);
			//卡面号
			Box.Add("cardNo", Ci.cardNo);
			string rString = "";
			try
			{
				ZZJCore.SuanFa.Proc.BW(Box.GetString(), "挂失接口");
				rString = YuanTuGatewaydo.Gateway.Query(MEF.GateUrl, Box.GetString());
				ZZJCore.SuanFa.Proc.BW(rString, "挂失接口");
				string returns = GetJsonValue(rString, "success");
				if (returns.ToUpper() == "TRUE") return 0;
				Msg = GetJsonValue(rString, "msg");
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				Msg = ex.ToString();
				return -1;
			}
			Console.WriteLine(rString);

			if (string.IsNullOrEmpty(rString)) Msg = "与服务器连接失败 !";
			return -1;
		}

		public static int BindNewCard(string tradeMode, Carditem Ci, string FlowID)
		{
			ItemBox Box = new ItemBox("yuantu.wap.renew.patient.card");
			string rString = "";
			//病人Id
			Box.Add("patientId", ZZJ_Module.Public_Var.PF.patientId);
			//卡面号
			Box.Add("cardNo", Ci.cardNo);
			//支付类别
			//交易方式 DB 银联 CA 现金 OC 预交金
			Box.Add("tradeMode", tradeMode);
			//银联交易流水
			if (tradeMode == "DB") Box.Add("bankTransNo", ZZJ_Module.UnionPay.RETDATA.strTrace);
			//新卡卡内号
			Box.Add("newSeqNo", ZZJCore.Public_Var.cardInfo.CardNo);
			//流水号
			Box.Add("flowId", FlowID);
			//订单号 用 flowid
			Box.Add("orderNo", FlowID);

			try
			{
				ZZJCore.SuanFa.Proc.BW(Box.GetString(), "补卡接口");
				rString = YuanTuGatewaydo.Gateway.Query(MEF.GateUrl, Box.GetString());
				ZZJCore.SuanFa.Proc.BW(rString, "补卡接口");
				string returns = GetJsonValue(rString, "success");
				if (returns.ToUpper() == "TRUE") return 0;
				Msg = GetJsonValue(rString, "msg");
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
			}
			if (string.IsNullOrEmpty(rString)) Msg = "与服务器连接失败 !";
			return -1;
		}

		/// <summary>
		/// 补卡
		/// </summary>
		/// <param name="tradeMode">交易方式  DB 银联 CA 现金 OC 预交金</param>
		/// <param name="CL">患者信息列表</param>
		/// <param name="NewCardNo">新卡内号</param>
		/// <param name="CardFaceNo">卡面号</param>
		/// <returns></returns>
		public static int ReCreateCard(string tradeMode, Carditem Ci)
		{
			string FlowID = DateTime.Now.ToString("yyyyMMddHHmmss");
			ZZJCore.SuanFa.Proc.Log("补卡流程开始!流水号:" + FlowID);
			if (GuaShi(FlowID, Ci) != 0) return -1;
			int iRet = BindNewCard(tradeMode, Ci, FlowID);
			if (iRet == 0)
			{
				ZZJ_Module.Public_Var.PF.patientCard = Ci.cardNo;
			}
			return iRet;
		}
	}//End Class
}//End NameSpace