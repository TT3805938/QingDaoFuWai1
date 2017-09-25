using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ZZJCore;
using Mw_Public;
using System.Data;
using Mw_Voice;
using System.Threading;
using System.Xml;
using System.IO;
using System.Drawing;
using Mw_MSSQL;
using System.Data.SqlClient;
using System.Collections;
using System.Drawing.Printing;
using HL.Devices.DKQ;
using HL.Devices.ZKJ;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft;
using System.Net;

namespace ZZJ_Module
{
	//满意度调查类
	public class MYD
	{
		/// <summary>
		/// 答案集合
		/// </summary>
		public List<Answer> listAns{get;set;}
		/// <summary>
		/// ID
		/// </summary>
		public string ID{get;set;}
		/// <summary>
		/// 电话号码
		/// </summary>
		public string phoneNo{get;set;} 


		/// <summary>
		/// 上传问卷结果
		/// </summary>
		/// <returns></returns>
		public  int updateMYD()
		{
			try
			{
				string answers="";
				foreach(Answer a in listAns)
				{
					answers+=a.Number+":"+a.AAnswer+"|";
				}
				//格式为 "AppCode=4005&ID=20170713105901132&phoneNo=13466765421&Answer=1:心内科|2:满意|3:非常满意"
				string postData="AppCode=4004&ID="+ID+"&Answer="+answers;
				//返回 成功或者失败
				string outJson=MYDDC.HttpPost("http://10.17.133.1:3000/console.aspx?"+postData,"");
				//string outJson= ZZJCore.SuanFa.POSTInvoke.Invoke("http://10.17.133.1:3000/console.aspx?"+postData,"");
			
				//调用接口去上传问卷结果
				JObject jobject=JObject.Parse(outJson);
				if(jobject["Code"].ToString().Trim()!="200")
				{
					return -99;
				}
				else
				{
					return 0;
				}
			}
			catch(Exception ex)
			{
				ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "提交问卷失败!\r\n" + ex.Message, true);
				return -99;
			}
		}
		
	}

	
	/// <summary>
	/// 满意度调查相关方法
	/// </summary>
	public static class MYDDC
	{
		
		public static string[] answerChoce={"非常满意","满意","不满意","不评价"};
	/// <summary>
	/// 满意度调查入口
	/// </summary>
		public static bool MYDDCMain()
		{
			//初始化
			ZZJCore.FormSkin.UseRetCard = false;
			ZZJCore.Public_Var.patientInfo.PatientName = "";
			ZZJCore.Public_Var.cardInfo.CardNo = "";
			ZZJCore.Public_Var.patientInfo.DepositAmount = "";
			ZZJCore.Public_Var.cardInfo.CardType = "0";
			ZZJCore.Public_Var.ModuleName = "满意度问卷";
			ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
			Application.DoEvents();
			MYD wj=new MYD();

		  //第一步
			ZZJCore.YesNoForm.YesNoFormParameter ynP=new YesNoForm.YesNoFormParameter();
			ynP.DJSTime=30;
			//ynP.LabelText="您好,感谢您信任并选择来我院就医!\n为了给您提供优质的医疗服务，及更好的就医环境，提升您就医体验，请留下您的宝贵意见，我们将积极改进，竭诚为您服务。\n下面开始您满意度调查";
			ynP.LabelText="满意度调查问卷,点击确认按钮继续";
			ynP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close, ZZJCore.ZZJControl.Button_OK };
			int iRet= ZZJCore.YesNoForm.ShowForm(ynP);
			if(iRet!=0)
			{
				ZZJCore.BackForm.CloseForm();
				return true;
			}
			//问题集合 通过http请求json拿
		  List<Subject> listSub=GetSubjectByHttpPost();
			if(listSub==null)
			{
				ZZJCore.BackForm.ShowForm("满意度问卷查询问题失败，可能为网络原因");
				ZZJCore.BackForm.CloseForm();
				return true;
			}
			
			string ans="";
			List<Answer> listAnswer=new List<Answer>();
			listAnswer.Clear();
			//开始答题
			foreach(Subject s in listSub)
			{
				if(s.Type=="1")//选择题
				{
					Answer ans1=new Answer();
					ans1.Number=s.Number;
					
				  iRet= ShowForm(s.SSubject);
					if(iRet<0)
					{
						ZZJCore.BackForm.CloseForm();
						return true;
					}
					ans1.AAnswer = answerChoce[iRet];
					listAnswer.Add(ans1);
				  
				}
				else if(s.Type=="2")//问答题
				{
					Answer ans2=new Answer();
					ans2.Number=s.Number;
					ans2.AAnswer = ShowKSForm(s.SSubject);
					if(ans2.AAnswer=="-99")
					{
						ZZJCore.BackForm.CloseForm();
						return true;
					}
					listAnswer.Add(ans2);
				}
				else if(s.Type=="3")//电话
				{
					Answer ans3=new Answer();
					ans3.Number=s.Number;
					ans3.AAnswer=getPhone();
					listAnswer.Add(ans3);
				}
			}
			

			wj.ID=DateTime.Now.ToString("yyyyMMddhhmmss")+new Random().Next(100,999).ToString();
			wj.phoneNo="";//电话号码
			wj.listAns=listAnswer;//答案集合


			if( wj.updateMYD()!=0)
			{
				ZZJCore.BackForm.ShowForm("提交问卷失败！可能为网络原因，请稍后重试",true);
				ZZJCore.BackForm.CloseForm();
				return true;
			}
			else
			{
				ZZJCore.BackForm.ShowForm("您的满意度调查问卷已提交!",true);
				return true;
			}

			////存数据库
			//MessageBox.Show(ans.ToString()+ksmc.ToString()+phoneNo);
			
		}

		public static Image[] MYDICON = new Image[] { 
			Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/MYD/veryGood.png"), 
			Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/MYD/good.png"),
			Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/MYD/bad.png"),
			Image.FromFile(ZZJCore.Public_Var.ModulePath + "Image/MYD/nochoce.png"),
		};
		/// <summary>
		///  抛出选择框
		/// </summary>
		/// <param name="BT">题目</param>
		/// <returns>0非常满意 1满意 2不满意 3不评价</returns>
		public static int ShowForm(string BT)
		{
			ZZJCore.SelectForm2.SelectFormParameter SFP = new SelectForm2.SelectFormParameter();
			
			byte[] RAX = new byte[5];
			try
			{
				int ReadIniRet = 0;
				List<Image> AL = new List<Image>();
				int AX = 0;
				{
					AL.Add(MYDICON[0]);
					RAX[AX] = 0;
					AX++;
				}


				AL.Add(MYDICON[1]);
					RAX[AX] = 1;
					AX++;
				
				{
					AL.Add(MYDICON[2]);
					RAX[AX] = 2;
					AX++;
				}

				{
					AL.Add(MYDICON[3]);
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


			List<ZZJCore.ZZJStruct.FontText> ALL = new List<ZZJCore.ZZJStruct.FontText>();
			ZZJCore.ZZJStruct.FontText AText = new ZZJCore.ZZJStruct.FontText();
			
			AText.X = 270;//设置位置
			AText.Y = 150;
			AText.W=800;
			AText.Text = BT.ToString();//内容
			ALL.Add(AText);

			SFP.Texts = ALL.ToArray();



			SFP.ContentAlignment = ContentAlignment.MiddleCenter;
			SFP.VDistance = 10;//垂直间距
			SFP.HDistance = 100;//水平间距
			SFP.Buttons = new ZZJCore.ZZJStruct.ZZJButton[] { ZZJCore.ZZJControl.Button_Close };

			int iRet = ZZJCore.SelectForm2.ShowForm(SFP);
			return iRet;
		}
		
		
		/// <summary>
		/// 抛出科室选择
		/// </summary>
		/// <param name="BT">标题</param>
		/// <returns>选择的科室名称</returns>
		public static string  ShowKSForm(string BT)
		{
			string XmlData = "";
			XmlDocument XmlDoc = new XmlDocument();
			string Msg="";
			List<string> listDpt=new List<string>();
			listDpt.Clear();
			listDpt.Add("不选择");
			//从his中得到所有科室列表
			XmlMaker Table = XmlMaker.NewTabel();
			Table.Add("lb", 0);// 查询类型 0普通 1专家 2 急诊
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
				//ZZJCore.BackForm.ShowForm("科室查询接口通讯错误!\r\n"+ex.Message, true);
				return "-99";
			}

			try
			{
				string ResultCode = XmlDoc.SelectSingleNode("zzxt/result/retcode").InnerText.Trim();
				Msg = XmlDoc.SelectSingleNode("zzxt/result/retmsg").InnerText.Trim();
				if (ResultCode != "0")
				{
					ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "没有查到科室信息!\r\n" + Msg, true);
					//ZZJCore.BackForm.ShowForm("没有查到科室信息!\r\n" + Msg, true);
					return "-99";
				}
				int TableCount = int.Parse(XmlDoc.SelectSingleNode("zzxt/tablecount").InnerText.Trim());
				//循环找出所有信息
				for (int i = 1; i <= TableCount; i++)
				{
					string scr = string.Format("zzxt/table{0}", i);
					XmlNode Node = XmlDoc.SelectSingleNode(scr);
					string sDeptName = Node.SelectSingleNode("ksmc").InnerText.Trim();
					if(!string.IsNullOrEmpty(sDeptName))
					{
						Deptment deptment=new Deptment();
						deptment.ksmc=sDeptName.ToString();
						listDpt.Add(deptment.ksmc);//添加进来
					}
				}
			}
			catch (System.Exception ex)
			{
				ZZJCore.SuanFa.Proc.ZZJMessageBox(null, "科室查询接口返回信息错误!\r\n" + ex.Message, true);
				//	ZZJCore.BackForm.ShowForm("科室查询接口返回信息错误!\r\n" + ex.Message, true);
				return "-99";
			}



			//添加进form中
			ZZJCore.SelectForm2.SelectFormParameter SFP = new SelectForm2.SelectFormParameter();
			SFP.Caption = BT;
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
			SFP.ButtonTexts =listDpt.ToArray() ;
			//###
			List<ZZJCore.ZZJStruct.ZZJButton> ZZJAL = new List<ZZJCore.ZZJStruct.ZZJButton>();
			if (false) ZZJAL.Add(ZZJCore.ZZJControl.Button_Ret);
			if (true) ZZJAL.Add(ZZJCore.ZZJControl.Button_Close);
			SFP.Buttons = ZZJAL.ToArray();
			//###
			int selectedIndex= ZZJCore.SelectForm2.ShowForm(SFP);
			//显示
			if(selectedIndex<0)
			{
				return "-99";
			}
			//返回选择的
			return listDpt[selectedIndex].ToString().Trim();
		}


		/// <summary>
		///  获取手机号
		/// </summary>
		/// <returns>手机号</returns>
		public static string getPhone()
		{
			ZZJCore.InputForm.InputHospitalIzationIDParameter IP = new ZZJCore.InputForm.InputHospitalIzationIDParameter();
			IP.MAX = 11;
			IP.MIN = 11;
			IP.DJSTime = 60;
			IP.Caption = "请输入您的手机号";
			IP.LabelText = "";
			IP.Voice = "";
			List<ZZJCore.ZZJStruct.ZZJButton> ALS = new List<ZZJCore.ZZJStruct.ZZJButton>();
			ALS.Add(ZZJCore.ZZJControl.Button_Close);
			IP.Buttons = ALS.ToArray();
			IP.XButton = false;
			//IP.RegMode = false;
			string ID = "";//////////////////////
			int iRett = ZZJCore.InputForm.InputHospitalIzationID(IP, out ID);
			if (iRett != 0)
			{
				return "-99";
			}
			return ID;
		}
		public static List<Subject> GetSubjectByHttpPost()
		{
			try
			{
				//string outJson= ZZJCore.SuanFa.POSTInvoke.Invoke("http://10.17.133.1:3000/console.aspx?AppCode=4001&isShow=1&Type=-1", "");
				string outJson=HttpPost("http://10.17.133.1:3000/console.aspx","AppCode=4001&isShow=1&Type=-1");
				JObject result=JObject.Parse(outJson);
				if(result["Code"].ToString().Trim()!="200")
				{
					return null;//说明请求失败了
				}
				JArray SubArray=JArray.Parse(result["result"].ToString());
				List<Subject> listSub=new List<Subject>();
				listSub.Clear();
				if(SubArray!=null)
				{
					foreach(JObject j in SubArray)
					{
							Subject sub=new Subject();
							sub.ID=j["ID"].ToString();
							sub.Number=j["Number"].ToString();
							sub.SSubject=j["Subject"].ToString();
							sub.isShow=j["isShow"].ToString();
							sub.Type=j["Type"].ToString();
							listSub.Add(sub);
					}
				}
				return listSub;
			}
			catch(Exception ex)
			{
				ZZJCore.SuanFa.Proc.Log("查询问题出错,出错原因可能为:"+ex.ToString());
				return null;
			}
		}



		 public static string HttpPost(string Url, string postDataStr)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = Encoding.UTF8.GetByteCount(postDataStr);
			//request.CookieContainer = cookie;
			Stream myRequestStream = request.GetRequestStream();
			StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
			myStreamWriter.Write(postDataStr);
			myStreamWriter.Close();

			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			//response.Cookies = cookie.GetCookies(response.ResponseUri);
			Stream myResponseStream = response.GetResponseStream();
			StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
			string retString = myStreamReader.ReadToEnd();
			myStreamReader.Close();
			myResponseStream.Close();

			return retString;
		}

	}
	public class Answer
	{
		public string Number{get;set;}
		public string AAnswer{get;set;}
	}
	public class Subject
	{
		public string ID{get;set;}
		public string Number{get;set;}
		public string SSubject{get;set;}
		public string isShow{get;set;}
		public string Type{get;set;}

	}
	public class Deptment
	{
	/// <summary>
	/// 科室名称
	/// </summary>
		public string ksmc{get;set;}
	} 
	
}
