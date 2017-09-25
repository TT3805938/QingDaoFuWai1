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

public static class XFMX
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
		/// 编号
		/// </summary>
		public string ID;
		/// <summary>
		/// 金额
		/// </summary>
		public decimal JE;
		/// <summary>
		/// 处方类型
		/// </summary>
		public string CFType;
		/// <summary>
		/// 地址
		/// </summary>
		public string address;
		/// <summary>
		/// 执行科室名称
		/// </summary>
		public string DeptName;
		/// <summary>
		/// 总金额
		/// </summary>
		public decimal AllAMount;
		/// <summary>
		/// 余额
		/// </summary>
		public decimal YE;
		/// <summary>
		/// 日期
		/// </summary>
		public DateTime DT;
		/// <summary>
		/// 处方数量
		/// </summary>
		public int CFSL;
		/// <summary>
		/// 处方明细
		/// </summary>
		public ACFMX[] CFMX;
	}

	struct ACFMX
	{
		/// <summary>
		/// 名称
		/// </summary>
		public string ItemName;
		/// <summary>
		/// 数量
		/// </summary>
		public double ItemCount;
		/// <summary>
		/// 单价
		/// </summary>
		public decimal ItemMuch;
		/// <summary>
		/// 规格
		/// </summary>
		public string gg;
		/// <summary>
		/// 自助名称
		/// </summary>
		public string zzmc;
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

	public static bool XFMXMain()
	{
		#region 初始化
		string InXML = "";
		string Msg = "";
		ZZJCore.FormSkin.UseRetCard = true;
		ZZJCore.Public_Var.ModuleName = "消费明细查询";
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.Initial.Read();//读取配置文件
		if (XMLCore.GetUserInfo(out Msg) != 0)
		{
			if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true);
			ZZJCore.BackForm.CloseForm();
			return false;
		}
		#endregion

		ZZJCore.SuanFa.Proc.Log("开始查询缴费数据");
		ACFList[] ACFL = null;
		{
			#region 整理报文并查询
			// string InXML = "";
			string XmlData = "";
			XmlMaker Table = XmlMaker.NewTabel(ZZJCore.Public_Var.patientInfo.PatientID);
			Table.Add("lb", "0");
			Table.Add("ksrq", "");
			Table.Add("jsrq", "");
			string DataIn = XmlMaker.NewXML("604", Table).ToString();
			try
			{
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.获取已缴费主数据");
				InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(XmlData, "Service.获取已缴费主数据");
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.BackForm.ShowForm("没有查询到您的消费医嘱,请稍候再试!", true);
				ZZJCore.SuanFa.Proc.Log("查询消费医嘱604时发生异常," + ex.Message);
				return true;
			}

			#endregion
			#region 测试数据
		DebugDataEnd:
			#endregion
			XmlNodeList nodelist = null;
			#region 解析数据
			{
				XmlDocument docXML = new XmlDocument();
				docXML.LoadXml(InXML);
				int nodesResult = Convert.ToInt32(docXML.SelectSingleNode("zzxt/result/retcode").InnerText.Trim());//返回结果
				if (nodesResult != 0)
				{
					ZZJCore.BackForm.ShowForm("没有查询到您的消费医嘱,请稍候再试!", true);
					return true;
				}

				ACFL = ReadNodes(docXML).ToArray();
				if (ACFL.Length <= 0)
				{
					ZZJCore.BackForm.ShowForm("没有查询到您的消费医嘱 ,请稍候再试!", true);
					return true;
				}

			}


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


		iRet -= 1;

		if (ac[iRet].CFMX == null) ac[iRet].CFMX = QueryCFMX(ac[iRet].ID, ac[iRet].CFSL);

		ac[iRet].CFMX = ac[iRet].CFMX;

		iRet = ShowData2(ac[iRet].CFMX);
		//如果处方明细是空的,就查询并填入
		//if (ACFL[iRet].CFMX == null) ACFL[iRet].CFMX = QueryCFMX(ACFL[iRet].ID, ACFL[iRet].CFSL);

		//ACFL[iRet].CFMX = ACFL[iRet].CFMX;

		//iRet = ShowData2(ACFL[iRet].CFMX);
		if (iRet == -1) goto S1;

		ZZJCore.BackForm.CloseForm();
		return true;

	}






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
		DVFP.ColumnsNames = new string[] { "日期", "科室", "总金额", "查看详细" };
		DVFP.Sizes = new byte[] { 40, 20, 20, 20 };
		string[,] Data = new string[4, ACFL.Length];
		#region 填充数据
		for (int i = 0; i < Data.GetLength(1); i++)
		{
			Data[0, i] = ACFL[i].DT.ToString();//"yyyy年MM月dd日"
			Data[1, i] = ACFL[i].DeptName;
			Data[2, i] = ACFL[i].JE.ToString("C");
			Data[3, i] = "查看详细";
		}

		#endregion
		DVFP.RowDatas = Data;
		DVFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
		int iRet = ZZJCore.DataViewForm.ShowForm(DVFP);
		Application.DoEvents();
		if (iRet == -9) return GSelectIndex;
		return iRet;
	}

	private static int ShowData2(ACFMX[] ACFL)
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
		DVFP.ColumnsNames = new string[] { "项目名称", "数量", "单价" };
		DVFP.Sizes = new byte[] { 50, 30, 20 };
		string[,] Data = new string[3, ACFL.Length];
		#region 填充数据
		for (int i = 0; i < Data.GetLength(1); i++)
		{
			Data[0, i] = ACFL[i].ItemName;
			Data[1, i] = ACFL[i].ItemCount.ToString();
			Data[2, i] = ACFL[i].ItemMuch.ToString("C");
		}

		#endregion
		DVFP.RowDatas = Data;
		DVFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
		int iRet = ZZJCore.DataViewForm.ShowForm(DVFP);
		Application.DoEvents();
		if (iRet == -9) return GSelectIndex;
		return iRet;
	}


	private static List<ACFList> ReadNodes(XmlDocument Xml)
	{
		XmlNodeList Nodes = Xml.SelectSingleNode("zzxt").ChildNodes;
		List<ACFList> AT = new List<ACFList>();
		foreach (XmlNode node in Nodes)
		{
			if (!node.Name.Contains("table") || node.Name.Contains("tablecount")) continue;

			ACFList A = new ACFList();
			A.address = node.SelectSingleNode("yfmc").InnerText.Trim();
			A.ID = node.SelectSingleNode("cfxh").InnerText.Trim();
			A.DeptName = node.SelectSingleNode("zxksmc").InnerText.Trim();
			A.JE = decimal.Parse(node.SelectSingleNode("cfje").InnerText.Trim());
			A.YE = decimal.Parse(node.SelectSingleNode("zhye").InnerText.Trim());
			A.AllAMount = decimal.Parse(node.SelectSingleNode("zje").InnerText.Trim());
			A.CFType = node.SelectSingleNode("cflx").InnerText.Trim();
			A.CFSL = int.Parse(node.SelectSingleNode("cfsl").InnerText.Trim());
			A.DT = DateTime.Parse(node.SelectSingleNode("sfrq").InnerText.Trim());
			//A.CFMX = QueryCFMX(A.ID,A.CFSL);
			AT.Add(A);
		}
		return AT;
	}

	/// <summary>
	/// 查询处方明细
	/// </summary>
	/// <param name="ID"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	private static ACFMX[] QueryCFMX(string ID, int count)
	{
		int TableCount = 0;
		XmlMaker Table = XmlMaker.NewTabel();
		Table.Add("patid", ZZJCore.Public_Var.patientInfo.PatientID);
		Table.Add("lb", 1);
		Table.Add("cfxh", ID);
		Table.Add("cflx", "");
		Table.Add("ksrq", "");
		Table.Add("jsrq", "");

		string DataIn = XmlMaker.NewXML("605", Table).ToString();
		string InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
		XmlDocument XD = new XmlDocument();
		XD.LoadXml(InXML);
		List<ACFMX> CFMXList = new List<ACFMX>();

		XmlNodeList NodeCount = XD.SelectSingleNode("zzxt").ChildNodes;
		foreach (XmlNode node in NodeCount)
		{
			if (!node.Name.Contains("table") || node.Name.Contains("tablecount")) continue;
			ACFMX CFMX = new ACFMX();
			CFMX.ItemName = node.SelectSingleNode("ypmc").InnerText.TrimEnd('\0');
			CFMX.zzmc = node.SelectSingleNode("zzmc").InnerText.TrimEnd('\0');
			CFMX.gg = node.SelectSingleNode("gg").InnerText.TrimEnd('\0');
			CFMX.ItemCount = double.Parse(node.SelectSingleNode("ypsl").InnerText.TrimEnd('\0'));
			CFMX.ItemMuch = decimal.Parse(node.SelectSingleNode("ypdj").InnerText.TrimEnd('\0'));
			CFMXList.Add(CFMX);
			//	TableCount++;
		}
		//for (int i = 0; i < TableCount; i++)
		//{
		//  XmlNode XN = XD.SelectSingleNode(@"zzxt/table" + (i + 1).ToString());
		//  ACFMX CFMX = new ACFMX();
		//  CFMX.ItemName = XN.SelectSingleNode("ypmc").InnerText.TrimEnd('\0');
		//  CFMX.zzmc = XN.SelectSingleNode("zzmc").InnerText.TrimEnd('\0');
		//  CFMX.gg = XN.SelectSingleNode("gg").InnerText.TrimEnd('\0');
		//  CFMX.ItemCount = double.Parse(XN.SelectSingleNode("ypsl").InnerText.TrimEnd('\0'));
		//  CFMX.ItemMuch = decimal.Parse(XN.SelectSingleNode("ypdj").InnerText.TrimEnd('\0'));
		//  CFMXList.Add(CFMX);
		//}
		return CFMXList.ToArray();
	}
}//End Class
