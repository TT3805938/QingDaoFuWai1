using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
namespace ZZJ_Module
{
	public static class Msprint
	{
		private static bool lstate = false;
		/// <summary>
		/// 自动设定USB端口
		/// </summary>
		/// <returns>返回0表示设定成功。</returns>
		[DllImport("MsprintsdkRM.dll", EntryPoint = "SetUsbportauto", CharSet = CharSet.Ansi)]
		private static extern int SetUsbportauto(); //unsafe

		/// <summary>
		/// 初始化打印机
		/// </summary>
		/// <returns>返回0表示设定成功。</returns>
		[DllImport("MsprintsdkRM.dll", EntryPoint = "SetInit", CharSet = CharSet.Ansi)]
		private static extern int SetInit();//unsafe

		/// <summary>
		/// 获取打印机的状态。
		/// </summary>
		/// <returns>
		/// 0：正常
		/// 1：打印机未连接
		/// 2：色带耗尽
		/// 3：上纸出错
		/// 4：出纸出错
		/// 5：纸仓无纸
		/// 6：打印头温度异常
		/// 7：抬压头异常
		/// 8：上纸或打印中
		/// </returns>
		[DllImport("MsprintsdkRM.dll", EntryPoint = "GetStatusPMDYJ", CharSet = CharSet.Ansi)]
		public static extern int GetStatusPMDYJ();//unsafe

		/// <summary>
		/// 打印指定的磁盘路径上的单色位图（8bit）文件
		/// </summary>
		/// <param name="strData">指定的位图的完整路径</param>
		/// <returns>返回0表示打印成功。</returns>
		[DllImport("MsprintsdkRM.dll", EntryPoint = "PrintDiskbmpfile", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern int PrintDiskbmpfile(ref byte strData);//unsafe

		/// <summary>
		/// 关闭USB端口
		/// </summary>
		/// <returns></returns>
		[DllImport("MsprintsdkRM.dll", EntryPoint = "SetClose", CharSet = CharSet.Ansi)]
		public static extern int SetClose();//unsafe

		/// <summary>
		/// 打印文件
		/// </summary>
		/// <param name="bmp8bitfile">8位Bmp文件路径</param>
		/// <returns></returns>
		public static int PrintBmp8(string bmp8bitfile)
		{
			StringBuilder sBuf = new StringBuilder(bmp8bitfile);
			try
			{
				byte[] H = Encoding.ASCII.GetBytes(bmp8bitfile);
				return PrintDiskbmpfile(ref H[0]);
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				return -999;
			}
		}

		/// <summary>
		/// 返回USB端口是否已打开
		/// </summary>
		/// <returns></returns>
		public static bool IsOpen()
		{
			return lstate;
		}

		/// <summary>
		/// 初始化Usb端口
		/// </summary>
		/// <returns></returns>
		public static int Open()
		{
			if (lstate) Close();

			if (SetUsbportauto() == 0)
			{
				if (SetInit() == 0)
				{
					lstate = true;
					return 0;
				}
				else
				{
					return -1;
				}
			}
			else
			{
				return -2;
			}
		}
		/// <summary>
		/// 关闭USB端口
		/// </summary>
		public static void Close()
		{
			if (lstate)
			{
				SetClose();
				lstate = false;
			}
		}

		/// <summary>
		/// 获取设备状态
		/// </summary>
		/// <returns></returns>
		public static int GetState()
		{
			if (lstate)
			{
				return GetStatusPMDYJ();
			}
			else
			{
				return -1;
			}
		}




	}
}