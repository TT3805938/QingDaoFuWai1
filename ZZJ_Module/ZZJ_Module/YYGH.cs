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
using System.Linq;
using System.Threading;

namespace ZZJ_Module
{
	
	public class YYGH
	{
		//预约不收费,没必要存数据库了
		/// <summary>
		/// 科室
		/// </summary>
		public class Deptment
		{
			public string DeptID { get; set; }//科室编号
			public string DeptName { get; set; }//科室名称
			public string DeptNum{get;set;}//剩余号源数

			public List<Doctor> listDoctor{get;set;}//科室下面的大夫
		}
		/// <summary>
		/// 医生
		/// </summary>
		public class Doctor
		{
			public string ysmc{get;set;}//医生名称
			public string ysdm{get;set;}//医生代码
			public string ksdm{get;set;}//科室代码
			public string ksmc{get;set;}//科室名称
			public string py{get;set;}//拼音
			public string wb{get;set;}//五笔
			public string ghf{get;set;}//挂号费
			public string zlf{get;set;}//诊疗费
			public string kszjbz{get;set;}//科室专家标志
			public List<PBInfo> listPB{get;set;}//一周的排班信息
		}
		/// <summary>
		/// 排版信息
		/// </summary>
		public class PBInfo
		{
			public string pbmxxh{get;set;}//排版明细序号
			public string ksdm { get; set; }//科室代码
			public string ksmc { get; set; }//科室名称
			public string ysdm { get; set; }//医生代码
			public string ysmc { get; set; }//医生名称
			public string ghf { get; set; }//挂号费
			public string zlf { get; set; }//诊疗费
			public string ghs { get; set; }//挂号数
			public string xhs { get; set; }//限号数
			public string kszjbz { get; set; }//科室专家标志
			public string yyrq { get; set; }//预约日期
			public string sjd { get; set; }//预约时间段
			public string mzxh { get; set; }//门诊序号
		}
		/// <summary>
		/// 打印信息
		/// </summary>
		public class PrintPTInfo
		{
			public string YYDate = "";//预约日期
			public string YYKs = "";//预约科室
			public string YYDoctorName = "";//预约医生
			public string ghf = "";//挂号费
		}

		public static bool YYGHMain()
		{
			string MSG = "";
			ZZJ_Module.ChargeClass.ChargePayMode = -1;
			ZZJCore.FormSkin.UseRetCard = true;
			ZZJCore.Public_Var.ModuleName = "预约挂号";
			ZZJCore.BackForm.ShowForm("正在查询,请稍候...");
			Application.DoEvents();
			Thread.Sleep(1500);
			ZZJCore.Initial.Read();//读取配置文件
			
			//获取用户信息
			if (XMLCore.GetUserInfo(out MSG) != 0)
			{
				if (!string.IsNullOrEmpty(MSG)) ZZJCore.BackForm.ShowForm(MSG, true);
				ZZJCore.BackForm.CloseForm();
				return true;
			}
			ZZJCore.SuanFa.Proc.Log("[进入预约挂号]");
			ZZJCore.SuanFa.Proc.Log("[开始初始化]");
			List<string> Buttons = new List<string>();
			string XMLData="";
			string YYDate="";//预约日期
			PrintPTInfo PrintInfo=new PrintPTInfo();
			string outXML303="";
			ZZJCore.SuanFa.Proc.Log("[初始化结束]");
			ZZJCore.SuanFa.Proc.Log("[开始获取挂号类别]");
			try
			{
			//1.获取挂号类别
			hqghlb:
			int ghlb=GetPbCategory();
			ZZJCore.SuanFa.Proc.Log("用户选择的挂号类别为:"+ghlb.ToString()+"  (0.普通号 1.专家号)");
		  if(ghlb<0)
			{
				ZZJCore.SuanFa.Proc.Log("在选择挂号类别时,用户选择了退出");
				ZZJCore.SuanFa.Proc.Log("[预约挂号功能退出]");
				ZZJCore.BackForm.CloseForm();
				return true;
			}
			else if(ghlb==0)//普通号
			{
				
				xzjzsj:
				ZZJCore.SuanFa.Proc.Log("[开始选择预约日期]");
					Buttons.Clear();
					Buttons.Clear();
					for (int i = 1; i < 8; i++)
					{
						Buttons.Add(DateTime.Now.AddDays(i).ToString("yyyy-MM-dd"));
					}
					int Result = ShowSelectWindow("请选择预约时间", Buttons.ToArray(), true);
					
					if (Result < 0)
					{
						if(Result==-1)
						{
							ZZJCore.SuanFa.Proc.Log("选择上一步,跳转到获取挂号类别");
							goto hqghlb;
						}
						ZZJCore.SuanFa.Proc.Log("用户未选择挂号时间");
						ZZJCore.SuanFa.Proc.Log("[预约挂号功能退出]");
						ZZJCore.BackForm.CloseForm();
						return true;
					}
					ZZJCore.SuanFa.Proc.Log("用户选择的普通号预约时间:"+YYDate);
					YYDate = Buttons[Result];
					YYDate = YYDate.ToDateTime().ToString("yyyyMMdd");
					ZZJCore.SuanFa.Proc.Log("开始调用his查询预约排班接口301,挂号类别为:["+ghlb+"],查询开始日期:["+YYDate+"],查询结束日期:["+YYDate+"]");
					XMLData = GetHisPbXml301(ghlb, YYDate, YYDate);
			}
			

			//2.获得his排班信息
			//XMLData = GetHisPbXml301(ghlb, YYDate, YYDate);
			if(ghlb==1)//专家
			{
				ZZJCore.SuanFa.Proc.Log("[开始查询预约排班信息]");
				ZZJCore.SuanFa.Proc.Log("开始调用his查询预约排班接口301,挂号类别为:[" + ghlb + "],查询开始日期:[" + DateTime.Now.AddDays(1).ToString("yyyyMMdd") + "],查询结束日期:[" + DateTime.Now.AddDays(7).ToString("yyyyMMdd") + "]");
				XMLData = GetHisPbXml301(ghlb, DateTime.Now.AddDays(1).ToString("yyyyMMdd"), DateTime.Now.AddDays(7).ToString("yyyyMMdd"));
			}
			
			if(XMLData=="-99")//说明接口不通啊
			{
				//ToDo  弹出窗体 说明his存在问题
				ZZJCore.SuanFa.Proc.Log("调用his查询预约排班接口失败,可能是接口不通");
				ZZJCore.SuanFa.Proc.Log("[预约挂号功能结束]");
				ZZJCore.BackForm.ShowForm("获取his排班接口不通,可能是网络原因！", true);
				return true;
			}
			else //接口是通的
			{
				ZZJCore.SuanFa.Proc.Log("调用his查询预约排班接口成功,下面开始解析");
				XmlDocument XD=new XmlDocument();
				XD.LoadXml(XMLData);
				//<zzxt><result><retcode>-1</retcode><retmsg>未取到数据</retmsg></result></zzxt>
			  string retCode=XD.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();//回参  0是查询成功 其他是失败
				if(retCode.Trim()!="0")//说明失败了
				{
					string errorMsg=XD.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
					ZZJCore.SuanFa.Proc.Log("his查询排班记录失败,可能的原因为:" + errorMsg.ToString());
					ZZJCore.SuanFa.Proc.Log("[预约挂号功能结束]");
					ZZJCore.BackForm.ShowForm("his查询排班记录失败,可能的原因为:"+errorMsg.ToString(),true);
					ZZJCore.BackForm.CloseForm();
					return true;
				}
				else//说明成功了
				{
					
					//ToDo  解析数据
					List<Deptment> listDeptment = new List<Deptment>();
					if(string.IsNullOrEmpty(XD.SelectSingleNode("zzxt/tablecount").InnerText))//说明没有记录
					{
						ZZJCore.SuanFa.Proc.Log("his没有排班记录");
						ZZJCore.SuanFa.Proc.Log("预约挂号功能结束");
						ZZJCore.BackForm.CloseForm();
						ZZJCore.BackForm.ShowForm("his没有排班记录", true);
						return true;
					}
					int tableCount = Convert.ToInt32(XD.SelectSingleNode("zzxt/tablecount").InnerText);
					List<Doctor> ld=new List<Doctor>();
					ld.Clear();
					ZZJCore.SuanFa.Proc.Log("开始加载科室信息和医生信息");
					for(int i=1;i<=tableCount;i++)
					{
						string src=string.Format("zzxt/table{0}",i.ToString());
						Doctor dr=new Doctor();
						dr.ksmc=XD.SelectSingleNode(src+"/ksmc").InnerText;
						dr.ksdm=XD.SelectSingleNode(src+"/ksdm").InnerText;
						if(ghlb==1)//专家
						{
							dr.ysmc=XD.SelectSingleNode(src+"/ysmc").InnerText;
							dr.ysdm=XD.SelectSingleNode(src+"/ysdm").InnerText;
						}
						
					  ld.Add(dr);
						int result=-99;
						foreach(Deptment d in listDeptment)
						{
							if(dr.ksdm!=d.DeptID)//说明没有
							{
								continue;
							}
							else
							{
								result=0;
								break;
							}
						}
						if(result!=0)//list中没有这个科室就添加进来
						{
								Deptment dept=new Deptment();
								dept.DeptID=dr.ksdm;
								dept.DeptName=dr.ksmc;
								listDeptment.Add(dept);
							}
					}
					ZZJCore.SuanFa.Proc.Log("加载科室信息和医生信息结束");
					//ToDo  抛出选择科室界面
					//3.选择科室
					xzks:
					ZZJCore.SuanFa.Proc.Log("[开始选择科室]");
					string selectedDeptmentID=GetSelectedDeptmentID(listDeptment);
					if(Convert.ToInt32( selectedDeptmentID)<0)
					{
						if (selectedDeptmentID == "-2")
						{
							ZZJCore.SuanFa.Proc.Log("用户选择了上一步,跳转到[获取挂号类别]");
							goto hqghlb;
						}
						ZZJCore.SuanFa.Proc.Log("用户选择了退出");
						ZZJCore.SuanFa.Proc.Log("[预约挂号功能退出]");
						ZZJCore.BackForm.CloseForm();
						return true;
					}
					ZZJCore.SuanFa.Proc.Log("选择科室成功,选择的科室:"+listDeptment.Find(p=>p.DeptID==selectedDeptmentID).DeptName);
					string outHis302PBXml="";
					
					//判断一个挂号类别是否需要继续抛出医生列表
					if(ghlb==0)//说明是普通号
					{
						ZZJCore.SuanFa.Proc.Log("开始查询" + listDeptment.Find(p => p.DeptID == selectedDeptmentID).DeptName + "的排班明细" + "参数为:" + ghlb + ",科室编码:" + selectedDeptmentID + ",开始日期:"
						+YYDate+",结束日期:"+YYDate);
						 outHis302PBXml = GetHisPbXml302(ghlb, selectedDeptmentID, YYDate, YYDate);
						

					}
					else if(ghlb==1)//专家号
					{
						//4选择专家
						//ToDo 抛出选择专家界面
						ZZJCore.SuanFa.Proc.Log("开始查询" + listDeptment.Find(p => p.DeptID == selectedDeptmentID).DeptName + "的排班明细" + "参数为:" + ghlb + ",科室编码:" + selectedDeptmentID + ",开始日期:"
						+ DateTime.Now.AddDays(1).ToString("yyyyMMdd") + ",结束日期:" + DateTime.Now.AddDays(7).ToString("yyyyMMdd"));
						outHis302PBXml = GetHisPbXml302(ghlb, selectedDeptmentID, DateTime.Now.AddDays(1).ToString("yyyyMMdd"), DateTime.Now.AddDays(7).ToString("yyyyMMdd"));
						List<Doctor> listDoct=new List<Doctor>();
						listDoct.Clear();
						foreach(Doctor dd in ld)
						{
							if(dd.ksdm==selectedDeptmentID)
							{
								listDoct.Add(dd);
							}
						}
					xzzj:
					ZZJCore.SuanFa.Proc.Log("[开始选择专家和时间]");
					List<Doctor> doctorNew=new List<Doctor>();
					foreach(Doctor d in listDoct)
					{
						outHis302PBXml=GetHisPbXml302(ghlb,d.ysdm,DateTime.Now.AddDays(1).ToString("yyyyMMdd"),DateTime.Now.AddDays(7).ToString("yyyyMMdd"));
						if(outHis302PBXml=="-99")
						{
							//不通
						}
						else
						{
							try
							{
								XmlDocument XDD = new XmlDocument();
								XDD.LoadXml(outHis302PBXml);
								//<zzxt><result><retcode>-1</retcode><retmsg>未取到数据</retmsg></result></zzxt>
								string retCode302 = XDD.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();//回参  0是查询成功 其他是失败
								if (retCode302.Trim() != "0")//说明失败了
								{
									string errorMsg = XDD.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
									ZZJCore.BackForm.ShowForm("his查询排班记录失败,可能的原因为:" + errorMsg.ToString(), true);
									return true;
								}
								else//说明成功了
								{
										if (string.IsNullOrEmpty(XDD.SelectSingleNode("zzxt/tablecount").InnerText))//说明没有记录
										{
											ZZJCore.BackForm.ShowForm("his没有排班记录", true);
										}
										int tableCount302 = Convert.ToInt32(XDD.SelectSingleNode("zzxt/tablecount").InnerText);
										List<PBInfo> ListPB = new List<PBInfo>();
										ListPB.Clear();
										for (int i = 1; i <= tableCount302; i++)
										{
											string src = string.Format("zzxt/table{0}", i.ToString());
											PBInfo pbInfo = new PBInfo();
											pbInfo.pbmxxh = XDD.SelectSingleNode(src + "/pbmxxh").InnerText;
											pbInfo.ksdm = XDD.SelectSingleNode(src + "/ksdm").InnerText;
											pbInfo.ksmc = XDD.SelectSingleNode(src + "/ksmc").InnerText;
											pbInfo.ysdm = XDD.SelectSingleNode(src + "/ysdm").InnerText;
											pbInfo.ysmc = XDD.SelectSingleNode(src + "/ysmc").InnerText;
											pbInfo.ghf = XDD.SelectSingleNode(src + "/ghf").InnerText;
											pbInfo.zlf = XDD.SelectSingleNode(src + "/zlf").InnerText;
											pbInfo.xhs = XDD.SelectSingleNode(src + "/xhs").InnerText;
											pbInfo.ghs = XDD.SelectSingleNode(src + "/ghs").InnerText;
											pbInfo.kszjbz = XDD.SelectSingleNode(src + "/kszjbz").InnerText;
											pbInfo.yyrq = XDD.SelectSingleNode(src + "/yyrq").InnerText;
											pbInfo.sjd = XDD.SelectSingleNode(src + "/kssj").InnerText + "-" + XDD.SelectSingleNode(src + "/jssj").InnerText;
											//pbInfo.mzxh = XDD.SelectSingleNode(src + "/mzxh").InnerText;
											ListPB.Add(pbInfo);
										}
										d.listPB=ListPB;
									doctorNew.Add(d);
									}

							}
							catch(Exception e)
							{
								
							}
						}
					}

					//带时间的挂号方法

					ZZJCore.ZZJStruct.YYDoctEx[] YYDC = new ZZJCore.ZZJStruct.YYDoctEx[doctorNew.Count];
					//doctorNew[0].listPB.RemoveAt(2);
					//doctorNew[1].listPB.RemoveAt(6);
					//doctorNew[2].listPB.RemoveAt(4);
					//doctorNew[2].listPB.RemoveAt(5);
					//doctorNew[2].listPB.RemoveAt(1);

					for(int i=0;i<doctorNew.Count;i++)
					{
						ZZJCore.ZZJStruct.YYDoctEx yyd=new YYDoctEx();
						string[] am=new string[]{"","","","","","",""};
						string[] pm = new string[] { "", "", "", "", "", "", ""};
						bool[] ame = new bool[] { false, false, false, false, false, false, false };
						bool[] pme = new bool[] { false, false, false, false, false, false, false };
						//List<string> am = new List<string>();
						//List<string> pm = new List<string>();
						//List<bool> ame = new List<bool>();
						//List<bool> pme = new List<bool>();

						yyd.GHbuttString = "";
						yyd.GHButt = false;
						yyd.Datas = new ZZJCore.ZZJStruct.FontText[8];
						yyd.Datas[0] = NewText(120, 10,System.Windows.Forms.Control.DefaultFont, "姓名" , Color.White);
						yyd.Datas[1] = NewText(250, 6,new Font("新宋体",20,FontStyle.Bold,GraphicsUnit.World), doctorNew[i].ysmc, Color.White);//Color.FromArgb(255,255,128,0).
						yyd.Datas[2] = NewText(120, 38, System.Windows.Forms.Control.DefaultFont, "科室", Color.White);
						yyd.Datas[3] = NewText(250, 38, new Font("新宋体", 20, FontStyle.Bold, GraphicsUnit.World), doctorNew[i].ksmc, Color.White);
						yyd.Datas[4]=NewText(120, 66, System.Windows.Forms.Control.DefaultFont, "类型", Color.White);
						yyd.Datas[5] = NewText(250, 66, new Font("新宋体", 20, FontStyle.Bold, GraphicsUnit.World), "副主任医师", Color.White);
						yyd.Datas[6] = NewText(120, 99, System.Windows.Forms.Control.DefaultFont, "诊查费", Color.White);
						yyd.Datas[7] = NewText(250, 99, new Font("新宋体", 20, FontStyle.Bold, GraphicsUnit.World), Convert.ToInt32(doctorNew[i].listPB[0].zlf).ToString("C"), Color.White);
						for(int j=1;j<8;j++)
						{
							
										foreach(PBInfo p in doctorNew[i].listPB)
										{
													if(p.yyrq==DateTime.Now.AddDays(j).ToString("yyyyMMdd"))//说明这个日期存在 而且还要有号
													{
														if (p.sjd == "08:00-12:00")//上下午
															{
																if (Convert.ToInt32(p.xhs)- Convert.ToInt32(p.ghs) > 0)
																{
																	am[j-1]="预约";
																	ame[j-1]=true;
																}
																else
																{
																	am[j - 1] = "已满";
																	ame[j - 1] = false;
																}
																
															}
														else if (p.sjd == "13:00-17:00")
															{
																if (Convert.ToInt32(p.xhs) - Convert.ToInt32(p.ghs) > 0)
																{
																	pm[j - 1] = "预约";
																	pme[j - 1] = true;
																}
																else
																{
																	am[j - 1] = "已满";
																	ame[j - 1] = false;
																}
																
															}
													}
										}
							}
							YYDC[i]=yyd;
							YYDC[i].AM=am;
							YYDC[i].AME=ame;
							YYDC[i].PM=pm;
							YYDC[i].PME=pme;
					}
					int line = 0;
					int index = 0;
					int selectDoctorID = ZZJCore.YYSelect.ShowForm("请选择预约医生", YYDC, ref line, ref index);
					
					if(selectDoctorID==-1)//返回
					{
						goto xzks;
					}
					if (selectDoctorID<0)
					{
						ZZJCore.BackForm.CloseForm();
						return true;
					}
					ZZJCore.SuanFa.Proc.Log("选择的医生:"+doctorNew[selectDoctorID].ysmc);
				//抛出界面
				xzsjd:
					
					//用日期和时间去拿挂号序号
					//去挂号了钓303
					PBInfo pbInfoo = new PBInfo();
					foreach (PBInfo pb in doctorNew[selectDoctorID].listPB)
					{
					 
						if (pb.yyrq ==DateTime.Now.AddDays(index+1).ToString("yyyyMMdd")&&pb.sjd == (line == 0 ? "08:00-12:00" : "13:00-17:00")) 
						{
							pbInfoo = pb;
						}
					}
					ZZJCore.SuanFa.Proc.Log("选择预约时间:"+pbInfoo.sjd);
					//是否要挂号界面
					//确认挂号信息
					try
					{
						//是否挂号
						ZZJCore.SuanFa.Proc.Log("->确定预约挂号信息!");
						ZZJCore.YesNoForm.YesNoFormParameter YNFP = new ZZJCore.YesNoForm.YesNoFormParameter();
						//YNFP.Caption = "请再次确认您的挂号信息！";

						YNFP.Lab1 = new string[] { "科室名称", "医生姓名", "就诊时间", "诊查费" };
						YNFP.Lab2 = new string[] { pbInfoo.ksmc, pbInfoo.ysmc, pbInfoo.yyrq + " " + pbInfoo.sjd, Convert.ToDecimal(pbInfoo.zlf).ToString("C") };
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
							AText.Text = "温馨提示:根据您的预约时间，提前半小时取号!";//内容
							ALL.Add(AText);

							ZZJCore.ZZJStruct.FontText AText1 = new ZZJCore.ZZJStruct.FontText();
							AText1.X = 350;//设置位置
							AText1.Y = 200;
							AText1.Text = "请再次确认您的预约信息!";//内容
							ALL.Add(AText1);
							YNFP.Texts = ALL.ToArray();
						}
						int SF = ZZJCore.YesNoForm.ShowForm(YNFP);
						Application.DoEvents();
						if (SF < 0)
						{
							if (SF == -1)
							{
								ZZJCore.SuanFa.Proc.Log("用户选择返回上一页");
								ZZJCore.SuanFa.Proc.Log("跳转到选择预约医生和预约时间界面");
								goto xzzj;
							}
							ZZJCore.BackForm.CloseForm();
							ZZJCore.SuanFa.Proc.Log("用户选择了退出");
							ZZJCore.SuanFa.Proc.Log("[预约挂号功能结束]");
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
					//为打印凭条
					PrintInfo.YYDate = pbInfoo.yyrq + " " + pbInfoo.sjd;
					PrintInfo.YYDoctorName = pbInfoo.ysmc;
					PrintInfo.YYKs = pbInfoo.ksmc;
					//去挂号.要成功了
					ZZJCore.SuanFa.Proc.Log("开始调用his303接口预约登记,参数为:");
					ZZJCore.SuanFa.Proc.Log("患者ID:" + ZZJCore.Public_Var.patientInfo.PatientID+"\r\n排班明细序号:" + pbInfoo.pbmxxh+"\r\n科室代码:" + pbInfoo.ksdm+"\r\n科室名称:" + pbInfoo.ksmc+"\r\n医生代码:" + pbInfoo.ysmc);
					outXML303 = GetHisPbXml303(ZZJCore.Public_Var.patientInfo.PatientID, pbInfoo.pbmxxh, pbInfoo.ksdm, pbInfoo.ksmc, pbInfoo.ysdm);
						 //outHis302PBXml = GetHisPbXml302(ghlb, selectDoctorID, YYDate, YYDate);
						 //outHis302PBXml=GetHisPbXml302(ghlb,selectDoctorID,DateTime.Now.AddDays(1).ToString("yyyyMMdd"),DateTime.Now.AddDays(7).ToString("yyyyMMdd"));
						//保存数据库

						//ToDo 打印凭条
					ZZJCore.SuanFa.Proc.Log("跳转到[解析挂号]");
						goto jxgh;
						return true;//返回 	
					}
					else if(ghlb==2)// 专病号
					{
						//ToDo 未知
					}
					else if(ghlb==3)//特需
					{
						//ToDo 未知
					}
					//6.排版信息

					if (outHis302PBXml == "-99")//说明接口不通啊
					{
						//ToDo  弹出窗体 说明his存在问题
						ZZJCore.SuanFa.Proc.Log("获取his排班明细303接口不通.可能是网络原因");
						ZZJCore.SuanFa.Proc.Log("预约挂号功能结束");
						ZZJCore.BackForm.ShowForm("获取his排班接口不通,可能是网络原因！", true);
						return true;
					}
					else//通的要解析
					{
						ZZJCore.SuanFa.Proc.Log("开始解析302接口数据");
						XmlDocument XDD = new XmlDocument();
						XDD.LoadXml(outHis302PBXml);
						//<zzxt><result><retcode>-1</retcode><retmsg>未取到数据</retmsg></result></zzxt>
						string retCode302 = XDD.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();//回参  0是查询成功 其他是失败
						if (retCode302.Trim() != "0")//说明失败了
						{
							ZZJCore.SuanFa.Proc.Log("调用302接口失败");
							ZZJCore.SuanFa.Proc.Log("预约挂号功能结束");
							string errorMsg = XDD.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
							ZZJCore.BackForm.ShowForm("his查询排班记录失败,可能的原因为:" + errorMsg.ToString(), true);
							return true;
						}
						else//说明成功了
						{
							if (string.IsNullOrEmpty(XDD.SelectSingleNode("zzxt/tablecount").InnerText))//说明没有记录
							{
								ZZJCore.SuanFa.Proc.Log("his没有排班记录");
								ZZJCore.SuanFa.Proc.Log("预约挂号功能结束");
								ZZJCore.BackForm.ShowForm("his没有排班记录", true);
								return true;
							}
							int tableCount302 = Convert.ToInt32(XDD.SelectSingleNode("zzxt/tablecount").InnerText);
							List<PBInfo> ListPB = new List<PBInfo>();
							ListPB.Clear();
							for (int i = 1; i <= tableCount302; i++)
							{
								string src = string.Format("zzxt/table{0}", i.ToString());
								PBInfo pbInfo = new PBInfo();
								pbInfo.pbmxxh = XDD.SelectSingleNode(src + "/pbmxxh").InnerText;
								pbInfo.ksdm = XDD.SelectSingleNode(src + "/ksdm").InnerText;
								pbInfo.ksmc = XDD.SelectSingleNode(src + "/ksmc").InnerText;
								pbInfo.ysdm = XDD.SelectSingleNode(src + "/ysdm").InnerText;
								pbInfo.ysmc = XDD.SelectSingleNode(src + "/ysmc").InnerText;
								pbInfo.ghf = XDD.SelectSingleNode(src + "/ghf").InnerText;
								pbInfo.zlf = XDD.SelectSingleNode(src + "/zlf").InnerText;
								pbInfo.xhs = XDD.SelectSingleNode(src + "/xhs").InnerText;
								pbInfo.ghs = XDD.SelectSingleNode(src + "/ghs").InnerText;
								pbInfo.kszjbz = XDD.SelectSingleNode(src + "/kszjbz").InnerText;
								pbInfo.yyrq = XDD.SelectSingleNode(src + "/yyrq").InnerText;
								pbInfo.sjd = XDD.SelectSingleNode(src + "/kssj").InnerText + "-" + XDD.SelectSingleNode(src + "/jssj").InnerText;
								//pbInfo.mzxh = XDD.SelectSingleNode(src + "/mzxh").InnerText;
								ListPB.Add(pbInfo);
							}
						//抛出界面
						xzsjd:
							ZZJCore.SuanFa.Proc.Log("开始选择预约时间段");
							PBInfo pbInfoo = GetSelectedPBInfoPBMxxh(ListPB);
							if (pbInfoo.pbmxxh == "-2")
							{
								ZZJCore.SuanFa.Proc.Log("用户选择了上一步");
								ZZJCore.SuanFa.Proc.Log("跳转到选择科室界面");
								goto xzks;
							}
							//是否要挂号界面
							//确认挂号信息
							try
							{
								//是否挂号
								ZZJCore.SuanFa.Proc.Log("->确定预约挂号信息!");
								ZZJCore.YesNoForm.YesNoFormParameter YNFP = new ZZJCore.YesNoForm.YesNoFormParameter();
								//YNFP.Caption = "请再次确认您的挂号信息！";

								YNFP.Lab1 = new string[] { "科室名称", "医生姓名", "就诊时间", "诊查费" };
								YNFP.Lab2 = new string[] { pbInfoo.ksmc, pbInfoo.ysmc, pbInfoo.yyrq + " " + pbInfoo.sjd, Convert.ToDecimal(pbInfoo.zlf).ToString("C") };
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
									AText.Text = "温馨提示:根据您的预约时间，提前半小时取号!";//内容
									ALL.Add(AText);

									ZZJCore.ZZJStruct.FontText AText1 = new ZZJCore.ZZJStruct.FontText();
									AText1.X = 350;//设置位置
									AText1.Y = 200;
									AText1.Text = "请再次确认您的预约信息!";//内容
									ALL.Add(AText1);
									YNFP.Texts = ALL.ToArray();
								}
								int SF = ZZJCore.YesNoForm.ShowForm(YNFP);
								Application.DoEvents();
								if (SF < 0)
								{
									if (SF == -1)
									{
										ZZJCore.SuanFa.Proc.Log("用户选择了上一步");
										ZZJCore.SuanFa.Proc.Log("跳转到选择时间段");
										goto xzsjd;
									}
									ZZJCore.BackForm.CloseForm();
									ZZJCore.SuanFa.Proc.Log("预约挂号功能结束");
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
							PrintInfo.YYDate = pbInfoo.yyrq + " " + pbInfoo.sjd;
							PrintInfo.YYDoctorName = pbInfoo.ysmc;
							PrintInfo.YYKs = pbInfoo.ksmc;
							outXML303 = GetHisPbXml303(ZZJCore.Public_Var.patientInfo.PatientID, pbInfoo.pbmxxh, pbInfoo.ksdm, pbInfoo.ksmc, pbInfoo.ysdm);
						}

					}

					jxgh:
					//解析挂号接口
					ZZJCore.SuanFa.Proc.Log("开始解析303挂号登记的接口回参");
					XmlDocument XDDD = new XmlDocument();
						XDDD.LoadXml(outXML303);
						//<zzxt><result><retcode>-1</retcode><retmsg>未取到数据</retmsg></result></zzxt>
						string retCode303 = XDDD.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();//回参  0是查询成功 其他是失败
						if (retCode303.Trim() != "0")//说明失败了
						{
							
							string errorMsg = XDDD.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
							ZZJCore.SuanFa.Proc.Log("his查询排班记录失败,可能的原因为:" + errorMsg.ToString());
							ZZJCore.SuanFa.Proc.Log("预约挂号功能结束");
							ZZJCore.BackForm.ShowForm("his查询排班记录失败,可能的原因为:" + errorMsg.ToString(), true);
							return true;
						}
						else//说明成功了
						{
							if (string.IsNullOrEmpty(XDDD.SelectSingleNode("zzxt/table/cgbz").InnerText))//数据
							{
								ZZJCore.SuanFa.Proc.Log("his没有排班记录");
								ZZJCore.SuanFa.Proc.Log("预约挂号功能结束");
								ZZJCore.BackForm.ShowForm("his没有排班记录", true);
								return true;
							}
							string  cgbz = XDDD.SelectSingleNode("zzxt/table/cgbz").InnerText.Trim();
							if(cgbz=="T")
							{
								string yyhx=XDDD.SelectSingleNode("zzxt/table/yyhx").InnerText.Trim();
								string[] NR1={"预约科室","医生名称","预约时间","预约号序"};
								string[] NR2 = { PrintInfo.YYKs,PrintInfo. YYDoctorName,PrintInfo. YYDate, yyhx };
								
								if( PrintPT(NR1,NR2))
								{
									ZZJCore.SuanFa.Proc.Log("打印预约挂号凭条成功!");
								}
								ZZJCore.SuanFa.Proc.Log("预约挂号成功!");
								ZZJCore.SuanFa.Proc.Log("预约挂号功能结束");
								ZZJCore.BackForm.ShowForm("预约成功,请按时取号就诊", true);
								return true;
							}
							else
							{
								ZZJCore.SuanFa.Proc.Log("预约挂号失败!");
								ZZJCore.BackForm.ShowForm("请去人工窗口查询是否预约成功",true);
								return true;
							}

						}
					//7.通知结果 打印凭证
					//ToDo 通知是否成功 并打印凭条


				}
			}
			return true;
			}
			catch(Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log("出现了一个无法处理的异常:"+ex.ToString());
				ZZJCore.BackForm.ShowForm("异常!请联系管理员");
				return true;
			}
		}

		public static bool PrintPT(string[] NR1,string[] NR2)
		{
				return	ZZJCore.SuanFa.PrintCall.PrintPT("预约挂号凭证", NR1, NR2, "", false);
		}

		/// <summary>
		/// 创建新的标签
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="fonts"></param>
		/// <param name="Text"></param>
		/// <param name="nColor"></param>
		/// <returns></returns>
		private static FontText NewText(int x, int y, Font fonts, string Text, Color nColor)
		{
			FontText Ro = new ZZJCore.ZZJStruct.FontText();
			Ro.FontColor = nColor;
			Ro.TextFont = fonts;
			Ro.Text = Text;
			Ro.X = x;
			Ro.Y = y;
			Ro.W = 500;
			return Ro;
		}

		/// <summary>
		/// 快速调试专用
		/// </summary>
		public static void TestQ()
		{
			List<Deptment> listDeptment = new List<Deptment>();
			Deptment dt1 = new Deptment();
			dt1.DeptID = "xzzx";
			dt1.DeptName = "心脏中心";
			dt1.DeptNum = "50";
			listDeptment.Add(dt1);
			Deptment dt2 = new Deptment();
			dt2.DeptID = "nk";
			dt2.DeptName = "内科";
			dt2.DeptNum = "20";
			listDeptment.Add(dt2);
			Deptment dt3 = new Deptment();
			dt3.DeptID = "wk";
			dt3.DeptName = "外科";
			dt3.DeptNum = "0";
			listDeptment.Add(dt3);
			
			while(true)
			{
				string selectedDeptmentID = GetSelectedDeptmentID(listDeptment);
				MessageBox.Show(selectedDeptmentID);
				if(selectedDeptmentID=="-1")
				{
					break;
				}
			}
			
		}

	/// <summary>
	/// 查询his预约排班信息 301
	/// </summary>
	/// <param name="ghlb">挂号类别</param>
	/// <param name="ksrq">开始日期</param>
	/// <param name="jsrq">结束日期</param>
	/// <returns>-99:HIS不通  </returns>
		public static string GetHisPbXml301(int ghlb,string ksrq,string jsrq)
		{
		  //初始化
			string XmlData="";
		
			//查询预约信息
			XmlMaker Table = XmlMaker.NewTabel();
			Table.Add("ghlb", ghlb);// 查询类型 0普通 1专家 2 专病 3特需
			Table.Add("ksrq", ksrq);//查询开始日期
			Table.Add("jsrq", jsrq);//查询的结束日期
			Table.Add("ksdm","");//科室代码 (普通挂号时才有效,传入的是二级科室代价)
			try
			{
				string DataIn = XmlMaker.NewXML("301", Table).ToString();
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.301");
				XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				//XmlData ="<zzxt><result><retcode>0</retcode><retmsg>交易成功</retmsg></result><tablecount>2</tablecount><table1><ysdm>001   </ysdm><ysmc>医技</ysmc><ksdm>3007</ksdm><ksmc>检验科</ksmc><py>yj      </py><wb>ar      </wb></table1><table2><ysdm>100   </ysdm><ysmc>史秀忠</ysmc><ksdm>1002</ksdm><ksmc>内科</ksmc><py>sxz     </py><wb>ktk     </wb></table2></zzxt>";
				ZZJCore.SuanFa.Proc.BW(XmlData, "Service.301");
				return XmlData;
			}
			catch (System.Exception ex)
			{
				//ZZJCore.BackForm.ShowForm("科室查询接口通讯错误!\r\n"+ex.Message, true);
				return "-99";
			}
			
		}
		/// <summary>
		/// 查询某科室或者专家的详细排版信息
		/// </summary>
		/// <param name="ghlb">挂号类别0 科室 1 专家 2 专病 3特需</param>
		/// <param name="kmdm">科室或医生代码</param>
		/// <param name="ksrq">开始日期</param>
		/// <param name="jsrq">结束日期</param>
		/// <returns></returns>
		public static string GetHisPbXml302(int ghlb,string kmdm,string ksrq,string jsrq)
		{
			//初始化
			string XmlData="";
			//查询某科室的预约信息
			XmlMaker Table=XmlMaker.NewTabel();
			Table.Add("ghlb", ghlb);// 查询类型 0普通 1专家 2 专病 3特需
			Table.Add("kmdm",kmdm);//科室或医生代码   //这个地方要传医生代码
			Table.Add("ksrq", ksrq);//查询开始日期
			Table.Add("jsrq", jsrq);//查询的结束日期
			//Table.Add("ksdm", "");//科室代码 (普通挂号时才有效,传入的是二级科室代价)
			try
			{
				string DataIn = XmlMaker.NewXML("302", Table).ToString();
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.302");
				XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(XmlData, "Service.302");
				return XmlData;
			}
			catch (System.Exception ex)
			{
				//ZZJCore.BackForm.ShowForm("科室查询接口通讯错误!\r\n"+ex.Message, true);
				return "-99";
			}
		}

		/// <summary>
		/// 获取排班类别
		/// </summary>
		/// <returns>-1 取消,0 普通,1 专家,2 专病,3 特需</returns>
		public static int GetPbCategory()
		{

			string[] category={"普通","专家"};
			int SelButton = 0;
			int SelRegType = ShowSelectWindow("请选择挂号类型", category, true);
			int SelRegType_by = SelRegType;
			if (SelRegType < 0)//说明取消了
			{
				if (SelRegType == -1)
				{
					return -2;
				}
				return -1;
			}
		 
			else
			{
				return SelRegType;
			}
		}


		/// <summary>
		/// 预约登记 303
		/// </summary>
		/// <param name="ghlb">挂号类别</param>
		/// <param name="ksrq">开始日期</param>
		/// <param name="jsrq">结束日期</param>
		/// <returns>-99:HIS不通  </returns>
		public static string GetHisPbXml303(string patid, string pbmxxh, string ksdm,string ksmc,string ysdm)
		{
			//初始化
			string XmlData = "";

			//查询预约信息
			XmlMaker Table = XmlMaker.NewTabel();
			Table.Add("patid",patid);// 病人唯一码
			Table.Add("pbmxxh", pbmxxh);//排版序号
			Table.Add("ksdm", ksdm);//科室代码
			//Table.Add("ksmc", ksmc);//科室名称
			Table.Add("ysdm", ysdm);//医生代码
			Table.Add("zzid","0");//

			//<zzxt><result><retcode>0</retcode><retmsg>交易成功</retmsg></result><table><cgbz>T</cgbz><patid>48827</patid><yyhx>1</yyhx></table></zzxt>
			try
			{
				string DataIn = XmlMaker.NewXML("303", Table).ToString();
				ZZJCore.SuanFa.Proc.BW(DataIn, "Service.303");
				XmlData = ZZJCore.SuanFa.WebServiceHelper.InvokeWebService(ZZJ_Module.MEF.URL, "CallService", new object[] { DataIn }) as string;
				ZZJCore.SuanFa.Proc.BW(XmlData, "Service.303");
				return XmlData;
			}
			catch (System.Exception ex)
			{
				//ZZJCore.BackForm.ShowForm("科室查询接口通讯错误!\r\n"+ex.Message, true);
				return "-99";
			}

		}


		/// <summary>
		/// 获取选择的科室
		/// </summary>
		/// <param name="listDept">科室对象列表</param>
		/// <returns>选择的科室  -1 未选择</returns>
		public static string  GetSelectedDeptmentID(List<Deptment> listDept)
		{
		  List<string> deptName=new List<string>();
			for(int i=0;i<listDept.Count;i++)
			{
				deptName.Add(listDept[i].DeptName);
			}
			int selectIndex=ShowSelectWindow("请选择您要挂号的科室",deptName.ToArray(),true);
			if(selectIndex<0)//说明取消了
			{
				if (selectIndex == -1)
				{
					return "-2";
				}
				return "-1";
			}
			else//说明选择了
			{
				return listDept[selectIndex].DeptID.ToString();
			}
		}


		/// <summary>
		/// 获取选择的排版信息
		/// </summary>
		/// <param name="listDept">排版信息列表</param>
		/// <returns>选择的科室  -1 未选择</returns>
		public static PBInfo GetSelectedPBInfoPBMxxh(List<PBInfo> listPBInfo)
		{
			List<string> PBsjd = new List<string>();
			for (int i = 0; i < listPBInfo.Count; i++)
			{
				PBsjd.Add(listPBInfo[i].sjd);
			}
			int selectIndex = ShowSelectWindow("请选择您要挂号时间段", PBsjd.ToArray(), true);
			if (selectIndex < 0)//说明取消了
			{
				if(selectIndex==-1)
				{
					PBInfo pbb=new PBInfo();
					pbb.pbmxxh="-2";
					return pbb;
				}
				return null;
			}
			else//说明选择了
			{
				return listPBInfo[selectIndex];
			}
		}


		/// <summary>
		/// 获取选择的医生
		/// </summary>
		/// <param name="listDoctor">医生对象列表</param>
		/// <returns>选择的科室  -1 未选择</returns>
		public static string GetSelectedDoctorID(List<Doctor> listDoctor)
		{
			List<string> doctorName = new List<string>();
			for (int i = 0; i < listDoctor.Count; i++)
			{
				doctorName.Add(listDoctor[i].ysmc);
			}
			int selectIndex = ShowSelectWindow("请选择您要挂号的医生", doctorName.ToArray(), true);
			if (selectIndex < 0)//说明取消了
			{
				if (selectIndex == -1)
				{
				return "-2";
				}
				return "-1";
			}
			else//说明选择了
			{
				return listDoctor[selectIndex].ysdm.ToString();
			}
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
}
