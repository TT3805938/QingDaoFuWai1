using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;//Image
using System.Runtime.InteropServices;
using System.Data;
using System.Data.SqlClient;
using Mw_MSSQL;
using System.Collections;
using System.Threading;//多线程
using System.Diagnostics;
using System.Threading.Tasks;

static class HYD
{
	/// <summary>
	/// 调用win api将指定名称的打印机设置为默认打印机
	/// </summary>
	/// <param name="Name"></param>
	/// <returns></returns>
	[DllImport("winspool.drv")]
	public static extern bool SetDefaultPrinter(string Name);

	private static DataTable dt = null;

	public static bool HYDMain()
	{
		#region 初始化
		ZZJCore.FormSkin.UseRetCard = true;
		ZZJCore.Public_Var.ModuleName = "报告单打印";
		ZZJCore.BackForm.ShowForm("正在查询,请稍候...", false);
		Application.DoEvents();
		ZZJCore.Initial.Read();
		string Msg = "";
		XMLCore.GetUserInfo(out Msg);
		#endregion
		new Task(Run).Start();
		ZZJCore.SuanFa.Proc.Log("建立瑞美线程成功!");
		#region 查询并统计化验单数量
		try
		{
			MSSQL_Init.Init_DBConfig("HYDConfig.db");
			ZZJCore.SuanFa.Proc.Log("初始化化验单数据库配置完成!");
			string msg = "";
			SqlParameter[] sqlParameter = new SqlParameter[3];
			#region 填充参数
			sqlParameter[0] = new SqlParameter();
			sqlParameter[0].ParameterName = "@AccdNo";//参数名称
			sqlParameter[0].SqlDbType = SqlDbType.VarChar;//参数类型
			sqlParameter[0].SqlValue = ZZJCore.Public_Var.patientInfo.PatientID;//"8346317";

			sqlParameter[1] = new SqlParameter();
			sqlParameter[1].ParameterName = "@StartDay";//参数名称
			sqlParameter[1].SqlDbType = SqlDbType.VarChar;//参数类型

			DateTime StartDT = DateTime.Now.AddDays(int.Parse("-" + ZZJCore.SuanFa.Proc.ReadPublicINI("HYDDAY", "15")));
			sqlParameter[1].SqlValue = StartDT.ToString("yyyy-M-d");//开始日期

			sqlParameter[2] = new SqlParameter();
			sqlParameter[2].ParameterName = "@EndDay";//参数名称
			sqlParameter[2].SqlDbType = SqlDbType.VarChar;//参数类型
			sqlParameter[2].SqlValue = DateTime.Now.ToString("yyyy-M-d"); //结束日期
			#endregion
			MSSQL_Operate mssql_Operate = new MSSQL_Operate();

			var result = mssql_Operate.DataTable_OperateSqlProc("[Proc_Print_GetSampleList]", sqlParameter, out dt, out msg);
			ZZJCore.SuanFa.Proc.Log("调用存储过程成功!");
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("调用存储过程失败!" + e.Message);
			ZZJCore.BackForm.CloseForm();
			return false;
		}
		if (dt == null)
		{
			ZZJCore.SuanFa.Proc.Log("DT=NULL!");
			ZZJCore.BackForm.ShowForm("报告单查询失败,请稍后再试!", true);
			return false;
		}
		int printCnt = 0;//统计可打印化验单数量
		List<DataRow> del = new List<DataRow>();
		ZZJCore.SuanFa.Proc.Log("统计可打印化验单数量");
		for (int i = 0; i < dt.Rows.Count; i++)
		{
			ZZJCore.Public_Var.patientInfo.PatientName = dt.Rows[i]["patname"].ToString();
			if (dt.Rows[i]["samplestate"].ToString().Contains("可打印")) printCnt++;
			if (dt.Rows[i]["samplestate"].ToString().Contains("打印")) del.Add(dt.Rows[i]);
		}

		foreach (DataRow dr in del)
		{
			dt.Rows.Remove(dr);
		}
		#endregion
		if (printCnt > 0) ZZJCore.SuanFa.Proc.MsgSend(0xFFA4, printCnt, true);
		ZZJCore.SuanFa.Proc.Log("可打印化验单数量=" + printCnt.ToString());
		if (printCnt > 0)
		{
			ZZJCore.SuanFa.Proc.Log("进入打印界面!");
			PrintHYDForm PF = new PrintHYDForm(printCnt);
			PF.ShowDialog(ZZJCore.BackForm.BF);
			PF.Dispose();
			Application.DoEvents();
			ZZJCore.SuanFa.Proc.Log("离开打印界面!");
		}

		#region 显示未能打印的化验单
		if (dt.Rows.Count > 0)
		{
			ZZJCore.SuanFa.Proc.Log(string.Format("显示未能打印的化验单,共{0}张", dt.Rows.Count.ToString()));
			#region 填充数据
			string[] LB = new string[] { "单号", "项目", "时间", "状态" };
			byte[] Sizes = new byte[] { 20, 35, 25, 20 };
			string[,] Data = new string[4, dt.Rows.Count];
			for (int i = 0; i < dt.Rows.Count; i++)
			{
				try
				{
					Data[0, i] = (string)dt.Rows[i]["applyno"];
				}
				catch (Exception e)
				{
					ZZJCore.SuanFa.Proc.Log(e);
					ZZJCore.SuanFa.Proc.Log("数据列1异常" + e.Message);
				}
				try
				{
					Data[1, i] = (string)dt.Rows[i]["requestitemsummary"];
				}
				catch (Exception e)
				{
					ZZJCore.SuanFa.Proc.Log(e);
					ZZJCore.SuanFa.Proc.Log("数据列2异常" + e.Message);
				}
				try
				{
					Data[2, i] = ((DateTime)dt.Rows[i]["requesttime"]).ToString();
				}
				catch (Exception e)
				{
					ZZJCore.SuanFa.Proc.Log(e);
					ZZJCore.SuanFa.Proc.Log("数据列3异常" + e.Message);
				}
				try
				{
					Data[3, i] = (string)dt.Rows[i]["samplestate"];
				}
				catch (Exception e)
				{
					ZZJCore.SuanFa.Proc.Log(e);
					ZZJCore.SuanFa.Proc.Log("数据列4异常" + e.Message);
				}
			}
			#endregion
			ZZJCore.SuanFa.Proc.Log("显示不可打印列表!");
			ZZJCore.DataGridViewForm2.ShowForm(new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close }, "", Sizes, LB, Data);
			Application.DoEvents();
			ZZJCore.BackForm.CloseForm();
			ZZJCore.SuanFa.Proc.Log("显示不可打印列表完成,关闭!");
			return true;
		}
		//(object sdr, int ButtonIndex, int SelectIndex) => { return -9; }
		#endregion 显示未能打印的化验单

		if (printCnt > 0)
		{
			ZZJCore.SuanFa.Proc.Log("显示打印完成");
			ZZJCore.BackForm.ShowForm("打印完成!", true);
			ZZJCore.SuanFa.Proc.MsgSend(0xC3, 0, true);
			Application.DoEvents();
			ZZJCore.SuanFa.Proc.Log("打印完成返回!");
			return true;
		}

		if (printCnt <= 0 && dt.Rows.Count <= 0)
		{
			ZZJCore.SuanFa.Proc.Log("显示无可打印!");
			ZZJCore.BackForm.ShowForm("您当前无可打印的报告单!", true);
			Application.DoEvents();
			ZZJCore.SuanFa.Proc.Log("无可打印返回!");
			return true;
		}
		ZZJCore.BackForm.CloseForm();
		ZZJCore.SuanFa.Proc.Log("所有信息完成关闭!");
		return true;

	}//End Main

	private static void Run()
	{
		foreach (Process item in Process.GetProcesses())
		{
			if (item.ProcessName.ToLower().Contains("mzquery")) item.Kill();
		}

		if (string.IsNullOrEmpty(ZZJCore.Public_Var.patientInfo.PatientID)) return;
		SetDefaultPrinter(ZZJCore.Public_Var.ZZJ_Config.HYD_Dev);
		Process process = new Process();
		process.StartInfo.FileName = @"D:\Lis2002\mzquery\mzquery.exe";
		process.StartInfo.Arguments = "/p:" + ZZJCore.Public_Var.patientInfo.PatientID + " /s:192.168.100.11 /d:lis2002 /l:sa /w:ruimei /autoprint /u:zzdy /-NotAutoUpdate";
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.RedirectStandardInput = true;
		try
		{
			process.Start();
			process.StandardInput.AutoFlush = true;
			process.WaitForExit();//等待运行结束
			process.Close();
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("打印化验单EXE异常!" + e.Message);
		}
		process.Dispose();
		ZZJCore.SuanFa.Proc.Log("打印化验单正常退出!");
	}//End Run
}