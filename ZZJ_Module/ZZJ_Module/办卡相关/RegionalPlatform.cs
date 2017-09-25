using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Web;

namespace ZZJ_Module
{
	public class ItemBox
	{
		public ItemBox(string service)
		{
			this.Clear();
			//医院代码
			this.Add("hospitalId", "263");
			//服务代码 固定
			this.Add("service", service);
			//自助机,固定
			this.Add("sourceCode", "ZZJ");
			//operId  必传
			this.Add("operId", ZZJCore.Public_Var.ZZJ_Config.ExtUserID);

			this.Add("hospCode", ZZJCore.Public_Var.ZZJ_Config.YQCode);
		}

		private string Text = "";

		public void Add(string key, int value)
		{
			Add(key, value.ToString());
		}

		public void Add(string key, byte value)
		{
			Add(key, value.ToString());
		}

		public void Add(string key, string value)
		{
			if (string.IsNullOrEmpty(value)) return;
			if (Text.Length != 0) Text += "&";
			Text += key + "=" + value;
		}

		public string GetString()
		{
			return Text;
		}

		public void Clear()
		{
			Text = "";
		}
	}

	public static class RegionalPlatform
	{
		public static string Err = "";

		/// <summary>
		/// 区域建立档案
		/// </summary>
		/// <param name="patientInfo">提交的信息</param>
		/// <param name="Err">返回的错误信息</param>
		/// <param name="Mode">0成人办卡 1儿童办卡 9医保签约</param>
		/// <returns>0:成功</returns>
		public static int CreateCardAddPosit(int Mode)
		{
			JObject jsonObj;
			ItemBox Box = new ItemBox("yuantu.wap.register.virtual.settlement");
			//编号清空
			Public_Var.PF.patientId = "";
			Public_Var.PF.patientCard = "";
			//卡号
			Box.Add("cardNo", Mode == 9 ? ZZJCore.Public_Var.patientInfo.IDNo : ZZJCore.Public_Var.cardInfo.CardNo);
			//卡类型
			Box.Add("cardType", Mode == 9 ? "10" : "2");
			//身份证
			if (Mode == 0) Box.Add("idNo", ZZJCore.Public_Var.patientInfo.IDNo);
			if (Mode == 1) Box.Add("guarderId", ZZJCore.Public_Var.patientInfo.IDNo);
			if (Mode == 9) Box.Add("guarderId", ZZJCore.Public_Var.patientInfo.IDNo);
			if (Mode != 0 && Mode != 1 && Mode != 9) Box.Add("guarderId", ZZJCore.Public_Var.patientInfo.IDNo);

			//证件类型 1表示身份证
			Box.Add("idType", "1");

			//姓名
			Box.Add("patientName", ZZJCore.Public_Var.patientInfo.PatientName);

			//民族
			if (!string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.Nation)) Box.Add("nation", ZZJCore.Public_Var.patientInfo.Nation);
			Box.Add("sex", ZZJCore.Public_Var.patientInfo.Sex);
			//出生日期
			Box.Add("birthday", ZZJCore.Public_Var.patientInfo.DOB);
			//家庭住址
			if (!string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.Address)) Box.Add("address", ZZJCore.Public_Var.patientInfo.Address);
			//联系电话
			Box.Add("phone", ZZJCore.Public_Var.patientInfo.Mobile);
			//Console.WriteLine("提交信息:\n" + Box.GetString());
			//提交信息
			string rString = "";
			try
			{
				ZZJCore.SuanFa.Proc.BW(Box.GetString(), "区域办卡");
				rString = YuanTuGatewaydo.Gateway.Query(MEF.GateUrl, Box.GetString());
				ZZJCore.SuanFa.Proc.BW(rString, "区域办卡");
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.Log("*区域接口调用失败!" + e.Message);
				Err = "连接平台异常!" + e.Message;
				return -1;
			}

			if (rString == "")
			{
				ZZJCore.SuanFa.Proc.Log("*平台服务器返回空报文!");
				Err = "平台服务器返回空报文!";
				return -1;
			}

			//Console.WriteLine("查询返回:\n" + rString);
			try
			{
				jsonObj = JObject.Parse(rString);
				string returns = jsonObj["success"].ToString();
				if (returns.ToUpper() == "FALSE")
				{
					Err = jsonObj["msg"].ToString();
					return -1;
				}
				//获取data
				jsonObj = JObject.Parse(jsonObj["data"].ToString());
				//data里面没数据
				if (jsonObj.Count < 1)
				{
					Err = "没有查到用户信息!";
					return 1;
				}
				//读取返回信息
				Public_Var.PF.patientId = jsonObj["patientId"].ToString();
				Public_Var.PF.patientCard = jsonObj["patientCard"].ToString();
				return 0;
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.Log("*平台服务器返回数据错误" + e.Message);
				Err = "平台服务器返回数据错误!";
				return -1;
			}
		}

		/// <summary>
		/// 查询用户信息
		/// </summary>
		/// <param name="Err">当Return返回非0时 返回错误信息</param>
		/// <param name="searchType">搜索类型 1本人 2监护人 默认为1</param>
		/// <returns>0:成功 -1:服务器错误   1:没查到用户信息</returns>
		public static int QueryPatient(string searchType = "", bool NeedExInfo = true)
		{
			string rString = "";

			ItemBox Box = new ItemBox("yuantu.wap.query.patient.info");
			Box.Add("cardNo", ZZJCore.Public_Var.cardInfo.CardNo);//查询卡号
			Box.Add("cardType", "2");//查询卡类型
			if (!string.IsNullOrEmpty(searchType)) Box.Add("searchType", "");//搜索类型 1本人 2监护人 默认为1
			try
			{
				ZZJCore.SuanFa.Proc.BW(Box.GetString(), "查询区域患者信息");
				rString = YuanTuGatewaydo.Gateway.Query(MEF.GateUrl, Box.GetString());
				ZZJCore.SuanFa.Proc.BW(rString, "查询区域患者信息");
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.Log("*区域接口调用失败!" + e.Message);
				ZZJCore.SuanFa.Proc.BW("*区域接口调用失败!" + e.Message, "查询区域患者信息");
			}

			//Public_Var.PF.patientCard = jsonObj["patientCard"].ToString();

			//Console.WriteLine("fanhhui信息:\n" + rString);
			if (rString == "")
			{
				Err = "与区域前置服务器连接失败 !";
				return -1;
			}
			//Console.WriteLine("查询返回:\n" + rString);
			try
			{
				JObject jsonObj = JObject.Parse(rString);
				//获取返回结果
				string returns = jsonObj["success"].ToString();
				if (returns.ToUpper() == "FALSE")
				{
					//返回错误信息
					Err = jsonObj["msg"].ToString();
					return 1;
				}
				//获取data数组
				JArray dataString = JArray.Parse(jsonObj["data"].ToString());
				// data里面没数据
				if (dataString.Count < 1)
				{
					Err = "没有查到用户信息!";
					return 1;
				}
				//读取返回信息
				//病人Id
				Public_Var.PF.patientId = dataString[0]["patientId"].ToString();
				//平台流水
				//Public_Var.PF.platformId = dataString[0]["platformId"].ToString();
				//证件 号码
				ZZJCore.Public_Var.patientInfo.IDNo = dataString[0]["idNo"].ToString();
				if (
				string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.IDNo) &&
				!string.IsNullOrEmpty(dataString[0]["guardianNo"].ToString()))
				{
					ZZJCore.Public_Var.patientInfo.IDNo = dataString[0]["guardianNo"].ToString();
				}

				Public_Var.IsHaveIDNo = !string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.IDNo);

				//平台Id
				//uData.cardNo = dataString[0]["cardNo"].ToString();
				//姓名
				if (NeedExInfo) ZZJCore.Public_Var.patientInfo.PatientName = dataString[0]["name"].ToString();
				//民族
				ZZJCore.Public_Var.patientInfo.Nation = dataString[0]["nation"].ToString();
				//性别
				ZZJCore.Public_Var.patientInfo.Sex = dataString[0]["sex"].ToString();
				//生日
				ZZJCore.Public_Var.patientInfo.DOB = dataString[0]["birthday"].ToString();
				//家庭住址
				ZZJCore.Public_Var.patientInfo.Address = dataString[0]["address"].ToString();
				//电话
				ZZJCore.Public_Var.patientInfo.Mobile = dataString[0]["phone"].ToString();
				//病人类型
				//uData.patientType = dataString[0]["patientType"].ToString();
				//卡状态
				//uData.cardStatus = dataString[0]["cardStatus"].ToString();
				//监护人卡号
				//uData.guardianNo = dataString[0]["guardianNo"].ToString();
				//账户余额
				ZZJCore.Public_Var.patientInfo.DepositAmount = (Convert.ToDecimal(dataString[0]["accBalance"].ToString()) / 100).ToString();
				//监护人姓名
				ZZJCore.Public_Var.patientInfo.ParentName = dataString[0]["guardianName"].ToString();
				//交易账号
				//uData.accountNo = dataString[0]["accountNo"].ToString();
				//未知  文档没写
				//uData.seqno = dataString[0]["seqno"].ToString();
				// uData.accountNo = dataString[0]["school"].ToString();
				return 0;
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				Err = "服务器返回错误的数据!";
				return -1;
			}
		}
	}
}
