using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace FormStyle
{
	public static class LoginStyle
	{
		public static int HKYesNo()
		{
			ZZJCore.YesNoForm.YesNoFormParameter YNFP = new ZZJCore.YesNoForm.YesNoFormParameter();
			ZZJCore.ZZJStruct.FontText AText = new ZZJCore.ZZJStruct.FontText();
			AText.X = 60;//设置位置
			AText.Y = 180;
			AText.H = 500;
			AText.W = 1160;
			AText.Text = "　　按照青岛市卫计委部署，2016年4月27日起，我院统一启用新的“青岛市区域诊疗卡”，原就诊卡更换之后作废。如果不能提供有效身份证件，只能在人工窗口办理临时卡，临时卡将不能使用自助设备的全部功能。";//内容
			YNFP.Texts = new ZZJCore.ZZJStruct.FontText[] { AText };
			YNFP.Voice = "请阅读温馨提示";
			#region 添加按钮
			ZZJCore.ZZJStruct.ZZJButton Button_F = ZZJCore.ZZJControl.NewButton_Ret();
			Button_F.Text = "否";
			Button_F.RetData = -5;
			Button_F.Rect.X = 800;
			ZZJCore.ZZJStruct.ZZJButton Button_S = ZZJCore.ZZJControl.NewButton_OK();
			Button_S.Text = "继续";
			YNFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, Button_S };
			#endregion
			int iRet = ZZJCore.YesNoForm.ShowForm(YNFP);
			Application.DoEvents();
			return iRet;
		}
	}//End Class
}