using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HLReadCard
{
   public	class QDQYCardInfo
	{
	   
    //卡的ID
    public string Crad_Id { get; set; }
    //患者姓名
    public string Patient_Name { get; set; }
    //持卡人证件
    public string Patient_CardType { get; set; }
    //患者身份证号
    public string Patient_sfz { get; set; }
    //患者Id
    public string Patient_Id { get; set; }
    //平台卡号
    public string PT_CradId { get; set; }
    //院区代码
    public string Hospit_Code { get; set; }
    //平台患者流水号
    public string PTLS_Id { get; set; }
    //持卡人性别
    public string Patient_Sex { get; set; }
    //持卡人联系方式
    public string Patient_Tel { get; set; }
    //发行日期
    public string FX_Date { get; set; }
    //有效日期
    public string YX_Date { get; set; }
    //启用日期
    public string QY_Date { get; set; }
 
	}
}
