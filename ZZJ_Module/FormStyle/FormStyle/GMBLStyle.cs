using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;


namespace FormStyle
{
	public static class GMBLStyle
	{
		public static int DYBLYesNo()
		{
			ZZJCore.YesNoForm.YesNoFormParameter YNFPM = new ZZJCore.YesNoForm.YesNoFormParameter();
			YNFPM.Caption = "是否开始打印病历本?";
			#region 添加按钮
			ZZJCore.ZZJStruct.ZZJButton Button_F = ZZJCore.ZZJControl.NewButton_Ret();
			Button_F.Text = "否";
			Button_F.RetData = -5;
			Button_F.Rect.X = 800;
			ZZJCore.ZZJStruct.ZZJButton Button_S = ZZJCore.ZZJControl.NewButton_OK();
			Button_S.Text = "是";
			YNFPM.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, Button_S, Button_F };
			#endregion
			int SFM = ZZJCore.YesNoForm.ShowForm(YNFPM);
			Application.DoEvents();
			return SFM;
		}



	}//End Class
}
