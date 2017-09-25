using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ZZJCore;
using System.Drawing;
using System.Xml;
using ZZJCore.ZZJStruct;

namespace ZZJ_Module
{
	public class DRGH
	{
		
		static string selectDeptName = "";
		static string selectDeptCode = "";
		static string selectDoctName = "";
		static string selectDoctCode = "";
		static string selectRegType = "";

		static string selectBLType = "";//病历

		static string selectRegTime = "";
		static string selectRegAmPm = "";
		static string gh_sjh = "";
		static string gh_ysje = "";
		static string gh_zhzf = "";
		static string gh_ghlb = "";
		static string gh_jlxh = "";
		static string zhye = "";//挂号后帐内余额
		static string YB_Flag = "";
		static string xzks = "9";//限制显示的科室

		static string isSuccess = "";//是否挂号成功(可能银联扣费成功,挂号失败,因此加个字段判别,方便去窗口退费)

		private static int iRet = 0;
		static string errorTxt="";

		private static DeptRoot RootDept = new DeptRoot();
		//static string webserviceUrl = "http://10.17.66.23/zzxt/KWSService.asmx";
		public static bool GHMain()
		{
			//GetNoOK();
			//return true;
			string Msg = "";
			string XmlData = "";
			int TableCount = 0;
			XmlDocument XmlDoc = new XmlDocument();
			string ResultCode = "";
			string MSG = "";
			#region 初始化
			YB_Flag="0";
			ZZJ_Module.ChargeClass.ChargePayMode = -1;
			ZZJCore.FormSkin.UseRetCard = true;
			ZZJCore.Public_Var.ModuleName = "当日挂号";
			ZZJCore.BackForm.ShowForm("正在查询,请稍候...");
			Application.DoEvents();
			ZZJCore.Initial.Read();//读取配置文件
			//获取用户信息
			if (XMLCore.GetUserInfo(out MSG) != 0)
			{
				if (!string.IsNullOrEmpty(MSG)) ZZJCore.BackForm.ShowForm(MSG, true);
				ZZJCore.BackForm.CloseForm();
				return true;
			}
		SELECTLB:
			#endregion
			int SelButton = 0;
			int SelRegType = ShowSelectWindow("请选择挂号类型", new string[] { "普通", "专家", "急诊","便民" }, false);
			int SelRegType_by = SelRegType;
			if (SelRegType==3)
			{
			 SelRegType= 0;
			}
			if (SelRegType < 0)
			{
				ZZJCore.BackForm.CloseForm();
				return true;
			}
			selectRegType = (new string[] { "普通", "专家", "急诊", "便民" })[SelRegType_by];
			xzks=selectRegType;//将选择的是普通、专家、还是急诊的类型赋值给xzks,下面做判断

			RootDept.Names.Clear();
			RootDept.Dept.Clear();
			XmlMaker Table = XmlMaker.NewTabel();
			Table.Add("lb", SelRegType.ToString());// 查询类型 0普通 1专家 2 急诊
			Table.Add("cxrq", "");//查询日期（为空表示当天）
			Table.Add("ksdm", "");//科室代码(普通挂号时才有效，传入的是二级科室代码
			try
			{
				string DataIn = XmlMaker.NewXML("201", Table).ToString();
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.201");
				XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(XmlData, "Service.201");
				XmlDoc = new XmlDocument();
				XmlDoc.LoadXml(XmlData);
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "科室查询接口通讯错误!\r\n" + ex.Message, true);
				ZZJCore.BackForm.CloseForm();
				//ZZJCore.BackForm.ShowForm("科室查询接口通讯错误!\r\n"+ex.Message, true);
				return true;
			}
			try
			{
				ResultCode = XmlDoc.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();
				Msg = XmlDoc.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
				if (ResultCode != "0")
				{
					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "没有查到科室信息!\r\n" + Msg, true);
					ZZJCore.BackForm.CloseForm();
					//ZZJCore.BackForm.ShowForm("没有查到科室信息!\r\n" + Msg, true);
					return true;
				}
				TableCount = int.Parse(XmlDoc.SelectSingleNode("zzxt/tablecount").InnerText.Trim());
				//循环找出所有信息
				for (int i = 1; i <= TableCount; i++)
				{
			   	string Yszc="";//医生职称
					string Zc="";//专长
					string Num="";//限号数
					string yghs="";//已挂号数
					string AmNum="";//上午限号数
					string PmNum="";//下午限号数
					string scr = string.Format("zzxt/table{0}", i);
					XmlNode Node = XmlDoc.SelectSingleNode(scr);
					string sDeptName = Node.SelectSingleNode("ksmc").InnerText.Trim();

					if (selectRegType == "急诊" && !sDeptName.Contains("急诊"))//xzks == "2" && 
					{//急诊（只显示急诊下的急诊科和急诊外科）
						continue;
					}
					if (selectRegType == "普通" && sDeptName.Contains("急诊"))//xzks == "2" && 
					{//普通（只显示普通下除急诊科和急诊外科的科室）
						continue;
					}
					if (selectRegType == "普通" && sDeptName.Contains("便民"))//xzks == "2" && 
					{//普通（只显示普通下除急诊科和急诊外科的科室）
						continue;
					}
					if (selectRegType == "专家" && sDeptName.Contains("急诊"))//xzks == "2" && 
					{//专家（只显示急诊科和急诊外科的科室）
						continue;
					}
					if (selectRegType == "便民" && !sDeptName.Contains("便民门诊"))//xzks == "2" && 
					{//专家（只显示急诊科和急诊外科的科室）
						continue;
					}
					if (sDeptName.Contains("出院带药门诊"))//出院带药门诊，在当日挂号中去除
					{
						continue;
					}
				  if(SelRegType==1){//为专家多部分字段
						 Yszc = Node.SelectSingleNode("zc_mc").InnerText.Trim();
						 Zc = Node.SelectSingleNode("zcsm").InnerText.Trim();
						 Num =Node.SelectSingleNode("syhs").InnerText.Trim();//当前号源数
					}
					string sDeptCode = Node.SelectSingleNode("ksdm").InnerText.Trim();
					string sDoctName = Node.SelectSingleNode("ysmc").InnerText.Trim();
					string sDoctCode = Node.SelectSingleNode("ysdm").InnerText.Trim();
					string sFree = Node.SelectSingleNode("zlf").InnerText.Trim();
					string sTime = Node.SelectSingleNode("sjdjl").InnerText.Trim();
					string AMPM = Node.SelectSingleNode("sjdsm").InnerText.Trim();
					DeptItem oDept = FindDept(RootDept, sDeptCode);
					if (oDept == null)
					{
						oDept = new DeptItem();
						oDept.DeptCode = sDeptCode;
						oDept.DeptName = sDeptName;
						oDept.Free = sFree;
						oDept.Time = sTime;
						oDept.AmPm = AMPM;
						RootDept.Dept.Add(oDept);
						RootDept.Names.Add(sDeptName);
					}
					if (!string.IsNullOrEmpty(sDoctCode))
					{ 
						DoctItem Doctor = new DoctItem();
						Doctor.Code = sDoctCode;
						Doctor.Name = sDoctName;
						Doctor.Free = sFree;
						Doctor.Time = sTime;
						Doctor.AmPm = AMPM;
						Doctor.Yszc = Yszc;
						Doctor.Zc = Zc;
						Doctor.Num=Num;
						oDept.Doctor.Add(Doctor);
						oDept.Names.Add(sDoctName + "[" + Yszc + "]" + "\n" + "专长" + ":" + Zc + "\n[" + Num + "]");//
					}
				}
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "科室查询接口返回信息错误!\r\n" + ex.Message, true);
				ZZJCore.BackForm.CloseForm();
				//	ZZJCore.BackForm.ShowForm("科室查询接口返回信息错误!\r\n" + ex.Message, true);
				return true;
			}
		SELECTDEPT:
			selectDeptName = "";
			selectDeptCode = "";
			selectDoctName = "";
			selectDoctCode = "";
			selectRegTime = "";
			selectRegAmPm = "";
			SelButton = ShowSelectWindow("请选择科室", RootDept.Names.ToArray());
			if (SelButton < 0)
			{
				if (SelButton == -1) goto SELECTLB;
				//返回主界面
				ZZJCore.BackForm.CloseForm();
				return true;
			}
			//选择的科室
			DeptItem SelDept = RootDept.Dept[SelButton];
			selectDeptName = SelDept.DeptName;
			selectDeptCode = SelDept.DeptCode;
			selectRegTime = SelDept.Time;
			selectRegAmPm = SelDept.AmPm;
		//如果选择专家号或急诊
		SELECTDOCTOR:
			if (SelRegType == 1)
			{
				//选择医生
				SelButton = ShowSelectWindow("请选择医生", SelDept.Names.ToArray());
				if (SelButton < 0)
				{
					if (SelButton == -1) goto SELECTDEPT;
					//返回主界面
					ZZJCore.BackForm.CloseForm();
					return true;
				}
				DoctItem SelDoctor = SelDept.Doctor[SelButton];
				selectDoctName = SelDoctor.Name;
				selectDoctCode = SelDoctor.Code;
				selectRegTime = SelDoctor.Time;
				selectRegAmPm = SelDoctor.AmPm;
				int num = -1;
				num = Convert.ToInt32(SelDoctor.Num);
				if (num < 1)//没号了不让挂
				{
					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "号源已用完,请选择其他医生", true);
					//ZZJCore.BackForm.ShowForm("号源已用完", "号源已用完,请选择其他医生", true);
					goto SELECTLB;
				}
			}

			

			//挂号预算
			Table = XmlMaker.NewTabel(ZZJCore.Public_Var.patientInfo.PatientID);
			//0挂号预算 1正式挂号
			Table.Add("flag", "0");
			//挂号类别：0普通  1急诊  2专家
			// Table.Add("ghlb", SelRegType.ToString());
			if (SelRegType.ToString() == "0")
			{
				Table.Add("ghlb", "0");
			}
			else
			{
				if (SelRegType.ToString() == "1")
				{
					Table.Add("ghlb", "2");
				}
				else
				{
					Table.Add("ghlb", "1");
				}
			}


			//科室代码
			Table.Add("ksdm", selectDeptCode);
			//医生代码  普通时 医生代码为空
			Table.Add("ysdm", selectDoctCode);
			//下列为空
			Table.Add("ksmc", "");//科室名称(不传)
			Table.Add("pbmxxh", "");//排班明细序号(不传)
			Table.Add("ghkedm", "");//挂号科室代码(不传)
			Table.Add("ybksdm", "");//医保科室代码 
			Table.Add("ybksmc", "");//医保科室名称(不传)
			//RecvData.Text = InvokeWeb(NewXML("202", Table));
			try
			{

				string DataIn = XmlMaker.NewXML("202", Table).ToString();
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.202");
				XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(XmlData, "Service.202");
				XmlDoc = new XmlDocument();
				XmlDoc.LoadXml(XmlData);
				ResultCode = XmlDoc.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();
				Msg = XmlDoc.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
				if (ResultCode != "0")
				{
					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "预算接口调用失败!\r\n" + Msg, true);
					ZZJCore.BackForm.CloseForm();
					//	ZZJCore.BackForm.ShowForm("预算接口调用失败!\r\n" + Msg, true);
					return true;
				}
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "挂号接口通讯错误!\r\n" + ex.Message, true);
				ZZJCore.BackForm.CloseForm();
				//	ZZJCore.BackForm.ShowForm("挂号接口通讯错误!\r\n" + ex.Message, true);
				return true;
			}
			//预算成功,开始走挂号接口

			Console.WriteLine("预算成功:" + XmlData);
			try
			{
				gh_sjh = XmlDoc.SelectSingleNode("zzxt/table/sjh").InnerText.Trim();
				gh_ysje = XmlDoc.SelectSingleNode("zzxt/table/ysje").InnerText.Trim();
				gh_zhzf = XmlDoc.SelectSingleNode("zzxt/table/zhzf").InnerText.Trim();
				gh_ghlb = XmlDoc.SelectSingleNode("zzxt/table/ghlb").InnerText.Trim();
				gh_jlxh = XmlDoc.SelectSingleNode("zzxt/table/jlxh").InnerText.Trim();

			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "预算接口返回数据错误!\r\n" + ex.Message, true);
				ZZJCore.BackForm.CloseForm();
				//ZZJCore.BackForm.ShowForm("预算接口返回数据错误!\r\n" + ex.Message, true);
				return true;
			}
			//确认挂号信息
			try
			{
				//是否挂号
				ZZJCore.SuanFa.Proc.Log("->确定挂号信息!");
				ZZJCore.YesNoForm.YesNoFormParameter YNFP = new ZZJCore.YesNoForm.YesNoFormParameter();
				//YNFP.Caption = "请再次确认您的挂号信息！";
				
				YNFP.Lab1 = new string[] { "科室名称", "医生姓名", "就诊时间", "诊查费" };
				YNFP.Lab2 = new string[] { selectDeptName, selectDoctName, selectRegTime, Convert.ToDecimal(gh_ysje).ToString("C") };
				////#region 添加按钮
				List<ZZJCore.ZZJStruct.ZZJButton> AL = new List<ZZJCore.ZZJStruct.ZZJButton>();
				AL.Add(ZZJCore.ZZJControl.Button_Ret);
				AL.Add(ZZJCore.ZZJControl.Button_Close);
				AL.Add(ZZJCore.ZZJControl.Button_OK);
				YNFP.Buttons = AL.ToArray();
				{
					List<ZZJCore.ZZJStruct.FontText> ALL = new List<ZZJCore.ZZJStruct.FontText>();
					ZZJCore.ZZJStruct.FontText AText = new ZZJCore.ZZJStruct.FontText();
					AText.X = 60;//设置位置
					AText.Y = 600;
					AText.Text = "温馨提示:请您根据就诊时间就诊!";//内容
					ALL.Add(AText);

					ZZJCore.ZZJStruct.FontText AText1 = new ZZJCore.ZZJStruct.FontText();
					AText1.X = 350;//设置位置
					AText1.Y = 200;
					AText1.Text = "请再次确认您的挂号信息!";//内容
					ALL.Add(AText1);
					YNFP.Texts = ALL.ToArray();
				}
				int SF = ZZJCore.YesNoForm.ShowForm(YNFP);
				Application.DoEvents();
				if (SF < 0)
				{
					if (SF == -1)
					{
						if (SelRegType == 1)
							goto SELECTDOCTOR;
						else
							goto SELECTDEPT;
					}
					ZZJCore.BackForm.CloseForm();
					ZZJCore.SuanFa.Proc.Log("->返回首页");
					return true;
				}
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "确定时出错!\r\n" + ex.Message, true);
				ZZJCore.BackForm.CloseForm();
				//	ZZJCore.BackForm.ShowForm("确定时出错!\r\n" + ex.Message, true);
				return true;
			}



			#region 如果是港内卡或者附属卡就弹出输入密码的界面

			if (ZZJ_Module.Public_Var.zhjb.ToString() == "2" || ZZJ_Module.Public_Var.zhjb.ToString() == "3")
			{
				ZZJCore.InputForm.InputPasswordParameter OIPP = new ZZJCore.InputForm.InputPasswordParameter();
				OIPP.Caption = "请输入就诊卡密码";
				OIPP.XButton = false;
				OIPP.RegMode = false;
				OIPP.Buttons = new ZZJButton[] { ZZJControl.Button_Close };
				if (ZZJCore.InputForm.InputPassword(OIPP) != 0)
				{
					ZZJCore.BackForm.CloseForm();
					return true;
				}
				Application.DoEvents();
				//	ZZJCore.Public_Var.patientInfo.Password = ZZJCore.SuanFa.Proc.MD5(ZZJCore.Public_Var.patientInfo.Password);
			}
			else
			{
				ZZJCore.Public_Var.patientInfo.Password = "";
			}
			#endregion


			#region  挂号缴费（余额足用余额，不足选择其他支付方式）
			StartGHJF:

			decimal KYE = Convert.ToDecimal(ZZJCore.Public_Var.patientInfo.DepositAmount);
			if (Convert.ToDecimal(gh_ysje) <= KYE)//
			{
				//重置挂号 gh_flag
				YB_Flag="0";
				if (Pay("预交金") != 0)//缴费
				{
					ZZJCore.SuanFa.Proc.Log("挂号扣费失败");
					ZZJCore.BackForm.ShowForm("挂号扣费失败,请稍后再试!"+errorTxt.ToString(), true);
					return true;
				}
				//ZZJCore.SuanFa.Proc.MsgSend(0xB5, 0, true);//向主控发出退卡指令
				//ZZJCore.BackForm.ShowForm("挂号成功!", true);
				//Application.DoEvents();
				//return true;

				ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "挂号成功!", true);
				ZZJCore.BackForm.CloseForm();
				if (int.Parse((ZZJCore.Public_Var.ZZJ_Config.ExtUserID).Substring(3,2))%2!=0)
				{
					if (int.Parse(gh_ysje) >= 7)
					{
						GMBL.GMBLMain();
					}
				}
				return true;
			}
			else
			{
				//ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "余额不足，请充值，港内卡不支持充值!", true);
				//ZZJCore.BackForm.CloseForm();
				//return true;
				YB_Flag = "0";
				if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "1")
				{
					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "余额不足,请至人工窗口办理!", true);
					ZZJCore.BackForm.CloseForm();
					return true;
				}
				#region   当开通余额不足时，允许其他方式支付时启用
				ZZJCore.SuanFa.Proc.Log("余额不足,显示其它方式供选择");
			//	decimal gh = Convert.ToDecimal(0.02);
				ZZJ_Module.ChargeClass.ChargeParameter CP = new ZZJ_Module.ChargeClass.ChargeParameter();
				CP.Caption = "";
				CP.PayType = "挂号收费";
				//CP.DC = Convert.ToDecimal(gh_ysje);
				CP.DC = Convert.ToDecimal(gh_ysje);
				int SPMiRet = ZZJ_Module.ChargeClass.Charge(CP);
				Application.DoEvents();
				//if (SPMiRet == -1) return -1;
				//if (SPMiRet == -2) return -2;
				//if (SPMiRet < -2) return -2;
				if (SPMiRet < 0)
				{
					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "交易取消或交易失败,请稍后再试!", true);
					ZZJCore.BackForm.CloseForm();
					return true;
				}

				int PayiRet = -1;
				if (SPMiRet == 0)
				{
						#region   如果银联扣费成功，先进行充值，在进行扣费
							iRet = YLCZ.Pub_CZ(CP.DC,1);//传1是说明 充值是从其他地方调用银联充值
						#endregion
						if(iRet==0)
						{
							YB_Flag="0";//清空掉这个医保标志,前面不知道谁写的不清空.会造成下个人用银联还是当成医保,最好的办法是做传一个标志参数
							PayiRet = Pay("银联POS", "", ZZJ_Module.UnionPay.UnionPayCardNo, ZZJ_Module.UnionPay.RETDATA.strRef, "", ZZJ_Module.UnionPay.RETDATA.strBatch, ZZJ_Module.UnionPay.RETDATA.strTrace, ZZJ_Module.UnionPay.RETDATA.strRef, ZZJ_Module.UnionPay.RETDATA.strTId);
							//ZZJ_Module.ChargeClass.SaveData((PayiRet == 0) ? 0 : -1);
						}
						else
						{
							ZZJCore.SuanFa.Proc.Log("银联扣费成功但充值失败");
							ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "扣费成功但充值失败,请到人工窗口处理!", true);
							ZZJCore.BackForm.CloseForm();
							return true;
						}
				}
				if (SPMiRet == 1)
				{
					YB_Flag = "1";
					ZZJCore.SuanFa.Proc.Log("医保支付成功!");
					PayiRet = Pay("社保银联", "", QDYBCard.GRBH, QDYBCard.sJYLSH, QDYBCard.sYYZFLSH, QDYBCard.PCH, QDYBCard.POSH, QDYBCard.YLJYCKH, QDYBCard.POSH);
					//if(PayiRet!=0)//说明his那边失败了
					//{
					//  ZZJCore.SuanFa.Proc.Log("提交his接口交易结果:"+(PayiRet==0?"提交成功":"提交失败")+"   开始冲正");
					//  //冲正这笔交易
					//  if( QDYBCard.GRBHCXZF(QDYBCard.GRBH,QDYBCard.sYYZFLSH,QDYBCard.sJYLSH,ZJE)==0)//说明失败了
					//  {
					//    ZZJCore.SuanFa.Proc.Log("已冲正");
					//    ZZJCore.BackForm.ShowForm("医保金已冲正",true);
					//  }
					//  else
					//  {
					//    ZZJCore.SuanFa.Proc.Log("冲正失败");
					//    ZZJCore.BackForm.ShowForm("医保金冲正失败,请持医保缴费凭证到窗口办理退费",true);
					//  }
					//}

					ZZJ_Module.ChargeClass.SaveData((PayiRet == 0) ? 0 : -1);
				}
				if (SPMiRet == 2) goto StartGHJF;
				//说明是港内卡支付
				if(SPMiRet==3)
				{
					YB_Flag = "0";//清空掉这个医保标志,前面不知道谁写的不清空.会造成下个人用银联还是当成医保,最好的办法是做传一个标志参数
					PayiRet = Pay("港内卡G", "","" ,"", "", "", "", "", "");

				}
				if (PayiRet != 0)
				{
				//	isSuccess = "失败";
				//	PrintPT();//银联缴费成功,挂号失败,打印凭条,方便窗口退费
					//	ZZJCore.BackForm.ShowForm("扣费成功但挂号失败,请到人工窗口处理!", true);
					
					if(PayiRet==-99)//说明是医保方式
					{
						ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "医保扣费成功,挂号失败!请去人工窗口处理", true);
						ZZJCore.SuanFa.Proc.Log("医保扣费成功，挂号失败!结束");
						ZZJCore.BackForm.CloseForm();
						return true;
					}
					
					ZZJCore.SuanFa.Proc.Log("挂号失败,可能是账户和密码不正确.");
					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "挂号失败!"+errorTxt, true);
					ZZJCore.BackForm.CloseForm();
					return true;
				}
				ZZJCore.SuanFa.Proc.Log("挂号成功");
				ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "挂号成功!", true);
				ZZJCore.BackForm.CloseForm();
				if (int.Parse((ZZJCore.Public_Var.ZZJ_Config.ExtUserID).Substring(3, 2)) % 2 != 0)
				{
					if (int.Parse(gh_ysje) >= 7)
					{
						GMBL.GMBLMain();
					}
				}
				return true;
				#endregion

			}
			#endregion
		}


		

		#region 挂号缴费XML
		public static int Pay(string PayMode = "", string Bankname = "", string TransCode = "",
										string TransSerialNo = "", string PayTransNo = "", string BatchNo = "", string PostransNo = "", string ReferenceNo = "", string TerminalNo = "")
		{
			#region 整理报文
			string PayModeTemp = PayMode;
			XmlMaker Table = XmlMaker.NewTabel();
			string XmlData = "";
			string ResultCode = "";
			string Msg = "";
			errorTxt="";
			XmlDocument XmlDoc = new XmlDocument();

			Table = XmlMaker.NewTabel(ZZJCore.Public_Var.patientInfo.PatientID);
			Table.Add("flag", "1");
			//收据号  挂号预算时返回
			Table.Add("sjh", gh_sjh);
			//预算金额  挂号预算时返回   如果银行卡支付成功了，挂号费就为0
			//if (PayMode.ToString() == "银联POS")
			//{
			//  Table.Add("ysje", "0");
			//}
			//else
			//{
				Table.Add("ysje", gh_ysje);
		//	}
			//账户支付  挂号预算时返回
			Table.Add("zhzf", gh_zhzf);
			//挂号类别  挂号预算时返回
			Table.Add("ghlb", gh_ghlb);


			if (PayMode == "预交金")
			{
				if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "1")//港内卡为自己支付
				{
					if (ZZJ_Module.Public_Var.zhjb.ToString() == "2")//港内卡
					{
						Table.Add("zffs", "G");
					}
					else//附属卡
					{
						Table.Add("zffs", "F");
					}
				}
				else //就诊卡
				{
					Table.Add("zffs", "J");
				}
			}
			else if (PayMode == "银联POS")
			{
				Table.Add("zffs", "J");
			}
			else if (PayMode == "社保银联")
			{
				Table.Add("zffs", "10");
			}
			else if (PayMode == "港内卡G")
			{
				if (GangCardPay.gangCard.payLevel == "2")//支付级别为2 
				{
					Table.Add("zffs", "G");
				}
				else
				{
					Table.Add("zffs", "F");//附属卡
				}
			}
			else
			{
				return -3;//返回 来路不明
			}

			//港内卡支付专用
			Table.Add("cardno", PayMode == "港内卡G" ? GangCardPay.GangCardNo : "");//港内卡卡号
			Table.Add("passwd", PayMode == "港内卡G" ? GangCardPay.Psd : ZZJCore.Public_Var.patientInfo.Password);//港内卡密码
			//GH_ZDGHJLK记录序号  挂号预算时返回
			Table.Add("jlxh", gh_jlxh);
			//密码(可选的,如果卡有密码必填)
			//Table.Add("passwd", ZZJCore.Public_Var.patientInfo.Password);
			//如果缴费方式是医保,才会传医保编号等.	不然会造成 其他方式也传了上一笔的医保支付信息	
			if(PayMode=="社保银联")
			{
				Table.Add("yyzflsh", QDYBCard.sYYZFLSH );
				Table.Add("grbh", QDYBCard.GRBH );
				Table.Add("jylsh", QDYBCard.sJYLSH );
				Table.Add("pch",  QDYBCard.PCH );
				Table.Add("posh", QDYBCard.POSH );
				Table.Add("yljyckh",  QDYBCard.YLJYCKH);
				Table.Add("je", gh_ysje);
			}
			else
			{
				Table.Add("yyzflsh", "");
				Table.Add("grbh", "");
				Table.Add("jylsh", "");
				Table.Add("pch","");
				Table.Add("posh",  "");
				Table.Add("yljyckh", "");
				Table.Add("je", gh_ysje);
			}











			//支付方式  J就诊卡 G港内卡 F附属卡 7Pos 10社保

			//1是港内卡(zhjb=2)以及附属卡(zhjb=3)   0是院内卡(zhjb=1)
			//if (YB_Flag == "1")// 医保支付
			//{
			//  Table.Add("zffs", "10");
			//}
			//else
			//{
			//  if (PayMode == "港内卡G")//支付方式为他人港内卡 支付
			//  {
			//    if(GangCardPay.gangCard.payLevel=="2")
			//    {
			//    Table.Add("zffs", "G");
			//    }
			//    else if(GangCardPay.gangCard.payLevel=="3")
			//    {
			//      Table.Add("zffs","F");
			//    }
			//  }
			//  else
			//  {
			//      if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "1")//港内卡为自己支付
			//      {
			//        if (ZZJ_Module.Public_Var.zhjb.ToString() == "2")
			//        {
			//          Table.Add("zffs", "G");
			//        }
			//        else
			//        {
			//          Table.Add("zffs", "F");
			//        }
			//      }
			//      if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "0" && ZZJ_Module.Public_Var.zhjb.ToString() == "1")//这是就诊卡支付
			//      {
			//        Table.Add("zffs", "J");
			//      }
			//      if(ZZJCore.Public_Var.cardInfo.CardType.ToString()=="2"&& ZZJ_Module.Public_Var.zhjb.ToString()=="1")//这是医保卡做就诊卡用就诊账户的钱支付
			//      {
			//        Table.Add("zffs","J");
			//      }
			//  }
			//}
		
			////港内卡支付专用
			//Table.Add("cardno",PayMode=="港内卡G"?GangCardPay.gangCard.GangCardNo:"");//港内卡卡号
			//Table.Add("passwd", PayMode == "港内卡G" ? GangCardPay.gangCard.psd : ZZJCore.Public_Var.patientInfo.Password);//港内卡密码
			////GH_ZDGHJLK记录序号  挂号预算时返回
			//Table.Add("jlxh", gh_jlxh);
			////密码(可选的,如果卡有密码必填)
			////Table.Add("passwd", ZZJCore.Public_Var.patientInfo.Password);
			////如果缴费方式是医保,才会传医保编号等.	不然会造成 其他方式也传了上一笔的医保支付信息	
			//Table.Add("yyzflsh",YB_Flag=="1"? QDYBCard.sYYZFLSH:"");
			//Table.Add("grbh",YB_Flag=="1"? QDYBCard.GRBH:"");
			//Table.Add("jylsh",YB_Flag=="1"? QDYBCard.sJYLSH:"");
			//Table.Add("pch",YB_Flag=="1"? QDYBCard.PCH:"");
			//Table.Add("posh",YB_Flag=="1"? QDYBCard.POSH:"");
			//Table.Add("yljyckh",YB_Flag=="1"? QDYBCard.YLJYCKH:"");
			//Table.Add("je", gh_ysje);


			#endregion

			try
			{
				errorTxt="";
				string DataIn = XmlMaker.NewXML("202", Table).ToString();
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.挂号");
				XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(XmlData, "Service.挂号");
				//XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(webserviceUrl, "CallService", new object[] { XmlMaker.NewXML("202", Table).ToString() }) as string;
				XmlDoc = new XmlDocument();
				XmlDoc.LoadXml(XmlData);
				ResultCode = XmlDoc.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();
				Msg = XmlDoc.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
				if (ResultCode == "0")
				{
					if (PayMode == "港内卡G" || ZZJ_Module.Public_Var.zhjb.ToString() == "2")
					{
						if(PayMode=="港内卡G")
						{
							GangCardPay.PrintGKPT();
						}
						else if(PayMode=="预交金")
						{
							PrintGKPT(ZZJ_Module.Public_Var.RCR.KH,Convert.ToDecimal(gh_ysje));
						}
					}
					else if (PayMode == "预交金" && ZZJ_Module.Public_Var.zhjb.ToString() == "1")
					{
							PrintGKPT(Convert.ToDecimal(gh_ysje));
					}
					zhye = XmlDoc.SelectSingleNode("zzxt/table/zhje").InnerText.Trim();
				}

				if (ResultCode != "0")
				{
					
					errorTxt=Msg;
					if (PayMode == "社保银联")
					{
						ZZJCore.SuanFa.Proc.Log("解析挂号扣费报文时出错." + "ResultCode:" + ResultCode + "Msg:" + Msg.ToString());
						return -99;
					}
					//ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "挂号失败!\r\n" + Msg, true);
					//ZZJCore.BackForm.CloseForm();
					//	ZZJCore.BackForm.ShowForm("挂号失败!\r\n" + Msg, true);
					ZZJCore.SuanFa.Proc.Log("解析挂号扣费报文时出错." + "ResultCode:" + ResultCode+"Msg:"+Msg.ToString());
					return -3;
				}

			}
			catch (Exception e)
			{//解析报文异常,网络异常等不可退费,因为可能缴费成功了
				//ZZJCore.SuanFa.Proc.Log("挂号扣费失败,可能是因为网络错误." + e.Message);
				//ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "结算接口通讯错误!\r\n" + e.Message, true);
				//ZZJCore.BackForm.CloseForm();
				ZZJCore.SuanFa.Proc.Log(e);
				ZZJCore.SuanFa.Proc.Log("挂号扣费失败,可能是因为网络错误." + e.Message);
				return -3;
			}
            #region 挂号记录写入数据库
            try
            {
                SQLSave.SaveDRGH(true, true, gh_ysje, selectDoctName,selectDeptName);
            }
            catch (Exception e)
            {
                ZZJCore.SuanFa.Proc.Log(e);
                ZZJCore.SuanFa.Proc.Log("挂号完成,写入数据库失败!" + e.Message);
            }
            #endregion
			//if (PayMode == "预交金") ZZJ_Module.Public_Var.NewAmount = ZZJ_Module.Public_Var.NewAmount - Convert.ToDecimal(gh_ysje);
			if (YB_Flag=="1"||PayMode=="港内卡G")
			{
				
			}
			else
			{
				ZZJCore.Public_Var.patientInfo.DepositAmount = Convert.ToDecimal(zhye).ToString();
			}
			//isSuccess="成功";
			PrintPT(PayMode);
			return 0;
		}
		#endregion 挂号

		#region 打印挂号凭条
		private static void PrintPT(string PayMode)
		{
			string PayProc = "";
			//if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "1")
			//{
			//  PayProc = "港内卡";
			//}
			//if (ZZJCore.Public_Var.cardInfo.CardType.ToString() == "0")
			//{
			//  PayProc = "预交金";
			//}
			
	if (ZZJ_Module.Public_Var.zhjb.ToString() == "1")
						{
							PayProc = "预交金";
						}
						else if (ZZJ_Module.Public_Var.zhjb.ToString() == "2")
						{
							PayProc = "港内卡";
						}
						else
						{
							PayProc = "附属卡";
						}

	if (PayMode == "港内卡G")
	{
		if (GangCardPay.gangCard.payLevel == "2")
		{
			PayProc = "港内卡";
		}
		else if(GangCardPay.gangCard.payLevel=="3")
		{
			PayProc = "附属卡";
		}
	}
			if (ZZJ_Module.ChargeClass.ChargePayMode == 0) PayProc = "银联";
			if (ZZJ_Module.ChargeClass.ChargePayMode == 1) PayProc = "医保";
			//停车票数据
			//ZZJCore.Public_Var.patientInfo.PatientName = "王诚";
			//selectRegTime = "13:00~17:00";
			//ZZJCore.Public_Var.patientInfo.DepositAmount = "174";
			//ZZJCore.Public_Var.SerialNumber = "101010-20170914-0000024";


			List<ZZJCore.ZZJStruct.DataLine> AL = new List<ZZJCore.ZZJStruct.DataLine>();
			AL.Add(new ZZJCore.ZZJStruct.DataLine("姓名", ZZJCore.Public_Var.patientInfo.PatientName, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊卡号", ZZJCore.Public_Var.cardInfo.CardNo, null, null));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("登记号", ZZJCore.Public_Var.patientInfo.PatientID, null, null));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊序号", gh_sjh, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊科室", selectDeptName, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("医生名称", selectDoctName, null, null));
			//	AL.Add(new ZZJCore.ZZJStruct.DataLine("队列号", gh_jlxh, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("队列类型", selectRegType, null, null));
			//AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊时间", Convert.ToDateTime(selectRegTime).ToString("yyyy-MM-dd"), null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("就诊时间", selectRegTime, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("挂号费", Convert.ToDecimal(gh_ysje).ToString("C"), null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("账户余额",Convert.ToDecimal( ZZJCore.Public_Var.patientInfo.DepositAmount).ToString("C"), null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));//Convert.ToDecimal(zhye).ToString("C")
			AL.Add(new ZZJCore.ZZJStruct.DataLine("支付方式", PayProc, null, null));
			if(PayMode=="港内卡G")
			{
				
				AL.Add(new ZZJCore.ZZJStruct.DataLine("港内卡号", GangCardPay.gangCard.GangCardNo, null, null));
				AL.Add(new ZZJCore.ZZJStruct.DataLine("港内卡姓名", GangCardPay.gangCard.GangCardName, null, null));
				AL.Add(new ZZJCore.ZZJStruct.DataLine("港内卡余额", zhye, null, null));
			}
			// ZFFS = "银联"
			//if (isSuccess.ToString()!=""||isSuccess.ToString()!=null)
			//{
			//AL.Add(new ZZJCore.ZZJStruct.DataLine("挂号描述", isSuccess, null, new Font(new FontFamily("黑体"), 15, FontStyle.Bold)));
			//	}
			AL.Add(new ZZJCore.ZZJStruct.DataLine("流水号", ZZJCore.Public_Var.SerialNumber, null, null));
			AL.Add(new ZZJCore.ZZJStruct.DataLine("打印时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), null, new Font(new FontFamily("黑体"), 13, FontStyle.Bold)));
			bool[] HideTYData = new bool[] { true, true, true, true, true, true };
			if (ZZJCore.SuanFa.PrintCall.PrintPT("门诊挂号凭证", AL.ToArray(), "当日挂号请于当日" + selectRegTime + "时内就诊，就诊时请出示本凭条和病历!", false, HideTYData))
			{
				ZZJCore.SuanFa.Proc.Log("->凭条打印成功");
			}
			else
			{
				ZZJCore.SuanFa.Proc.Log("->凭条打印打印异常");
			}
		}

		#endregion


		
		private static DeptItem FindDept(DeptRoot ROOT, string DeptCode)
		{
			foreach (DeptItem item in ROOT.Dept)
			{
				if (item.DeptCode == DeptCode) return item;
			}
			return null;
		}
		/// <summary>
		/// 打印港内卡缴费凭条
		/// </summary>
		public static void PrintGKPT(string KH,decimal je)
		{
			string[] NR1 = new string[] { "港内卡号", "交易金额", "交易时间" };
			string[] NR2 = new string[] { KH.ToString(), je.ToString("C"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") };
			ZZJCore.SuanFa.PrintCall.PrintPT("港内卡支付凭证", NR1, NR2, "", false);
		}

		/// <summary>
		/// 打印预交金缴费凭条
		/// </summary>
		public static void PrintGKPT(decimal je)
		{
			string[] NR1 = new string[] { "交易金额", "交易时间" };
			string[] NR2 = new string[] { je.ToString("C"), DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") };
			ZZJCore.SuanFa.PrintCall.PrintPT("预交金支付凭证", NR1, NR2, "", false);
		}

		/// <summary>
		/// 显示选择窗口
		/// </summary>
		/// <param name="Caption"></param>
		/// <param name="Buttons"></param>
		/// <returns></returns>
		private static int ShowSelectWindow(string Caption, string[] Buttons, bool retbtn = true, bool Closebtn = true)
		{
			ZZJCore.SelectForm2.SelectFormParameter SFP = new SelectForm2.SelectFormParameter();
			SFP.Caption = Caption;
			SFP.ContentAlignment = System.Drawing.ContentAlignment.MiddleCenter;
			SFP.VDistance = 20;//垂直间距
			SFP.HDistance = 60;//水平
			SFP.TextButtonWidth = 320;
			SFP.TextButtonHeight = 140;
			SFP.TextButtonFont = new System.Drawing.Font("黑体", 20, FontStyle.Bold);
			//、、SFP.Title.Font = new Font(new FontFamily("黑体"), 52, FontStyle.Bold);
			//SFP.HDistance = 60;//水平间距 
			SFP.VDistance = 20;//垂直间距
			/////
			SFP.EndLineAlignment = HorizontalAlignment.Left;
			SFP.ButtonTexts = Buttons;
			//###
			List<ZZJCore.ZZJStruct.ZZJButton> ZZJAL = new List<ZZJCore.ZZJStruct.ZZJButton>();
			if (retbtn) ZZJAL.Add(ZZJCore.ZZJControl.Button_Ret);
			if (Closebtn) ZZJAL.Add(ZZJCore.ZZJControl.Button_Close);
			SFP.Buttons = ZZJAL.ToArray();
			//###
			return ZZJCore.SelectForm2.ShowForm(SFP);
		}
}

  
	public class DoctItem
	{
		public string Name = "";
		public string Code = "";
		public string Free = "";
		public string Time = "";
		public string AmPm = "";
		public string Yszc = "";
		public string Zc="";
		//号源数
		public string Num = "";
	}

	public class DeptItem
	{
		public string Time = "";
		public string AmPm = "";
		public string DeptName = "";
		public string DeptCode = "";
		public string Free = "";
		
		//医生名字列表
		public List<string> Names = new List<string>();
		//医生列表
		public List<DoctItem> Doctor = new List<DoctItem>();
	}

	public class DeptRoot
	{
		//科室名字列表
		public List<string> Names = new List<string>();
		//科室列表
		public List<DeptItem> Dept = new List<DeptItem>();
		}



}
