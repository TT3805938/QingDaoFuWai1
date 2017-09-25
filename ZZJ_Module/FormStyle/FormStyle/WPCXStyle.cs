using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FormStyle
{
	public static class WPCXStyle
	{
		public static int Table(string[,]Datas)
		{
			List<ZZJCore.ZZJStruct.ZZJButton> bts = new List<ZZJCore.ZZJStruct.ZZJButton>();
			bts.Add(ZZJCore.ZZJControl.Button_Close);
			string[] BT = { "名称", "规格", "单位", "零售价格" };//表头
			byte[] Sizes = { 50, 20, 10, 20 };//列宽
			ZZJCore.DataGridViewForm2.ShowForm(bts.ToArray(), "", Sizes, BT, Datas, false, 1, 20, 10);
			return 0;
		}
	}//Enc Class
}
