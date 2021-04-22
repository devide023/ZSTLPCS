using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZSTJPCS
{
    /// <summary>设备数据采集器 - 基类
    /// 
    /// </summary>
    public abstract class DataCollecter
    {
        #region 需重载的方法

        /// <summary>让设备切换工作程序
        /// 
        /// </summary>
        /// <param name="channel"></param>
        public abstract void DC_Channel(string channel);

        /// <summary>让设备开始运转
        /// 
        /// </summary>
        public abstract void DC_Start();

        /// <summary>让设备切换工作程序并开始运转
        /// 
        /// </summary>
        /// <param name="channel"></param>
        public abstract void DC_Start(string channel);

        /// <summary>锁定或解锁设备,不允许进行相应操作,true=锁定,false=解锁
        /// 
        /// </summary>
        /// <param name="as_Enable"></param>
        public abstract void DC_Lock(bool as_Enable);

        /// <summary>放行命令,让设备驱动岗位阻挡器放行
        /// 
        /// </summary>
        public abstract void DC_ByPass();

        /// <summary>让设备停止运转
        /// 
        /// </summary>
        public abstract void DC_Stop();

        /// <summary>尝试关闭数据采集器
        /// 
        /// </summary>
        public abstract void DC_Close();

        #endregion

        /// <summary>设备的最后通讯时间
        /// 
        /// </summary>
        public DateTime LastCommcationTime { get; set; }

        ///// <summary>默认通讯超时时间
        ///// 
        ///// </summary>
        //public int DefultCommcationTimeout { get; set; } = 60;

        //protected bool IsConnected_;

        //protected bool IsCommcation_;

        /// <summary>设备是否处于连接状态
        /// 
        /// </summary>
        //public bool IsConnected { get { return (DateTime.Now - LastCommcationTime).TotalSeconds > DefultCommcationTimeout; } }
        public abstract /*virtual*/ bool DC_IsConnected { get; }

        /// <summary>设备是否和上位机程序在进行数据交互
        /// 
        /// </summary>
        public abstract/*virtual*/ bool DC_IsCommcation { get; }

        #region 触发消息事件

        public void OnGetData_Event(params object[] os)
        {
            GetData_Event?.Invoke(os);
        }

        public void OnMsg_Event(params object[] os)
        {
            Msg_Event?.Invoke(os);
        }

        /// <summary>获取到过程数据
        /// 
        /// </summary>
        public event Action<object[]> GetData_Event;

        /// <summary>获取到其他消息
        /// 
        /// </summary>
        public event Action<object[]> Msg_Event;

        /// <summary>待处理的业务数据
        /// 
        /// </summary>
        List<object[]> ReceData = new List<object[]>();

        public void AddReceData(object[] os)
        {
            lock (ReceData)
            {
                ReceData.Add(os);
            }
        }

        public void AddReceData(object os)
        {
            lock (ReceData)
            {
                ReceData.Add(new object[] { os });
            }
        }

        public object[] GetReceData()
        {
            lock (ReceData)
            {
                if (ReceData.Count == 0)
                {
                    return null;
                }
                else
                {
                    var re = ReceData[0];
                    ReceData.RemoveAt(0);
                    return re;
                }
            }
        }

        #endregion
    }
}
