using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Collections;
using System.IO;
using ZZJCore;
using System.Drawing;
using Mw_Public;
using ZZJCore.ZZJStruct;
namespace ZZJ_Module
{
	public static class FWJGQD
	{
		public static string ImgUrl = "http://10.17.133.1:3000/h5/JG/fwjg.html";
		public static bool Main()
		{
			string InXML = "";
			string Msg = "";
			ZZJCore.Public_Var.patientInfo.PatientName="";
			ZZJCore.Public_Var.patientInfo.Address="";
			ZZJCore.FormSkin.UseRetCard = true;
			ZZJCore.Public_Var.ModuleName = "服务价格清单";
			ZZJCore.BackForm.ShowForm("正在准备,请稍候...");
			Application.DoEvents();
			ZZJCore.Initial.Read();//读取配置文件

			YYJSForm yyjs = new YYJSForm();
			yyjs.ShowDialog();
			ZZJCore.BackForm.CloseForm();
			return true;
		}
	}
}
