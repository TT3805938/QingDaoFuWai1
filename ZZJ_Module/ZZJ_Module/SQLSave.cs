using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Mw_MSSQL;
using System.Collections;
using System.Data;

namespace ZZJ_Module
{
    public class SQLSave
    {
        private static bool Init_DBConfig()//加载数据库
        {
            try
            {
                int d = MSSQL_Init.Init_DBConfig();
                ZZJCore.SuanFa.Proc.Log("->加载数据库->配置文件加载" + (d == 0 ? "成功" : "失败") + "!");//加载配置文件失败
                return d == 0;
            }
            catch (Exception ex)
            {
                ZZJCore.SuanFa.Proc.Log("初始化数据库时出现致命错误");
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        ////#region 挂号入库
        private static SqlParameter AddGH(SqlDbType SDT, string PName, object Value)
        {
            SqlParameter parameter = new SqlParameter();
            parameter.SqlDbType = SDT;
            parameter.ParameterName = PName;
            parameter.Value = Value;
            return (parameter);
        }
        public static void SaveDRGH(bool iOk, bool zfflag, string regFee, string DoctorName, string DeptName)//挂号入库
        {
            string msg = "";
            ////#region 网络数据库
            if (!Init_DBConfig())
            {
                return;
            }
            try
            {
                //如果是社保 支付 或 银联支付 要保存到数据库
                //if (GHZFFS == 4 || GHZFFS == 2) ZZJ_Module.ChargeClass.SaveData(GHBZ);
                //..........
                MSSQL_Operate mssql_operate = new MSSQL_Operate();
                SqlParameter[] parameter = null;
                ArrayList AL = new ArrayList();
                //挂号记录        

                //--就诊卡号
                AL.Add(AddGH(SqlDbType.VarChar, "@KH", ZZJCore.Public_Var.cardInfo.CardNo));
                //登记号
                AL.Add(AddGH(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.patientInfo.PatientID));
                //卡片类型
                AL.Add(AddGH(SqlDbType.VarChar, "@KPLX", ZZJCore.Public_Var.cardInfo.CardType));
                //姓名
                AL.Add(AddGH(SqlDbType.VarChar, "@XM", ZZJCore.Public_Var.patientInfo.PatientName));
                //HIS操作员
                AL.Add(AddGH(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));
                //--zzj编号
                AL.Add(AddGH(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));
                //挂号时间
                AL.Add(AddGH(SqlDbType.DateTime, "@GHSJ", DateTime.Now.ToString()));
                //就诊时间
                AL.Add(AddGH(SqlDbType.VarChar, "@JZSJ", DateTime.Now.ToString("yyyy-MM-dd")));
                //总金额(本次费用发生总金额)
                AL.Add(AddGH(SqlDbType.VarChar, "@JE", regFee));
                //医生姓名
                //if () regDoctName = "普通";
                AL.Add(AddGH(SqlDbType.VarChar, "@YSXM", DoctorName));
                //科室名称
                AL.Add(AddGH(SqlDbType.VarChar, "@KS", DeptName));
                //医生级别
                AL.Add(AddGH(SqlDbType.VarChar, "@YSJB", ""));//DoctorTitle
                //挂号标志
                AL.Add(AddGH(SqlDbType.VarChar, "@GHBZ", iOk ? "成功" : "失败"));
                //自助机流水号
                AL.Add(AddGH(SqlDbType.VarChar, "@LSH", ZZJCore.Public_Var.SerialNumber));
                //支付方式
                AL.Add(AddGH(SqlDbType.Int, "@ZFFS", 0));
                //支付金额
                AL.Add(AddGH(SqlDbType.Decimal, "@ZFJE", regFee));
                //支付结果
                AL.Add(AddGH(SqlDbType.Int, "@ZFJG", zfflag ? 0 : -1));
                //支付结果描述
                AL.Add(AddGH(SqlDbType.VarChar, "@ZFJGMS", ""));
                //支付流水号
                //附加
                AL.Add(AddGH(SqlDbType.VarChar, "@BL1", ""));
                //附加
                AL.Add(AddGH(SqlDbType.VarChar, "@BL2", "挂号"));
                //挂号Id序列号
                AL.Add(AddGH(SqlDbType.VarChar, "@BL3", ""));
                //自助机流水号
                AL.Add(AddGH(SqlDbType.VarChar, "@ZZJLSH", ZZJCore.Public_Var.SerialNumber));
                //自助机流水号
                AL.Add(AddGH(SqlDbType.VarChar, "@ZFLSH", ZZJCore.Public_Var.SerialNumber));

                parameter = (SqlParameter[])AL.ToArray(typeof(SqlParameter));
                if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_MZ_DRGH_MX_V4]", parameter, out msg) > 0)
                {
                    ZZJCore.SuanFa.Proc.Log("->挂号记录写入服务器成功");
                    return;
                }
                ZZJCore.SuanFa.Proc.Log("->挂号记录写入服务器失败" + msg);
            }
            catch (Exception Ex)
            {
                ZZJCore.SuanFa.Proc.Log("->挂号记录写入服务器失败，可能是网络中断或远程自助机服务器未开启，");
                ZZJCore.SuanFa.Proc.Log(Ex);
            }
            ////#endregion
        }

        public static void SaveYYQH(bool iOk, bool zfflag, string regFee, string DoctorName, string DeptName, string DoctorTitle, string scheduleId)//挂号入库
        {
            string msg = "";
            ////#region 网络数据库
            if (!Init_DBConfig())
            {
                return;
            }
            try
            {
                //如果是社保 支付 或 银联支付 要保存到数据库
                //if (GHZFFS == 4 || GHZFFS == 2) ZZJ_Module.ChargeClass.SaveData(GHBZ);
                //..........
                MSSQL_Operate mssql_operate = new MSSQL_Operate();
                SqlParameter[] parameter = null;
                ArrayList AL = new ArrayList();
                //挂号记录        

                //--就诊卡号
                AL.Add(AddGH(SqlDbType.VarChar, "@KH", ZZJCore.Public_Var.cardInfo.CardNo));
                //登记号
                AL.Add(AddGH(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.patientInfo.PatientID));
                //卡片类型
                AL.Add(AddGH(SqlDbType.VarChar, "@KPLX", ZZJCore.Public_Var.cardInfo.CardType));
                //姓名
                AL.Add(AddGH(SqlDbType.VarChar, "@XM", ZZJCore.Public_Var.patientInfo.PatientName));
                //HIS操作员
                AL.Add(AddGH(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));
                //--zzj编号
                AL.Add(AddGH(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));
                //挂号时间
                AL.Add(AddGH(SqlDbType.DateTime, "@GHSJ", DateTime.Now.ToString()));
                //就诊时间
                AL.Add(AddGH(SqlDbType.VarChar, "@JZSJ", DateTime.Now.ToString("yyyy-MM-dd")));
                //总金额(本次费用发生总金额)
                AL.Add(AddGH(SqlDbType.VarChar, "@JE", regFee));
                //医生姓名
                //if () regDoctName = "普通";
                AL.Add(AddGH(SqlDbType.VarChar, "@YSXM", DoctorName));
                //科室名称
                AL.Add(AddGH(SqlDbType.VarChar, "@KS", DeptName));
                //医生级别
                AL.Add(AddGH(SqlDbType.VarChar, "@YSJB", DoctorTitle));
                //挂号标志
                AL.Add(AddGH(SqlDbType.VarChar, "@GHBZ", iOk ? "成功" : "失败"));
                //自助机流水号
                AL.Add(AddGH(SqlDbType.VarChar, "@LSH", ZZJCore.Public_Var.SerialNumber));
                //支付方式
                AL.Add(AddGH(SqlDbType.Int, "@ZFFS", "预交金"));
                //支付金额
                AL.Add(AddGH(SqlDbType.Decimal, "@ZFJE", regFee));
                //支付结果
                AL.Add(AddGH(SqlDbType.Int, "@ZFJG", zfflag));
                //支付结果描述
                AL.Add(AddGH(SqlDbType.VarChar, "@ZFJGMS", ""));
                //支付流水号
                //附加
                AL.Add(AddGH(SqlDbType.VarChar, "@BL1", ""));
                //附加
                AL.Add(AddGH(SqlDbType.VarChar, "@BL2", "挂号"));
                //挂号Id序列号
                AL.Add(AddGH(SqlDbType.VarChar, "@BL3", scheduleId));
                //自助机流水号
                AL.Add(AddGH(SqlDbType.VarChar, "@ZZJLSH", ZZJCore.Public_Var.SerialNumber));
                //自助机流水号
                AL.Add(AddGH(SqlDbType.VarChar, "@ZFLSH", ZZJCore.Public_Var.SerialNumber));

                parameter = (SqlParameter[])AL.ToArray(typeof(SqlParameter));
                if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_MZ_DRGH_MX_V4]", parameter, out msg) > 0)
                {
                    ZZJCore.SuanFa.Proc.Log("->挂号记录写入服务器成功");
                    return;
                }
                ZZJCore.SuanFa.Proc.Log("->挂号记录写入服务器失败" + msg);
            }
            catch (Exception Ex)
            {
                ZZJCore.SuanFa.Proc.Log("->挂号记录写入服务器失败，可能是网络中断或远程自助机服务器未开启，");
                ZZJCore.SuanFa.Proc.Log(Ex);
            }
            ////#endregion
        }

        public static void SaveYYGH(bool iOk, string meDate, string regFee, string DoctorName, string DeptName, string DoctorTitle, string scheduleId)//挂号入库
        {
            string msg = "";
            Init_DBConfig();
            try
            {
                //..........
                MSSQL_Operate mssql_operate = new MSSQL_Operate();
                SqlParameter[] parameter = null;
                ArrayList AL = new ArrayList();
                //挂号记录
                //--就诊卡号
                AL.Add(AddGH(SqlDbType.VarChar, "@KH", ZZJCore.Public_Var.cardInfo.CardNo));
                //账户号
                AL.Add(AddGH(SqlDbType.VarChar, "@DJH", ZZJCore.Public_Var.patientInfo.PatientID));
                //自助机流水号
                AL.Add(AddGH(SqlDbType.VarChar, "@ZZJLSH", ZZJCore.Public_Var.SerialNumber));
                //卡片类型
                AL.Add(AddGH(SqlDbType.VarChar, "@KPLX", ZZJCore.Public_Var.cardInfo.CardType));
                //姓名
                AL.Add(AddGH(SqlDbType.VarChar, "@XM ", ZZJCore.Public_Var.patientInfo.PatientName));
                //HIS操作员
                AL.Add(AddGH(SqlDbType.VarChar, "@HisCZY", ZZJCore.Public_Var.ZZJ_Config.ExtUserID));
                //--zzj编号
                AL.Add(AddGH(SqlDbType.VarChar, "@ZZJBH", ZZJCore.Public_Var.ZZJ_Config.TerminalID));
                //手机号
                AL.Add(AddGH(SqlDbType.VarChar, "@SJH", ZZJCore.Public_Var.patientInfo.Mobile));
                //时间
                AL.Add(AddGH(SqlDbType.DateTime, "@SJ", DateTime.Now.ToString()));
                //就诊时间
                AL.Add(AddGH(SqlDbType.VarChar, "@YYJZSJ", meDate));
                //总金额(本次费用发生总金额)
                AL.Add(AddGH(SqlDbType.VarChar, "@JE", regFee));
                //医生姓名
                AL.Add(AddGH(SqlDbType.VarChar, "@YSXM", DoctorName));
                //科室名称
                AL.Add(AddGH(SqlDbType.VarChar, "@KS", DeptName));
                //医生级别
                AL.Add(AddGH(SqlDbType.VarChar, "@YSJB", DoctorTitle));
                //预约标志 0 成功 非0失败
                AL.Add(AddGH(SqlDbType.VarChar, "@YYBZ", iOk ? "0" : "1"));
                //支付方式
                AL.Add(AddGH(SqlDbType.Int, "@ZFFS", "0"));
                //支付金额
                AL.Add(AddGH(SqlDbType.Decimal, "@ZFJE", "0.00"));
                //支付结果
                AL.Add(AddGH(SqlDbType.Int, "@ZFJG", "1"));
                //支付结果描述
                AL.Add(AddGH(SqlDbType.VarChar, "@ZFJGMS", ""));
                //支付流水号
                AL.Add(AddGH(SqlDbType.VarChar, "@ZFLSH ", ""));
                //附加
                AL.Add(AddGH(SqlDbType.VarChar, "@BL1", scheduleId));
                //附加
                AL.Add(AddGH(SqlDbType.VarChar, "@BL2", scheduleId));

                //@KH  varchar(50), --卡号
                //@DJH  varchar(50),  --账户号
                //@ZZJLSH  varchar(50),--自助机流水号
                //@KPLX varchar(50),-- 卡片类型
                //@XM  varchar(50),--姓名
                //@HisCZY varchar(50),--HIS操作员
                //@ZZJBH varchar(50),--自助机编号
                //@SJ datetime, --时间
                //@YYJZSJ datetime,--预约就诊时间
                //@JE varchar(50), --金额
                //@YSXM varchar(50),--医生姓名
                //@KS varchar(50), --科室
                //@YSJB varchar(50), --医生级别
                //@YYBZ varchar(50),--预约标志（0，预约成功，非0，失败）
                //@ZFFS tinyint,   --支付方式 (1 ：院内预交金； 2：社卡保金； 3： 区域预交金； 4：银联卡）
                //@ZFJE decimal(18,2),--支付金额 
                //@ZFJG int, -- 支付结果    (-1：支付失败；0 ：支付成功；1： 未支付）
                //@ZFJGMS varchar(50),--支付结果描述
                //@ZFLSH varchar(50),--支付流水号
                //@BL1 varchar(50), --保留1
                //@BL2 varchar(50) --保留2
                //附加
                //AL.Add(AddGH(SqlDbType.VarChar, "@YYJZSJ", GHID));
                //交易流水号
                //	AL.Add(AddGH(SqlDbType.VarChar, "@LSH", ZZJCore.Public_Var.SerialNumber));
                //挂号Id序列号
                //	AL.Add(AddGH(SqlDbType.VarChar, "@BL3", id));
                parameter = (SqlParameter[])AL.ToArray(typeof(SqlParameter));
                if (mssql_operate.NonQuery_OperateSqlProc("[sp_Insert_Tb_His_MZ_YYGH_MX]", parameter, out msg) > 0)
                {
                    ZZJCore.SuanFa.Proc.Log("->预约挂号记录写入服务器成功");
                    return;
                }
                ZZJCore.SuanFa.Proc.Log("->预约挂号记录写入服务器失败" + msg);
            }
            catch (Exception Ex)
            {
                ZZJCore.SuanFa.Proc.Log("->预约挂号记录写入服务器失败，可能是网络中断或远程自助机服务器未开启，");
                ZZJCore.SuanFa.Proc.Log(Ex);
            }
        }

    }
}

