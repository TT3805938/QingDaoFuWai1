using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace FormStyle
{
	public static class JFStyle
	{
		public static int DateGridView001(XmlNode[][] JFSJ2, string[] SJLists, ZZJCore.DataGridViewForm.ButtonEvent DTGButtonEvent)
		{
			string[] BT = new string[] { "日期", "总金额", "查看详细", "缴费" };
			byte[] Sizes = new byte[] { 40, 20, 20, 20 };
			string[,] Datas = new string[BT.Length, JFSJ2.Length];
			for (int i = 0; i < JFSJ2.Length; i++)
			{//枚举所有日期
				decimal YZJE = (from ST in JFSJ2[i] select Convert.ToDecimal(ST.SelectSingleNode("Price").InnerText.Trim())).Sum();

				Datas[1, i] = YZJE.ToString("C");
				Datas[0, i] = SJLists[i].Substring(0, SJLists[i].IndexOf(" "));//截取日期
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
			string[] BT = new string[] { "项目名称", "项目类型", "金额", "数量" };
			byte[] Sizes = new byte[] { 45, 15, 20, 20 };
			string[,] Data = new string[BT.Length, JFSJ2[GSelectIndex - 1].Length];
			for (int i = 0; i < JFSJ2[GSelectIndex - 1].Length; i++)
			{
				XmlNode node = JFSJ2[GSelectIndex - 1][i];
				Data[0, i] = node.SelectSingleNode("DeptName").InnerText.Trim();
				Data[1, i] = node.SelectSingleNode("CateName").InnerText.Trim();
				Data[2, i] = Convert.ToDecimal(node.SelectSingleNode("Price").InnerText.Trim()).ToString("C");
				Data[3, i] = node.SelectSingleNode("Num").InnerText.Trim();
				Data[3, i] = Data[3, i].Substring(0, Data[3, i].Length - 2);
			}

			#region 添加按钮
			List<ZZJCore.ZZJStruct.ZZJButton> AL = new List<ZZJCore.ZZJStruct.ZZJButton>();
			ZZJCore.ZZJStruct.ZZJButton zzjbutt = ZZJCore.ZZJControl.NewButton_OK();
			zzjbutt.Text = "全部缴费";//按钮上的字
			zzjbutt.Rect.X -= zzjbutt.Rect.Width / 2;
			zzjbutt.Rect.Width += zzjbutt.Rect.Width / 2;
			zzjbutt.RetData = -7;//返回值自己定一个(必须是负数)
			AL.Add(zzjbutt);
			AL.Add(ZZJCore.ZZJControl.Button_Close);
			AL.Add(ZZJCore.ZZJControl.Button_Ret);
			#endregion
			ZZJCore.SuanFa.Proc.Log("显示第2层界面");
			int iRet = ZZJCore.DataGridViewForm2.ShowForm(AL.ToArray(), "", Sizes, BT, Data, null, null, false, false);
			ZZJCore.SuanFa.Proc.Log("第2层界面返回iRet=" + iRet.ToString());
			Application.DoEvents();
			return iRet;
		}
	}//End Class
}
