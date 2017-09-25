using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SDSYB_ReadCard;
using System.IO;
using System.Threading;

namespace HLReadCard
{
	public class SDSYBCardInfo
	{
		 
		static int deviceHandle = -1;
	   
	  
			public static int ReadSDSYB(out SDSYB_CardInfo sdsybCardinfo,out string msg)
			{
				int iReturn = -1;
				sdsybCardinfo=null;
			msg = "";
			try
			{
				deviceHandle = SS728M05Comom.ss_reader_open();
				if (deviceHandle <= 0)
				{
					msg = "连接设备失败！";
					return -1;
				}
			}
			catch (Exception ex)
			{
				msg = "连接设备异常！" + ex.ToString();
				return -1;
			}
			try
			{
				do
				{
					 
					iReturn = SS728M05SSSE.ss_rf_sb_FindCard();
					Thread.Sleep(10);
					if (iReturn != 0)
					{
					   msg="上电错误！" + iReturn.ToString();
						return -1;
					}
				}
				while (iReturn != 0);
				//读发卡机构信息
				byte[] CardIdentifier = new byte[1024]; //识别码
				byte[] CardType = new byte[1024];       //卡类型
				byte[] CardVersion = new byte[1024];    //版本号
				byte[] IssuersID = new byte[1024];      //机构号
				byte[] IssuingDate = new byte[1024];    //发卡期
				byte[] EffectiveData = new byte[1024];  //有效期
				byte[] CardID = new byte[1024];         //卡号  
				iReturn = SS728M05SSSE.ss_rf_sb_ReadCardIssuers(CardIdentifier, CardType, CardVersion, IssuersID, IssuingDate, EffectiveData, CardID);
				if (iReturn < 0)
				{
					SS728M05Comom.ss_reader_close(deviceHandle);
					msg="读发卡机构信息失败！" + iReturn.ToString();
					return -1;
				}
				/******************************************
				 *    读发卡机构信息
				 * ****************************************/
			  sdsybCardinfo = new SDSYB_CardInfo();
				sdsybCardinfo.CardIdentifier = SetText(CardIdentifier);//识别码
				sdsybCardinfo.CardID = SetText(CardID); //卡号
				sdsybCardinfo.IssuersID = SetText(IssuersID);   //机构号
				sdsybCardinfo.CardType = SetText(CardType);   //卡类型
				sdsybCardinfo.EffectiveData = SetText(EffectiveData);   //有效期
				sdsybCardinfo.IssuingDate = SetText(IssuingDate);   //发卡期
				sdsybCardinfo.CardVersion = SetText(CardVersion);   //版本号

				/******************************************
				*    读持卡人信息
				* ****************************************/
				byte[] ID = new byte[1024];        //身份证号
				byte[] Name = new byte[1024];      //姓名
				byte[] Name_ = new byte[1024];     //姓名后缀
				byte[] Sex = new byte[2];          //性别
				byte[] Folk = new byte[512];       //民族
				byte[] Origin = new byte[1024];    //籍贯
				byte[] Birthday = new byte[8];     //出生日期
				iReturn = SS728M05SSSE.ss_rf_sb_ReadCardholder(ID, Name, Name_, Sex, Folk, Origin, Birthday);
				if (iReturn < 0)
				{
					SS728M05Comom.ss_reader_close(deviceHandle);
					msg="读持卡人信息失败！" + iReturn.ToString();
					return -1;
				}

				sdsybCardinfo.ID = SetText(ID);//身份证号
				sdsybCardinfo.Name = SetText(Name);//姓名
				sdsybCardinfo.Sex = SetText(Sex);//性别
				sdsybCardinfo.Birthday = SetText(Birthday);//出生日期
				sdsybCardinfo.Origin = SetText(Origin);//籍贯
				sdsybCardinfo.Folk = SetText(Folk);//民族
				SS728M05Comom.ss_reader_close(deviceHandle);
				msg="读卡成功！" + iReturn.ToString();
				 iReturn=0;
			}
			catch (Exception ex)
			{
				SS728M05Comom.ss_reader_close(deviceHandle);
				msg	 = ex.ToString();
				iReturn=-1;
			}
			 
			return 	iReturn;
}


			private static string SetText(byte[] content, int len = 0)
			{

				string data = "";
				for (int i = 0; i < len; i++)
				{
					//压缩转非压缩以下两种方法均可以
					//data += String.Format("{0:X2}", content[i]);
					data += content[i].ToString("X2");
				}
				if (len == 0)
				{
					data = Encoding.Default.GetString(content);
				}
				return data;
			}


	}
}
