using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using QLib;
using System.Configuration;

namespace ZSTJPCS
{
    /// <summary>表示一个机号及相关信息
    /// 
    /// </summary>
    public class VinMsg
    {
        VinMsg()
        {

        }

        public static VinMsg Create(string as_vin, out string err)
        {
            //var sw = System.Diagnostics.Stopwatch.StartNew();
            VinMsg vin = new VinMsg
            {
                vin = as_vin
            };
            #region 查询机型和状态编码
            var dt = SQL.SQLConnect.SelectSQL("select t.ztbm,t.write_req from BARCODE_PRINT t where t.vin='" + vin.vin + "'", out err);
            //System.Diagnostics.Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
            if (err != "")
            {
                err = "从数据库查询数据失败:" + err;
                return null;
            }
            if (dt.Rows.Count == 0)
            {
                err = "数据库中未查询到机号相关信息:" + vin.vin;
                return null;
            }
            vin.ZTBM = dt.Rows[0]["ztbm"].ToString();
            vin.JX = dt.Rows[0]["write_req"].ToString();
            #endregion
            #region 查询前端状态
            switch (AppConfig.GXType_Enum)
            {
                case AppConfig.GXType_EnumType.人工:
                case AppConfig.GXType_EnumType.干检:
                case AppConfig.GXType_EnumType.扭力:
                    vin.QDZT = SQL.GetLastWork(vin.vin, AppConfig.GX, AppConfig.SCX, out string[] qdzt2, out err);
                    if (!vin.QDZT)
                    {
                        vin.QDBHGGX = qdzt2/*.Split('|')*/.ToList().ConvertAll(qw => { string str_tmp = SQL.GetWorkNameByWorkNO(qw); return string.IsNullOrWhiteSpace(str_tmp) ? qw : str_tmp; });
                    }
                    break;
            }
            #endregion
            #region 查询工艺数据
            switch (AppConfig.GXType_Enum)
            {
                case AppConfig.GXType_EnumType.扭力:
                    var dt_nl_temp = SQL.SQLConnect.SelectSQL("select * from ZXJC_NL_CSYQ where ztbm='" + vin.ZTBM + "' and work_no='" + AppConfig.GX + "'", out err);
                    //System.Diagnostics.Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
                    if (err != "")
                    {
                        err = "查询扭力数据出错:" + err;
                        return null;
                    }
                    if (dt_nl_temp.Rows.Count == 0)
                    {
                        err = "查询扭力数据出错:在表ZXJC_NL_CSYQ未查询到任何数据,状态编码为" + vin.ZTBM;
                    }
                    if (!dt_nl_temp.Rows[0]["SHBZ"].ToType<bool>())
                    {
                        err = "查询扭力数据出错:在表ZXJC_NL_CSYQ中查询到未审核数据,请联系工艺人员,状态编码为" + vin.ZTBM;
                    }
                    vin.DQCXH = dt_nl_temp.Rows[0]["GS"].ToType<string>();
                    vin.NLSX = dt_nl_temp.Rows[0]["SX"].ToType<decimal>();
                    vin.NLXX = dt_nl_temp.Rows[0]["XX"].ToType<decimal>();
                    vin.TotalCount = dt_nl_temp.Rows[0]["CS"].ToType<int>();
                    break;
                case AppConfig.GXType_EnumType.人工:

                    break;
                case AppConfig.GXType_EnumType.干检:

                    var dt_gj_temp = SQL.SQLConnect.SelectSQL("select * from ZXJC_GJ_CSYQ where ztbm='" + vin.ZTBM + "'", out err);
                    //System.Diagnostics.Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
                    if (err != "")
                    {
                        err = "查询干检数据出错:" + err;
                        return null;
                    }
                    if (dt_gj_temp.Rows.Count == 0)
                    {
                        err = "查询干检数据出错:在表ZXJC_GJ_CSYQ未查询到任何数据,状态编码为" + vin.ZTBM;
                    }
                    if (!dt_gj_temp.Rows[0]["SHBZ"].ToType<bool>())
                    {
                        err = "查询干检数据出错:在表ZXJC_GJ_CSYQ中查询到未审核数据,请联系工艺人员,状态编码为" + vin.ZTBM;
                    }
                    vin.DQCXH = dt_gj_temp.Rows[0]["GJGS"].ToType<string>();
                    vin.NLSX = dt_gj_temp.Rows[0]["GJYLSX"].ToType<decimal>();
                    vin.NLXX = dt_gj_temp.Rows[0]["GJYLXX"].ToType<decimal>();
                    vin.充气时间_干检 = dt_gj_temp.Rows[0]["GJCQSJ"].ToType<decimal>();
                    vin.允许泄漏_干检 = dt_gj_temp.Rows[0]["GJXLZ"].ToType<decimal>();
                    break;
            }
            #endregion
            return vin;
        }

        /// <summary>当前机号已经处理结束
        /// 
        /// </summary>
        public bool IsEnd { get; set; }

        /// <summary>产品唯一号
        /// 
        /// </summary>
        public string vin { get; set; }

        /// <summary>产品机型
        /// 
        /// </summary>
        public string JX { get; set; }

        /// <summary>产品状态编码
        /// 
        /// </summary>
        public string ZTBM { get; set; }

        /// <summary>扭力上限 或 压力上限
        /// 
        /// </summary>
        public decimal NLSX { get; set; }

        /// <summary>扭力下限 或 压力下限
        /// 
        /// </summary>
        public decimal NLXX { get; set; }

        /// <summary>产品要打的总螺丝数
        /// 
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>当前应当打的螺栓号,从1开始
        /// 
        /// </summary>
        public int NowCount { get; set; } = 1;

        /// <summary>当前程序号
        /// 
        /// </summary>
        public string DQCXH { get; set; }

        /// <summary>前端状态
        /// 
        /// </summary>
        public bool QDZT { get; set; }

        /// <summary>当前不合格工序
        /// 
        /// </summary>
        public List<string> QDBHGGX { get; set; }

        public decimal 充气时间_干检 { get; set; }

        public decimal 允许泄漏_干检 { get; set; }

        public override string ToString()
        {
            return vin;
        }
    }
}
