public static class YECX
{
	public static bool YECXMain()
	{
		string Msg = "";
		ZZJCore.Public_Var.patientInfo.DepositAmount = "";
		ZZJCore.FormSkin.UseRetCard = true;
		ZZJCore.Public_Var.ModuleName = "余额查询";
		ZZJCore.BackForm.ShowForm("正在查询,请稍候...");
		System.Windows.Forms.Application.DoEvents();
		ZZJCore.Initial.Read();
		if (XMLCore.GetUserInfo(out Msg) != 0)
		{
			if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true);
			ZZJCore.BackForm.CloseForm();
			return true;
		}
		ShowData.ShowForm();
		ZZJCore.BackForm.CloseForm();
		return true;
	}
}//End Class