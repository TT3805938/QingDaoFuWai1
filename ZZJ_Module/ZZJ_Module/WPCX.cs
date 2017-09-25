using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using ZZJCore.ZZJStruct;
using System.Windows.Forms;
using System;

public static class WPCX
{
	private static string[,] FYPDATA = null;//非药品数据
	private static string[,] YPDATA = null;//药品数据
	private static bool HavaData = false;
	public static bool WPCXMain(byte Mode)
	{
		string Msg = "";
		#region 初始化
		ZZJCore.FormSkin.UseRetCard = false;
		ZZJCore.Public_Var.ModuleName = Mode == 0 ? "非药品查询" : "药品查询";
		ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
		Application.DoEvents();
		ZZJCore.Initial.Read();
		XMLCore.GetUserInfo(out Msg);
		if (!HavaData) HavaData = Initial(out Msg);
		if (!HavaData)
		{
			if (Msg.Length > 0) ZZJCore.BackForm.ShowForm(Msg, true, "首页");
			ZZJCore.BackForm.CloseForm();
			return false;
		}
		#endregion
		FormStyle.WPCXStyle.Table(Mode == 0 ? FYPDATA : YPDATA);
		ZZJCore.BackForm.CloseForm();
		return true;
	}

	public static bool Initial(out string Msg)
	{
		OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + ZZJCore.Public_Var.ModulePath + "o70078.mdb; Jet OLEDB:Database Password=");
		
		using (con)
		{
			try
			{
				DataTable DT = new DataTable();
				DataTable DT2 = new DataTable();
				con.Open();
				using (OleDbDataAdapter adapter = new OleDbDataAdapter())
				{
					adapter.SelectCommand = new OleDbCommand("select * from FYP", con);
					adapter.Fill(DT);
					adapter.SelectCommand = new OleDbCommand("select * from YP", con);
					adapter.Fill(DT2);
				}
				con.Close();
				FYPDATA = new string[4, DT.Rows.Count];//查询数据
				for (int i = 0; i < DT.Rows.Count; i++)
				{
					FYPDATA[0, i] = DT.Rows[i]["Item_Name"].ToString();
					FYPDATA[1, i] = DT.Rows[i]["Item_Spec"].ToString();
					FYPDATA[2, i] = DT.Rows[i]["Units"].ToString();
					FYPDATA[3, i] = "￥" + DT.Rows[i]["Price"].ToString();
				}

				YPDATA = new string[4, DT2.Rows.Count];//查询数据
				for (int i = 0; i < DT2.Rows.Count; i++)
				{
					YPDATA[0, i] = DT2.Rows[i]["Item_Name"].ToString();
					YPDATA[1, i] = DT2.Rows[i]["Item_Spec"].ToString();
					YPDATA[2, i] = DT2.Rows[i]["Units"].ToString();
					YPDATA[3, i] = "￥" + DT2.Rows[i]["Price"].ToString();
				}
			}
			catch(Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				Msg = "系统错误!请联系管理员!";
				return false;
			}
			Msg="";
			return true;
		}


	}//End Proc
}//End WPCX
