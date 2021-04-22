using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSTJPCS
{
    /// <summary>处理机号和夹具绑定的逻辑
    /// 
    /// </summary>
    class JHBinding
    {
        #region 字段和属性

        static PLCConnect plcconnect;

        static RFID_And_IO_Connect rfid;

        public enum WorkModeEnum
        {
            以RFID作为夹具号来源,
            以扫描作为夹具号来源,
        }

        static WorkModeEnum WorkMode_;

        /// <summary>工作模式
        /// 
        /// </summary>
        public static WorkModeEnum WorkMode
        {
            get
            {
                return WorkMode_;
            }
            set
            {
                WorkMode_ = value;
                jjhInRFID_ = "";
                jjhInScan_ = "";
            }
        }

        static string jjhInScan_;

        /// <summary>以扫描方式读取的夹具号
        /// 
        /// </summary>
        public static string jjhInScan
        {
            get
            {
                return jjhInScan_;
            }
            set
            {
                jjhInScan_ = value;
                TryBinding();
            }
        }

        static string jjhInRFID_;

        /// <summary>从RFID读取的夹具号
        /// 
        /// </summary>
        public static string jjhInRFID
        {
            get
            {
                return jjhInRFID_;
            }
            set
            {
                jjhInRFID_ = value;
                TryBinding();
            }
        }

        static string vinInScan_;

        public static string vinInScan
        {
            get
            {
                return vinInScan_;
            }
            set
            {
                vinInScan_ = value;
                jjhInScan_ = "";
                jjhInRFID_ = "";
                if (WorkMode == WorkModeEnum.以RFID作为夹具号来源)
                {
                    rfid?.RFIDRead(AppConfig.RFIDSTARTINDEX, 4);
                }
            }
        }

        public enum BindingResultEnum
        {
            绑定成功,
            在数据库查询机号信息时失败,
            机号在数据库中未查询到,
            在数据库查询夹具数量时失败,
            在数据库执行绑定操作时失败,
            在数据库复查绑定结果时失败,
            复查绑定结果时校验失败,
        }

        public delegate void BindingOver(BindingResultEnum bindingResultEnum, DateTime bindingtime, string jjh, string vin, string err);

        public static event BindingOver BindingOver_Event;

        #endregion

        public static void Init(RFID_And_IO_Connect as_rfid, PLCConnect as_plcconnect)
        {
            plcconnect = as_plcconnect;
            rfid = as_rfid;
            //if (rfid != null)
            //{
            //rfid.RFID_Event += Rfid_RFID_Event;
            //}
            new System.Threading.Thread(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(AppConfig.RFIDAUTOSCANINTERVAL);
                    if (WorkMode == WorkModeEnum.以扫描作为夹具号来源)
                    {
                        rfid?.RFIDRead(AppConfig.RFIDSTARTINDEX, 4);
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        static void TryBinding()
        {
            string vin_temp = vinInScan_;
            string jjhInRFID_temp = jjhInRFID_;
            string jjhInScan_temp = jjhInScan_;
            string jjh_temp = (WorkMode == WorkModeEnum.以RFID作为夹具号来源 ? jjhInRFID : jjhInScan);
            if (!string.IsNullOrWhiteSpace(vin_temp) && !string.IsNullOrWhiteSpace(jjh_temp))
            {
                jjhInRFID_ = "";
                jjhInScan_ = "";
                var bindingre = BindingJJHAndVin(vin_temp, jjh_temp, out string err);
                if (bindingre == BindingResultEnum.绑定成功)
                {
                    plcconnect?.PLCSend(new byte[] { 1 }, "绑定完成放行");
                }
                BindingOver_Event?.Invoke(bindingre, DateTime.Now, jjh_temp, vin_temp, err);
                if (WorkMode == WorkModeEnum.以扫描作为夹具号来源)
                {
                    AppMessage.Add(vin_temp, AppMessage.MsgType.绑定机号, false, AppMessage.ImportantEnum.Alarm, jjhInScan_temp, jjhInRFID_temp, true);
                }
                return;
            }
        }

        private static BindingResultEnum BindingJJHAndVin(string vin_temp, string jjh_temp, out string err)
        {
            #region 从机号表查询确定机号是否有错误
            var 从数据库查询机号是否存在 = SQL.SQLConnect.SelectResult<string>("select vin from barcode_print where vin='" + vin_temp + "'", out err);
            if (err != "")
            {
                //BindingOver_Event?.Invoke(BindingResultEnum.在数据库查询机号信息时失败, dt_now, jjh_temp, vin_temp, err);
                return BindingResultEnum.在数据库查询机号信息时失败;
            }
            if (string.IsNullOrWhiteSpace(从数据库查询机号是否存在))
            {
                //BindingOver_Event?.Invoke(BindingResultEnum.机号在数据库中未查询到, dt_now, jjh_temp, vin_temp, "");
                return BindingResultEnum.机号在数据库中未查询到;
            }
            #endregion
            #region 查询更新或插入数据
            var count_temp = SQL.SQLConnect.SelectResult<int>("select count(1) from ZXJC_JRJJ where JJH='" + jjh_temp + "'", out err);
            if (err != "")
            {
                //BindingOver_Event?.Invoke(BindingResultEnum.在数据库查询夹具数量时失败, dt_now, jjh_temp, vin_temp, err);
                return BindingResultEnum.在数据库查询夹具数量时失败;
            }
            #endregion
            #region 更新或插入数据
            string sql;
            if (count_temp == 0)
            {
                sql = "insert into ZXJC_JRJJ values('" + jjh_temp + "','" + vin_temp + "',SYSDATE,'" + SParms.User + "')";
            }
            else
            {
                sql = "update ZXJC_JRJJ set VIN='" + vin_temp + "',LRSJ=SYSDATE,lrr='" + SParms.User + "' where JJH='" + jjh_temp + "'";
            }
            count_temp = SQL.SQLConnect.ExeSQL(sql, out err);
            if (err != "")
            {
                //BindingOver_Event?.Invoke(BindingResultEnum.在数据库执行绑定操作时失败, dt_now, jjh_temp, vin_temp, err);
                return BindingResultEnum.在数据库执行绑定操作时失败;
            }
            #endregion
            #region 写入流水日志
            SQL.SQLConnect.ExeSQL(string.Format("insert into ZXJC_JRJJ_List values('{0}','{1}',sysdate,'{2}')", jjh_temp, vin_temp, SParms.User), out err);
            #endregion
            #region 结果复查
            var vin_result = SQL.SQLConnect.SelectResult<string>("select  vin from zxjc_jrjj where jjh='" + jjh_temp + "'", out err);
            if (err != "")
            {
                //BindingOver_Event?.Invoke(BindingResultEnum.在数据库复查绑定结果时失败, dt_now, jjh_temp, vin_temp, err);
                return BindingResultEnum.在数据库复查绑定结果时失败;
            }
            if (!string.IsNullOrWhiteSpace(vin_result) && vin_result == vin_temp)
            {
                //BindingOver_Event?.Invoke(BindingResultEnum.绑定成功, dt_now, jjh_temp, vin_temp, "");
                return BindingResultEnum.绑定成功;
            }
            else
            {
                //BindingOver_Event?.Invoke(BindingResultEnum.复查绑定结果时校验失败, dt_now, jjh_temp, vin_temp, err);
                return BindingResultEnum.复查绑定结果时校验失败;
            }
            #endregion
        }
    }
}