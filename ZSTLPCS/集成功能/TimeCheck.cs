using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSTJPCS
{
    /// <summary>将计算机时间与数据库进行时间同步或检查的工具
    /// 
    /// </summary>
    public static class TimeCheck
    {
        const int 最大允许偏差时间 = 60;

        public static DateTime GetSysTime(out string err)
        {
            return SQL.SQLConnect.SelectResult<DateTime>("select sysdate from dual", out err);
        }

        /// <summary>获取本地时间与数据库时间之间是否需要同步
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool CheckTime()
        {
            var dt = GetSysTime(out string err);
            if (err == "")
            {
                return Math.Abs((DateTime.Now - dt).TotalSeconds) > 最大允许偏差时间;
            }
            else
            {
                return false;
            }
        }

        /// <summary>将本地时间与数据库时间同步,需要弹出一个黑框
        /// 
        /// </summary>
        public static bool SyncTime(out string err)
        {
            var dt = GetSysTime(out err);
            if (err != "")
            {
                err = "从数据库获取时间失败:" + err;
                return false;
            }
            QLib.QL_Windows.SetLocalTime_CMD(dt);
            //Process proStep1 = new Process
            //{
            //    StartInfo = new ProcessStartInfo
            //    {
            //        FileName = "cmd.exe",
            //        Arguments = "/C date " + dt.ToString("yyyy-MM-dd"),
            //        WindowStyle = ProcessWindowStyle.Hidden
            //    }
            //};
            //Process proStep2 = new Process
            //{
            //    StartInfo = new ProcessStartInfo
            //    {
            //        FileName = "cmd.exe",
            //        Arguments = "/C time " + dt.ToString("HH:mm:ss"),
            //        WindowStyle = ProcessWindowStyle.Hidden
            //    }
            //};
            //try
            //{
            //    proStep1.Start();
            //    proStep2.Start();QLib.QL_Windows
            //}
            //catch (Exception exc)
            //{
            //    err = "启动时间同步程序失败:" + exc.Message;
            //    return false;
            //}
            err = "";
            return true;
        }
    }
}
