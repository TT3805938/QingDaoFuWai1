using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ZZJ_Module
{
	public static class Lisreportdll
	{
		/// <summary>
		/// 数据库连接和初始化,调用一次即可
		/// </summary>
		/// <param name="connectstring">连接字符串</param>
		/// <param name="printer">打印机名称，默认打印机传入string.Empty</param>
		/// <param name="hosname">本机名称</param>
		/// <returns></returns>
		[DllImport("HYD/lisreportdll.dll", EntryPoint = "f_lisinit", CharSet = CharSet.Ansi)]
		public extern static int f_lisinit(string connectstring, string printer, string hosname);

		/// <summary>
		/// 更改打印策略，自助打印传2
		/// </summary>
		/// <param name="stylename"></param>
		[DllImport("HYD/lisreportdll.dll", EntryPoint = "pro_changereportstyle", CharSet = CharSet.Ansi)]
		public extern static void pro_changereportstyle(string stylename);

		/// <summary>
		/// 更改打印纸张
		/// </summary>
		/// <param name="ai_paper">纸张类型</param>
		/// <param name="ar_wight">纸张宽度（0为原始大小），单位cm</param>
		/// <param name="ar_height">纸张高度（0为原始大小），单位cm</param>
		/// <param name="ar_left">左边距（0为原始大小），单位cm</param>
		/// <param name="ar_top">上边距（0为原始大小），单位cm</param>
		/// <param name="ar_right">右边距（0为原始大小），单位cm</param>
		/// <param name="ar_bottom">下边距（0为原始大小），单位cm</param>
		/// <param name="direction">打印方向，1直印，2横印</param>
		/// <param name="as_printer">打印机名称，传 string.Empty 使用默认打印机</param>
		/// <returns>0表示设置成功</returns>
		/// <remarks>不调用此函数则会使用报告单原始样式中的设置信息</remarks>
		[DllImport("HYD/lisreportdll.dll", EntryPoint = "f_setpapersize", CharSet = CharSet.Ansi)]
		public extern static int f_setpapersize(int ai_paper, double ar_wight, double ar_height, double ar_left, double ar_top, double ar_right, double ar_bottom, int direction, string as_printer);

		/// <summary>
		/// 打印报告
		/// </summary>
		/// <param name="reportid">用于报告单ID</param>
		/// <param name="opertype">操作方式：1：打印，2：预览</param>
		/// <param name="sysname">调用模块名，自助打印可传入"AUTOPRINT"</param>
		/// <param name="username">操作用户姓名，用于记录打印人，以及打印记录</param>
		/// <returns></returns>
		[DllImport("HYD/lisreportdll.dll", EntryPoint = "f_rmlisreport_ext", CharSet = CharSet.Ansi)]
		public extern static int f_rmlisreport_ext(string reportid, int opertype, string sysname, string username);

		/// <summary>
		/// 断开数据库连接和释放相关对象
		/// </summary>
		/// <returns></returns>
		[DllImport("HYD/lisreportdll.dll", EntryPoint = "f_lisunint", CharSet = CharSet.Ansi)]
		public extern static int f_lisunint();
	}
}
