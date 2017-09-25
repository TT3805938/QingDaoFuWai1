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

public static class ZYQD
{
	public static  List<List<string>> ammList=new List<List<string>>();//一级菜单
	public static List<T> AddDTT<T>(this List<T> AL, T Data)
	{
		for (int i = 0; i < AL.Count; i++)
		{
			if (AL[i].ToString() == Data.ToString()) return AL;
		}
		AL.Add(Data);
		return AL;
	}

	struct ACFLists
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

	struct ACFList
	{
		/// <summary>
		/// 名称
		/// </summary>
		public string ypmc;
		/// <summary>
		/// 规格
		/// </summary>
		public string gg;
		/// <summary>
		/// 单位
		/// </summary>
		public string dw;
		/// <summary>
		/// 数量
		/// </summary>
		public string sl;
		/// <summary>
		/// 单价
		/// </summary>
		public decimal price;
		/// <summary>
		/// 总金额
		/// </summary>
		public decimal AllAMount;
		/// <summary>
		/// 日期
		/// </summary>
		public string rq;
	}

	struct AMM
	{
		/// <summary>
		/// 
		/// </summary>
		public string DT;
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

	//System.Collections.Generic.IComparer<ACFList> ACff;
	
	public static bool ZYQDMain()
	{
		#region 初始化
		string InXML = "";
		string Msg = "";
		ZZJCore.Public_Var.patientInfo.PatientName="";
		ZZJCore.Public_Var.patientInfo.Address="";
		ZZJCore.FormSkin.UseRetCard = true;
		ZZJCore.Public_Var.ModuleName = "住院明细查询";
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.Initial.Read();//读取配置文件


		ZZJCore.InputForm.InputHospitalIzationIDParameter IP = new ZZJCore.InputForm.InputHospitalIzationIDParameter();
		IP.MAX = 8;
		IP.MIN = 4;
		IP.DJSTime = 120;
		IP.Caption = "请输入住院号";
		IP.LabelText = "";
		IP.Voice = "";
		List<ZZJCore.ZZJStruct.ZZJButton> ALS = new List<ZZJCore.ZZJStruct.ZZJButton>();
		ALS.Add(ZZJCore.ZZJControl.Button_Close);
		IP.Buttons = ALS.ToArray();
		IP.XButton = false;
		//IP.RegMode = false;
		string ID = "";//////////////////////
		int iRett = ZZJCore.InputForm.InputHospitalIzationID(IP, out ID);
		if (iRett != 0)
		{
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		#endregion
		//将住院号填充至8位
		if(ID.Length!=8)
		{
			string zyhBuLing="";
			for(int i=0;i<8-ID.Length;i++)
			{
				zyhBuLing+="0";
			}
			ID=zyhBuLing+ID;
		}
		{
			#region 整理报文并查询
			// string InXML = "";
//      <zzxt>
//<transcode>804</transcode>
//<table>
//<cardno>00013295</cardno>
//<cardtype>-1</cardtype>
//</table>
//</zzxt>

			string XmlData = "";
			XmlMaker Table = XmlMaker.NewTabel1();
			Table.Add("cardno",ID );//卡号"00013295" 
			Table.Add("cardtype", -1);//
			string DataIn = XmlMaker.NewXML("804", Table).ToString();
			try
			{
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.获取住院基本信息入参");
				InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(InXML, "Service.获取住院基本信息");
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.BackForm.ShowForm("没有查询到您的信息,请稍候再试!", true);
				ZZJCore.SuanFa.Proc.Log("查询基本信息记录804时发生异常," + ex.Message);
				return true;
			}

			XmlNodeList nodelist = null;
			#region 解析数据
			{
				XmlDocument docXML = new XmlDocument();
				docXML.LoadXml(InXML);
				int nodesResult = Convert.ToInt32(docXML.SelectSingleNode("zzxt/result/retcode").InnerText.Trim());//返回结果
				if (docXML.SelectSingleNode("zzxt/table/syxh")!=null)
				{
					ID = docXML.SelectSingleNode("zzxt/table/syxh").InnerText.Trim();
					ZZJCore.Public_Var.patientInfo.ParentIDNo=ID;
					ZZJCore.Public_Var.patientInfo.PatientName = docXML.SelectSingleNode("zzxt/table/hzxm").InnerText.Trim() + "              " + docXML.SelectSingleNode("zzxt/table/ksmc").InnerText.Trim();
					//ZZJCore.Public_Var.patientInfo.Address = docXML.SelectSingleNode("zzxt/table/ksmc").InnerText.Trim();
					ZZJCore.Public_Var.patientInfo.Address = docXML.SelectSingleNode("zzxt/table/zje").InnerText.Trim();

			//	ZZJCore.Public_Var.patientInfo.ParentName = docXML.SelectSingleNode("zzxt/table/hzxm").InnerText.Trim();
				}
				else{
					ZZJCore.BackForm.ShowForm("您不是住院病人!", true);
					return true;
				}
				if (nodesResult != 0)
				{
					ZZJCore.BackForm.ShowForm("您不是住院病人!", true);
					return true;
				}
				#endregion
			

			}


			#endregion

		ZZJCore.SuanFa.Proc.Log("开始查询消费记录");
		ACFList[] ACFL = null;
		{
			#region 整理报文并查询
			// string InXML = "";
			 XmlData = "";

			 Table = XmlMaker.NewTabel1("","",-1,false);
			 Table.Add("syxh", ID);//6810       四位数从上面取得
			 Table.Add("dyrq",DateTime.Now.ToString("yyyyMMdd"));
			 Table.Add("ksrq",DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
			 Table.Add("jsrq", DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
			 Table.Add("czlb",0);
			 Table.Add("atfz",1);
			 DataIn = XmlMaker.NewXML("806", Table).ToString();
			try
			{
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.获取住院明细记录");
				InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(XmlData, "Service.获取住院明细记录");
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.BackForm.ShowForm("没有查询到您的住院明细记录,请稍候再试!", true);
				ZZJCore.SuanFa.Proc.Log("查询住院明细记录806时发生异常," + ex.Message);
				return true;
			}

			#endregion
			#region 测试数据
		DebugDataEnd:
			#endregion
			//XmlNodeList
			 nodelist = null;
			#region 解析数据
			{
				XmlDocument docXML = new XmlDocument();
				docXML.LoadXml(InXML);
				int nodesResult = Convert.ToInt32(docXML.SelectSingleNode("zzxt/result/retcode").InnerText.Trim());//返回结果
				if (nodesResult != 0)
				{
					ZZJCore.BackForm.ShowForm("没有查询到您的清单记录,请稍候再试!", true);
					return true;
				}
			
				ACFL = ReadNodes(docXML).ToArray();
				if (ACFL.Length <= 0)
				{
					ZZJCore.BackForm.ShowForm("没有查询到您的清单记录 ,请稍候再试!", true);
					return true;
				}

			}

			#endregion
		}


		//List<AMM> MMList = new List<AMM>();
		//foreach (ACFList CFL in ACFL)
		//{

		//  AMM AM = new AMM();
		//  AM.MM = CFL.DT.ToString("yyyy年MM月");
		//  AM.DT = CFL.DT;
		//  if (MMList.Contains(AM))
		//  {
		//    continue;
		//  }
		//  for (int i = 0; i < ACFL.Length; i++)
		//  {
		//    if (ACFL[i].DT.ToString("yyyy年MM月") != AM.MM) continue;
		//    AM.AMOUNT += ACFL[i].JE;
		//  }
		//  MMList.AddDT(AM);
		//}
	//S2:
	//  int iRet = ShowDateMM(MMList.ToArray());
	//  if (iRet <= 0)
	//  {
	//    ZZJCore.BackForm.CloseForm();
	//    return true;
	//  }

	//  ACFList[] ac = (from ACFList id in ACFL where id.DT.ToString("yyyy年MM月") == MMList[iRet - 1].DT.ToString("yyyy年MM月") orderby id.DT descending select id).ToArray();//orderby id.DT descending

	S1:
		//显示
		int iRet = ShowDateMM(ACFL);
		if (iRet == -1) goto S1;
		if (iRet <= 0)
		{
			ZZJCore.BackForm.CloseForm();
			return true;
		}

		ZZJCore.BackForm.CloseForm();
		return true;

	}
	}

	private static int ShowDateMM(ACFList[] ACFL)
	{
		ZZJCore.DataViewForm.DataViewFormParameter DVFP = new DataViewForm.DataViewFormParameter();
		DVFP.LabelText = "昨日消费明细,仅限参考,具体以出院结算为准！";
		DVFP.ShowName=true;
		int GButtonIndex = 0;//按钮号
		int GSelectIndex = 0;//选择的行
		DVFP.BtnEvent = new ZZJCore.DataGridViewForm.ButtonEvent[] { 
	  (object sender, int ButtonIndex, int SelectIndex) => 
	  {

	    GButtonIndex=ButtonIndex;
	    GSelectIndex=SelectIndex;

			List<ACFList> selectACFL = new List<ACFList>();
			foreach(ACFList aaa in ACFL)
			{
			  if(aaa.rq==ammList[SelectIndex-1][0].ToString())
			  {
			    selectACFL.Add(aaa);
			  }
			}
			GButtonIndex= ShowData1(selectACFL.ToArray());
			return GButtonIndex;
	  } };
		DVFP.ButtonText = new string[] { "查看详细" };
		DVFP.ColumnsNames = new string[] { "日期", "总金额", "查看详细" };
		DVFP.Sizes = new byte[] { 50,30,30 };
		DVFP.Version = 2;
		string[,] Data = new string[3, ammList.Count+1];
		#region 填充数据
		for (int i = 0; i < Data.GetLength(1); i++)
		{
			if (i == Data.GetLength(1)-1)
			{
				Data[0, i] = "截止到昨天总金额";
				Data[1, i] = "￥" + ZZJCore.Public_Var.patientInfo.Address;//截止昨天总金额 从his那取
				Data[2, i] = "";
			}
			else{
			Data[0, i] =ammList[i][0];
			Data[1, i] = "￥"+ammList[i][1];
			Data[2, i]="查看详细";
			}
			
			//	Data[1, i] = MM[i].AMOUNT.ToString("C");
			//Data[2, i] = ammList[i].dw;
		}

		#endregion
		DVFP.RowDatas = Data;
		DVFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
		int iRet = ZZJCore.DataViewForm.ShowForm(DVFP);
		Application.DoEvents();
		if (iRet == -9) return GSelectIndex;
		return iRet;
	}

	
	//#region  显示第一层  按月分类
	//private static int ShowDateMM(AMM[] MM)
	//{
	//  ZZJCore.DataViewForm.DataViewFormParameter DVFP = new DataViewForm.DataViewFormParameter();
	//  DVFP.LabelText = null;
	//  int GButtonIndex = 0;//按钮号
	//  int GSelectIndex = 0;//选择的行
	//  DVFP.BtnEvent = new ZZJCore.DataGridViewForm.ButtonEvent[] { 
	//  (object sender, int ButtonIndex, int SelectIndex) => 
	//  {
	//    GButtonIndex=ButtonIndex;
	//    GSelectIndex=SelectIndex;
	//    return -9;
	//  } };
	//  DVFP.ButtonText = new string[] { "查看详细" };
	//  DVFP.ColumnsNames = new string[] { "日期", "总金额", "查看详细" };
	//  DVFP.Sizes = new byte[] { 50, 20, 30 };
	//  DVFP.Version = 2;
	//  string[,] Data = new string[3, MM.Length];
	//  #region 填充数据
	//  for (int i = 0; i < Data.GetLength(1); i++)
	//  {
	//    Data[0, i] = MM[i].MM;
	//    Data[1, i] = MM[i].AMOUNT.ToString("C");
	//    Data[2, i] = "查看详细";
	//  }

	//  #endregion
	//  DVFP.RowDatas = Data;
	//  DVFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
	//  int iRet = ZZJCore.DataViewForm.ShowForm(DVFP);
	//  Application.DoEvents();
	//  if (iRet == -9) return GSelectIndex;
	//  return iRet;
	//}
	//#endregion


	
	#region 显示第一层对应的第二层明细
	private static int ShowData1(ACFList[] ACFL)
	{
		ZZJCore.DataViewForm.DataViewFormParameter DVFP = new DataViewForm.DataViewFormParameter();
		DVFP.LabelText="消费明细" ;
		DVFP.FontSize=14;
		int GButtonIndex = 0;//按钮号
		int GSelectIndex = 0;//选择的行
		DVFP.BtnEvent = new ZZJCore.DataGridViewForm.ButtonEvent[] { 
		(object sender, int ButtonIndex, int SelectIndex) => 
		{
			GButtonIndex=ButtonIndex;
			GSelectIndex=SelectIndex;
			MessageBox.Show(ButtonIndex.ToString());
			return ButtonIndex;
		} };
		DVFP.ButtonText = new string[] { "查看详细" };
		DVFP.ColumnsNames = new string[] { "名称","规格", "金额", "数量", "总金额" };
		DVFP.Sizes = new byte[] { 40,40, 20, 20, 20 };
		string[,] Data = new string[5, ACFL.Length];
		#region 填充数据
		for (int i = 0; i < Data.GetLength(1); i++)
		{
			Data[0, i] = ACFL[i].ypmc.ToString();//"yyyy年MM月dd日"
			Data[1, i] = ACFL[i].gg.ToString();
			Data[2, i] = ACFL[i].price.ToString("C");
			Data[3, i] = ACFL[i].sl.ToString();
			Data[4, i] = ACFL[i].AllAMount.ToString("C");
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
		ammList.Clear();
		XmlNodeList Nodes = Xml.SelectSingleNode("zzxt").ChildNodes;
		int aindex=Nodes.Count;
		List<ACFList> AT = new List<ACFList>();

		foreach (XmlNode node in Nodes)
		{
			if (!node.Name.Contains("table") || node.Name.Contains("tablecount")) continue;
			
			ACFList A = new ACFList();
   //  A.sl=(aindex-2).ToString();
			if (node.SelectSingleNode("ypdw").InnerText.Trim() != null && node.SelectSingleNode("ypdw").InnerText.Trim() != "")
			{
			A.ypmc = node.SelectSingleNode("ypmc").InnerText.Trim();
			A.gg = node.SelectSingleNode("ypgg").InnerText.Trim();
			A.dw = node.SelectSingleNode("ypdw").InnerText.Trim();
			A.sl = node.SelectSingleNode("ypsl").InnerText.Trim();
			A.rq = node.SelectSingleNode("qqrq").InnerText.Trim();
			A.price = decimal.Parse(node.SelectSingleNode("ypdj").InnerText.Trim());
			A.AllAMount = decimal.Parse(node.SelectSingleNode("zje").InnerText.Trim());
			//A.DT = DateTime.Parse(node.SelectSingleNode("czrq").InnerText.Trim());
			//A.JYMC = node.SelectSingleNode("jymc").InnerText.Trim();
			AT.Add(A);
			}
		}
		string rqMid="";
		decimal zje=0;
		foreach(ACFList a in AT)
		{
				if(a.rq!=rqMid) //说明没重复
				{
					List<string> amm=new List<string>();
			    amm.Add(a.rq);//0
					ammList.Add(amm);
					rqMid=a.rq;
				}
		}

		foreach(List<string> st in ammList)
		{
		zje=0;
			foreach(ACFList aa in AT)
			{
				if(aa.rq==st[0])
				{
					zje+=aa.AllAMount;
				}
			}
			st.Add(Convert.ToString(zje));
		}


		return AT;
	}
	#endregion

}//End Class
