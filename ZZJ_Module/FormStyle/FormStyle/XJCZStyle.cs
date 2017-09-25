using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FormStyle
{
	public static class XJCZStyle
	{
		public static int XJSFForm(bool NeedRetButt, ref decimal DC)
		{
			ZZJCore.XJSFForm2.XJSFFormParameter XJSFP = new ZZJCore.XJSFForm2.XJSFFormParameter();
			XJSFP.Caption = "请逐张放入纸币";
			XJSFP.TS3 = "3.本机只接收100元,50元纸币";
			if (!NeedRetButt)
			{
				XJSFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_OK, ZZJCore.ZZJControl.Button_Close };
			}
			else
			{
				XJSFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_OK, ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret };
			}
			int iRet = ZZJCore.XJSFForm2.ShowForm(XJSFP, ref DC);
			Application.DoEvents();
			return iRet;
		}
	}//End Class
}
