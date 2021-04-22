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
    /// <summary>处理程序中的机号录入和缓存机制
    /// 
    /// </summary>
    public static class JHQueue
    {
        /// <summary>从数据库持久化数据中提取缓存的机号队列,一般在程序开始时使用
        /// 
        /// </summary>
        static JHQueue()
        {
            if (!AppConfig.ISCACHE)
            {
                return;
            }
            var s_temp = SQL.SQLConnect.SelectResult<string>("select CaCheJiHao from ZXJC_ZSTJPCS_CONFIG2 where GONGWEI='" + AppConfig.GONGWEI + "'", out string err);
            //var dt = SQL.SQLConnect.SelectSQL("select CaCheJiHao from ZXJC_ZSTJPCS_CONFIG2 where GONGWEI='" + AppConfig.GONGWEI + "'", out string err);
            if (err != "")
            {
                //err = "读取数据库时发生错误,原因是:" + err;
                AppMessage.Add("提取机号持久化数据错误:" + err, AppMessage.MsgType.应用程序错误, true, AppMessage.ImportantEnum.Alarm, err);
                return;
            }
            //if (dt.Rows.Count == 0)
            //{
            //    err = "未从数据库中读取到有效行";
            //    AppMessage.Add("从数据库持久化数据中提取缓存的机号队列错误", AppMessage.MsgType.应用程序错误, true, AppMessage.ImportantEnum.Alarm, err);
            //    return;
            //}
            //string s_temp = dt.Rows[0][0].ToType<string>();
            if (string.IsNullOrEmpty(s_temp))
            {
                s_temp = "";
            }
            sns = new Queue<string>(s_temp.Split(','));
            if (sns == null)
            {
                sns = new Queue<string>();
            }
        }

        /// <summary>缓存的机号队列
        /// 
        /// </summary>
        static readonly Queue<string> sns = new Queue<string>();

        /// <summary>查询当前所有缓存的机号
        /// 
        /// </summary>
        /// <returns></returns>
        public static Queue<string> GetAllCache()
        {
            return sns;
        }

        /// <summary>尝试从缓冲队列中取出一个有效的机号信息,否则返回null
        /// 
        /// </summary>
        /// <returns></returns>
        public static VinMsg GetVin()
        {
            //var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sns.Count > 0)
            {
                //System.Diagnostics.Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
                VinMsg vinmsg = VinMsg.Create(sns.Dequeue(), out string err);
                CacheJHCountChanged?.Invoke();
                //System.Diagnostics.Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
                if (err == "")
                {
                    return vinmsg;
                }
            }
            //System.Diagnostics.Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
            return null;
        }

        /// <summary>将一个待处理的机号存入缓存队列中,并返回队列中的数量
        /// 
        /// </summary>
        /// <returns></returns>
        public static int SetVin(string vin, out string err)
        {
            if (sns.Contains(vin))
            {
                err = "机号" + vin + "已在缓存队列中存在";
                return sns.Count;
            }
            sns.Enqueue(vin);
            CacheJHCountChanged?.Invoke();
            err = "";
            return sns.Count;
        }

        /// <summary>将一个待处理的机号存入缓存队列中,并返回队列中的数量
        /// 
        /// </summary>
        /// <returns></returns>
        public static int SetVin(VinMsg vin, out string err)
        {
            if (sns.Contains(vin.vin))
            {
                err = "机号" + vin.vin + "已在缓存队列中存在";
                return sns.Count;
            }
            sns.Enqueue(vin.vin);
            err = "";
            CacheJHCountChanged?.Invoke();
            return sns.Count;
        }

        /// <summary>返回当前缓存队列中的数量
        /// 
        /// </summary>
        /// <returns></returns>
        public static int GetCount()
        {
            return sns.Count;
        }

        /// <summary>清除所有缓存的机号和当前正在操作的机号
        /// 
        /// </summary>
        /// <returns></returns>
        public static void ClearAll()
        {
            sns.Clear();
            CacheJHCountChanged?.Invoke();
        }

        /// <summary>将机号缓存数据持久化到数据库中,一般在程序退出时使用
        /// 
        /// </summary>
        /// <param name="nowvin">要写入的当前机号,如果当前机号未完成应当一并写入</param>
        public static void SaveToDB(string nowvin=null)
        {
            if (AppConfig.ISCACHE)
            {
                List<string> sns_temp = sns.ToList();
                if (!string.IsNullOrWhiteSpace(nowvin))
                {
                    sns_temp.Insert(0, nowvin);
                }
                SQL.SQLConnect.ExeSQL(string.Format("update ZXJC_ZSTJPCS_CONFIG2 set CaCheJiHao='{0}' where GONGWEI='{1}'", sns_temp.ToList().Join(), AppConfig.GONGWEI), out string err_temp);
                if (err_temp != "")
                {
                    AppMessage.Add("将机号缓存数据持久化到数据库中错误", AppMessage.MsgType.应用程序错误, true, AppMessage.ImportantEnum.Alarm, err_temp);
                }
            }
        }

        /// <summary>缓存的机号数量发生了变化
        /// 
        /// </summary>
        public static event Action CacheJHCountChanged;
    }
}
