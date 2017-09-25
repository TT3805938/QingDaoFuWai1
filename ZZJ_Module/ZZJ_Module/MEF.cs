using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
namespace ZZJ_Module
{
	public class MEF : ZZJSubModuleInterface.SubModuleLib
	{
		#region 获取自身路径
		public static string GetAssemblyPath()
		{
			string _CodeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
			_CodeBase = _CodeBase.Substring(8, _CodeBase.Length - 8);    // 8是 file:// 的长度
			string[] arrSection = _CodeBase.Split(new char[] { '/' });
			string _FolderPath = "";
			for (int i = 0; i < arrSection.Length - 1; i++)
			{
				_FolderPath += arrSection[i] + "/";
			}
			return _FolderPath;
		}
		#endregion

		public static string URL = "";
		public static string URL2 = "";
		public static string GateUrl = "";


        public static WCFQDYBYL.SYBYL IY = new WCFQDYBYL.SYBYL();

		public string GetModuleVer()
		{
			return "2.0";
		}

		public string[] GetSubModuleVer()
		{
			return new string[] { "1.0", "1.0", "1.0", "1.0", "1.0", "1.0", "1.0", "1.0", "1.0", 
			"1.0" ,"1.0" ,"1.0" ,"1.0",  "1.0", "1.0", "1.0", "1.0", "1.0", "1.0", "1.0","1.0","1.0","1.0"};
		}

		public string[] GetSubModuleCode()
		{
			return new string[] {
                "XJCZ", "YECX", "XFMXCX", "MMXG",
                "BGDDY2","BGDDY", "ETWZBK", "LOGIN", "GMBL",
                "FYPCX", "YPCX", "ZZBK", "YLCZ",
                "JF", "YYQH" ,"QYZZBK","QYETBK","QBLB","YNKHQYK","DRGH","CZMXCX","ZYQD","YYJS","MYDDC","GHNY","YYGH","YYQH","FWJG"};
		}

		public string[] GetSubModuleName()
		{
			return new string[] {
        "现金充值", "余额查询", "消费明细查询", "密码修改",
				"报告单打印2", " 报告单打印", "儿童无证办卡",
				"登录", "购买病历", "非药品查询",
				"药品查询", "自助办卡", "银联充值",
				"自助缴费", "预约取号" ,"区域卡办卡","区域儿童办卡","取病历本","换卡","当日挂号","充值明细查询","住院清单","医院介绍","满意度调查","出院拿药挂号","预约挂号","取预约号","服务价格"
			};
		}

		public bool srunmode = false;
		bool InitialFlag = false;
		public bool InitSubModule(string ModuleCode, object[] argv)
		{
			if (InitialFlag) return true;
			InitialFlag = true;
			//获取模块自身的目录
			ZZJCore.Public_Var.ModulePath = GetAssemblyPath();

			//被作为EXE模式模块调用时,显示正在准备界面
			if (srunmode) ZZJCore.BackForm.ShowForm("正在准备,请稍候...");

			//初始化核心库
			ZZJCore.Initial.CoreInitial();

			//办卡模块初始化
			bool bRet = BK.BKInitial();

			URL = ZZJCore.SuanFa.Proc.ReadPublicINI("WebserviceURL", "");
			URL2 = ZZJCore.SuanFa.Proc.ReadPublicINI("WebserviceURL2", "");
			GateUrl = ZZJCore.SuanFa.Proc.ReadPublicINI("PlatformURL", "");

			QDYBCard.YYDM = ZZJCore.SuanFa.Proc.ReadPublicINI("YBYYDM", "");
			QDYBCard.SFZD = ZZJCore.SuanFa.Proc.ReadPublicINI("YBSFZD", "");
			QDYBCard.SFRY = ZZJCore.SuanFa.Proc.ReadPublicINI("YBSFRY", "");

			//ZZJCore.SuanFa.Proc.ZZJMessageBox("", QDYBCard.SFZD, true);

			if (URL.Length == 0) bRet = false;
			return bRet;
		}

		public bool StartSubModule(string ModuleCode, object[] argv, out string msg)
		{
			msg = "";
			//如果是在模块中,则禁止进入Login模块
			if (ModuleCode == "LOGIN" && ZZJCore.Public_Var.ModuleRun) return false;
			ZZJCore.Public_Var.ModuleRun = true;//模块运行标志置位
			bool ModuleRet = false;
			try
			{
				ModuleRet = MK(ModuleCode, argv);//调用模块
			}
			catch (Exception e)
			{
				ZZJCore.SuanFa.Proc.Log(e);

			}
			ZZJCore.BackForm.CloseForm();
			ZZJCore.Public_Var.ModuleRun = false;//模块结束标志
			return ModuleRet;
		}

		public bool MK(string ModuleCode, object[] argv)
		{
			ZZJ_Module.Public_Var.RCR = null;
			if (argv.Length == 1)
			{
				ZZJ_Module.Public_Var.RCR = argv[0] as ZZJSubModuleInterface.ReadCardResultInfoCls;
			}

			if (ZZJ_Module.Public_Var.RCR != null)
			{
				ZZJCore.Public_Var.cardInfo.CardNo = ZZJ_Module.Public_Var.RCR.KH;
				//MessageBox.Show(ZZJCore.Public_Var.cardInfo.CardNo);
				ZZJCore.Public_Var.cardInfo.CardType = ZZJ_Module.Public_Var.RCR.KPLX.ToString();
			}
			if (ModuleCode == "XJCZ") return XJCZ.XJCZMain();
			if (ModuleCode == "YECX") return YECX.YECXMain();
			if (ModuleCode == "XFMXCX") return XFMX.XFMXMain();
			if (ModuleCode == "CZMXCX") return CZMX.CZMXMain();
			if (ModuleCode == "MMXG") return MMXG.MMXGMain();
			//if (ModuleCode == "BGDDY") return HYD.HYDMain();
			if (ModuleCode == "BGDDY2") return NewHYD.HYDMain();
			if (ModuleCode == "LOGIN") return Login.LoginMain();
			if (ModuleCode == "ETWZBK") return BK.BKMain(1);
			if (ModuleCode == "ZZBK") return BK.BKMain(0);
			if (ModuleCode == "YNKHQYK") return ZZJ_Module.YNCardToQYCard.ChangeCard();
			if (ModuleCode == "QYZZBK") return QYKBK.BKMain(0);
			if (ModuleCode == "QYETBK") return QYKBK.BKMain(1);
			if (ModuleCode == "GMBL" || ModuleCode == "QBLB") return GMBL.GMBLMain();
			if (ModuleCode == "FYPCX") return WPCX.WPCXMain(0);
			if (ModuleCode == "YPCX") return WPCX.WPCXMain(1);
			if (ModuleCode == "YLCZ") return YLCZ.YLCZMain();
			if (ModuleCode == "JF") return JF.JFMain();
			if (ModuleCode == "YYQH") return YYQH.QHMain();
			if (ModuleCode == "ZYQD") return ZYQD.ZYQDMain();
           if (ModuleCode == "DRGH") return DRGH.GHMain();
			if(ModuleCode=="YYJS") return YYJS.YYJSMain(); 
			if(ModuleCode=="MYDDC") return MYDDC.MYDDCMain();
			if(ModuleCode=="GHNY") return GHNY.GHMain();
			//if (ModuleCode == "TESTQYZZBK") return TESTQYKBK.BKMain(0);
			if(ModuleCode=="YYGH") return YYGH.YYGHMain();
			if(ModuleCode=="YYQH") return YYQH.QHMain();
			if(ModuleCode=="FWJG") return FWJGQD.Main();
			return true;
		}
	}//End Class
}