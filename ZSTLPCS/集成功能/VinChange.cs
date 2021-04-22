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
    /// <summary>换产提醒功能
    /// 
    /// </summary>
    public static class VinChange
    {
        /// <summary>程序上次处理的机号信息,用于判断是否要弹出换产提醒界面
        /// 
        /// </summary>
        static VinMsg LastVinMsg;

        /// <summary>判断是否需要显示换产提醒
        /// 
        /// </summary>
        /// <param name="newvin"></param>
        public static void GetChange(VinMsg newvin)
        {
            if (newvin != null)
            {
                if (LastVinMsg == null || LastVinMsg.ZTBM != newvin.ZTBM)
                {
                    var s1 = LastVinMsg?.ZTBM ?? "空";
                    var s2 = newvin?.ZTBM ?? "空";
                    ToolAlarm.ShowDialog(string.Format("换型:\r\n{0}→{1}", s1, s2));
                }
            }
        }

        /// <summary>将指定机号设定为程序最新的机号
        /// 
        /// </summary>
        /// <param name="newvin"></param>
        public static void SetChange(VinMsg newvin)
        {
            LastVinMsg = newvin;
        }
    }
}
