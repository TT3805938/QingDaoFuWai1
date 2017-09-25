using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZZJ_Module
{
	public static class Public_Proc
	{
		/// <summary>
		/// 检测其它模块是否可用
		/// </summary>
		/// <param name="ModuleCode">模块代码(并不是MEF模块代码)</param>
		/// <returns></returns>
		public static bool TestModule(string ModuleCode)
		{
			string IniPath = @"D:\ZZJ\Hospital\Config\MKConfig.ini";
			int iRet = 0;
			if (ZZJCore.SuanFa.Proc.ReadIniData(ModuleCode, "Is_Enabled", "0", IniPath, ref iRet) != "1") return false;
			if (ZZJCore.SuanFa.Proc.ReadIniData(ModuleCode, "Is_Visible", "0", IniPath, ref iRet) != "1") return false;
			string StartTime = "";
			string EndTime = "";
			if (DateTime.Now.Hour < 12)
			{
				StartTime = ZZJCore.SuanFa.Proc.ReadIniData(ModuleCode, "AM_Start_Time", "23:59:59", IniPath, ref iRet);
				EndTime = ZZJCore.SuanFa.Proc.ReadIniData(ModuleCode, "AM_End_Time", "00:00:00", IniPath, ref iRet);
			}
			else
			{
				StartTime = ZZJCore.SuanFa.Proc.ReadIniData(ModuleCode, "PM_Start_Time", "23:59:59", IniPath, ref iRet);
				EndTime = ZZJCore.SuanFa.Proc.ReadIniData(ModuleCode, "PM_End_Time", "00:00:00", IniPath, ref iRet);
			}

			int iStartTime = int.Parse(StartTime.Substring(0, 2)) * 60 * 60;
			iStartTime += int.Parse(StartTime.Substring(3, 2)) * 60;
			iStartTime += int.Parse(StartTime.Substring(6, 2));
			if (((DateTime.Now.Hour * 3600) + (DateTime.Now.Minute * 60) + DateTime.Now.Second) < iStartTime) return false;

			int iEndTime = int.Parse(EndTime.Substring(0, 2)) * 60 * 60;
			iEndTime += int.Parse(EndTime.Substring(3, 2)) * 60;
			iEndTime += int.Parse(EndTime.Substring(6, 2));
			if (((DateTime.Now.Hour * 3600) + (DateTime.Now.Minute * 60) + DateTime.Now.Second) > iEndTime) return false;

			return true;
		}

		/// <summary>
		/// 读取数据字典
		/// </summary>
		/// <param name="DataCode"></param>
		/// <returns></returns>
		public static string ReadData(string DataCode)
		{
			int iRet = 0;
			return "";
		}

		/// <summary>
		/// 读取自助机配置
		/// </summary>
		/// <param name="DataCode"></param>
		/// <returns></returns>
		public static string ReadZZJConfig(string DataCode)
		{
			int iRet = 0;
			return ZZJCore.SuanFa.Proc.ReadIniData("ZZJ", DataCode, "0", @"D:\ZZJ\Hospital\Config\ZZJConfig.ini", ref iRet);
		}





	}
}
