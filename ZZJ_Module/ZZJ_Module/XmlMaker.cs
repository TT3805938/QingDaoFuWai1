using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class XmlMaker
{
    private bool isHead;

    public static XmlMaker NewTabel(string Patid = "", string CardNo = "", int CardType = -1, bool ZZJBH = true)
    {
        XmlMaker Table = new XmlMaker(false);
				if (ZZJBH) Table.Add("czyh", ZZJCore.Public_Var.ZZJ_Config.ExtUserID);//His操作员号   ZZJCore.Public_Var.ZZJ_Config.ExtUserID
        if (!string.IsNullOrEmpty(CardNo)) Table.Add("cardno", CardNo);//卡号
        if (CardType > 0) Table.Add("cardtype", CardType);//卡类型
        if (!string.IsNullOrEmpty(Patid)) Table.Add("patid", Patid);//账户号
        return Table;
    }

		
		public static XmlMaker NewTabel1(string Patid = "", string CardNo = "", int CardType = -1, bool ZZJBH = true)
		{
			XmlMaker Table = new XmlMaker(false);
			if (ZZJBH) Table.Add("czyh", ZZJCore.Public_Var.ZZJ_Config.ExtUserID);//His操作员号
			return Table;
		}


    public static XmlMaker NewXML(string transcode, XmlMaker XM = null)
    {
        XmlMaker XML = new XmlMaker();
        XML.Add("transcode", transcode);//交易号
        if (XM != null) XML.AddNode("table", XM.ToString());
        return XML;
    }

    public XmlMaker(bool Head = true)
    {
        isHead = Head;
        XML = Head ?"<zzxt>\r\n" :"";
    }







    private string XML = "";
    /// <summary>
    /// 
    /// </summary>
    /// <param name="d"></param>
    /// <param name="value"></param>
    public void Add(string cli, Object value)
    {
			XML = XML + string.Format("<{0}>{1}</{0}>\r\n", cli, value.ToString());
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="d"></param>
    /// <param name="value"></param>
    public void AddNode(string cli, Object value)
    {
        XML = XML + "<" + cli + ">\r\n" + value.ToString() + "</" + cli + ">\r\n";
    }

    /// <summary>
    /// 清理数据创建头
    /// </summary>
    public void Clear(bool Head = true)
    {
        XML = Head ? "<zzxt>\r\n" : "";
        isHead = Head;
    }

    /// <summary>
    /// 获取数据创建尾
    /// </summary>
    /// <returns></returns>
    public string ToString()
    {
        return isHead ? XML + "</zzxt>" : XML;
    }
}
