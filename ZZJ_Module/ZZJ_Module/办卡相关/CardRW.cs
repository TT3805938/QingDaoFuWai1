using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HL.Devices.ZKJ;//证件卡 
using Mw_Public;
using Mw_Voice;
using System.Threading;
using System.Collections;
using System.IO;
using System.Windows.Forms;

public static class CardRW
{
	#region 走卡
	/// <summary>
	/// 走卡
	/// </summary>
	/// <param name="pwd">密码</param>
	/// <param name="port">端口号</param>
	/// <param name="Address">走卡位</param>
	/// <returns></returns>
	public static int MoveCard(byte[] pwd, int port, byte Address)
	{
		#region 打开发卡器
		string msg = "";
		ZZJCore.SuanFa.Proc.Log("发卡器端口:" + port.ToString());
		if (!ZKJ_Dev.M1_Init(ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev, port, pwd, ref msg, 0))//读写模块初始化失败
		{
			Pl_Fuction.PostMessage(ZZJCore.Public_Var.MainFrmHandleIntptr, ZZJCore.Public_Var.MsgID, new IntPtr(0x24), new IntPtr(0));// 0x24发卡器初始化失败
			//ErrSave("初始化时异常!" + msg, 1);
			ZZJCore.SuanFa.Proc.Log("打开读卡器失败!");
			return -1;
		}
		ZZJCore.SuanFa.Proc.Log("打开读卡器成功!");
		#endregion
		try
		{
			if (ZKJ_Dev.RequestM1Card() == 0)
			{
				ZKJ_Dev.CloseDev();
				ZZJCore.SuanFa.Proc.Log("寻卡成功!已经有卡不用走卡了!");
				return 0;
			}
			ZZJCore.SuanFa.Proc.Log("寻卡失败!需要从卡槽走一张卡出来!");
			if (ZKJ_Dev.MoveCard(3) != 0)
			{
				ZKJ_Dev.CloseDev();
				ZZJCore.SuanFa.Proc.Log("走卡失败!");
				return -2;
			}
			ZZJCore.SuanFa.Proc.Log("走卡成功!");
			ZZJCore.SuanFa.Proc.sleep(0xFF);
			if (ZKJ_Dev.RequestM1Card() != 0)
			{
				ZZJCore.SuanFa.Proc.Log("走卡成功,但寻卡失败!");
				ZKJ_Dev.CloseDev();
				return -3;
			}
			ZZJCore.SuanFa.Proc.Log("走卡成功!寻卡成功!");
			ZKJ_Dev.CloseDev();
			return 0;
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			ZZJCore.SuanFa.Proc.Log("走卡过程发生异常!" + e.Message);
			ZKJ_Dev.CloseDev();
			return -4;
		}
	}
	#endregion

	#region 读卡
	/// <summary>
	/// 读卡(本函数不负责走卡)
	/// </summary>
	/// <param name="pwd">密码</param>
	/// <param name="port">端口</param>
	/// <param name="SQ">扇区</param>
	/// <param name="cache">块</param>
	/// <param name="start">起始字节</param>
	/// <param name="Buffer">缓冲区</param>
	/// <returns></returns>
	public static int ReadCard(byte[] pwd, int port, byte SQ, byte cache, byte start, ref byte[] Buffer)
	{
		#region 打开发卡器
		string msg = "";
		ZZJCore.SuanFa.Proc.Log("发卡器端口:" + port.ToString());
		if (!ZKJ_Dev.M1_Init(ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev, port, pwd, ref msg, SQ))//读写模块初始化失败
		{
			Pl_Fuction.PostMessage(ZZJCore.Public_Var.MainFrmHandleIntptr, ZZJCore.Public_Var.MsgID, new IntPtr(0x24), new IntPtr(0));// 0x24发卡器初始化失败
			ZZJCore.SuanFa.Proc.Log("打开读卡器失败!");
			return -1;
		}
		ZZJCore.SuanFa.Proc.Log("打开读卡器成功!");
		#endregion

		#region 验证密码
		Thread.Sleep(100);
		int iRet = ZKJ_Dev.Authentication_passaddr(pwd);
		if (iRet != 0)
		{
			ZKJ_Dev.CloseDev();
			ZZJCore.SuanFa.Proc.Log("密码错误!" + iRet.ToString());
			return -2;
		}
		#endregion

		byte[] current_kh = null;
		iRet = ZKJ_Dev.ReadM1Card(ref current_kh);
		ZKJ_Dev.CloseDev();

		if (iRet != 0)
		{
			ZZJCore.SuanFa.Proc.Log("读卡失败!" + iRet.ToString());
			return -3;
		}

		for (int i = 0; i < Buffer.Length; i++)
		{
			if (current_kh.Length <= (i + start)) return -6;//如果源数组的长度不足.则判为异常
			Buffer[i] = current_kh[i + start];
		}
		return 0;
	}
	#endregion

	#region 写卡
	/// <summary>
	/// 写卡
	/// </summary>
	/// <param name="pwd">密码</param>
	/// <param name="port">端口</param>
	/// <param name="SQ">扇区</param>
	/// <param name="cache">块</param>
	/// <param name="start">起点</param>
	/// <param name="Buffer">数据</param>
	/// <returns></returns>
	public static int WriteCard(byte[] pwd, int port, byte SQ, byte cache, byte start, byte[] Buffer)
	{
		byte[] LocalBuffer = new byte[16];
		//判断数据长度是否正常
		if ((Buffer.Length + start) > 16) return -5;
		//读卡
		if (ReadCard(pwd, port, SQ, cache, 0, ref LocalBuffer) != 0) return -12;

		for (int i = 0; i < Buffer.Length; i++)
		{
			LocalBuffer[i + start] = Buffer[i];
		}

		#region 打开发卡器
		string msg = "";
		ZZJCore.SuanFa.Proc.Log("发卡器端口:" + port.ToString());
		if (!ZKJ_Dev.M1_Init(ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev, port, pwd, ref msg, SQ))//读写模块初始化失败
		{
			Pl_Fuction.PostMessage(ZZJCore.Public_Var.MainFrmHandleIntptr, ZZJCore.Public_Var.MsgID, new IntPtr(0x24), new IntPtr(0));// 0x24发卡器初始化失败
			ZZJCore.SuanFa.Proc.Log("打开读卡器失败!");
			return -1;
		}
		ZZJCore.SuanFa.Proc.Log("打开读卡器成功!");
		#endregion

		#region 验证密码
		Thread.Sleep(100);
		int iRet = ZKJ_Dev.Authentication_passaddr(pwd);
		if (iRet != 0)
		{
			ZKJ_Dev.CloseDev();
			ZZJCore.SuanFa.Proc.Log("密码错误!" + iRet.ToString());
			return -2;
		}
		#endregion

		ZKJ_Dev.M1_WriteCard(cache, LocalBuffer);
		ZKJ_Dev.CloseDev();
		return 0;
	}
	#endregion

	public static bool FKQReadCard()
	{
		string msg = "";
		int port = 0;

		//byte[] pwd = ZZJCore.Public_Var.SPK_Key;//卡校验码
		byte[] pwd = new byte[] { 0xA1, 0xA3, 0xA2, 0xA4, 0xA6, 0xA5 };
		byte[] bCardNo = new byte[10];//卡号缓冲区

		#region 获取端口号
		try
		{
			port = int.Parse(ZZJCore.Public_Var.ZZJ_Config.FKQ_Port.Substring(3));
		}
		catch (Exception e)
		{
			ZZJCore.SuanFa.Proc.Log(e);
			Pl_Fuction.PostMessage(ZZJCore.Public_Var.MainFrmHandleIntptr, ZZJCore.Public_Var.MsgID, new IntPtr(0x24), new IntPtr(0));// 0x24发卡器初始化失败
			ZZJCore.SuanFa.Proc.Log("发卡器端口配置异常!" + ZZJCore.Public_Var.ZZJ_Config.FKQ_Port);
			return false;
		}
		#endregion

		//走卡
		if (MoveCard(pwd, port, 3) != 0) return false;

		//写卡
		//WriteCard(pwd, port, 0, 0, 0, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });

		//读卡
		if (ReadCard(pwd, port, 0, 1, 0, ref bCardNo) != 0) return false;

		ZZJCore.Public_Var.cardInfo.CardNo = Encoding.Default.GetString(bCardNo, 0, 10).Replace("\0", "").Trim();
		ZZJCore.SuanFa.Proc.Log("卡号:" + ZZJCore.Public_Var.cardInfo.CardNo);
		if (ZZJCore.Public_Var.cardInfo.CardNo != "") return (true);
		return (ErrorShow(msg, "发卡器故障!请到人工窗口办理!"));
	}

	public static bool WriteCard()
	{
		//string Msg="";
		byte[] CardNoB = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
		CardNoB[0] = (byte)((Convert.ToByte(ZZJCore.Public_Var.cardInfo.CardNo.Substring(0, 1)) * 0x10) + Convert.ToByte(ZZJCore.Public_Var.cardInfo.CardNo.Substring(1, 1)));
		CardNoB[1] = (byte)((Convert.ToByte(ZZJCore.Public_Var.cardInfo.CardNo.Substring(2, 1)) * 0x10) + Convert.ToByte(ZZJCore.Public_Var.cardInfo.CardNo.Substring(3, 1)));
		CardNoB[2] = (byte)((Convert.ToByte(ZZJCore.Public_Var.cardInfo.CardNo.Substring(4, 1)) * 0x10) + Convert.ToByte(ZZJCore.Public_Var.cardInfo.CardNo.Substring(5, 1)));
		CardNoB[3] = (byte)((Convert.ToByte(ZZJCore.Public_Var.cardInfo.CardNo.Substring(6, 1)) * 0x10) + Convert.ToByte(ZZJCore.Public_Var.cardInfo.CardNo.Substring(7, 1)));
		CardNoB[4] = (byte)((Convert.ToByte(ZZJCore.Public_Var.cardInfo.CardNo.Substring(8, 1)) * 0x10) + Convert.ToByte(ZZJCore.Public_Var.cardInfo.CardNo.Substring(9, 1)));
		if (!ZKJ_Dev.M1_WriteCard(5, CardNoB))
		{
			//ErrSave("[自助办卡]在写卡过程中出现错误,可能是偶然现象,如果反复发生,可能读卡器中的卡为坏卡,请排除再试一次", 1);
			return (ErrorShow("在写卡过程中出现错误,可能是偶然现象,如果反复发生,可能读卡器中的卡为坏卡,请排除再试一次", "发卡器故障!请到人工窗口办理!"));
		}
		//ZKJ_Dev.Out_Card(ref Msg);
		ZZJCore.SuanFa.Proc.Log("写卡成功!卡号:" + ZZJCore.Public_Var.cardInfo.CardNo + "姓名:" + ZZJCore.Public_Var.patientInfo.PatientName);
		return true;
	}

	private static bool ErrorShow(string Msg1, string Msg2)
	{
		if (!string.IsNullOrEmpty(Msg1)) ZZJCore.SuanFa.Proc.Log(Msg1);
		//if (ZZJCore.Public_Var.basesCls.IsVoice) Talker.Speak(Msg2);
		//ZZJCore.ShowMessage ErrM = new ShowMessage(Msg2, true, true, 10);
		//ErrM.ShowDialog();
		return false;
	}

}
