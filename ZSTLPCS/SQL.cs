using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QLib;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace ZSTJPCS
{
    public static class SQL
    {
        public const string DBIP = "172.16.200.200";

        public static Class_SQL SQLConnect = new Class_SQL_Oracle(DBIP, 1521, "zsdl", "rlm2s", "rlm2s998", "SQLConnect");

        /// <summary>从数据库中查询配置文件
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool GetConfig(List<string> enableip, out string err)
        {
            string sql = "select * from ZXJC_ZSTJPCS_CONFIG where PCIP in ('" + enableip.Join("','") + "')";
            DataTable dt = SQLConnect.SelectSQL(sql, out err);
            if (err != "")
            {
                err = "尝试从数据库中查询IP地址失败,错误原因是:" + err;
                return false;
            }
            if (dt.Rows.Count == 0)
            {
                err = "计算机当前有效网络地址为:" + enableip.Join() + "\r\n未从数据库中查询到相关的设置行";
                return false;
            }
            DataRow dr = dt.Rows[0];
            #region 将数据库行注入AppConfig静态属性中
            var ps = typeof(AppConfig).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var p in ps)
            {
                string pname = p.Name.ToUpper();
                if (dr.Table.Columns.Contains(pname))
                {
                    if (p.PropertyType == typeof(int))
                    {
                        p.SetValue(null, dr[pname].ToType<int>());
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        p.SetValue(null, dr[pname].ToType<string>());
                    }
                    else if (p.PropertyType == typeof(bool))
                    {
                        p.SetValue(null, dr[pname].ToType<bool>());
                    }
                    else if (p.PropertyType == typeof(System.Net.IPEndPoint))
                    {
                        p.SetValue(null, dr[pname].ToType<System.Net.IPEndPoint>());
                    }
                    else if (p.PropertyType == typeof(System.Net.IPAddress))
                    {
                        p.SetValue(null, dr[pname].ToType<System.Net.IPAddress>());
                    }
                }
            }
            #endregion
            AppConfig.GetOtherProp();
            return true;
        }

        /// <summary>读取数据库中当前日志使能状态
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool GetLogEnable(out string err)
        {
            string sql = string.Format("select LOGENABLE from ZXJC_ZSTJPCS_CONFIG2 where GONGWEI='{0}'", AppConfig.GONGWEI);
            DataTable dt_temp = SQLConnect.SelectSQL(sql, out err);
            if (err == "")
            {
                if (dt_temp.Rows.Count > 0)
                {
                    return dt_temp.Rows[0][0].ToType<bool>();
                }
                else
                {
                    err = "未从数据库中读取到有效行";
                }
            }
            return false;
        }

        /// <summary>设置程序的最后运行时间
        /// 
        /// </summary>
        /// <param name="PCIP"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int SetApplicationLastRunTime(/*DateTime dt,*/ out string err)
        {
            string sql = "update ZXJC_ZSTJPCS_CONFIG2 set APPLICATIONLASTRUNTIME=sysdate where GONGWEI=:gongwei";
            List<DbParameter> parms = new List<DbParameter>
            {
                new OracleParameter("gongwei", OracleDbType.Varchar2,  AppConfig.GONGWEI, ParameterDirection.Input),
                //new OracleParameter("lasttime", OracleDbType.Date, dt, ParameterDirection.Input)
            };
            return SQLConnect.ExeCmdWithParm(sql, parms, out err);
        }

        /// <summary>系统登录
        /// 
        /// </summary>
        public static string Login(string id, string password, out string err)
        {
            List<DbParameter> parms = new List<DbParameter>
            {
                new OracleParameter("ReturnValue", OracleDbType.Varchar2, 64, ParameterDirection.ReturnValue, true, 0, 0, "", DataRowVersion.Default, Convert.DBNull),
                new OracleParameter("ls_user", OracleDbType.Varchar2, 32, id, ParameterDirection.Input),
                new OracleParameter("ls_password", OracleDbType.Varchar2, 32, password, ParameterDirection.Input),
                new OracleParameter("ls_workno", OracleDbType.Varchar2, 32, AppConfig.GX, ParameterDirection.Input),
                new OracleParameter("ls_scx", OracleDbType.Varchar2, 32, AppConfig.SCX, ParameterDirection.Input)
            };
            SQLConnect.ExeStoredProcedure("F_user_login", parms, out err);
            if (err == "")
            {
                return parms[0].Value.ToString();
            }
            return "";
        }

        public static void SaveLog(List<AppMessage.Log> logs)
        {
            string sql = "insert into ZXJC_ZSTJPCS_LSXX(GONGWEI,LSLX,LSSJ,LSID,LSXX1,LSXX2,LSXX3) values(:gongwei,:lslx,:lssj,:lsid,:lsxx1,:lsxx2,:lsxx3)";
            var parms = new List<List<DbParameter>>();
            foreach (var log in logs)
            {
                var logparm = new List<DbParameter>();
                parms.Add(logparm);
                logparm.Add(new OracleParameter("gongwei", OracleDbType.Varchar2, log.GONGWEI, ParameterDirection.Input));
                logparm.Add(new OracleParameter("lslx", OracleDbType.Varchar2, log.LSLX, ParameterDirection.Input));
                logparm.Add(new OracleParameter("lssj", OracleDbType.Date, log.LSSJ, ParameterDirection.Input));
                logparm.Add(new OracleParameter("lsid", OracleDbType.Int32, log.LSID, ParameterDirection.Input));
                logparm.Add(new OracleParameter("lsxx1", OracleDbType.Varchar2, log.LSXX1, ParameterDirection.Input));
                logparm.Add(new OracleParameter("lsxx2", OracleDbType.Varchar2, log.LSXX2, ParameterDirection.Input));
                logparm.Add(new OracleParameter("lsxx3", OracleDbType.Varchar2, log.LSXX3, ParameterDirection.Input));
            }
            SQLConnect.ExeCmdWithParms(sql, parms, out _);
        }

        public static string SearchCodeByRFID(string jjh)
        {
            var re = SQLConnect.SelectResult<string>("select VIN from zxjc_jrjj where JJH='" + jjh + "' and VIN is not null", out string _);
            return re;
        }

        /// <summary>将指定的夹具号和机号绑定
        /// 
        /// </summary>
        /// <returns></returns>
        public static int BindingVINToJJH(string vin, string jjh, out string err)
        {
            string sql = "update ZXJC_JRJJ set VIN='" + vin + "',LRSJ=SYSDATE where JJH='" + jjh + "'";
            return SQLConnect.ExeSQL(sql, out err);
        }

        /// <summary>上传过程数据
        /// 
        /// </summary>
        /// <param name="vin"></param>
        /// <param name="ok"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static bool Insert_ZXJC_Data_List(VinMsg vin, bool ok, out string err)
        {
            if (SParms.DebugMode)
            {
                err = "";
                return true;
            }

            if (AppConfig.SCX == 1 && AppConfig.GX == 22)
            {
                string sql1 = "insert into ZXJC_DATA_LIST(VIN,ZTBM,WORK_FLOW,JCSJ,JCRY,JCJG,GZXX,SCX,JJH) values ('" + vin.vin + "','" + vin.ZTBM + "','" + 49 + "',sysdate,'" + SParms.User + "','" + (ok ? "合格" : "不合格") + "','','" + AppConfig.SCX + "','')";
                string sql2 = "insert into ZXJC_DATA_LIST(VIN,ZTBM,WORK_FLOW,JCSJ,JCRY,JCJG,GZXX,SCX,JJH) values ('" + vin.vin + "','" + vin.ZTBM + "','" + 50 + "',sysdate,'" + SParms.User + "','" + (ok ? "合格" : "不合格") + "','','" + AppConfig.SCX + "','')";
                SQLConnect.ExeSQL(sql1, out string err1);
                SQLConnect.ExeSQL(sql2, out string err2);
                err = (err1 + err2).Trim();
                return err == "";
            }

            string sql = "insert into ZXJC_DATA_LIST(VIN,ZTBM,WORK_FLOW,JCSJ,JCRY,JCJG,GZXX,SCX,JJH) values ('" + vin.vin + "','" + vin.ZTBM + "','" + AppConfig.GX + "',sysdate,'" + SParms.User + "','" + (ok ? "合格" : "不合格") + "','','" + AppConfig.SCX + "','')";
            SQLConnect.ExeSQL(sql, out err);
            return err == "";
        }

        /// <summary>上传干检数据
        /// 
        /// </summary>
        /// <param name="vin"></param>
        /// <param name="gjre"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static bool Insert_ZXJC_QMD(VinMsg vin, Cosmo.GJResult gjre, out string err)
        {
            if (SParms.DebugMode)
            {
                err = "";
                return true;
            }
            string sql = "insert into ZXJC_QMD(BARCODE,MODEL,P1_VALUE,P2_VALUE,DP_VALUE,V_VALUE,TEST_RESULT,TEST_DATE,TEST_RY) values ('" + vin.vin + "','" + vin.ZTBM + "','" + gjre.Hi + "','" + gjre.Lo + "','" + gjre.DP + "','" + gjre.XLZ + "','" + (gjre.OK ? "合格" : "不合格") + "',sysdate,'" + SParms.User + "')";
            /*int re = */
            SQLConnect.ExeSQL(sql, out err);
            return err == "";
        }

        /// <summary>上传扭力数据
        /// 
        /// </summary>
        /// <param name="vin"></param>
        /// <param name="nl"></param>
        /// <param name="ok"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static bool Insert_BOSCHPDC(VinMsg vin, decimal nl, bool ok, out string err)
        {
            if (SParms.DebugMode)
            {
                err = "";
                return true;
            }
            int DQCXH_Int_Temp = Regex.Replace(vin.DQCXH, @"[^0-9]+", "").ToInt();//因为BOSCHPDC表的PROGRAM字段是数字类型,没有理由,就是这么流氓,只能提取数字部分存取进去
            string sql = "insert into BOSCHPDC(IDENTIFIER,NODE,RESULT,IDATE,ITIME,CHANNEL,PROGRAM,DATA1,SCX,LRR) values ('" + vin.vin + "_" + vin.NowCount + "','" + AppConfig.GX + "','" + (ok ? "OK" : "NOK") + "'," + "to_char(sysdate, 'yyyy-mm-dd')" + "," + "to_char(sysdate, 'hh24:mi:ss')" + ",'1','" + DQCXH_Int_Temp + "','" + nl + "','" + AppConfig.SCX + "','" + SParms.User + "' )";
            /* int re = */
            SQLConnect.ExeSQL(sql, out err);
            return err == "";
        }

        /// <summary>查询人工岗位的检测项目
        /// 
        /// </summary>
        /// <returns></returns>
        public static string SelectJCXM()
        {
            string sql = "select jcxm from ZJC_XM where WORK_NO = '" + AppConfig.GX + "' and scx='" + AppConfig.SCX + "'";
            return SQLConnect.SelectResult<string>(sql, out string _);
        }

        /// <summary>查询前端状态数据
        /// 
        /// </summary>
        /// <param name="vin"></param>
        /// <param name="workno"></param>
        /// <param name="scx"></param>
        /// <param name="qdbhg"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static bool GetLastWork(string vin, int workno, int scx, out string[] qdbhg, out string err)
        {
            List<DbParameter> parameters = new List<DbParameter>()
            {
                  new OracleParameter("ReturnValue", OracleDbType.Varchar2, 64, ParameterDirection.ReturnValue, true, 0, 0, "",DataRowVersion.Default, Convert.DBNull ),
                  new OracleParameter("ls_vin", OracleDbType.Varchar2,32,vin, ParameterDirection.Input),
                  new OracleParameter("ls_workno", OracleDbType.Varchar2,32,workno.ToString(), ParameterDirection.Input),
                  new OracleParameter("ls_scx", OracleDbType.Varchar2,32,scx.ToString(), ParameterDirection.Input)
            };
            SQLConnect.ExeStoredProcedure("f_get_lastwork", parameters, out err);

            string returnValue = parameters[0].Value.ToString();
            //jg = returnValue.Split('|')[0];
            if (returnValue.Length == 0)
            {
                qdbhg = new string[] { "数据库返回内容为空" };
                err = "";
                return false;
            }
            //int index = returnValue.IndexOf('|');
            //if (index == -1 || index == 0)
            //{
            //    qdbhg = "数据库返回内容不正确:" + returnValue;
            //    return false;
            //}
            //bool ok = returnValue.Substring(0, index - 1).ToBool();
            var ss = returnValue.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            bool ok;
            if (ss.Length >= 2)
            {
                ok = ss[0].ToBool();
                qdbhg = ss.Skip(1).ToArray();
            }
            else
            {
                ok = false;
                qdbhg = new string[] { "数据库返回内容不正确:" + returnValue };
            }
            return ok;
        }

        public static string GetWorkNameByWorkNO(string work_no)
        {
            string sql = "select work_name from ZXJC_GXZD where WORK_NO in (" + work_no + ")";
            return SQLConnect.SelectResult<string>(sql, out string _);
        }
    }
}

