using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace FormStyle
{
	public static class YLCZStyle
	{
		public static int InputAmount(out decimal DC)
		{
			ZZJCore.InputForm.InputAmountParameter IAP = new ZZJCore.InputForm.InputAmountParameter();
			IAP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };
			int iRet = ZZJCore.InputForm.InputAmount(IAP, out DC);
			Application.DoEvents();
			return iRet;
		}

		public static int Alert(decimal DC)
		{
			ZZJCore.YesNoForm.YesNoFormParameter YNFP = new ZZJCore.YesNoForm.YesNoFormParameter();
			YNFP.Caption = string.Format("您将充值{0}元", DC.ToString("C"));//标题
			YNFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_Ret, ZZJCore.ZZJControl.Button_OK };//添加按钮
			int iRet = ZZJCore.YesNoForm.ShowForm(YNFP);
			Application.DoEvents();
			return iRet;
		}
	}//End Class
}
