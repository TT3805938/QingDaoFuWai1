using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Mw_Public;
using System.Xml;
using System.Collections;
using System.Drawing;
using HL.Devices.DKQ;
using System.Threading;
//using Mw_Voice;
using System.Data.OleDb;
using System.Drawing.Printing;
using Mw_MSSQL;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using ZZJCore.ZZJStruct;
using System.IO;
using System.Threading.Tasks;


public static class JF
{
	#region 数据类型
	public static XmlNode[] JFSJ;//所有缴费的数据
	public static XmlNode[][] JFSJ2;//所有缴费的数据
	public static decimal ZJE = 0;//缴费数据总金额
	public static int GButtonIndex = 0;//按钮号
	public static int GSelectIndex = 0;//选择的号
	public static string YB_Flag  = "0" ;//医保支付标识
	public static string PayModeStatic="";//港内卡支付标示
	public static string Msg = "";
	public static string isJFSuccess = "";
	public static string xfmxXML="";
	#endregion

	public static List<T> AddDT<T>(this List<T> AL, T Data)
	{
		for (int i = 0; i < AL.Count; i++)
		{
			if (AL[i].ToString() == Data.ToString()) return AL;
		}
		AL.Add(Data);
		return AL;
	}
	public static List<string> AddDT(List<string> AL, string Data)
	{
		for (int i = 0; i < AL.Count; i++)
		{
			if (AL[i].ToString() == Data.ToString()) return AL;
		}
		AL.Add(Data);
		return AL;
	}
	public static bool JFMain()
	{
		#region 1.初始化
		string Msg = "";
		ZZJCore.Public_Var.patientInfo.Password = "";
		ZZJ_Module.ChargeClass.ChargePayMode = -1;
		xfmxXML="";//医嘱信息
		YB_Flag="0";// 每次进来都要将医保标志字段初始化为0;不然后面会出错
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.Public_Var.ModuleName = "自助缴费";
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.Initial.Read();
		//获取用户信息
		if (XMLCore.GetUserInfo(out Msg) != 0)
		{
			if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true);
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		#endregion

		//开始先解锁一下医嘱.然后在锁,这样即使出现异常状况,也不会造成死锁
		#region 解锁医嘱
		//if (LockTheBill(0) == false)//解锁失败了
		//{
		//  ZZJCore.SuanFa.Proc.Log("返回");
		//  ZZJCore.BackForm.ShowForm("医嘱解锁失败,请找医生核实医嘱", true);
		//  Application.DoEvents();
		//  ZZJCore.BackForm.CloseForm();
		//  return true;
		//}
		#endregion

		ZZJCore.SuanFa.Proc.Log("开始查询缴费数据");
		{
			#region 2.获取缴费数据
			#region 整理报文并查询
			string InXML = "";
			string XmlData = "";
			XmlMaker Table = XmlMaker.NewTabel(ZZJCore.Public_Var.patientInfo.PatientID);
			Table.Add("zzjzdbh", ZZJCore.Public_Var.ZZJ_Config.ExtUserID);
			string DataIn = XmlMaker.NewXML("601", Table).ToString();
			try
			{
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.获取缴费数据入参");
				InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				//InXML=@"<zzxt><result><retcode>0</retcode><retmsg>交易成功</retmsg></result><tablecount>16</tablecount><table1><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>1</cfxh><cfje>8</cfje><cfsl>1</cfsl><cflx>注射费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table1><table2><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3007</yfdm><cfxh>583090</cfxh><cfje>44</cfje><cfsl>1</cfsl><cflx>化验费</cflx><yfmc>检验科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table2><table3><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3007</yfdm><cfxh>583091</cfxh><cfje>145</cfje><cfsl>1</cfsl><cflx>化验费</cflx><yfmc>检验科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table3><table4><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3007</yfdm><cfxh>583092</cfxh><cfje>135</cfje><cfsl>1</cfsl><cflx>化验费</cflx><yfmc>检验科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table4><table5><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3010</yfdm><cfxh>583095</cfxh><cfje>123.12</cfje><cfsl>4</cfsl><cflx>西药费</cflx><yfmc>门诊药房</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table5><table6><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3010</yfdm><cfxh>583096</cfxh><cfje>373.68</cfje><cfsl>4</cfsl><cflx>西药费</cflx><yfmc>门诊药房</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table6><table7><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3010</yfdm><cfxh>583097</cfxh><cfje>191.64</cfje><cfsl>2</cfsl><cflx>西药费</cflx><yfmc>门诊药房</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table7><table8><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583098</cfxh><cfje>18</cfje><cfsl>2</cfsl><cflx>治疗费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table8><table9><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583098</cfxh><cfje>52</cfje><cfsl>3</cfsl><cflx>材料费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table9><table10><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>10</cfje><cfsl>1</cfsl><cflx>床位费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table10><table11><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>24</cfje><cfsl>2</cfsl><cflx>检查费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table11><table12><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>16</cfje><cfsl>1</cfsl><cflx>输氧费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table12><table13><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>38.5</cfje><cfsl>1</cfsl><cflx>材料费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table13><table14><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583100</cfxh><cfje>26</cfje><cfsl>1</cfsl><cflx>诊察费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table14><table15><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583100</cfxh><cfje>26</cfje><cfsl>3</cfsl><cflx>注射费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table15><table16><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583101</cfxh><cfje>610</cfje><cfsl>4</cfsl><cflx>化验费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table16></zzxt>";
				ZZJCore.SuanFa.Proc.BW(InXML, "Service.获取缴费数据HIS出参");
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.BackForm.ShowForm("没有查询到您的医嘱,请稍候再试!", true);
				ZZJCore.SuanFa.Proc.Log("查询医嘱时发生异常," + ex.Message);
				return true;
			}
			
			#endregion
			#region 测试数据
		// if (ZZJCore.SuanFa.Proc.ReadPublicINI("JFDebugData", "0") == "0") goto DebugDataEnd;
		//InXML = @"";
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
					ZZJCore.BackForm.ShowForm("没有查询到您的医嘱,请稍候再试!", true);
					return true;
				}
				xfmxXML = XMLNodeToString(docXML);// 最后时候要比对

	
				List<XmlNode> AL = ReadNodes(docXML);
				if (AL.Count <= 0)
				{
					ZZJCore.BackForm.ShowForm("没有查询到您的医嘱 ,请稍候再试!", true);
					return true;
				}
				JFSJ = AL.ToArray();
				AL.Clear();
			}
			#endregion
			#endregion
		}
		ZZJCore.SuanFa.Proc.Log("查询并解析缴费数据完毕");
		

		#region 锁医嘱
		//if (LockTheBill(5) == false)//锁定失败了
		//{
		//  ZZJCore.SuanFa.Proc.Log("返回");
		//  ZZJCore.BackForm.ShowForm("医嘱锁定失败,请找医生核实医嘱", true);
		//  Application.DoEvents();
		//  ZZJCore.BackForm.CloseForm();
		//  return true;
		//}

		#endregion

		

		#region 3.如果是港内卡或者附属卡就弹出输入密码的界面

		if (ZZJ_Module.Public_Var.zhjb.ToString() == "2" || ZZJ_Module.Public_Var.zhjb.ToString() == "3")
		{
			ZZJCore.InputForm.InputPasswordParameter OIPP = new ZZJCore.InputForm.InputPasswordParameter();
			OIPP.Caption = "请输入就诊卡密码";
			OIPP.XButton = false;
			OIPP.RegMode = false;
			OIPP.Buttons = new ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
			if (ZZJCore.InputForm.InputPassword(OIPP) != 0)
			{
				//unlockTheBill();//解锁医嘱
				ZZJCore.BackForm.CloseForm();
				return true;
			}
			Application.DoEvents();
			//	ZZJCore.Public_Var.patientInfo.Password = ZZJCore.SuanFa.Proc.MD5(ZZJCore.Public_Var.patientInfo.Password);
		}


		#endregion

		#region 4.整理数据
		string[] SJLists = null;//时间表
		{
			List<string> AL = new List<string>();
			foreach (XmlNode node in JFSJ)
			{//枚举找出所有日期
				AL = AddDT(AL, node.SelectSingleNode("cfxh").InnerText.Trim());
			}
			SJLists = AL.ToArray();

			JFSJ2 = new XmlNode[SJLists.Length][];
			for (int i = 0; i < SJLists.Length; i++)
			{//按照日期,区分缴费数据
				string Day = SJLists[i];
				List<XmlNode> YZJH = new List<XmlNode>();//一天缴费数据的集合
				foreach (XmlNode node in JFSJ)
				{//整理出同一天内的数据
					if (node.SelectSingleNode("cfxh").InnerText.Trim() != SJLists[i]) continue;
					YZJH.Add(node);
				}
				JFSJ2[i] = YZJH.ToArray();//一天的缴费数据 转换为XML格式
			}
		}
		#endregion

		ZZJCore.SuanFa.Proc.Log("整理缴费数据完毕");

	ShowJFLB:
		#region 5显示第一层
		{
			int Ret = DateGridView001(JFSJ2, SJLists, DTGButtonEvent);
			if (Ret == -2 || Ret == -3)
			{
				//unlockTheBill();
				ZZJCore.SuanFa.Proc.Log("在第一层界面关闭缴费模块");
				ZZJCore.BackForm.CloseForm();
				return true;
			}
			if (Ret == -6)
			{
				int iRet = JFADay(JFSJ2[GSelectIndex - 1]);
				if (iRet == -1) goto ShowJFLB;
				if (iRet <= -2 || iRet == 0)
				{
					//unlockTheBill();
					ZZJCore.SuanFa.Proc.Log("在缴费界面选择关闭模块");
					ZZJCore.BackForm.CloseForm();
					return true;
				}
			}
			if (Ret == -7)
			{
				int iRet = JFALLDay();
				if (iRet == -1) goto ShowJFLB;
				if (iRet <= -2 )
				{
					//unlockTheBill();
					ZZJCore.BackForm.ShowForm("交易失败或用户取消!", true);
					ZZJCore.SuanFa.Proc.Log("在缴费界面选择关闭模块");
					ZZJCore.BackForm.CloseForm();
					return true;
				}
				else if(iRet==0)
				{
					//成功
					//unlockTheBill();
					ZZJCore.SuanFa.Proc.Log("成功，返回");
					ZZJCore.BackForm.CloseForm();
					return true;
				}
			}
		}
		#endregion

		#region 6显示第二层列表
		{
		ShowList2:
			Int32 Ret1 = DateGridView002(GSelectIndex, JFSJ2);
			if (Ret1 == -1) goto ShowJFLB;//跳转到第一层
			if (Ret1 == -7)
			{
				int iRet = JFADay(JFSJ2[GSelectIndex - 1]);
				if (iRet == -1) goto ShowList2;
				if (iRet <= -2 || iRet == 0)
				{
					ZZJCore.SuanFa.Proc.Log("在缴费界面选择关闭模块");
					//unlockTheBill();
					ZZJCore.BackForm.CloseForm();
					return true;
				}
			}

			if (Ret1 < 0)
			{
			  //unlockTheBill();
				ZZJCore.SuanFa.Proc.Log("数据异常,关闭模块");
				ZZJCore.BackForm.CloseForm();
				return true;
			}
		}
		#endregion
		return true;
	}

	#region  (作废)缴费第一层显示原版
	//public static int DateGridView001(XmlNode[][] JFSJ2, string[] SJLists, ZZJCore.DataGridViewForm.ButtonEvent DTGButtonEvent)
	//{
	//  string[] BT = new string[] { "项目名称", "金额", "查看详细", "缴费" };
	//  byte[] Sizes = new byte[] { 40, 20, 20, 20 };
	//  string[,] Datas = new string[BT.Length, JFSJ2.Length];
	//  for (int i = 0; i < JFSJ2.Length; i++)
	//  {//枚举所有日期
	//    //    decimal YZJE = (from ST in JFSJ2[i] select Convert.ToDecimal(ST.SelectSingleNode("zje").InnerText.Trim())).Sum();
	//    Datas[1, i] = JFSJ2[i][0].SelectSingleNode("cfje").InnerText;//截取日期 YZJE.ToString("C");
	//    Datas[0, i] = JFSJ2[i][0].SelectSingleNode("cflx").InnerText;//截取日期
	//  }

	//  #region 添加按钮
	//  List<ZZJCore.ZZJStruct.ZZJButton> AL = new List<ZZJCore.ZZJStruct.ZZJButton>();
	//  ZZJCore.ZZJStruct.ZZJButton zzjbutt = ZZJCore.ZZJControl.NewButton_OK();//new 一个模式的OK按钮
	//  zzjbutt.Text = "全部缴费";//改按钮上的字
	//  zzjbutt.RetData = -7;//改返回值
	//  zzjbutt.Rect.X -= (zzjbutt.Rect.Width / 2);//改位置
	//  zzjbutt.Rect.Width += zzjbutt.Rect.Width / 2;//改宽度
	//  AL.Add(zzjbutt);
	//  AL.Add(ZZJCore.ZZJControl.Button_Close);
	//  #endregion
	//  //if (ZZJCore.Public_Var.IsVoice) Talker.Speak("缴费列表");
	//  ZZJCore.SuanFa.Proc.Log("显示第1层界面");
	//  int iRet = ZZJCore.DataGridViewForm2.ShowForm(AL.ToArray(), "", Sizes, BT, Datas, new ZZJCore.DataGridViewForm.ButtonEvent[] { DTGButtonEvent, DTGButtonEvent }, new string[] { "查看详细", "缴费" }, false, false);
	//  ZZJCore.SuanFa.Proc.Log("第1层界面返回iRet=" + iRet.ToString());
	//  Application.DoEvents();
	//  return iRet;
	//}
	#endregion

	public static int DateGridView001(XmlNode[][] JFSJ2, string[] SJLists, ZZJCore.DataGridViewForm.ButtonEvent DTGButtonEvent)
	{
		string[] BT = new string[] { "项目名称", "金额", "查看详细" };
		byte[] Sizes = new byte[] { 40, 20, 20 };
		string[,] Datas = new string[BT.Length, JFSJ2.Length];
		for (int i = 0; i < JFSJ2.Length; i++)
		{//枚举所有日期
			//    decimal YZJE = (from ST in JFSJ2[i] select Convert.ToDecimal(ST.SelectSingleNode("zje").InnerText.Trim())).Sum();
			Datas[1, i] = JFSJ2[i][0].SelectSingleNode("cfje").InnerText;//截取日期 YZJE.ToString("C");
			Datas[0, i] = JFSJ2[i][0].SelectSingleNode("cflx").InnerText;//截取日期
		}

		#region 添加按钮
		List<ZZJCore.ZZJStruct.ZZJButton> AL = new List<ZZJCore.ZZJStruct.ZZJButton>();
		ZZJCore.ZZJStruct.ZZJButton zzjbutt = ZZJCore.ZZJControl.NewButton_OK();//new 一个模式的OK按钮
		zzjbutt.Text = "全部缴费";//改按钮上的字
		zzjbutt.RetData = -7;//改返回值
		zzjbutt.Rect.X -= (zzjbutt.Rect.Width / 2);//改位置
		zzjbutt.Rect.Width += zzjbutt.Rect.Width / 2;//改宽度
		AL.Add(zzjbutt);
		AL.Add(ZZJCore.ZZJControl.Button_Close);
		#endregion
		//if (ZZJCore.Public_Var.IsVoice) Talker.Speak("缴费列表");
		ZZJCore.SuanFa.Proc.Log("显示第1层界面");
		int iRet = ZZJCore.DataGridViewForm2.ShowForm(AL.ToArray(), "", Sizes, BT, Datas, new ZZJCore.DataGridViewForm.ButtonEvent[] { DTGButtonEvent, DTGButtonEvent }, new string[] { "查看详细", "缴费" }, false, false);
		ZZJCore.SuanFa.Proc.Log("第1层界面返回iRet=" + iRet.ToString());
		Application.DoEvents();
		return iRet;
	}
	public static int DateGridView002(int GSelectIndex, XmlNode[][] JFSJ2)
	{
		//string[] BT = new string[] { "项目名称", "项目类型", "金额", "数量" };
		//byte[] Sizes = new byte[] { 45, 15, 20, 20 };
		//string[,] Data = new string[BT.Length, JFSJ2[GSelectIndex - 1].Length];

		string InXML = "";
		List<XmlNode> ATT = new List<XmlNode>();
		XmlDocument XD = new XmlDocument();
		for (int i = 0; i < JFSJ2[GSelectIndex - 1].Length; i++)
		{
			XmlNode node = JFSJ2[GSelectIndex - 1][i];
			string cfxh = node.SelectSingleNode("cfxh").InnerText.Trim();

			#region  查询处方明细
			try
			{
				XmlMaker Table = XmlMaker.NewTabel();
				Table.Add("patid", ZZJCore.Public_Var.patientInfo.PatientID);
				Table.Add("cfxh", cfxh);
				Table.Add("cflx", "");
				Table.Add("zzjzdbh", ZZJCore.Public_Var.ZZJ_Config.ExtUserID);

				string DataIn = XmlMaker.NewXML("602", Table).ToString();
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.获取缴费数据明细602入参");
				InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(InXML, "Service.获取缴费数据明细602HIS出参");
				XD.LoadXml(InXML);


				//ZZJCore.SuanFa.Proc.BW(OutXml, "查询消费明细");
				//InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new string[] { OutXml }) as string;
				//ZZJCore.SuanFa.Proc.BW(InXML, "查询消费明细");
				//XD.LoadXml(InXML);

				XmlNodeList NodesT = XD.SelectSingleNode("zzxt").ChildNodes;

				for (int j = 0; j < NodesT.Count; j++)
				{
					if (NodesT.Item(j).Name.Contains("table") && !NodesT.Item(j).Name.Contains("tablecount"))
					{
						ATT.Add(NodesT.Item(j));
					}
				}

				//	 XmlNodeList R01 = XD.SelectNodes("zzxt/tablecount");
				if (ATT.Count == 0)
				{
					ZZJCore.BackForm.ShowForm("没有查询到信息!", true);
					return -1;
				}// */
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.BackForm.ShowForm("调用接口失败,请稍候再试!", true);
				return -1;
			}// */
			#endregion



			//Data[0, i] = node.SelectSingleNode("yfmc").InnerText.Trim();
			//Data[1, i] = node.SelectSingleNode("yfmc").InnerText.Trim();

			//Data[2, i] = Convert.ToDecimal(node.SelectSingleNode("zje").InnerText.Trim()).ToString("C");
			//Data[3, i] = node.SelectSingleNode("yfmc").InnerText.Trim();
			//  Data[3, i] = Data[3, i].Substring(0, Data[3, i].Length - 2);

		}
		string[] BT = new string[] { "项目名称", "规格", "数量", "单价", "总价" };
		byte[] Sizes = new byte[] { 35, 30, 10, 10, 15 };
		string[,] Data = new string[5, ATT.Count];
		#region 填充数据
		for (int ii = 0; ii < ATT.Count; ii++)
		{
			Data[0, ii] = ATT[ii].SelectSingleNode("ypmc").InnerText.Trim();
			Data[1, ii] = ATT[ii].SelectSingleNode("gg").InnerText.Trim();
			Data[2, ii] = ATT[ii].SelectSingleNode("ypsl").InnerText.Trim();
			Data[3, ii] = double.Parse(ATT[ii].SelectSingleNode("ypdj").InnerText.Trim()).ToString();
			Data[4, ii] = ATT[ii].SelectSingleNode("zje").InnerText.Trim();
		}
		#endregion

		#region 添加按钮
		List<ZZJCore.ZZJStruct.ZZJButton> AL = new List<ZZJCore.ZZJStruct.ZZJButton>();
		//ZZJCore.ZZJStruct.ZZJButton zzjbutt = ZZJCore.ZZJControl.NewButton_OK();
		//zzjbutt.Text = "全部缴费";//按钮上的字
		//zzjbutt.Rect.X -= zzjbutt.Rect.Width / 2;
		//zzjbutt.Rect.Width += zzjbutt.Rect.Width / 2;
		//zzjbutt.RetData = -7;//返回值自己定一个(必须是负数)
		//AL.Add(zzjbutt);
		AL.Add(ZZJCore.ZZJControl.Button_Close);
		AL.Add(ZZJCore.ZZJControl.Button_Ret);
		#endregion
		ZZJCore.SuanFa.Proc.Log("显示第2层界面");
		int iRet = ZZJCore.DataGridViewForm2.ShowForm(AL.ToArray(), "", Sizes, BT, Data, null, null, false, false);
		ZZJCore.SuanFa.Proc.Log("第2层界面返回iRet=" + iRet.ToString());
		Application.DoEvents();
		return iRet;
	}
	
	/// <summary>
	/// 读取xmlNodes转换为list
	/// </summary>
	/// <param name="Xml"></param>
	/// <returns></returns>
	private static List<XmlNode> ReadNodes(XmlDocument Xml)
	{
		XmlNodeList Nodes = Xml.SelectSingleNode("zzxt").ChildNodes;
		List<XmlNode> AT = new List<XmlNode>();
		for (int i = 0; i < Nodes.Count; i++)
		{
			Console.WriteLine(Nodes.Item(i).Name);
			if (Nodes.Item(i).Name.Contains("table") && !Nodes.Item(i).Name.Contains("tablecount"))
			{
				AT.Add(Nodes.Item(i));
			}
		}
		return AT;
	}




	//缴费所有数据 返回: 0:成功 -1:用户返回 -2:用户关闭 -3:业务异常 -4:设备异常
	public static int JFALLDay()
	{
		//public static XmlNode[][] JFSJ2;//所有缴费的数据
		List<XmlNode> AL = new List<XmlNode>();
		for (int i = 0; i < JFSJ2.Length; i++)
		{
			AL.AddRange(JFSJ2[i]);
		}
		// */
		return JFADay(AL.ToArray());
	}

	//缴费一组数据(主要用于交一整天的数据) 返回: 0:成功 -1:用户返回 -2:用户关闭 -3:业务异常 -4:设备异常
	public static int JFADay(XmlNode[] XN)
	{
		PayModeStatic="";
		ZJE = 0;
		Msg="";
		foreach (XmlNode node in XN)
		{//统计总金额
			//ZJE += Convert.ToDecimal(node.SelectSingleNode("cfje").InnerText.Trim());
			ZJE = Convert.ToDecimal(node.SelectSingleNode("zje").InnerText.Trim());

		}
		{
			#region 弹出一个确认界面
			ZZJCore.YesNoForm.YesNoFormParameter YNFP = new ZZJCore.YesNoForm.YesNoFormParameter();
			YNFP.Caption = "您所选的医嘱一共需要缴费" + ZJE.ToString("C") + "元";
			YNFP.Buttons = new ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret, ZZJCore.ZZJControl.Button_OK };
			int iRet = ZZJCore.YesNoForm.ShowForm(YNFP);
			Application.DoEvents();
			if (iRet == -2) return -2;
			if (iRet != 0) return -1;

			//多核对一次缴费601数据,如果修改了医嘱就直接返回不去扣费 以免单边
			if(!GetMX())
			{
				ZZJCore.SuanFa.Proc.Log("医嘱被更改与之前不一致");
				//这里要区分一下，医保去人工退费
				Msg = "医嘱不匹配,返回";
				ZZJCore.BackForm.ShowForm("医嘱被更改!",true);
				return -3;
			}
			#endregion
		}
		ZZJCore.SuanFa.Proc.Log("确定缴费," + ZJE.ToString("C") + "元");

	StartJF:
		decimal YE = Convert.ToDecimal(ZZJCore.Public_Var.patientInfo.DepositAmount);
		ZZJCore.SuanFa.Proc.Log("余额," + YE.ToString("C") + "元");
		#region 如果预交金充足,则直接用预交金缴费,否则选择方式缴费
		ZZJ_Module.Public_Var.OldAmount = Convert.ToDecimal(ZZJCore.Public_Var.patientInfo.DepositAmount);
		ZZJ_Module.Public_Var.NewAmount = Convert.ToDecimal(ZZJCore.Public_Var.patientInfo.DepositAmount);
		if (ZJE <= YE)
		{
			YB_Flag = "0";
			if (Pay(XN) != 0)//缴费
			{
				ZZJCore.SuanFa.Proc.Log("缴费失败");
				ZZJCore.BackForm.ShowForm("缴费失败,请稍后再试!", true);
				return -3;
			}
			ZZJCore.SuanFa.Proc.MsgSend(0xB5, 0, true);//向主控发出退卡指令
			ZZJCore.BackForm.ShowForm("缴费成功!", true);
			Application.DoEvents();
			return 0;
		}
		//else
		//{
		//  //if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "1")
		//  ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "余额不足,请充值，港内卡与附属卡不支持充值!", true);
		//  ZZJCore.BackForm.CloseForm();
		//  return -3;
		//}
        //if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "1")
        //{
        //    ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "余额不足,请至人工窗口办理!", true);
        //ZZJCore.BackForm.CloseForm();
        //return -3;
        //}
		#region   卡内余额不足选择扣费方式
		ZZJCore.SuanFa.Proc.Log("余额不足,显示其它方式供选择");
		ZZJ_Module.ChargeClass.ChargeParameter CP = new ZZJ_Module.ChargeClass.ChargeParameter();
		CP.Caption =""; //"您的余额不足,请选择其它缴费方式:";
		CP.PayType = "缴费支付";
		CP.DC = ZJE;
		int SPMiRet = ZZJ_Module.ChargeClass.Charge(CP);
		Application.DoEvents();
		if (SPMiRet == -1) return -1;
		if (SPMiRet == -2) return -2;
		if (SPMiRet < -2) return -2;
		int PayiRet = -21;
		int	iR=-21;
		if (SPMiRet == 0)
		{
	     	#region   如果银联扣费成功，先进行充值，在进行扣费
					iR = YLCZ.Pub_CZ(CP.DC,1);//传1是说明 充值是从其他地方调用银联充值
				#endregion
					if(iR==0)
					{
						YB_Flag = "0";//这里为银联,所以YB标志要设为0,不然后面缴费接口会按照医保的方式去提交
						PayiRet = Pay(XN, "银联POS", "", ZZJ_Module.UnionPay.UnionPayCardNo, ZZJ_Module.UnionPay.RETDATA.strRef, "", ZZJ_Module.UnionPay.RETDATA.strBatch, ZZJ_Module.UnionPay.RETDATA.strTrace, ZZJ_Module.UnionPay.RETDATA.strRef, ZZJ_Module.UnionPay.RETDATA.strTId);
						//ZZJ_Module.ChargeClass.SaveData((PayiRet == 0) ? 0 : -1); 因为银联是走的充值.再扣费,所以这里不需要存
					}
					else
					{
						ZZJCore.SuanFa.Proc.Log("卡内余额:" + ZZJCore.Public_Var.patientInfo.DepositAmount);
						ZZJCore.SuanFa.Proc.Log("银联卡扣费成功但往卡内充值失败:"+ CP.DC);
						ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "扣费成功但充值失败,请到人工窗口处理!", true);
						ZZJCore.BackForm.CloseForm();
						return -3;
					}
		}
		if (SPMiRet == 1)
		{
		  YB_Flag = "1" ;
			PayiRet = Pay(XN, "社保银联", "", QDYBCard.GRBH, QDYBCard.sJYLSH, QDYBCard.sYYZFLSH, QDYBCard.PCH, QDYBCard.POSH, QDYBCard.YLJYCKH, QDYBCard.POSH);
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
		if (SPMiRet == 2) goto StartJF;
		if (SPMiRet==3)
		{
			YB_Flag = "0";//这里为银联,所以YB标志要设为0,不然后面缴费接口会按照医保的方式去提交
			PayModeStatic="港内卡G";
			PayiRet = Pay(XN, "港内卡G");
		}
		if (PayiRet != 0)
		{
			//isJFSuccess="失败";
			//if (ZZJCore.SuanFa.Proc.ReadPublicINI("JFDoNotVoucher", "0") != "1") ZZJCore.SuanFa.PrintCall.Print(ZZJCore.Public_Var.ZZJ_Config.PT_Dev, PC, 300, 0, false);
			
			if(PayiRet==-99)//医保要去人工退费
			{
				ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "医保扣费成功但缴费失败,请到人工窗口处理!", true);
				//ZZJCore.BackForm.ShowForm("医保扣费成功但缴费失败,请到人工窗口处理!", true);
				return -3;
			}
			//ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "缴费失败,请到人工窗口处理!", true);
			ZZJCore.BackForm.ShowForm("缴费失败,请到人工窗口处理!", true);
			Application.DoEvents();
			//ZZJCore.BackForm.ShowForm( Msg , true);
			ZZJCore.SuanFa.Proc.Log("院内卡余额:" + ZZJCore.Public_Var.patientInfo.DepositAmount);

			return -3;
		}
		ZZJCore.SuanFa.Proc.Log("缴费成功");
		ZZJCore.BackForm.ShowForm("缴费成功!", true);
		return 0;
		#endregion
		#endregion
	}

	//医嘱号 收费模式 开户行 银行卡号社保个人编号 交易流水号 医院支付流水号 银联批次号 pos流水号 参考号TerminalNo 终端号
	static decimal YZJE = 0;//单个医嘱的总金额
	static string ReceiptNo = "";//收据号
	static XmlNode[] XM = null;//项目列表
	static List<XTTM> ATTMX = new List<XTTM>();


	#region   结构枚举
	public struct XTTM
	{
		public string ID;
		public string CFLB;
		public XmlNode xn;

		public override string ToString()
		{
			return ID;
		}
	}

	#endregion

	static int AllPage = 0;
	static int PageIndex = 0;

	#region 缴费XML
	public static int Pay(XmlNode[] XN, string PayMode = "预交金", string Bankname = "", string TransCode = "",
									string TransSerialNo = "", string PayTransNo = "", string BatchNo = "", string PostransNo = "", string ReferenceNo = "", string TerminalNo = "")
	{
		
		Msg="";//错误信息.
		PayModeStatic="";
		if(PayMode=="港内卡G") PayModeStatic="港内卡G";
		ATTMX.Clear();
		#region 获取医嘱ID列表
		string[] GroupIDList = null;
		{
			List<string> AL = new List<string>();
			for (int i = 0; i < XN.Length; i++)
			{
				AL = AddDT(AL, XN[i].SelectSingleNode("cfxh").InnerText.Trim());
			}
			GroupIDList = AL.ToArray();
		}
		#endregion
		//AllPage = GroupIDList.Length;
		//PageIndex = 0;
		//foreach (string GroupID in GroupIDList)
		//{//循环缴单子
		//  YZJE = 0;
		//  int XPHeight = 0;
		//  List<XmlNode> XMList = new List<XmlNode>();//项目列表
		//  foreach (XmlNode node in XN)
		//  {//获取该医嘱的总金额
		//    if (node.SelectSingleNode("cfxh").InnerText.Trim() != GroupID) continue;
		//    int linen = node.SelectSingleNode("cflx").InnerText.Trim().Length / 13;
		//    if ((node.SelectSingleNode("cflx").InnerText.Trim().Length % 13) > 0) linen += 1;
		//    XPHeight += linen * 20;

		//    XMList.Add(node);
		//    YZJE += Convert.ToDecimal(node.SelectSingleNode("cfje").InnerText.Trim());
		//  }
		//  XM = XMList.ToArray();
		//}
		AllPage = GroupIDList.Length;
		PageIndex = 0;
		string InMXXML = "";

		XmlDocument XD = new XmlDocument();
		foreach (string GroupID in GroupIDList)
		{//循环缴单子
			YZJE = 0;
			int XPHeight = 0;
			List<XmlNode> XMList = new List<XmlNode>();//项目列表

			try
			{

				XmlMaker TableMX = XmlMaker.NewTabel();
				TableMX.Add("patid", ZZJCore.Public_Var.patientInfo.PatientID);
				TableMX.Add("cfxh", GroupID);
				TableMX.Add("cflx", "");
				TableMX.Add("zzjzdbh", ZZJCore.Public_Var.ZZJ_Config.ExtUserID);

				string DataIn = XmlMaker.NewXML("602", TableMX).ToString();
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.InMXXML.获取缴费602HIS出参");
				InMXXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(InMXXML, "Service.InMXXML.获取缴费602HIS出参");
				XD.LoadXml(InMXXML);

				XmlNodeList NodesT = XD.SelectSingleNode("zzxt").ChildNodes;

				for (int j = 0; j < NodesT.Count; j++)
				{
					if (NodesT.Item(j).Name.Contains("table") && !NodesT.Item(j).Name.Contains("tablecount"))
					{
						XTTM XM = new XTTM();
						XM.xn = NodesT.Item(j);
						XM.ID = GroupID;
						XM.CFLB = NodesT.Item(j).SelectSingleNode("name").InnerText.Trim();
						ATTMX.Add(XM);
					}
				}
				if (ATTMX.Count == 0)
				{
					Msg="无缴费项目";
					ZZJCore.BackForm.ShowForm("没有查询到信息!", true);
					return -1;
				}// */
			}
			catch (Exception e)
			{
				Msg="调用接口失败,请稍候再试!";
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.BackForm.ShowForm("调用接口失败,请稍候再试!", true);
				return -1;
			}



			//foreach (XmlNode node in ATT)
			//{//获取该医嘱的总金额
			//  if (node.SelectSingleNode("cfxh").InnerText.Trim() != GroupID) continue;
			//  int linen = node.SelectSingleNode("cflx").InnerText.Trim().Length / 13;
			//  if ((node.SelectSingleNode("cflx").InnerText.Trim().Length % 13) > 0) linen += 1;
			//  XPHeight += linen * 20;

			//  XMList.Add(node);
			//  YZJE += Convert.ToDecimal(node.SelectSingleNode("cfje").InnerText.Trim());
			//}
			//XM = XMList.ToArray();
		}

		#region 整理报文
		string PayModeTemp = PayMode;
		XmlMaker Table = XmlMaker.NewTabel();
		string XmlData = "";
		//if (PayModeTemp == "预交金" && ZZJCore.Public_Var.cardInfo.CardType == "1")
		//{
		//    if (ZZJ_Module.UserData.yjjxf(YZJE) != 0)
		//    {
		//        ZZJCore.SuanFa.Proc.Log("区域收费失败!" + ZZJ_Module.UserData.Msg);
		//        return -2;
		//    }
		//    TransSerialNo = ZZJ_Module.UserData.sFlowId;
		//    PostransNo = ZZJ_Module.UserData.LSH;
		//    TransCode = ZZJCore.Public_Var.cardInfo.CardNo;
		//    PayModeTemp = "区域预交";
		//}

		//string OutXML = "<Request>";
		//OutXML += "<CardNo>" + ZZJCore.Public_Var.patientInfo.PatientID + "</CardNo>";
		//OutXML += "<RcptGroupID>" + GroupID + "</RcptGroupID>";
		//OutXML += "<Amt>" + YZJE.ToString("f4") + "</Amt>";
		//OutXML += "<SecrityNo>666</SecrityNo>";
		//OutXML += "<CardSerNo></CardSerNo>";
		//OutXML += "<Moneytype>" + PayModeTemp + "</Moneytype>";
		//OutXML += "<TransType>院内自助</TransType>";
		//OutXML += "<Bankname>" + ZZJCore.Public_Var.cardInfo.CardType + "</Bankname>";//开户行
		//OutXML += "<TransCode>" + TransCode + "</TransCode>";//银行卡号社保个人编号
		//OutXML += "<TransSerialNo>" + TransSerialNo + "</TransSerialNo>";//交易流水号
		//OutXML += "<PayTransNo>" + PayTransNo + "</PayTransNo>";//医院支付流水号
		//OutXML += "<BatchNo>" + BatchNo + "</BatchNo>";//银联批次号
		//OutXML += "<PostransNo>" + PostransNo + "</PostransNo>";//pos流水号
		//OutXML += "<ReferenceNo>" + ReferenceNo + "</ReferenceNo>";//参考号
		//OutXML += "<TerminalNo>" + TerminalNo + "</TerminalNo>";//终端号
		//OutXML += "<UserId>" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID + "</UserId>";
		//OutXML += "</Request>";

		Table = XmlMaker.NewTabel(ZZJCore.Public_Var.patientInfo.PatientID);
		//1是港内卡(zhjb=2)以及附属卡(zhjb=3)   0是院内卡(zhjb=1)     ZZJ_Module.ChargeClass.ChargePayMode == 0为银联
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
			if (ZZJ_Module.GangCardPay.gangCard.payLevel == "2")//支付级别为2 
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
		Table.Add("cardno", PayMode == "港内卡G" ? ZZJ_Module.GangCardPay.gangCard.GangCardNo : "");//港内卡卡号
		Table.Add("Passwd", PayMode == "港内卡G" ? ZZJ_Module.GangCardPay.gangCard.psd : ZZJCore.Public_Var.patientInfo.Password);//港内卡密码
		//Table.Add("Passwd", ZZJCore.Public_Var.patientInfo.Password);
		
		if(PayMode=="社保银联")
		{
			Table.Add("yyzflsh", QDYBCard.sYYZFLSH );
			Table.Add("grbh",  QDYBCard.GRBH );
			Table.Add("jylsh", QDYBCard.sJYLSH );
			Table.Add("pch",  QDYBCard.PCH );
			Table.Add("posh", QDYBCard.POSH );
			Table.Add("yljyckh", QDYBCard.YLJYCKH );
			Table.Add("je", ZJE);
		}
		else
		{
			Table.Add("yyzflsh", "");
			Table.Add("grbh","");
			Table.Add("jylsh", "");
			Table.Add("pch", "");
			Table.Add("posh","");
			Table.Add("yljyckh", "");
			Table.Add("je", ZJE);
		}
		
		
		//if (YB_Flag == "1")//判断是否为医保支付
		//{
		//  Table.Add("zffs", "10");
		//}
		//else 
		//{
		//  if (PayMode == "港内卡G")//支付方式为他人港内卡 支付
		//  {
		//    if (ZZJ_Module.GangCardPay.gangCard.payLevel == "2")
		//    {
		//      Table.Add("zffs", "G");
		//    }
		//    else if(ZZJ_Module.GangCardPay.gangCard.payLevel=="3")
		//    {
		//      Table.Add("zffs", "F");
		//    }
		//  }
		//  else
		//  {
		//    if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "1")
		//    {
		//      if (ZZJ_Module.Public_Var.zhjb.ToString() == "2")
		//      {
		//        Table.Add("zffs", "G");
		//      }
		//      else
		//      {
		//        Table.Add("zffs", "F");
		//      }
		//    }
		//    if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "0" && ZZJ_Module.Public_Var.zhjb.ToString() == "1")
		//    {
		//      Table.Add("zffs", "J");
		//    }
		//    if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "2" && ZZJ_Module.Public_Var.zhjb.ToString() == "1")//这是医保卡做就诊卡用就诊账户的钱支付
		//    {
		//      Table.Add("zffs", "J");
		//    }
		//  }
		//}

		////港内卡支付专用
		//Table.Add("cardno", PayMode == "港内卡G" ?ZZJ_Module.GangCardPay.gangCard.GangCardNo : "");//港内卡卡号
		//Table.Add("Passwd", PayMode == "港内卡G" ? ZZJ_Module.GangCardPay.gangCard.psd : ZZJCore.Public_Var.patientInfo.Password);//港内卡密码
		////Table.Add("Passwd", ZZJCore.Public_Var.patientInfo.Password);
		//Table.Add("yyzflsh",YB_Flag=="1"? QDYBCard.sYYZFLSH:"");
		//Table.Add("grbh", YB_Flag == "1"? QDYBCard.GRBH :"");
		//Table.Add("jylsh", YB_Flag=="1"? QDYBCard.sJYLSH:"");
		//Table.Add("pch", YB_Flag == "1"? QDYBCard.PCH :"");
		//Table.Add("posh", YB_Flag == "1"? QDYBCard.POSH :"");
		//Table.Add("yljyckh",YB_Flag=="1"?  QDYBCard.YLJYCKH:"");
		//Table.Add("je", ZJE);
		


		#endregion
		//    XmlDocument docXML = null;
		try
		{
			string InXML = "";
			//查询医嘱核对与之前数据是否一致 一致返回true 不一致返回false
			if (!GetMX())
			{

				ZZJCore.SuanFa.Proc.Log("医嘱被更改与之前不一致");
				//这里要区分一下，医保去人工退费
				if (PayMode == "社保银联")
				{
					Msg = "缴费失败了";
					return -99;
				}
				Msg = "缴费失败,医嘱不匹配";
				return -3;
			}

			//if (ZZJCore.SuanFa.Proc.ReadPublicINI("JFVirtualPay", "0") == "0")
			//{
				string DataIn = XmlMaker.NewXML("603", Table).ToString();
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.缴费");
				XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(XmlData, "Service.缴费");
				XmlDocument docXML = new XmlDocument();
				docXML.LoadXml(XmlData);
				string iRet = docXML.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();
				if (iRet.ToString() != "0")
				{
					ZZJCore.SuanFa.Proc.Log("解析缴费报文时出错." + "iRet:" + iRet);
					//这里要区分一下，医保去人工退费
					if (PayMode == "社保银联")
					{
						Msg="缴费失败了";
						return -99;
					}
					Msg = docXML.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
					return -3;
				}
			//}
			//else
			//{
			//    InXML = "<Response><ResultCode>0</ResultCode><ErrorMsg></ErrorMsg><RcptNo>2016031431507718</RcptNo></Response>";
			//}

			//   docXML.LoadXml(InXML);

			//if (docXML.SelectSingleNode("Response/ResultCode").InnerText.Trim() != "0")
			//{
			//    if (PayModeTemp == "区域预交") ZZJ_Module.UserData.BackCash();//如果是用区域预交方式缴费,失败了则尝试退费
			//    return -3;
			//}
			//ReceiptNo = docXML.SelectSingleNode("Response/RcptNo").InnerText.Trim();
		}
		catch (Exception e)
		{//解析报文异常,网络异常等不可退费,因为可能缴费成功了
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("调缴费接口603返回: " + XmlData);
			ZZJCore.SuanFa.Proc.Log("缴费失败,可能是因为网络错误." + e.Message);
			return -3;
		}
		if (PayMode == "预交金") ZZJ_Module.Public_Var.NewAmount = ZZJ_Module.Public_Var.NewAmount - ZJE;
		ZZJCore.Public_Var.patientInfo.DepositAmount = ZZJ_Module.Public_Var.NewAmount.ToString();
		int XHeight = 0;
		//  XPHeight += 380;
		// isJFSuccess = "成功";

		//if (ZZJCore.SuanFa.Proc.ReadPublicINI("JFDoNotVoucher", "0") != "1") 
		
		ZZJCore.SuanFa.PrintCall.Print(ZZJCore.Public_Var.ZZJ_Config.PT_Dev, PC, 300, XHeight, false);

		// PageIndex++;
		// ZZJCore.BackForm.ShowForm(string.Format("一共{1}单,正在缴费第{0}单", (PageIndex + 1).ToString(), GroupIDList.Length.ToString()));
		//  }
		return 0;
	}
	#endregion 缴费

	#region 打印凭条
	private static void PC(object sender, PrintPageEventArgs e)
	{
		decimal JES = 0;//每种费用的总额
		int HP = 0, WP = 0;
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
		e.Graphics.DrawString("账户号　：" + ZZJCore.Public_Var.patientInfo.PatientID, NRFont, System.Drawing.Brushes.Black, WP, HP);//
		HP += 16;
		//e.Graphics.DrawString("收据号:" + ReceiptNo, NRFont, System.Drawing.Brushes.Black, WP, HP);//
		//HP += 16;
		e.Graphics.DrawString("已缴金额：" + ZJE.ToString("C"), LXBTFont, System.Drawing.Brushes.Black, WP, HP);
		HP += 21;
		e.Graphics.DrawString("账户余额：" + ZZJ_Module.Public_Var.NewAmount.ToString("C"), NRFont, System.Drawing.Brushes.Black, WP, HP);//
		HP += 16;
		if (PayModeStatic=="港内卡G")
		{
			e.Graphics.DrawString("港内卡号：" + ZZJ_Module.GangCardPay.gangCard.GangCardNo, NRFont, System.Drawing.Brushes.Black, WP, HP);//
			HP += 20;
			e.Graphics.DrawString("港内卡姓名：" + ZZJ_Module.GangCardPay.gangCard.GangCardName, NRFont, System.Drawing.Brushes.Black, WP, HP);//
			HP += 20;
			e.Graphics.DrawString("港内卡余额：" + (ZZJ_Module.GangCardPay.gangCard.YE-Convert.ToDecimal(ZJE)), NRFont, System.Drawing.Brushes.Black, WP, HP);//
			HP += 20;
		}
		
		//e.Graphics.DrawString("缴费描述：" + isJFSuccess, LXBTFont, System.Drawing.Brushes.Black, WP, HP);//
		//HP += 20;
		e.Graphics.DrawString("自助机编号：" + ZZJCore.Public_Var.ZZJ_Config.ExtUserID, NRFont, System.Drawing.Brushes.Black, WP, HP);
		HP += 16;
		e.Graphics.DrawString("打印时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), NRFont, System.Drawing.Brushes.Black, WP, HP);
		HP += 16;
		e.Graphics.DrawString("流水号：" + ZZJCore.Public_Var.SerialNumber, NRFont, System.Drawing.Brushes.Black, WP, HP);
		HP += 26;

		#region

		#region 一天的药品类型表
		List<XTTM> TypeList = new List<XTTM>();//类型表

		foreach (XTTM node in ATTMX)
		{
			TypeList = TypeList.AddDT(node);
		}
		#endregion

		e.Graphics.DrawLine(pen, 8, HP, 277, HP);
		HP += 5;
		e.Graphics.DrawString("项目", NRFont, System.Drawing.Brushes.Black, WP, HP);
		e.Graphics.DrawString("金额", NRFont, System.Drawing.Brushes.Black, 190, HP);
		e.Graphics.DrawString("数量", NRFont, System.Drawing.Brushes.Black, 240, HP);
		HP += 16;
		string DeptName = "";

		foreach (XTTM id in TypeList)
		{
			e.Graphics.DrawLine(pen, 8, HP, 277, HP);
			HP += 5;
			SizeF sizeF2 = e.Graphics.MeasureString(id.CFLB, BTFont);//获取内容的宽度
			e.Graphics.DrawString(id.CFLB, BTFont, System.Drawing.Brushes.Black, WP + ((300 - sizeF2.Width) / 2), HP);
			HP += 28;
			e.Graphics.DrawLine(pen, 8, HP, 277, HP);
			HP += 5;

			foreach (XTTM nodes in ATTMX)
			{
				if (nodes.ID != id.ID) continue;

				#region 打印项目名称,支持换行
				int MeiHangZiShu = 13;//每行字数
				string XMName = nodes.xn.SelectSingleNode("ypmc").InnerText.Trim();//项目名称
				//     DeptName = nodes.SelectSingleNode("CtLoc").InnerText.Trim();//位置
				while (XMName.Length > MeiHangZiShu)
				{
					e.Graphics.DrawString(XMName.Substring(0, MeiHangZiShu), NRFont, System.Drawing.Brushes.Black, WP, HP);
					HP += 16;
					XMName = XMName.Substring(MeiHangZiShu, XMName.Length - MeiHangZiShu);//去前部
				}
				e.Graphics.DrawString(XMName, new Font(new FontFamily("黑体"), 10), System.Drawing.Brushes.Black, WP, HP);
				#endregion
				JES += Convert.ToDecimal(nodes.xn.SelectSingleNode("zje").InnerText.Trim());
				e.Graphics.DrawString(nodes.xn.SelectSingleNode("zje").InnerText.Trim(), NRFont, System.Drawing.Brushes.Black, 190, HP);
				string Pcs = nodes.xn.SelectSingleNode("fysl").InnerText.Trim() + nodes.xn.SelectSingleNode("ypdw").InnerText.Trim();
				// Pcs = Pcs.Substring(0, Pcs.Length - 2);

				e.Graphics.DrawString(Pcs, NRFont, System.Drawing.Brushes.Black, 240, HP);
				HP += 26;
			}
		}


		#endregion
		//  e.Graphics.DrawString("已缴金额：" + JES.ToString("C"), LXBTFont, System.Drawing.Brushes.Black, WP, HP);
		// HP += 26;
		string PayProc = "";
		
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

		if (PayModeStatic == "港内卡G")
		{
			if (ZZJ_Module.GangCardPay.gangCard.payLevel == "2")
			{
				PayProc = "港内卡";
			}
			else if(ZZJ_Module.GangCardPay.gangCard.payLevel=="3")
			{
				PayProc = "附属卡";
			}
		}

		if (ZZJ_Module.ChargeClass.ChargePayMode == 0) PayProc = "银联";
		if (ZZJ_Module.ChargeClass.ChargePayMode == 1) PayProc = "医保";
		e.Graphics.DrawLine(pen, 8, HP, 277, HP);
		e.Graphics.DrawString("支付方式：" + PayProc, LXBTFont, System.Drawing.Brushes.Black, WP, HP);
		HP += 26;
		//  e.Graphics.DrawString("科室：" + DeptName, LXBTFont, System.Drawing.Brushes.Black, WP, HP);//拿药位置
		JES = 0;
		ZJE = 0;
		//  HP += 25;

		e.Graphics.DrawLine(pen, 8, HP, 277, HP);
		HP += 5;
		//e.Graphics.DrawString("本次共打印 " + AllPage.ToString() + " 页当前第 " + (PageIndex + 1) + " 页", NRFont, System.Drawing.Brushes.Black, WP, HP);
		//HP += 16;
		e.Graphics.DrawString("温馨提示：", NRFont, System.Drawing.Brushes.Black, WP, HP);
		HP += 16;
		e.Graphics.DrawString("　　如有药品请到取药窗口领取。", NRFont, System.Drawing.Brushes.Black, WP, HP);
		HP += 16;
		//graphics.Dispose();
	}
	#endregion	

	#region 解锁医嘱
	/// <summary>
	/// 解锁医嘱
	/// </summary>
	private static void unlockTheBill()
	{
	
		if (LockTheBill(0) == false)//解锁失败了
		{
			ZZJCore.SuanFa.Proc.Log("返回");
			ZZJCore.BackForm.ShowForm("医嘱解锁失败,请找医生核实医嘱", true);
			Application.DoEvents();
			ZZJCore.BackForm.CloseForm();
		}
	
	}
	#endregion

	#region 返回医嘱字符串 去掉账户余额的

	/// <summary>
	/// 返回医嘱字符串 去掉账户余额的
	/// </summary>
	/// <param name="docXML"></param>
	/// <returns>处理后的字符串</returns>

	private static string XMLNodeToString(XmlDocument docXML)
	{
		try
		{
			List<XmlNode> AL = ReadNodes(docXML);
			string newdocXML = "";
			foreach (XmlNode x in AL)
			{
				if(x.SelectSingleNode("zhye")!=null)
				{
					XmlNode xx = x.SelectSingleNode("zhye");
					x.RemoveChild(xx);
					newdocXML += x.InnerXml;
				}
			}
			return newdocXML;
		}
		catch(Exception ex)
		{
			ZZJCore.SuanFa.Proc.Log("医嘱转换将zhye节点去掉出错,错误原因:"+ex.ToString());
			return "";
		}
	}

	#endregion

	#region 锁定医嘱
	/// <summary>
	/// 锁定医嘱
	/// </summary>
	/// <param name="jylx">0 解锁 5 锁定</param>
	/// <returns>true 成功 false 失败</returns>
	private static bool LockTheBill(int jylx)
	{
		  ZZJCore.SuanFa.Proc.Log("开始锁定医嘱");
			string InXML="";
			string XmlData="";
			XmlMaker Table=XmlMaker.NewTabel(ZZJCore.Public_Var.patientInfo.PatientID);
			Table.Add("jylx",jylx.ToString());
			string DataIn=XmlMaker.NewXML("717",Table).ToString();
			try
			{
				ZZJCore.SuanFa.Proc.BW(DataIn,"医嘱"+(jylx==5?"锁定":"解锁")+"接口入参");
				InXML=ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL,"CallService",new object[]{DataIn}) as string;
				ZZJCore.SuanFa.Proc.BW(InXML, "医嘱" + (jylx == 5 ? "锁定" : "解锁") + "接口出参");
			}
			catch(Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log((jylx == 5 ? "锁定" : "解锁")+"医嘱出错," + ex.Message);
				return false;
			}
			XmlDocument XD=new XmlDocument();
			XD.LoadXml(InXML);
			if(string.IsNullOrEmpty(XD.SelectSingleNode("zzxt/result/retcode").InnerText))
			{
				ZZJCore.SuanFa.Proc.Log((jylx == 5 ? "锁定" : "解锁")+"医嘱失败");
			  return false;
			}
			string RetCode=XD.SelectSingleNode("zzxt/result/retcode").InnerText;
			if(RetCode=="0") 
			{
				ZZJCore.SuanFa.Proc.Log((jylx == 5 ? "锁定" : "解锁") + "医嘱成功");
				return true;
			}
			else
			{
				ZZJCore.SuanFa.Proc.Log((jylx == 5 ? "锁定" : "解锁") + "医嘱失败");
				return false;
			}
		
	}
	#endregion

	#region 获得医嘱明细,核对与之前医嘱是否一致
	/// <summary>
	/// 获得医嘱明细,核对与之前医嘱是否一致
	/// </summary>
	/// <returns>false 不一致,true 一致</returns>
	private static bool GetMX()
	{
		ZZJCore.SuanFa.Proc.Log("开始查询缴费数据");
		{
		
			#region 整理报文并查询
			string InXML = "";
			string XmlData = "";
			XmlMaker Table = XmlMaker.NewTabel(ZZJCore.Public_Var.patientInfo.PatientID);
			Table.Add("zzjzdbh", ZZJCore.Public_Var.ZZJ_Config.ExtUserID);
			string DataIn = XmlMaker.NewXML("601", Table).ToString();
			try
			{
				ZZJCore.SuanFa.Proc.BW(DataIn, "刷费前Service.获取缴费数据入参");
				InXML = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				//InXML=@"<zzxt><result><retcode>0</retcode><retmsg>交易成功</retmsg></result><tablecount>16</tablecount><table1><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>1</cfxh><cfje>8</cfje><cfsl>1</cfsl><cflx>注射费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table1><table2><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3007</yfdm><cfxh>583090</cfxh><cfje>44</cfje><cfsl>1</cfsl><cflx>化验费</cflx><yfmc>检验科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table2><table3><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3007</yfdm><cfxh>583091</cfxh><cfje>145</cfje><cfsl>1</cfsl><cflx>化验费</cflx><yfmc>检验科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table3><table4><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3007</yfdm><cfxh>583092</cfxh><cfje>135</cfje><cfsl>1</cfsl><cflx>化验费</cflx><yfmc>检验科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table4><table5><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3010</yfdm><cfxh>583095</cfxh><cfje>123.12</cfje><cfsl>4</cfsl><cflx>西药费</cflx><yfmc>门诊药房</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table5><table6><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3010</yfdm><cfxh>583096</cfxh><cfje>373.68</cfje><cfsl>4</cfsl><cflx>西药费</cflx><yfmc>门诊药房</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table6><table7><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3010</yfdm><cfxh>583097</cfxh><cfje>191.64</cfje><cfsl>2</cfsl><cflx>西药费</cflx><yfmc>门诊药房</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table7><table8><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583098</cfxh><cfje>18</cfje><cfsl>2</cfsl><cflx>治疗费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table8><table9><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583098</cfxh><cfje>52</cfje><cfsl>3</cfsl><cflx>材料费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table9><table10><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>10</cfje><cfsl>1</cfsl><cflx>床位费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table10><table11><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>24</cfje><cfsl>2</cfsl><cflx>检查费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table11><table12><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>16</cfje><cfsl>1</cfsl><cflx>输氧费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table12><table13><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>38.5</cfje><cfsl>1</cfsl><cflx>材料费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table13><table14><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583100</cfxh><cfje>26</cfje><cfsl>1</cfsl><cflx>诊察费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table14><table15><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583100</cfxh><cfje>26</cfje><cfsl>3</cfsl><cflx>注射费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table15><table16><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583101</cfxh><cfje>610</cfje><cfsl>4</cfsl><cflx>化验费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table16></zzxt>";
				ZZJCore.SuanFa.Proc.BW(InXML, "刷费前Service.获取缴费数据HIS出参");
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
				ZZJCore.SuanFa.Proc.Log("核对医嘱出错," + ex.Message);
				return false;
			}
			XmlDocument docXML = new XmlDocument();
			docXML.LoadXml(InXML);
			int nodesResult = Convert.ToInt32(docXML.SelectSingleNode("zzxt/result/retcode").InnerText.Trim());//返回结果
			if (nodesResult != 0)
			{
				ZZJCore.SuanFa.Proc.Log("第二次核对医嘱时出错");
				return false;
			}
			
			//ZZJCore.SuanFa.Proc.Log(InXML.Trim());
			//ZZJCore.SuanFa.Proc.Log(xfmxXML.Trim());
			
			//if(InXML.Trim()!=xfmxXML.Trim())
			if (xfmxXML.Trim() != XMLNodeToString(docXML).Trim())
			{
				ZZJCore.SuanFa.Proc.Log("第二次核对医嘱时出错");
				return false;
			}
			else
			{
				return true;
			}
			#endregion
		}
	}

	#endregion

	#region 界面1按钮返回
	private static int DTGButtonEvent(object sender, int ButtonIndex, int SelectIndex)
	{
		int iRet = -6;
		GButtonIndex = ButtonIndex;
		GSelectIndex = SelectIndex;
		if (GButtonIndex == 0)
		{
			iRet = -5;
		}
		else
		{
			iRet = -6;
		}
		//ZZJCore.ShowMessage.ShowForm(null, "你点击的按钮ID是:" + ButtonIndex.ToString() + "你点击的行是:" + SelectIndex.ToString(), false);
		return iRet;//如果返回负数的话.窗口就会关闭
	}
	#endregion
}