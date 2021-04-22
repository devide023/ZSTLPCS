using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
//using QLib4;
using Oracle.ManagedDataAccess.Client;

namespace ZSTJPCS
{
    /// <summary>应用程序的日志消息
    /// 
    /// </summary>
    public static class AppMessage
    {
        static AppMessage()
        {
            System.Threading.Thread th = new System.Threading.Thread(() =>
            {
                List<Log> logs_temp = new List<Log>();
                while (true)
                {
                    System.Threading.Thread.Sleep(5000);
                    
                    if (logs.Count == 0)
                    {
                        continue;
                    }
                    lock (logs)
                    {
                        logs_temp.AddRange(logs);
                        logs.Clear();
                    }
                    SQL.SaveLog(logs_temp);                        
                    logs_temp.Clear();
                }
            })
            { IsBackground = true };
            th.Start();
        }

        /// <summary>缓存将要向数据库存入的日志消息
        /// 
        /// </summary>
        static List<Log> logs = new List<Log>();

        /// <summary>该条日志在数据库中的连续累加ID号,主要作用是分了区分在同一秒中的日志先后顺序
        /// 
        /// </summary>
        static int LogID = 1;

        //static int WindowsLogID = 0;

        //static int WindowsLogMaxCount = 99;

        public enum MsgType
        {
            一般,
            干检,
            触发新条码,
            板卡接收消息,
            板卡发送消息,
            新扭力消息,
            应用程序错误,
            飞轮岗位A1A3交互消息,
            飞轮岗位A1A3板卡消息,
            空夹具提示,
            接收串口数据,
            RFID,
            数据库,
            机号,
            阿特拉斯网络指令,
            阿特拉斯串口指令,
            线体PLC通讯,
            数据采集,
            英格索兰,
            绑定机号,
        }

        /// <summary>消息的重要程度,决定该消息的界面中显示的颜色
        /// 
        /// </summary>
        public enum ImportantEnum
        {
            /// <summary>一个严重的程序错误 或 需要操作人员关注的产品质量问题
            /// 
            /// </summary>
            Err,
            /// <summary>一个中等的程序错误 或 需要操作人员反馈的问题,但并不影响生产
            /// 
            /// </summary>
            Alarm,
            /// <summary>一个微小的程序错误 或 操作人员在需要的时候需要看到的消息
            /// 
            /// </summary>
            Normal,
        }

        /// <summary>添加一条应用程序日志[到数据库]和[显示界面]
        /// 
        /// </summary>
        /// <param name="日志消息">日志内容</param>
        /// <param name="日志类型">日志类型</param>
        /// <param name="isShowInWindow">是否在应用程序窗体日志栏中显示</param>
        /// <param name="ie">日志的重要程度,它决定在窗体中显示的日志的颜色</param>
        /// <param name="日志消息2">日志内容-附加消息1</param>
        /// <param name="日志消息3">日志内容-附加消息2</param>
        /// <param name="isDBImportant">该条信息是否是一个数据库重要信息,如果是,不管当前AppConfig.LogEnable设定为是否将日志保存到数据库,则都会保存到数据库</param>
        public static void Add(string 日志消息, MsgType 日志类型, bool isShowInWindow = false, ImportantEnum ie = ImportantEnum.Normal, string 日志消息2 = "", string 日志消息3 = "", bool isDBImportant = false)
        {
            if (!AppConfig.LOGENABLE && !isShowInWindow && !isDBImportant)
            {
                return;
            }

            //生成日志
            Log log = new Log()
            {
                GONGWEI = AppConfig.GONGWEI,
                LSID = LogID,
                LSSJ = DateTime.Now,
                LSLX = 日志类型.ToString(),
                LSXX1 = 日志消息.Length > 1300 ? 日志消息.Substring(0, 1300) : 日志消息,//数据库字段限制为4000,每一个字符最大占用3个空间
                LSXX2 = 日志消息2.Length > 1300 ? 日志消息2.Substring(0, 1300) : 日志消息2,
                LSXX3 = 日志消息3.Length > 1300 ? 日志消息3.Substring(0, 1300) : 日志消息3,
                importantEnum = ie,
            };

            //保存到数据库
            if (AppConfig.LOGENABLE || isDBImportant)
            {
                LogID++;
                lock (logs)
                {
                    logs.Add(log);
                }
            }

            //显示到页面
            if (isShowInWindow)
            {
                HaveShowLog_Event?.Invoke(log);
            }

            return;
        }

        public static event Action<Log> HaveShowLog_Event;

        public class Log
        {
            public string GONGWEI { get; set; }
            public string LSLX { get; set; }
            public DateTime LSSJ { get; set; }
            public int LSID { get; set; }
            public string LSXX1 { get; set; }
            public string LSXX2 { get; set; }
            public string LSXX3 { get; set; }
            public ImportantEnum importantEnum { get; set; }
        }
    }
}
