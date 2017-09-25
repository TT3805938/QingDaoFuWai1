using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HL.Devices.ZKJ;
using ZZJCore.ZZJStruct;


namespace ZZJ_ModuleTest
{
	public partial class ModuleDBG : Form
	{
		ZZJ_Module.MEF M = new ZZJ_Module.MEF();
		string S = "";
		ZZJSubModuleInterface.ReadCardResultInfoCls RCR = new ZZJSubModuleInterface.ReadCardResultInfoCls();

		public ModuleDBG()
		{
			InitializeComponent();
		}

		private void ModuleDBG_Load(object sender, EventArgs e)
		{
			this.Left = 1000;
			M.InitSubModule("", null);
			//RCR.KH = "2012418422";
			//RCR.KH = "4000191036";
			RCR.KH ="00405396";//"0871496800012239"; //"00405396";// "37028519780630352X"; //"0871496800012239";//"00405627";// "00405396";// "00410475";//    00051056    00020322    "370203199007137611";//
			RCR.ZJHM ="370481198201284311";//"37028519780630352X"; //"370203199007137611";//  "430521199406088736";//"370203199007137611";// 
			RCR.KPLX = 0;
			this.Text = RCR.KPLX.ToString() + " " + RCR.KH;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			M.StartSubModule("LOGIN", new object[] { RCR }, out S);
			MessageBox.Show(RCR.LoginFlag.ToString());
		}

		private void button2_Click(object sender, EventArgs e)
		{
			M.StartSubModule("MMXG", new object[] { RCR }, out S);
		}

		private void button3_Click(object sender, EventArgs e)
		{
			M.StartSubModule("XFMXCX", new object[] { RCR }, out S);
		}

		private void button4_Click(object sender, EventArgs e)
		{
			M.StartSubModule("XJCZ", new object[] { RCR }, out S);
		}

		private void button5_Click(object sender, EventArgs e)
		{
			M.StartSubModule("ETWZBK", new object[] { }, out S);
		}

		private void button6_Click(object sender, EventArgs e)
		{
			M.StartSubModule("BGDDY2", new object[] { RCR }, out S);
		}

		private void button7_Click(object sender, EventArgs e)
		{
			M.StartSubModule("GMBL", new object[] { RCR }, out S);
		}

		private void button8_Click(object sender, EventArgs e)
		{
			M.StartSubModule("FYPCX", new object[] { RCR }, out S);
		}

		private void button9_Click(object sender, EventArgs e)
		{
			M.StartSubModule("YPCX", new object[] { }, out S);
		}

		private void button10_Click(object sender, EventArgs e)
		{
			M.StartSubModule("YECX", new object[] { RCR }, out S);
		}

		private void button11_Click(object sender, EventArgs e)
		{
			M.StartSubModule("ZZBK", new object[] { RCR }, out S);
		}

		private void button12_Click(object sender, EventArgs e)
		{
			string Msg = "";
			if (RCR.KPLX != 2)
			{
				MessageBox.Show("卡类型不是社保卡!");
				return;
			}
			if (XMLCore.GetCardNO(out Msg) != 0)
			{
				MessageBox.Show(Msg);
				return;
			}
			XMLCore.XK(ZZJCore.Public_Var.patientInfo.PatientID);
			MessageBox.Show("OK!");
		}

		private void button13_Click(object sender, EventArgs e)
		{
			M.StartSubModule("YLCZ", new object[] { RCR }, out S);
		}

		private void button14_Click(object sender, EventArgs e)
		{
			string M = "";
			int InvokeRet = 0;
			int iRet = XMLCore.CZ(0.01m, out M, out InvokeRet, 1, "银联", "充值");
			if (iRet != 0)
			{
				MessageBox.Show(M);
			}
		}

		private void button15_Click(object sender, EventArgs e)
		{
			ZZJCore.SuanFa.Proc.CloseMonitor(this);
		}

		private void button16_Click(object sender, EventArgs e)
		{
			ZZJCore.SuanFa.Proc.OpenMonitor(this);
		}

		private void button17_Click(object sender, EventArgs e)
		{
			BK.CallOtherModule();
		}

		private void button18_Click(object sender, EventArgs e)
		{
			ZZJ_Module.UnionPay.DC = 0.01m;
			ZZJ_Module.UnionPay.Payment();
		}

		private void button19_Click(object sender, EventArgs e)
		{
			M.StartSubModule("JF", new object[] { RCR }, out S);
		}

		private void button20_Click(object sender, EventArgs e)
		{


			//if (QDYBCard.GRBHZF("7609269", "汇利斯通", "", ZZJCore.Public_Var.patientInfo.PatientID, "交易", 0.01M, 0.01M, 0.01M) != 0)
			//{
			//  ZZJCore.BackForm.ShowForm("支付不成功！" + QDYBCard.Err, true);

			//}// */

			Console.WriteLine(QDYBCard.PCH);
			Console.WriteLine(QDYBCard.POSH);
			Console.WriteLine(QDYBCard.YLJYCKH);
			Console.WriteLine(QDYBCard.YYDM);

			Console.WriteLine(QDYBCard.sYYZFLSH);
			Console.WriteLine(QDYBCard.sJYLSH);
			Console.WriteLine(QDYBCard.Err);

			if (QDYBCard.GRBHTH("7609269", "4abd24469da646749bed00bfe0bd2198", "000048038693313432313136313435373130", 7M) == 0)
			{
				ZZJCore.BackForm.ShowForm("退费成功！", true);

				return;
			}
			return;

			if (QDYBCard.GRBHCXZF("2666555", "ed8eeabbc92248138627138787a99b13", "000023000648313033333537393031393433", 0.02m) == 0)
			{
				ZZJCore.BackForm.ShowForm("退费成功！", true);

				return;
			}
			return;

			int iRet = QDYBCard.YBKHGetHZInfo("0029167915191112");
			iRet = iRet;
		}

		private void button21_Click(object sender, EventArgs e)
		{
			ZZJCore.SuanFa.Proc.MsgSend(0xB6, 2);
		}

		private void button22_Click(object sender, EventArgs e)
		{
			M.StartSubModule("YYQH", new object[] { RCR }, out S);
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			try
			{
				button21.Text = "发送消息:" + string.Format("{0:X}", int.Parse(textBox1.Text));
			}
			catch (Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log(ex);
			}
		}

		private void button23_Click(object sender, EventArgs e)
		{
			ZZJCore.Public_Var.patientInfo.PatientName = "FFF";
			ZZJCore.Public_Var.cardInfo.CardNo = "0007512121";
			ZZJCore.Public_Var.patientInfo.PatientID = "000123";
			ZZJ_Module.ChargeClass.ChargeParameter CP = new ZZJ_Module.ChargeClass.ChargeParameter();
			CP.Caption = "请付款0.03元";
			CP.DC = 0.03m;
			ZZJ_Module.ChargeClass.Charge(CP);
		}

		private void button24_Click(object sender, EventArgs e)
		{
			M.StartSubModule("QYZZBK", new object[] { RCR }, out S);
		}

		private void button25_Click(object sender, EventArgs e)
		{
			M.StartSubModule("QYETBK", new object[] { RCR }, out S);
		}

		private void button26_Click(object sender, EventArgs e)
		{
			ZZJ_Module.UserData.yjjxf(3);
		}

		private void button27_Click(object sender, EventArgs e)
		{
			PrintHYDForm PHF = new PrintHYDForm(2);
			PHF.ShowDialog();

		}

		private void button28_Click(object sender, EventArgs e)
		{
			M.StartSubModule("BGDDY2", new object[] { RCR }, out S);
		}

		private void button29_Click(object sender, EventArgs e)
		{
			ZZJCore.InputForm.InputParameter ip = new ZZJCore.InputForm.InputParameter();
			ip.LabelText = "输入卡号:";
			ip.MAX = 18;
			ip.MIN = 1;
			ip.Buttons = null;
			ulong CardNo = 0;
			ZZJCore.InputForm.InputInt64(ip, ref CardNo);
			RCR.KH = CardNo.ToString();
			RCR.KPLX = int.Parse(textBox2.Text);
		}

		private void button30_Click(object sender, EventArgs e)
		{
			ZZJCore.Public_Var.patientInfo.PatientName = ".";
			ZZJCore.Public_Var.cardInfo.CardNo = "/";
			QYKBK.PrintCard();
		}

		private void button31_Click(object sender, EventArgs e)
		{
			//	string InXML = ZZJCore.SuanFa.POSTInvoke.Invoke("http://10.17.133.1:8080/SetBK.php?IDNo=430521199406088736","");
			string InXML = ZZJCore.SuanFa.POSTInvoke.Invoke("http://10.17.133.1:8080/QueryBK.php?IDNo=430521199406088735", "");
			MessageBox.Show(InXML);
		}

		private void button32_Click(object sender, EventArgs e)
		{
			ZKJ_Dev.ResetPrinter(1);
		}

		private void button33_Click(object sender, EventArgs e)
		{
			M.StartSubModule("TESTQYZZBK", new object[] { RCR }, out S);
		}

		private void button34_Click(object sender, EventArgs e)
		{
			M.StartSubModule("YNKHQYK", new object[] { RCR }, out S);
		}

		private void textBox3_TextChanged(object sender, EventArgs e)
		{
			RCR.KH = textBox3.Text;
		}

		private void textBox2_TextChanged(object sender, EventArgs e)
		{
			RCR.KPLX = int.Parse(textBox2.Text);
		}

		private void button35_Click(object sender, EventArgs e)
		{
			M.StartSubModule("DRGH", new object[] { RCR }, out S);
		}

		private void button36_Click(object sender, EventArgs e)
		{
			M.StartSubModule("CZMXCX", new object[] { RCR }, out S);
		}

		private void button37_Click(object sender, EventArgs e)
		{
			M.StartSubModule("ZYQD", new object[] { RCR }, out S);
		}

		private void button38_Click(object sender, EventArgs e)
		{
			M.StartSubModule("YYJS", new object[] { RCR }, out S);
		}

		private void button39_Click(object sender, EventArgs e)
		{
			//ZZJ_Module.MYDDC.MYDDCMain();
			M.StartSubModule("MYDDC", new object[] { RCR }, out S);
		}
		/// <summary>
		/// 创建新的标签
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="fonts"></param>
		/// <param name="Text"></param>
		/// <param name="nColor"></param>
		/// <returns></returns>
		private static FontText NewText(int x, int y, Font fonts, string Text, Color nColor)
		{
			FontText Ro = new ZZJCore.ZZJStruct.FontText();
			Ro.FontColor = nColor;
			Ro.TextFont = fonts;
			Ro.Text = Text;
			Ro.X = x;
			Ro.Y = y;
			Ro.W = 500;
			return Ro;
		}
		private void button40_Click(object sender, EventArgs e)
		{
			//ZZJCore.ZZJStruct.YYDoctEx[] YYDC = new ZZJCore.ZZJStruct.YYDoctEx[2];
			//YYDC[0]=new YYDoctEx();
			//List<string> am=new List<string>();
			//for(int i=0;i<7;i++)
			//{
			//  am.Add("预约");
			//}
			//YYDC[0].AM=new string[7];
			//YYDC[0].PM = new string[7];
			//YYDC[0].AM = am.ToArray();
			//YYDC[0].PM = am.ToArray();
			//YYDC[0].AME = new bool[] { true, true, true, true, true, true, true };
			//YYDC[0].PME = new bool[] { true, true, true, true, true, true, true };
			//YYDC[0].GHbuttString="";
			//YYDC[0].GHButt=false;
			//YYDC[0].Datas = new ZZJCore.ZZJStruct.FontText[2];
			//YYDC[0].Datas[0] = NewText(100, 10, DefaultFont,"姓名:", Color.White);
			//YYDC[0].Datas[1] = NewText(180, 6, DefaultFont, "邢燕", Color.White);//Color.FromArgb(255,255,128,0)


			//YYDC[1] = new YYDoctEx();
			//List<string> amm = new List<string>();
			//for (int i = 0; i < 7; i++)
			//{
			//  amm.Add("无号");
			//}
			//YYDC[1].AM = new string[7];
			//YYDC[1].PM = new string[7];
			//YYDC[1].AM = am.ToArray();
			//YYDC[1].PM = am.ToArray();
			//YYDC[1].AME = new bool[] { true, true, true, true, true, true, true };
			//YYDC[1].PME = new bool[] { true, true, true, true, true, true, true };
			//YYDC[1].GHbuttString = "";
			//YYDC[1].GHButt = false;
			//YYDC[1].Datas = new ZZJCore.ZZJStruct.FontText[2];
			//YYDC[1].Datas[0] = NewText(100, 10, DefaultFont, "姓名:", Color.White);
			//YYDC[1].Datas[1] = NewText(180, 6, DefaultFont, "王晓", Color.White);//Color.FromArgb(255,255,128,0)
			


			//int line=0;
			//int index=0;
			//int a= ZZJCore.YYSelect.ShowForm("请选择预约医生", YYDC,ref line,ref index);


			//MessageBox.Show(string.Format("您选择的是第{2}个医生{0}行{1}列",line,index,a));








			ZZJCore.Public_Var.patientInfo.PatientName = "王诚";
			ZZJCore.Public_Var.SerialNumber = "101010-20170914-0000023";
			string[] NR1 = new string[] { "交易金额", "交易时间" };
			string[] NR2 = new string[] { "￥26", DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") };
			ZZJCore.SuanFa.PrintCall.PrintPT("预交金支付凭证", NR1, NR2, "", false);
			//ZZJ_Module.YYGH.TestQ();
		  //string retStr=	ZZJ_Module.YYGH.GetHisPbXml301(0,"2017-07-25","2017-07-30");
			//<zzxt><result><retcode>0</retcode><retmsg>交易成功</retmsg></result><tablecount>2</tablecount><table1><ysdm>001   </ysdm><ysmc>医技</ysmc><ksdm>3007</ksdm><ksmc>检验科</ksmc><py>yj      </py><wb>ar      </wb></table1><table2><ysdm>100   </ysdm><ysmc>史秀忠</ysmc><ksdm>1002</ksdm><ksmc>内科</ksmc><py>sxz     </py><wb>ktk     </wb></table2></zzxt>
			//ZZJ_Module.YYGH.YYGHMain();
			//
			//<zzxt><result><retcode>0</retcode><retmsg>交易成功</retmsg></result><tablecount>1</tablecount><table1><ksdm>1003</ksdm><ksmc>外科</ksmc><py>wk      </py><wb>qt      </wb></table1></zzxt>
			//string retStr = ZZJ_Module.YYGH.GetHisPbXml302(1, "001", "20170904", "20170930");
			//<zzxt><result><retcode>0</retcode><retmsg>交易成功</retmsg></result><tablecount>1</tablecount><table1><pbmxxh>54</pbmxxh><ksdm>3007</ksdm><ksmc>检验科</ksmc><ysdm>001</ysdm><ysmc>医技</ysmc><py>yj      </py><wb>ar      </wb><ghf>19</ghf><zlf>19</zlf><ghs>0</ghs><xhs>110</xhs><kszjbz>0</kszjbz><ybks_id></ybks_id><ybks_mc></ybks_mc><yyrq>20170905</yyrq><kssj>17:00</kssj><jssj>21:00</jssj></table1></zzxt>
			//string InXML=@"<zzxt><result><retcode>0</retcode><retmsg>交易成功</retmsg></result><tablecount>16</tablecount><table1><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>1</cfxh><cfje>8</cfje><cfsl>1</cfsl><cflx>注射费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table1><table2><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3007</yfdm><cfxh>583090</cfxh><cfje>44</cfje><cfsl>1</cfsl><cflx>化验费</cflx><yfmc>检验科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table2><table3><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3007</yfdm><cfxh>583091</cfxh><cfje>145</cfje><cfsl>1</cfsl><cflx>化验费</cflx><yfmc>检验科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table3><table4><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3007</yfdm><cfxh>583092</cfxh><cfje>135</cfje><cfsl>1</cfsl><cflx>化验费</cflx><yfmc>检验科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table4><table5><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3010</yfdm><cfxh>583095</cfxh><cfje>123.12</cfje><cfsl>4</cfsl><cflx>西药费</cflx><yfmc>门诊药房</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table5><table6><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3010</yfdm><cfxh>583096</cfxh><cfje>373.68</cfje><cfsl>4</cfsl><cflx>西药费</cflx><yfmc>门诊药房</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table6><table7><patid>84835</patid><hzxm>党福海</hzxm><yfdm>3010</yfdm><cfxh>583097</cfxh><cfje>191.64</cfje><cfsl>2</cfsl><cflx>西药费</cflx><yfmc>门诊药房</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table7><table8><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583098</cfxh><cfje>18</cfje><cfsl>2</cfsl><cflx>治疗费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table8><table9><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583098</cfxh><cfje>52</cfje><cfsl>3</cfsl><cflx>材料费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table9><table10><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>10</cfje><cfsl>1</cfsl><cflx>床位费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table10><table11><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>24</cfje><cfsl>2</cfsl><cflx>检查费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table11><table12><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>16</cfje><cfsl>1</cfsl><cflx>输氧费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table12><table13><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583099</cfxh><cfje>38.5</cfje><cfsl>1</cfsl><cflx>材料费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table13><table14><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583100</cfxh><cfje>26</cfje><cfsl>1</cfsl><cflx>诊察费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table14><table15><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583100</cfxh><cfje>26</cfje><cfsl>3</cfsl><cflx>注射费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table15><table16><patid>84835</patid><hzxm>党福海</hzxm><yfdm>1005</yfdm><cfxh>583101</cfxh><cfje>610</cfje><cfsl>4</cfsl><cflx>化验费</cflx><yfmc>急诊科</yfmc><cardno>370204193911090814</cardno><zje>1840.9</zje><zhye>1840.9</zhye></table16></zzxt>";
			//string retStr=ZZJ_Module.YYGH.GetHisPbXml303(ZZJCore.Public_Var.patientInfo.PatientID,"54","3007","检验科","001");
		}

		private void button41_Click(object sender, EventArgs e)
		{
			ZZJCore.Public_Var.patientInfo.PatientName="王诚";
			ZZJCore.Public_Var.SerialNumber="101010-20170911-0000022";
			List<ZZJCore.ZZJStruct.DataLine> AL = new List<ZZJCore.ZZJStruct.DataLine>();
			AL.Add(new ZZJCore.ZZJStruct.DataLine("金额", "￥200", null, new Font(new FontFamily("黑体"), 20, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("充值结果",  "【成功】", null, new Font(new FontFamily("黑体"), 20, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("余额", "￥200", null, null));
		
			ZZJCore.SuanFa.PrintCall.PrintPT("自助机充值凭证", AL.ToArray(), "如果充值失败请持本凭证至窗口!", false);
		}

		private void button42_Click(object sender, EventArgs e)
		{
			M.StartSubModule("GHNY", new object[] { RCR }, out S);
			//ZZJ_Module.GHNY.GHMain();
		}

		private void button43_Click(object sender, EventArgs e)
		{
			M.StartSubModule("YYGH", new object[] { RCR }, out S);
		}

		private void button44_Click(object sender, EventArgs e)
		{
			M.StartSubModule("YYQH", new object[] { RCR }, out S);
		}

		private void button45_Click(object sender, EventArgs e)
		{
			ZZJ_Module.FWJGQD.Main();
		}




	}//End Class
}