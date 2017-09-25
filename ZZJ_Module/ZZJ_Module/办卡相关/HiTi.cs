using System;
using System.Text;
using HL.Devices.ZKJ;
using System.Drawing.Printing;
using System.Drawing;


namespace ZZJ_Module
{
  public static class EasyHiTi
  {
    public static string Errors = "";

    /// <summary>
    /// 读取卡号
    /// </summary>
    /// <returns></returns>
    public static int ReadCard()
    {
      #region 打开发卡器
      try
      {
        if (HiTi.Open(ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev, Convert.ToInt32(ZZJCore.Public_Var.ZZJ_Config.FKQ_Port.Substring(3))) != 0)
        {
          Errors = "打不开发卡器,请换其他机器办理!";
          ZZJCore.SuanFa.Proc.Log(string.Format("打开发卡器失败,发卡器:{0},端口:{1}", ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev, ZZJCore.Public_Var.ZZJ_Config.FKQ_Port));
          return -1;
        }
      }
      catch (Exception e)
      {
        ZZJCore.SuanFa.Proc.Log(e);
        Errors = "打不开发卡器,请换其他机器办理!";
        ZZJCore.SuanFa.Proc.Log("打开发卡器失败," + e.Message);
        return -1;
      }
      #endregion

      #region 走卡
      uint CardPosition = 0;
      ZKJ_Dev.GetCardPosition(ref CardPosition);
      if (HiTi.EnterCrad() != 0)
      {
        Errors = "取卡失败,请换其他机器办理!";
        ZZJCore.SuanFa.Proc.Log("走卡失败1次");
        HiTi.Close();
        return -1;
      }
      #endregion

      ZZJCore.SuanFa.Proc.sleep(500);

      #region 读卡
      string xlh = "";
      string kwyh = "";
      HiTi.VerifyPassword("7E8FE85C7142");

      if (HiTi.ReadCardNo(out xlh, out kwyh) != 0)
      {
        Errors = "读卡失败,请稍后再试!";
        ZZJCore.SuanFa.Proc.Log("读卡失败");
        HiTi.Close();
        return -1;
      }
      #endregion

      ZZJCore.Public_Var.cardInfo.CardNo = kwyh;

      HiTi.Close();
      return 0;
    }

    /// <summary>
    /// 写卡并且出卡
    /// </summary>
    /// <returns></returns>
    public static int WriterCardAndOutCard()
    {
      #region 准备数据
      CardInfo info = new CardInfo();
      //平台ID
      info.PT_CradId = Public_Var.PF.patientCard;
      //平台流水ID
      info.PTLS_Id = Public_Var.PF.patientId;
      //患者ID
      info.Patient_Id = info.PTLS_Id;
      //院区ID
      info.Hospit_Code = ZZJCore.Public_Var.ZZJ_Config.YQCode;
      #endregion

      #region 打开发卡器
      try
      {
        if (HiTi.Open(ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev, Convert.ToInt32(ZZJCore.Public_Var.ZZJ_Config.FKQ_Port.Substring(3))) != 0)
        {
          Errors = "打不开发卡器,请换其他机器办理!";
          ZZJCore.SuanFa.Proc.Log(string.Format("打开发卡器失败,发卡器:{0},端口:{1}", ZZJCore.Public_Var.ZZJ_Config.FKQ_Dev, ZZJCore.Public_Var.ZZJ_Config.FKQ_Port));
          return -1;
        }
      }
      catch (Exception e)
      {
        ZZJCore.SuanFa.Proc.Log(e);
        Errors = "打不开发卡器,请换其他机器办理!";
        ZZJCore.SuanFa.Proc.Log("打开发卡器失败," + e.Message);
        return -1;
      }
      #endregion

      //写入数据 验证密码

      HiTi.VerifyPassword("7E8FE85C7142");
      int Iret = HiTi.WriteCardData(info, ref Errors);
      if (!string.IsNullOrEmpty(Errors)) ZZJCore.SuanFa.Proc.Log(Errors);
      HiTi.Close();
      return Iret;
    }
  }

  public static class HiTi
  {
    private static bool b_init = false;
    private static byte[] Key_Code = null;

    public static byte[] HexConvert(string hex)
    {
      hex = hex.ToUpper();
      //int len = hex.Length / 2;
      //if (len * 2 < hex.Length) len++;

      int len = (hex.Length / 2);
      byte[] result = new byte[len];
      char[] achar = hex.ToCharArray();
      string by = "";
      for (int i = 0; i < len; i++)
      {
        int pos = i * 2;
        by = new string(new char[] { achar[pos], achar[pos + 1] });
        result[i] = Convert.ToByte(by, 16);
      }
      return result;
    }

    /// <summary>
    /// 打开设备
    /// </summary>
    /// <param name="devname">设备名</param>
    /// <param name="port">端口</param>
    /// <param name="baud">波特率</param>
    /// <returns></returns>
    public static int Open(string devname = "HiTi CS-200e", int port = 11, int baud = 9600)
    {
      if (b_init) ZKJ_Dev.CloseDev();
      int ir = ZKJ_Dev.InitDev(devname, 1, port, baud);
      b_init = (ir == 0);
      return ir;
    }

    public static int VerifyPassword(string Key)
    {
      Key_Code = HexConvert(Key);
      // return ZKJ_Dev.Authentication_passaddr(Key_Code);
      return 0;
    }

    /// <summary>
    /// 读取卡号
    /// </summary>
    /// <param name="xlh">返回16进制序列号</param>
    /// <param name="kwyh">返回十进制卡号</param>
    /// <returns></returns>
    public static int ReadCardNo(out string xlh, out string kwyh)
    {
      string Msg = "";
      byte[] Data = new byte[16];
      //bool Iret = ZKJ_Dev.M1_ReadCard(ref Data, 0, ref Msg,0);
      int Iret = ZKJ_Dev.M1_ReadData(0, 0, M1Key_Mode.KEYSET0_KEYA, Key_Code, ref Data, out Msg);
      byte[] item = new byte[4];
      item[0] = Data[3];
      item[1] = Data[2];
      item[2] = Data[1];
      item[3] = Data[0];
      //卡的ID
      xlh = ByteToHexString(item, 4);
      kwyh = Convert.ToInt64(xlh, 16).ToString();
      return Iret;
    }

    //将字节数组转换成十六进制字符串
    private static string ByteToHexString(byte[] bytes, int Count)
    {
      StringBuilder result = new StringBuilder();
      for (int i = 0; i < Count; i++)
      {
        result.Append(bytes[i].ToString("X2"));
      }
      return result.ToString();
    }

    /// <summary>
    /// 新卡改密码  
    /// </summary>
    /// <param name="oldKey">新卡默认FFFFFFFFFFFF</param>
    /// <param name="newKey"></param>
    /// <param name="Msg"></param>
    /// <returns></returns>
    public static int NewCardModPassWord(string oldKey, string newKey, out string Msg)
    {
      byte[] Key_CodeOld = HexConvert(oldKey);
      byte[] Key_CodeNew = HexConvert(newKey);
      ZKJ_Dev.M1_UpDatePassWordA(0, M1Key_Mode.KEYSET0_KEYA, Key_CodeOld, Key_CodeNew, out Msg);
      ZKJ_Dev.M1_UpDatePassWordA(1, M1Key_Mode.KEYSET0_KEYA, Key_CodeOld, Key_CodeNew, out Msg);
      ZKJ_Dev.M1_UpDatePassWordA(2, M1Key_Mode.KEYSET0_KEYA, Key_CodeOld, Key_CodeNew, out Msg);
      ZKJ_Dev.M1_UpDatePassWordA(3, M1Key_Mode.KEYSET0_KEYA, Key_CodeOld, Key_CodeNew, out Msg);
      ZKJ_Dev.M1_UpDatePassWordA(4, M1Key_Mode.KEYSET0_KEYA, Key_CodeOld, Key_CodeNew, out Msg);
      ZKJ_Dev.M1_UpDatePassWordA(5, M1Key_Mode.KEYSET0_KEYA, Key_CodeOld, Key_CodeNew, out Msg);
      return 0;
    }

    /// <summary>
    /// 关闭设备
    /// </summary>
    public static void Close()
    {
      if (b_init) ZKJ_Dev.CloseDev();
    }

    /// <summary>
    /// 出卡
    /// </summary>
    /// <returns></returns>
    public static bool OutCard(ref string Msg)
    {
      /// 1，接触式IC卡模块位置
      /// 2，写磁模块位置
      /// 3，非接触式IC卡模块位置
      /// 4，错误卡排出口
      /// 5，出卡槽
      return ZKJ_Dev.Out_Card(ref Msg);
    }

    /// <summary>
    /// 进卡
    /// </summary>
    /// <returns></returns>
    public static int EnterCrad()
    {
      /// 1，接触式IC卡模块位置
      /// 2，写磁模块位置
      /// 3，非接触式IC卡模块位置
      /// 4，错误卡排出口
      /// 5，出卡槽
      return ZKJ_Dev.MoveCard(3);
    }

    public static int GetCardPosition(ref uint cradpoint)
    {
      return ZKJ_Dev.GetCardPosition(ref cradpoint);
    }

    /// <summary>
    /// 写卡
    /// </summary>
    /// <param name="info">数据类</param>
    /// <param name="Msg">错误消息</param>
    /// <returns>返回0成功</returns>
    public static int WriteCardData(CardInfo info, ref string Msg)
    {
      #region 声明变量
      int Iret = 0;
      //0快数据
      byte[] Data0 = new byte[16];
      //1快数据
      byte[] Data1 = new byte[16];
      //2快数据
      byte[] Data2 = new byte[16];
      //临时缓存
      byte[] Temp = new byte[16];
      //日期转换
      DateTime TmpDate = DateTime.Now;
      //校验
      int sum = 0;
      #endregion

      #region 扇区0

      //块0卡号 数据 (不可写)
      //组建块1的数据
      Data1 = new byte[] { 00, 01, 02, 03, 04, 05, 0xFF, 0xFF, 0xFF };
      Iret = ZKJ_Dev.M1_WriteCard(0, 1, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data1, out Msg);
      if (Iret != 0) return Iret;
      //块2数据保留
      #endregion

      #region 扇区1
      //0快数据
      Data0 = new byte[16];
      //1快数据
      Data1 = new byte[16];
      //快0和块1 为 持卡人姓名
      //转换姓名为Unicode编码
      Temp = Encoding.Unicode.GetBytes(ZZJCore.Public_Var.patientInfo.PatientName);
      //填充进去
      Array.Copy(Temp, 0, Data0, 0, Temp.Length < 16 ? Temp.Length : 16);
      //写入块0
      Iret = ZKJ_Dev.M1_WriteCard(1, 0, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data0, out Msg);
      if (Iret != 0) return Iret;
      //名字长度大16字节 需呀写入块1
      if (Temp.Length > 16)
      {
        //
        Array.Copy(Temp, 16, Data1, 0, Temp.Length - 16);
        //写入块1
        Iret = ZKJ_Dev.M1_WriteCard(1, 1, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data1, out Msg);
        if (Iret != 0) return Iret;
      }
      #endregion

      #region 扇区2
      Data0 = new byte[16];
      //1快数据
      Data1 = new byte[16];
      //身份证号码
      Temp = Encoding.ASCII.GetBytes(ZZJCore.Public_Var.patientInfo.IDNo);
      Array.Copy(Temp, 0, Data0, 0, Temp.Length < 16 ? Temp.Length : 16);
      //写入块0
      Iret = ZKJ_Dev.M1_WriteCard(2, 0, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data0, out Msg);
      if (Iret != 0) return Iret;
      //剩余字节写入块2
      if (Temp.Length == 18)
      {
        //
        Array.Copy(Temp, 15, Data1, 0, 3);
        //写入块1
        Iret = ZKJ_Dev.M1_WriteCard(2, 1, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data1, out Msg);
        if (Iret != 0) return Iret;
      }
      #endregion

      #region 扇区3
      Data0 = new byte[16];
      //
      Temp = Encoding.ASCII.GetBytes(info.Patient_Id);
      //填充
      Array.Copy(Temp, 0, Data0, 0, Temp.Length > 10 ? 10 : Temp.Length);
      //写入
      Iret = ZKJ_Dev.M1_WriteCard(3, 0, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data0, out Msg);
      if (Iret != 0) return Iret;
      #endregion

      #region 扇区4
      Data0 = new byte[16];
      //1快数据
      Data1 = new byte[16];
      //2快数据
      Data2 = new byte[16];


      //******平台卡号转BCD
      Temp = IntStrToBcd(info.PT_CradId);
      //从后面填充 
      Array.Copy(Temp, 0, Data0, 9 - Temp.Length, Temp.Length);

      //******院区代码转BCD 5字节10位
      Temp = IntStrToBcd(info.Hospit_Code);
      //填充第一块 前半部分
      Array.Copy(Temp, 0, Data0, 14 - Temp.Length, Temp.Length > 5 ? 5 : Temp.Length);
      //启用标志 2启用
      Data0[14] = 2;

      // 发行日期
      TmpDate = Convert.ToDateTime(info.FX_Date);
      Temp = HiTi.DateTimeToBCD(TmpDate, 4);
      Array.Copy(Temp, 0, Data1, 0, 4);

      //有效日期
      TmpDate = Convert.ToDateTime(info.YX_Date);
      Temp = HiTi.DateTimeToBCD(TmpDate, 4);
      Array.Copy(Temp, 0, Data1, 4, 4);

      //启用日期
      TmpDate = Convert.ToDateTime(info.QY_Date);
      Temp = HiTi.DateTimeToBCD(TmpDate, 4);
      Array.Copy(Temp, 0, Data1, 8, 4);

      //******平台患者流水号转BCD
      Temp = IntStrToBcd(info.PTLS_Id);
      //从后面填充
      Array.Copy(Temp, 0, Data2, 15 - Temp.Length, Temp.Length);

      //计算块0校验
      sum = CheckSum(Data0, 15);
      Temp = IntStrToBcd(sum.ToString());
      Data0[15] = Temp[Temp.Length - 1];

      //计算块1块2校验
      sum = CheckSum(Data1, 15);
      Temp = IntStrToBcd(sum.ToString());
      Data1[15] = Temp[Temp.Length - 1];

      sum = CheckSum(Data2, 15);
      Temp = IntStrToBcd(sum.ToString());
      Data2[15] = Temp[Temp.Length - 1];
      //写入0块
      Iret = ZKJ_Dev.M1_WriteCard(4, 0, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data0, out Msg);
      if (Iret != 0) return Iret;
      //写入1块
      Iret = ZKJ_Dev.M1_WriteCard(4, 1, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data1, out Msg);
      if (Iret != 0) return Iret;
      //写入2块
      Iret = ZKJ_Dev.M1_WriteCard(4, 2, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data2, out Msg);
      if (Iret != 0) return Iret;
      #endregion

      #region 扇区5
      Data0 = new byte[16];
      //1快数据
      Data1 = new byte[16];
      //2快数据
      Data2 = new byte[16];
      //块0 持卡人姓名 GB2312码格式
      Temp = Encoding.GetEncoding("GB2312").GetBytes(ZZJCore.Public_Var.patientInfo.PatientName);
      //填充
      Array.Copy(Temp, 0, Data0, 0, Temp.Length > 14 ? 14 : Temp.Length);
      //性别,,
      Data0[14] = 0;
      if (ZZJCore.Public_Var.patientInfo.Sex == "男") Data0[14] = 1;
      if (ZZJCore.Public_Var.patientInfo.Sex == "女") Data0[14] = 2;
      //块1 证件类型
      Data1[0] = 0;
      //块1 证件类型
      Data1[1] = byte.Parse(info.Patient_CardType);
      byte[] cache = new byte[18];
      //证件号转ASCII
      Temp = Encoding.ASCII.GetBytes(ZZJCore.Public_Var.patientInfo.IDNo);
      Array.Copy(Temp, 0, cache, 0, Temp.Length > 18 ? 18 : Temp.Length);
      //填充前15位身份证
      Array.Copy(cache, 0, Data1, 2, 14);
      //填充 块2 后3位
      Array.Copy(cache, 14, Data2, 0, 4);

      //联系方式ASCCII
      Temp = Encoding.ASCII.GetBytes(ZZJCore.Public_Var.patientInfo.Mobile);
      //填充块2 联系地址
      Array.Copy(Temp, 0, Data2, 4, Temp.Length < 11 ? Temp.Length : 11);
      //计算块0校验
      sum = CheckSum(Data0, 15);
      Temp = IntStrToBcd(sum.ToString());
      Data0[15] = Temp[Temp.Length - 1];
      //计算块1块2校验
      sum = CheckSum(Data1, 16);
      sum += CheckSum(Data2, 15);
      Temp = IntStrToBcd(sum.ToString());
      Data2[15] = Temp[Temp.Length - 1];
      //写入0块
      Iret = ZKJ_Dev.M1_WriteCard(5, 0, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data0, out Msg);
      if (Iret != 0) return Iret;
      //写入1块
      Iret = ZKJ_Dev.M1_WriteCard(5, 1, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data1, out Msg);
      if (Iret != 0) return Iret;
      //写入2块
      Iret = ZKJ_Dev.M1_WriteCard(5, 2, M1Key_Mode.KEYSET0_KEYA, Key_Code, Data2, out Msg);
      if (Iret != 0) return Iret;
      #endregion

      return 0;
    }

    /// <summary>
    /// 计算半加和校验码
    /// </summary>
    /// <param name="bys"></param>
    /// <param name="Length"></param>
    /// <returns></returns>
    public static int CheckSum(byte[] bys, int Length)
    {
      int Ret = 0;
      byte item;
      for (int i = 0; i < Length; i++)
      {
        item = bys[i];
        Ret += (item >> 4) + (item & 0x0f);
      }
      return Ret;
    }

    private static uint HornerScheme(uint num, uint divider, uint factor)
    {
      uint remainder = 0, quotient = 0, result = 0;
      remainder = num % divider;
      quotient = num / divider;
      if (!(quotient == 0 && remainder == 0))
        result += HornerScheme(quotient, divider, factor) * factor + remainder;
      return result;
    }

    /// <summary>
    /// Converts from binary coded decimal to integer
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static uint BcdToDec(uint num)
    {
      return HornerScheme(num, 0x10, 10);
    }

    /// <summary>
    /// Converts from integer to binary coded decimal
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static uint DecToBcd(uint num)
    {
      return HornerScheme(num, 10, 0x10);
    }

    /// <summary>
    /// 日期转BCD数组
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="type">4 6 7</param>
    /// <returns></returns>
    public static byte[] DateTimeToBCD(DateTime dateTime, ushort type)
    {
      string strServerTime = string.Format("{0:yyyyMMddHHmmss}", dateTime);

      byte[] bcd = new byte[type];
      if (type == 4)
      {
        bcd[0] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(0, 2))).ToString("D2"));
        bcd[1] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(2, 2))).ToString("D2"));
        bcd[2] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(4, 2))).ToString("D2"));
        bcd[3] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(6, 2))).ToString("D2"));
      }
      if (type == 6)
      {
        bcd[0] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(2, 2))).ToString("D2"));
        bcd[1] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(4, 2))).ToString("D2"));
        bcd[2] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(6, 2))).ToString("D2"));
        bcd[3] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(8, 2))).ToString("D2"));
        bcd[4] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(10, 2))).ToString("D2"));
        bcd[5] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(12, 2))).ToString("D2"));
      }
      if (type == 7)
      {
        bcd[0] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(0, 2))).ToString("D2"));
        bcd[1] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(2, 2))).ToString("D2"));
        bcd[2] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(4, 2))).ToString("D2"));
        bcd[3] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(6, 2))).ToString("D2"));
        bcd[4] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(8, 2))).ToString("D2"));
        bcd[5] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(10, 2))).ToString("D2"));
        bcd[5] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(12, 2))).ToString("D2"));
      }
      return bcd;
    }

    /// <summary>
    /// BCD时间转日期时间
    /// </summary>
    /// <param name="bcdTime"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static DateTime BCDToDateTime(byte[] bcdTime, ushort type)
    {
      StringBuilder sb = new StringBuilder();
      if (type == 4) //4位BCD码的日期
      {
        sb.Append(BcdToDec(bcdTime[0]).ToString("D2"));
        sb.Append(BcdToDec(bcdTime[1]).ToString("D2"));
        sb.Append('-' + BcdToDec(bcdTime[2]).ToString("D2"));
        sb.Append('-' + BcdToDec(bcdTime[3]).ToString("D2") + " ");
      }
      if (type == 6) //6位BCD码的时间
      {
        sb.Append(DateTime.Now.ToString("yyyy").Substring(0, 2));
        sb.Append(BcdToDec(bcdTime[0]).ToString("D2"));
        sb.Append('-' + BcdToDec(bcdTime[1]).ToString("D2"));
        sb.Append('-' + BcdToDec(bcdTime[2]).ToString("D2") + " ");
        sb.Append(BcdToDec(bcdTime[3]).ToString("D2") + ":");
        sb.Append(BcdToDec(bcdTime[4]).ToString("D2") + ":");
        sb.Append(BcdToDec(bcdTime[5]));
      }
      if (type == 7) //7位BCD码的日期
      {
        sb.Append(BcdToDec(bcdTime[0]).ToString("D2"));
        sb.Append(BcdToDec(bcdTime[1]).ToString("D2"));
        sb.Append('-' + BcdToDec(bcdTime[2]).ToString("D2"));
        sb.Append('-' + BcdToDec(bcdTime[3]).ToString("D2") + " ");
        sb.Append(BcdToDec(bcdTime[4]).ToString("D2") + ":");
        sb.Append(BcdToDec(bcdTime[5]).ToString("D2") + ":");
        sb.Append(BcdToDec(bcdTime[6]));
      }

      DateTime dt;
      //2011-3-26 当日期出错时的处理
      DateTime.TryParse(sb.ToString(), out dt);

      return dt;
    }

    /// <summary>
    /// 10进制串转为BCD码 
    /// </summary>
    /// <param name="asc">10进制串 </param>
    /// <returns>BCD码 </returns>
    public static byte[] IntStrToBcd(String asc)
    {
      int len = asc.Length;
      int mod = len % 2;

      if (mod != 0)
      {
        asc = "0" + asc;
        len = asc.Length;
      }

      byte[] abt = new byte[len];
      if (len >= 2)
      {
        len = len / 2;
      }

      byte[] bbt = new byte[len];
      abt = System.Text.Encoding.ASCII.GetBytes(asc);
      int j, k;

      for (int p = 0; p < asc.Length / 2; p++)
      {
        if ((abt[2 * p] >= '0') && (abt[2 * p] <= '9'))
        {
          j = abt[2 * p] - '0';
        }
        else if ((abt[2 * p] >= 'a') && (abt[2 * p] <= 'z'))
        {
          j = abt[2 * p] - 'a' + 0x0a;
        }
        else
        {
          j = abt[2 * p] - 'A' + 0x0a;
        }

        if ((abt[2 * p + 1] >= '0') && (abt[2 * p + 1] <= '9'))
        {
          k = abt[2 * p + 1] - '0';
        }
        else if ((abt[2 * p + 1] >= 'a') && (abt[2 * p + 1] <= 'z'))
        {
          k = abt[2 * p + 1] - 'a' + 0x0a;
        }
        else
        {
          k = abt[2 * p + 1] - 'A' + 0x0a;
        }

        int a = (j << 4) + k;
        byte b = (byte)a;
        bbt[p] = b;
      }
      return bbt;
    }
  }

}
