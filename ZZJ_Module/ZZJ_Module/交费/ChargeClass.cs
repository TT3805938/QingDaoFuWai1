using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZZJCore;
using System.Windows.Forms;
using System.Drawing;
using System.Data.OleDb;
using System.Drawing.Printing;
using Mw_MSSQL;
using System.Data.SqlClient;
using System.Data;

namespace ZZJ_Module
{
	
	public static class ChargeClass
	{
		public static int ChargePayMode = -1;

		private static ChargeParameter sCP = new ChargeParameter();

		public class ChargeParameter
		{
			public decimal DC = 0;//金额
			public string Caption = "请选择缴费方式";//标题
			public bool Cash = true;//现金
			public bool BankCard = true;//银行卡
			public bool YBCard = true;//医保
			public bool GangCard=true;//港内卡
			
			public string PayType = "";
		}

		public static Image[] ICON = new Image[] { 
			Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/Card/银联卡支付.png"), 
			Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/Card/社保卡支付.png"),
			Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/Card/现金充值.png"),
			Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/Card/医疗卡支付.png")
		};

		//返回值 0:银联交易成功 1:医保交易成功 2:预交金已经足以扣费 3:港内卡支付准备成功 -1:用户返回 -2:用户关闭 -3:用户选银联交易后业务交易失败 -4:用户选医保卡交易后交易失败 -5:数据异常 
		public static int Charge(ChargeParameter CP)
		{
			ChargePayMode = -1;
			sCP = CP;
			ZZJCore.SelectForm2.SelectFormParameter SFP = new SelectForm2.SelectFormParameter();
			SFP.Caption = CP.Caption;

			byte[] RAX = new byte[5];
			//ZZJCore.Public_Var.patientInfo.PatientID
			bool XJBZ = true;//现金标志  银联标志
			if (ZZJCore.Public_Var.cardInfo.CardType == "0" && ZZJ_Module.Public_Proc.ReadZZJConfig("YNK_XJCZ") == "0") XJBZ = false;
			if (ZZJCore.Public_Var.cardInfo.CardType == "1" && ZZJ_Module.Public_Proc.ReadZZJConfig("QYK_XJCZ") == "0") XJBZ = false;
			if (ZZJCore.Public_Var.cardInfo.CardType == "2" && ZZJ_Module.Public_Proc.ReadZZJConfig("SBK_XJCZ") == "0") XJBZ = false;

			try
			{
				int ReadIniRet = 0;
				List<Image> AL = new List<Image>();
				int AX = 0;
				if (CP.BankCard &&
					ZZJ_Module.Public_Proc.ReadZZJConfig("ZF_YLKJF") == "1" && XJBZ
				)
				{
					AL.Add(ICON[0]);
					RAX[AX] = 0;
					AX++;
				}

				if (ZZJ_Module.Public_Proc.ReadZZJConfig("ZF_SBKJF") == "1" && CP.YBCard)
				{
					AL.Add(ICON[1]);
					RAX[AX] = 1;
					AX++;
				}

				if (CP.Cash &&
					ZZJ_Module.Public_Proc.ReadZZJConfig("ZF_XJ") == "1" &&
					ZZJ_Module.Public_Proc.TestModule("XJCZ") &&
					XJBZ
				)
				{
					AL.Add(ICON[2]);
					RAX[AX] = 2;
					AX++;
				}
				//港内卡
				if (CP.GangCard)
				{
					AL.Add(ICON[3]);
					RAX[AX] = 3;
					AX++;
				}

				SFP.Images = AL.ToArray();
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.Log("Charge 加载图片资源失败!" + e.Message);
			}
			//SFP.ButtonTexts = new string[] {"银行卡", "社保卡" };
			// 搞一个提示文字框 tiantao 应对caption在顶端的问题
			List<ZZJCore.ZZJStruct.FontText> ALL = new List<ZZJCore.ZZJStruct.FontText>();
			ZZJCore.ZZJStruct.FontText AText = new ZZJCore.ZZJStruct.FontText();
			AText.X = 270;//设置位置
			AText.Y = 150;
			AText.Text = "您的余额不足,请选择其它支付方式:";//内容
			ALL.Add(AText);

			SFP.Texts = ALL.ToArray();




			SFP.ContentAlignment = ContentAlignment.MiddleCenter;
			SFP.VDistance = 10;//垂直间距
			SFP.HDistance = 40;//水平间距
			SFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Ret, ZZJCore.ZZJControl.Button_Close };

		SelectPayMode:
			//0银行卡,1社保卡 2现金 3港内卡
			int PayMode = ZZJCore.SelectForm2.ShowForm(SFP);
			if (PayMode == -1) return -1;//用户返回
			if (PayMode == -2) return -2;//用户关闭
			if (PayMode == -3) return -2;//倒计时结束
			if (PayMode < 0) return -2;//其它原因关闭
			#region 银联卡缴费
			if (RAX[PayMode] == 0)
			{
				ZZJCore.SuanFa.Proc.MsgSend(0xB6, 0);//申请使用读卡器
				Application.DoEvents();
				//ZZJ_Module.UnionPay.Mode = 0;//收费模式: 1:充值预交金  0:挂号缴费
				ZZJ_Module.UnionPay.Mode = 1;//收费模式: 1:充值预交金  0:挂号缴费
				ZZJ_Module.UnionPay.DC = CP.DC;//要收的金额
				int iRet = 0;
				ZZJCore.FormSkin.UseRetCard=false;
				iRet = ZZJ_Module.UnionPay.Payment();
				ZZJCore.SuanFa.Proc.MsgSend(0xB7, 0);//归还读卡器
				if (iRet != 0)
				{
					ZZJCore.BackForm.CloseForm();
					return -3;
				}
				ChargePayMode = 0;
				return 0;
			}
			#endregion
			#region 医保卡缴费
			if (RAX[PayMode] == 1)
			{
				ZZJCore.SuanFa.Proc.MsgSend(0xB6, 0);//申请使用读卡器
				ZZJ_Module.YBPay.DC = CP.DC;
				int iRet = 0;
				ZZJCore.FormSkin.UseRetCard = false;
				iRet = ZZJ_Module.YBPay.Payment();
				ZZJCore.SuanFa.Proc.MsgSend(0xB7, 0);//归还读卡器
				if (iRet == -1) goto SelectPayMode;
				if (iRet != 0)
				{
					ZZJCore.BackForm.CloseForm();
					return -2;
				}
				ChargePayMode = 1;
				return 1;
			}
			#endregion
			#region 现金充值
			if (RAX[PayMode] == 2)
			{
				ZZJCore.SuanFa.Proc.Log("进入现金充值");
				int XJCZRet = XJCZ.XJCZModule(true);
				Application.DoEvents();
				if (XJCZRet == -1) goto SelectPayMode;
				if (XJCZRet != 0)
				{
					ZZJCore.BackForm.CloseForm();
					return -2;
				}
				if (XJCZRet == 0)
				{
					if (decimal.Parse(ZZJCore.Public_Var.patientInfo.DepositAmount) < CP.DC) goto SelectPayMode;
					ChargePayMode = 2;
					return 2;
				}
			}
			#endregion
			#region 港卡缴费
			//ZZJCore.PatientInfo PI=ZZJCore.Public_Var.patientInfo;
			if (RAX[PayMode] == 3)
			{
				ZZJCore.SuanFa.Proc.Log("选择了港内卡支付");
				ZZJCore.SuanFa.Proc.MsgSend(0xB6, 0);//申请使用读卡器
				Application.DoEvents();
				//读取港内卡号
				string GangCardID = "";
				//要收的金额
				decimal Je = CP.DC;
				int iRet = 0;
				GangCardPay.DC = Je;
				try
				{
					iRet = GangCardPay.Payment();
					ZZJCore.SuanFa.Proc.Log("港内卡交易返回值 0:成功 其他失败.返回值为:"+iRet.ToString());
				}
				catch (Exception ee)
				{
					ZZJCore.SuanFa.Proc.Log("港内卡缴费失败拉,可能的原因为:" + ee.ToString());
					iRet = -3;

				}
				ZZJCore.SuanFa.Proc.MsgSend(0xB7, 0);//归还读卡器
				if (iRet != 0)
				{
					ZZJCore.BackForm.CloseForm();
					return -3;
				}
				ChargePayMode = 3;
				return 3;
			}
			#endregion
			return -5;
		}

		#region 数据库操作
		private static void Init_DBConfig()//加载数据库
		{
			try
			{
				if (MSSQL_Init.Init_DBConfig() != 0)
				{
					ZZJCore.SuanFa.Proc.Log("加载数据库配置文件失败");//加载配置文件失败
					return;
				}
				ZZJCore.SuanFa.Proc.Log("加载配置文件成功");//加载配置文件成功
				if (MSSQL_Operate.Test_MSSQL_Conn() != 0)
				{//数据库测试连接失败
					ZZJCore.SuanFa.Proc.Log("数据库测试连接失败");
					return;
				}
				ZZJCore.SuanFa.Proc.Log("数据库测试连接成功");//数据库测试连接成功
			}
			catch (Exception e)
			{ ZZJCore.SuanFa.Proc.Log(e); }
		}

		private static SqlParameter AddSP(SqlDbType SDT, string PName, object Value)
		{
			SqlParameter parameter = new SqlParameter();
			parameter.SqlDbType = SDT;
			parameter.ParameterName = PName;
			parameter.Value = Value;
			return (parameter);
		}

		public static void SaveData(int HISPayJG = 0)
		{
			if (ChargePayMode != 0 && ChargePayMode != 1) return;
			string msg = "";
			int iRet = 0;
			#region 网络数据库
			Init_DBConfig();
			try
			{
				MSSQL_Operate mssql_operate = new MSSQL_Operate();
				List<SqlParameter> AL = new List<SqlParameter>();
				if (string.IsNullOrEmpty(ZZJCore.Public_Var.cardInfo.CardType)) ZZJCore.Public_Var.cardInfo.CardType = "0";
				AL.Add(AddSP(SqlDbType.VarChar, "@JZKH", ZZJCore.Public_Var.cardInfo.CardNo));//--就诊卡号
				AL.Add(AddSP(SqlDbType.Int, "@KPLX", ZZJCore.Public_Var.cardInfo.CardType));//卡片类别
				AL.Add(AddSP(SqlDbType.VarChar, "@XM ", ZZJCore.Public_Var.patientInfo.PatientName));//姓名
				AL.Add(AddSP(SqlDbType.VarChar, "@ZZJLSH", ZZJCore.Public_Var.SerialNumber));//自助机流水号
				AL.Add(AddSP(SqlDbType.VarChar, "@BL1", sCP.PayType));//交易类型 挂号支付,缴费支付
				AL.Add(AddSP(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));//自助机编号
				AL.Add(AddSP(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));//HIS操作员
				#region 医保模式
				if (ChargePayMode == 1)
				{//医保模式
					AL.Add(AddSP(SqlDbType.VarChar, "@GRBH", QDYBCard.GRBH));//--科室编码
					AL.Add(AddSP(SqlDbType.VarChar, "@POSLSH", QDYBCard.POSH));//POS流水号
					AL.Add(AddSP(SqlDbType.VarChar, "@KSBM", ""));//--科室编码
					AL.Add(AddSP(SqlDbType.VarChar, "@ZZYS", ""));//主治医师
					AL.Add(AddSP(SqlDbType.VarChar, "@JYMC", ""));//交易名称 (例如：体检费用、购买药品等)
					AL.Add(AddSP(SqlDbType.Decimal, "@ZJE", sCP.DC));//总金额	 本次费用发生总金额
					AL.Add(AddSP(SqlDbType.Decimal, "@JYJE", sCP.DC));//交易金额	 当前订单交易待支付额
					AL.Add(AddSP(SqlDbType.Decimal, "@GRZHZFJE", sCP.DC));//个人账户支付额	 社保卡支付金额
					AL.Add(AddSP(SqlDbType.VarChar, "@SBYLJYZT", ""));//银联交易状态
					AL.Add(AddSP(SqlDbType.VarChar, "@SBYLJYJG", "成功"));//银联交易结果
					AL.Add(AddSP(SqlDbType.Decimal, "@SBYLJYJE", sCP.DC));//交易金额
					AL.Add(AddSP(SqlDbType.VarChar, "@YYZFLSH", QDYBCard.sYYZFLSH + ""));//医院支付流水号
					AL.Add(AddSP(SqlDbType.Decimal, "@ZHYE", QDYBCard.YBYE));//账户余额(QDYBCard.YBYE-Convert.ToDecimal(Reg_Fee) )
					//QDYBCard.JYSJ = @"20160101/020726";
					AL.Add(AddSP(SqlDbType.DateTime, "@ZFSJ", QDYBCard.JYSJ));//订单支付时间
					AL.Add(AddSP(SqlDbType.VarChar, "@JYLSH", QDYBCard.sJYLSH));//交易流水号
					AL.Add(AddSP(SqlDbType.VarChar, "@YLPCH", QDYBCard.PCH));//银联批次号
					AL.Add(AddSP(SqlDbType.VarChar, "@POSCKH", QDYBCard.YLJYCKH));//银联交易参考号
					AL.Add(AddSP(SqlDbType.VarChar, "@SFZ", QDYBCard.SFZH));//身份证号
					AL.Add(AddSP(SqlDbType.VarChar, "@SJ", DateTime.Now));//时间
					AL.Add(AddSP(SqlDbType.Int, "@HISBZ", HISPayJG));//HIS结果
					AL.Add(AddSP(SqlDbType.VarChar, "@HISJGMS", ""));//HIS结果描述
					iRet = mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_SBYL_MX_V1]", AL.ToArray(), out msg);
				}
				#endregion
				#region 银联
				if (ChargePayMode == 0)
				{//银联模式
					AL.Add(AddSP(SqlDbType.VarChar, "@POSLSH", ZZJ_Module.UnionPay.RETDATA.strTrace));//POS流水号(终端流水号)
					AL.Add(AddSP(SqlDbType.VarChar, "@POSZDH", ZZJ_Module.UnionPay.RETDATA.strTId));//POS终端号
					AL.Add(AddSP(SqlDbType.Decimal, "@POSJE", sCP.DC));//POS金额
					AL.Add(AddSP(SqlDbType.Decimal, "@YCQJE", Public_Var.OldAmount));//充值前金额
					AL.Add(AddSP(SqlDbType.DateTime, "@RQSJ", DateTime.Now));//日期时间

					DateTime DT = DateTime.Now;
					try
					{
						int DT_MM = int.Parse(ZZJ_Module.UnionPay.RETDATA.strTransDate.Substring(0, 2));
						int DT_DD = int.Parse(ZZJ_Module.UnionPay.RETDATA.strTransDate.Substring(2, 2));

						int DT_hh = int.Parse(ZZJ_Module.UnionPay.RETDATA.strTransTime.Substring(0, 2));
						int DT_mm = int.Parse(ZZJ_Module.UnionPay.RETDATA.strTransTime.Substring(2, 2));
						int DT_ss = int.Parse(ZZJ_Module.UnionPay.RETDATA.strTransTime.Substring(4, 2));

						DT = new DateTime(DateTime.Now.Year, DT_MM, DT_DD, DT_hh, DT_mm, DT_ss);
					}
					catch (Exception e)
					{
						ZZJCore.SuanFa.Proc.Log(e);
					}

					AL.Add(AddSP(SqlDbType.VarChar, "@JYSJ", DT.ToString("yyyy-MM-dd HH:mm:ss")));//交易时间
					AL.Add(AddSP(SqlDbType.VarChar, "@YHKH", ZZJ_Module.UnionPay.RETDATA.strCardNo));//银行卡号
					AL.Add(AddSP(SqlDbType.VarChar, "@FKH", ""));//发卡行
					AL.Add(AddSP(SqlDbType.VarChar, "@SQM", ZZJ_Module.UnionPay.RETDATA.strAuth));//授权码
					AL.Add(AddSP(SqlDbType.VarChar, "@CKH", ZZJ_Module.UnionPay.RETDATA.strRef));//银联交易参考号
					AL.Add(AddSP(SqlDbType.VarChar, "@YHJYJG", "0"));//银行操作标识 0成功 其他失败
					AL.Add(AddSP(SqlDbType.VarChar, "@YHJYJGMS", ZZJ_Module.UnionPay.RETDATA.strRespInfo));//银行交易描述
					AL.Add(AddSP(SqlDbType.Int, "@YCJG", HISPayJG));//预存结果
					AL.Add(AddSP(SqlDbType.VarChar, "@YCJGMS", ""));//HIS充值描述
					AL.Add(AddSP(SqlDbType.Decimal, "@YCHJE", Public_Var.NewAmount));//预存后金额
					AL.Add(AddSP(SqlDbType.VarChar, "@BRLX", "自费"));//病人类型
					AL.Add(AddSP(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.patientInfo.PatientID));//登记号
					AL.Add(AddSP(SqlDbType.VarChar, "@SendYHBW", ""));//银行发送报文
					AL.Add(AddSP(SqlDbType.VarChar, "@ReceiveYHBW", ""));//银行返回报文
					iRet = mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_MZ_YHKYC_MX_ForZZJ_V4]", AL.ToArray(), out msg);
				}
				#endregion
				if (iRet <= 0)
				{
					ZZJCore.SuanFa.Proc.Log("银联(或医保)挂号取号缴费记录写入服务器失败PayMode=" + ChargePayMode.ToString() + msg);
					return;
				}
				ChargePayMode = -1;
			}
			catch (Exception Ex)
			{
				ZZJCore.SuanFa.Proc.Log(Ex);
				ZZJCore.SuanFa.Proc.Log("银联(或医保)挂号取号缴费记录写入服务器失败PayMode=" + ChargePayMode.ToString() + Ex.Message);
			}
			#endregion
			ZZJCore.SuanFa.Proc.Log("银联(或医保)挂号取号缴费记录写入服务器成功");
		}
		#endregion
	}//End Class


}
