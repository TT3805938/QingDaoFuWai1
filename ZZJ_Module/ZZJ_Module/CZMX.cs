using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Collections;
using System.IO;
using ZZJCore;
using System.Drawing;
using Mw_Public;
using ZZJCore.ZZJStruct;

public static class CZMX
{

	public static List<T> AddDTT<T>(this List<T> AL, T Data)
	{
		for (int i = 0; i < AL.Count; i++)
		{
			if (AL[i].ToString() == Data.ToString()) return AL;
		}
		AL.Add(Data);
		return AL;
	}

	struct ACFList
	{
		/// <summary>
		/// 交易名称
		/// </summary>
		public string JYMC;
		/// <summary>
		/// 金额
		/// </summary>
		public decimal JE;
		/// <summary>
		/// 处方类型
		/// </summary>
		public string CZType;
		
		/// <summary>
		/// 总金额
		/// </summary>
		public decimal AllAMount;
		
		/// <summary>
		/// 日期
		/// </summary>
		public DateTime DT;
		
		/// <summary>
		/// 处方明细
		/// </summary>
		//public ACFMX[] CFMX;
	}

	struct AMM
	{
		/// <summary>
		/// 
		/// </summary>
		public DateTime DT;
		/// <summary>
		/// 
		/// </summary>
		public string MM;
		/// <summary>
		/// 
		/// </summary>
		public decimal AMOUNT;
		public override string ToString()
		{
			return MM;
		}
	}

	//System.Collections.Generic.IComparer<ACFList>

	public static bool CZMXMain()
	{
		#region 初始化
		string InXML = "";
		string Msg = "";
		ZZJCore.FormSkin.UseRetCard = true;
		ZZJCore.Public_Var.ModuleName = "充值明细查询";
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.Initial.Read();//读取配置文件
		if (XMLCore.GetUserInfo(out Msg) != 0)
		{
			if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true);
			ZZJCore.BackForm.CloseForm();
			return false;
		}
		ZZJCore.SuanFa.Proc.Log("读取配置文件成功.");
		#endregion

		if (ZZJCore.Public_Var.cardInfo.CardType == "1")
		{
			ZZJCore.SuanFa.Proc.ZZJMessageBox("港内卡与附属卡充值查询未开启", "港内卡与附属卡充值查询功能当前禁止使用!", true);
			ZZJCore.SuanFa.Proc.Log("港内卡与附属卡不支持充值查询.");
			ZZJCore.BackForm.CloseForm();
			return true;
		}

		ZZJCore.SuanFa.Proc.Log("开始查询充值数据");
		ACFList[] ACFL = null;
		{
			#region 整理报文并查询
			// string InXML = "";
			string XmlData = "";
			XmlMaker Table = XmlMaker.NewTabel(ZZJCore.Public_Var.patientInfo.PatientID);
			string DataIn = XmlMaker.NewXML("502", Table).ToString();
			try
			{
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.获取已充值记录");
				InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				//InXML = "<zzxt><result><retcode>0</retcode><retmsg>交易成功</retmsg></result><tablecount>4</tablecount><table1><patid>29266</patid><czlx>银联卡</czlx><czje>1</czje><czrq>2017-07-03 13:48:17</czrq><djje>0</djje></table1><table2><patid>29266</patid><czlx></czlx><czje>-1</czje><czrq>2017-07-03 13:48:44</czrq><djje>0</djje></table2><table3><patid>29266</patid><czlx>现金</czlx><czje>1</czje><czrq>2017-07-06 09:57:46</czrq><djje>0</djje></table3><table4><patid>29266</patid><czlx></czlx><czje>-1</czje><czrq>2017-07-06 09:57:48</czrq><djje>0</djje></table4></zzxt>";
				ZZJCore.SuanFa.Proc.BW(InXML, "Service.获取已充值记录");
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.BackForm.ShowForm("没有查询到您的充值记录,请稍候再试!", true);
				ZZJCore.SuanFa.Proc.Log("查询充值记录502时发生异常," + ex.Message);
				return true;
			}
			ZZJCore.SuanFa.Proc.Log("查询充值记录成功!["+InXML.Length+"]");
			#endregion
			#region 测试数据
		DebugDataEnd:
			#endregion
			XmlNodeList nodelist = null;
		  ZZJCore.SuanFa.Proc.Log("开始解析数据!");
			#region 解析数据
			{
				try
				{
					XmlDocument docXML = new XmlDocument();
					docXML.LoadXml(InXML);
					int nodesResult=-1;
				  nodesResult = Convert.ToInt32(docXML.SelectSingleNode("zzxt/result/retcode").InnerText.Trim());//返回结果				
					if (nodesResult != 0)
					{
						ZZJCore.BackForm.ShowForm("没有查询到您的充值记录,请稍候再试!", true);
						return true;
					}
					if(ReadNodes(docXML)==null) {ZZJCore.BackForm.ShowForm("没有查询到您的充值记录,请稍候再试!", true);return true;} 
					ACFL = ReadNodes(docXML).ToArray();
					if (ACFL.Length <= 0)
					{
						ZZJCore.BackForm.ShowForm("没有查询到您的充值记录 ,请稍候再试!", true);
						return true;
					}
				}
				catch (Exception exx)
				{
					ZZJCore.SuanFa.Proc.Log("回参xml解析时出错,错误信息:"+exx.ToString());
					ZZJCore.BackForm.ShowForm("没有查询到您的充值记录,请去人工窗口查询.",true);
					return true;
				}
			}

			ZZJCore.SuanFa.Proc.Log("回参xml解析成功,生成数据集合成功");
			ZZJCore.SuanFa.Proc.Log("数据集合开始排序");
			//冒泡排序
			for (int i = ACFL.Length - 1; i > 0; i--)
			{
				for (int l = 0; l < i; l++)
				{
					if (ACFL[l].DT >= ACFL[l + 1].DT) continue;
					ACFList AAAA = ACFL[l];
					ACFL[l] = ACFL[l + 1];
					ACFL[l + 1] = AAAA;
				}
			}
			ACFL = ACFL;
			ZZJCore.SuanFa.Proc.Log("数据集合排序成功!");
			#endregion
		}


		List<AMM> MMList = new List<AMM>();
		foreach (ACFList CFL in ACFL)
		{

			AMM AM = new AMM();
			AM.MM = CFL.DT.ToString("yyyy年MM月");
			AM.DT = CFL.DT;
			if (MMList.Contains(AM))
			{
				continue;
			}
			for (int i = 0; i < ACFL.Length; i++)
			{
				if (ACFL[i].DT.ToString("yyyy年MM月") != AM.MM) continue;
				AM.AMOUNT += ACFL[i].JE;
			}
			MMList.AddDT(AM);
		}
	S2:
		int iRet = ShowDateMM(MMList.ToArray());
		if (iRet <= 0)
		{
			ZZJCore.BackForm.CloseForm();
			return true;
		}

		ACFList[] ac = (from ACFList id in ACFL where id.DT.ToString("yyyy年MM月") == MMList[iRet - 1].DT.ToString("yyyy年MM月") orderby id.DT descending select id).ToArray();//orderby id.DT descending

	S1:
		//显示
		iRet = ShowData1(ac);
		if (iRet == -1) goto S2;
		if (iRet <= 0)
		{
			ZZJCore.BackForm.CloseForm();
			return true;
		}

		ZZJCore.BackForm.CloseForm();
		return true;

	}

	#region  显示第一层  按月分类
	private static int ShowDateMM(AMM[] MM)
	{
		ZZJCore.DataViewForm.DataViewFormParameter DVFP = new DataViewForm.DataViewFormParameter();
		DVFP.LabelText = null;
		int GButtonIndex = 0;//按钮号
		int GSelectIndex = 0;//选择的行
		DVFP.BtnEvent = new ZZJCore.DataGridViewForm.ButtonEvent[] { 
		(object sender, int ButtonIndex, int SelectIndex) => 
		{
			GButtonIndex=ButtonIndex;
			GSelectIndex=SelectIndex;
			return -9;
		} };
		DVFP.ButtonText = new string[] { "查看详细" };
		DVFP.ColumnsNames = new string[] { "日期", "总金额", "查看详细" };
		DVFP.Sizes = new byte[] { 50, 20, 30 };
		DVFP.Version = 2;
		string[,] Data = new string[3, MM.Length];
		#region 填充数据
		for (int i = 0; i < Data.GetLength(1); i++)
		{
			Data[0, i] = MM[i].MM;
			Data[1, i] = MM[i].AMOUNT.ToString("C");
			Data[2, i] = "查看详细";
		}

		#endregion
		DVFP.RowDatas = Data;
		DVFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
		int iRet = ZZJCore.DataViewForm.ShowForm(DVFP);
		Application.DoEvents();
		if (iRet == -9) return GSelectIndex;
		return iRet;
	}
	#endregion

	#region 显示第一层对应的第二层明细
	private static int ShowData1(ACFList[] ACFL)
	{
		ZZJCore.DataViewForm.DataViewFormParameter DVFP = new DataViewForm.DataViewFormParameter();
		DVFP.LabelText = null;
		int GButtonIndex = 0;//按钮号
		int GSelectIndex = 0;//选择的行
		DVFP.BtnEvent = new ZZJCore.DataGridViewForm.ButtonEvent[] { 
		(object sender, int ButtonIndex, int SelectIndex) => 
		{
			GButtonIndex=ButtonIndex;
			GSelectIndex=SelectIndex;
			return -9;
		} };
		DVFP.ButtonText = new string[] { "查看详细" };
		DVFP.ColumnsNames = new string[] { "日期", "金额", "消费方式","交易名称" };
		DVFP.Sizes = new byte[] { 40, 20, 20,20};
		string[,] Data = new string[4, ACFL.Length];
		#region 填充数据
		for (int i = 0; i < Data.GetLength(1); i++)
		{
			Data[0, i] = ACFL[i].DT.ToString();//"yyyy年MM月dd日"
			Data[1, i] = ACFL[i].JE.ToString("C");
			Data[2, i] = ACFL[i].CZType.ToString();
			Data[3, i] = ACFL[i].JYMC.ToString();
		}

		#endregion
		DVFP.RowDatas = Data;
		DVFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
		int iRet = ZZJCore.DataViewForm.ShowForm(DVFP);
		Application.DoEvents();
		if (iRet == -9) return GSelectIndex;
		return iRet;
	}
	#endregion

	#region 读取xml  放入集合AT，作为遍历数据
	private static List<ACFList> ReadNodes(XmlDocument Xml)
	{
		try
		{
			XmlNodeList Nodes = Xml.SelectSingleNode("zzxt").ChildNodes;
			List<ACFList> AT = new List<ACFList>();
			foreach (XmlNode node in Nodes)
			{
				if (!node.Name.Contains("table") || node.Name.Contains("tablecount")) continue;

				ACFList A = new ACFList();
				//里面有返回负数的要去除掉
				if (decimal.Parse(node.SelectSingleNode("czje").InnerText.Trim())<0)
				{
					continue;
				}
				A.JE = decimal.Parse(node.SelectSingleNode("czje").InnerText.Trim());
				//A.AllAMount = decimal.Parse(node.SelectSingleNode("zje").InnerText.Trim());
				if (node.SelectSingleNode("xffs")!=null)
				{
					A.CZType = node.SelectSingleNode("xffs").InnerText.Trim();
				}
				A.CZType=node.SelectSingleNode("czlx").InnerText.Trim();
				A.DT = DateTime.Parse(node.SelectSingleNode("czrq").InnerText.Trim());
				//
				if (node.SelectSingleNode("jymc")!=null)
				{
					A.JYMC = node.SelectSingleNode("jymc").InnerText.Trim();
				}
				else
				{
					A.JYMC="充值";
				}
				AT.Add(A);
			}
			return AT;
		}
		catch(Exception ex)
		{
			ZZJCore.SuanFa.Proc.Log("xml实例化对象集合时发生异常.原因可能是:"+ ex.ToString());
			return null;
		}
	}
	#endregion

}//End Class
