//#define 处理心跳

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using QLib;

namespace ZSTJPCS
{
    /// <summary>英格索兰扭力枪通讯驱动_使用明匠板卡连接的驱动程序
    /// 
    /// </summary>
    public class IngersollRandConnecter : DataCollecter
    {
        public IngersollRandConnecter(IPEndPoint as_ip, IPEndPoint as_bindingip = null)
        {
            IP = as_ip.Address.ToString();
            Port = as_ip.Port;
            BindingIp = as_bindingip;
            SocketReconnect();

            //发送心跳和问询数据
            new System.Threading.Thread(() =>
            {
#if 处理心跳
                int count_temp = 0;
#endif
                while (true)
                {
                    System.Threading.Thread.Sleep(200);
                    lock (SendCache)
                    {
                        SendCache.Add(AskTightResult);
#if 处理心跳
                        if (++count_temp >= 5)
                        {
                            SendCache.Add(HeartData);
                            count_temp = 0;
                        }
#endif
                    }
                }
            })
            { IsBackground = true }.Start();

            //发送数据
            new System.Threading.Thread(() =>
            {
                byte[] senddata = null;
                while (true)
                {
                    System.Threading.Thread.Sleep(100);
                    lock (SendCache)
                    {
                        if (SendCache.Count > 10)
                        {
                            SendCache.Clear();
                        }
                        if (SendCache.Count > 0)
                        {
                            senddata = SendCache[0];
                            SendCache.RemoveAt(0);
                        }
                    }

                    if (senddata == null)
                    {
                        continue;
                    }
                    if (s == null)
                    {
                        SocketReconnect();
                        System.Threading.Thread.Sleep(5000);
                    }
                    try
                    {
                        s.Send(senddata);
                        AppMessage.Add("发送数据", AppMessage.MsgType.英格索兰, false, AppMessage.ImportantEnum.Normal, senddata.ToStandardString(false, senddata.Length));
                        //LastCommcationTime = DateTime.Now;
                    }
                    catch { SocketReconnect(); System.Threading.Thread.Sleep(5000); }
                    senddata = null;
                }
            })
            { IsBackground = true }.Start();

            //接收数据
            new System.Threading.Thread(() =>
            {
                byte[] recedata = new byte[1024];
                while (true)
                {
                    System.Threading.Thread.Sleep(1);
                    if (s != null && s.Available > 0)
                    {
                        int rece_count_temp = s.Receive(recedata);
                        AppMessage.Add("接收数据", AppMessage.MsgType.英格索兰, false, AppMessage.ImportantEnum.Normal, recedata.ToStandardString(false, rece_count_temp));
                        if (rece_count_temp == 17)
                        {
                            bool newdata= (recedata[9] == 0 && recedata[10] == 1);
                            if (newdata)
                            {
                                bool ok = (recedata[11] == 0 && recedata[12] == 1);
                                decimal tr = (recedata[13] * 257 + recedata[14]) / 100m;
                                decimal angle = recedata[15] * 257 + recedata[16];
                                if (tr != 0 || angle != 0)//复位后可能数据都是0,需要排除
                                {
                                    lock (SendCache)//发送复位指令,否则下次又要接收到相同的数据了
                                    {
                                        SendCache.Add(Reset);
                                    }
                                    OnGetData_Event(new TightResult(ok, tr, angle));
                                    //AddReceData(new TightResult(ok, tr, angle));
                                }
                            }
#if 处理心跳

#else
                            LastCommcationTime = DateTime.Now;
#endif

                        }
#if 处理心跳
                        else if (rece_count_temp == 11)//心跳数据
                        {
                            LastCommcationTime = DateTime.Now;
                        }
#endif
                    }
                }
            })
            { IsBackground = true }.Start();
        }

#region 字段和属性

        public readonly byte[] HeartData = { 67, 80, 0, 0, 0, 6, 1, 3, 0, 0, 0, 1 };

        public readonly byte[] AskTightResult = { 67, 80, 0, 0, 0, 6, 1, 3, 0, 2, 0, 4 };

        //public readonly byte[] Reset = { 67, 80, 0, 0, 0, 6, 1, 6, 0, 2, 0, 4, 0, 0, 0, 0 };

        public readonly byte[] Reset = { 67, 80, 0, 0, 0, 6, 1, 6, 0, 2, 0, 0 };

        string IP { get; set; }

        public int Port { get; set; }

        /// <summary>是否使用本地绑定的IP,主要解决A3线飞轮岗位 英格索兰拧紧枪和线体主控PLC的IP在同一网段的 不同网卡不同网段问题
        /// 
        /// </summary>
        IPEndPoint BindingIp { get; set; }

        Socket s;

        List<byte[]> SendCache = new List<byte[]>();

#endregion

#region 私有方法

        /// <summary>Socket重新实例化并连接至目标地址
        /// 
        /// </summary>
        /// <returns></returns>
        bool SocketReconnect()
        {
            //释放连接
            if (this.s != null)
            {
                try
                {
                    this.s.Disconnect(false);
                }
                catch
                {

                }
            }

            //尝试连接Socket,成功后赋值
            Socket socket_temp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (BindingIp != null)
            {
                socket_temp.Bind(BindingIp);
            }
            IAsyncResult connResult = socket_temp.BeginConnect(IP, Port, null, null);
            connResult.AsyncWaitHandle.WaitOne(2000, true);  //等待2秒
            if (!connResult.IsCompleted)
            {
                socket_temp.Close();
                ////处理连接不成功的动作
                //if (this.s != null)
                //{
                //    this.s(RFID_And_IO_Connect.SocketEventType.连接失败);
                //}
                return false;
            }
            else
            {
                //处理连接成功的动作
                this.s = socket_temp;
                //if (this.SocketEvent != null)
                //{
                //    this.SocketEvent(RFID_And_IO_Connect.SocketEventType.连接成功);
                //}
                return true;
            }
        }

#endregion

#region 继承的字段、属性、方法

        public override bool DC_IsConnected { get { return s == null ? false : s.Connected; } }

        public override bool DC_IsCommcation { get => (DateTime.Now - LastCommcationTime).TotalSeconds < 5; }

        public override void DC_Channel(string channel)
        {
            //形式为M11S
            var channel_temp = channel.ToUpper();
            if (channel_temp.Length != 4 || channel_temp[0] != 'M' || !channel_temp.Substring(1, 2).IsInt())
            {
                return;
            }
            byte[] senddata = { 67, 80, 0, 0, 0, 6, 1, 6, 0, 0, 0, 0 };
            senddata[10] = (byte)channel[3];//轴位置
            senddata[11] = (byte)channel_temp.Substring(1, 2).ToInt();//轴程序号
            lock (SendCache)
            {
                SendCache.Add(senddata);
            }
        }

        public override void DC_Start()
        {
        }

        public override void DC_Start(string channel)
        {
        }

        public override void DC_Lock(bool as_Enable)
        {

        }

        public override void DC_ByPass()
        {
            //this.SetDO(1, true);
            //this.SetDO(1, false);
        }

        public override void DC_Stop()
        {
            return;
        }

        public override void DC_Close()
        {
            if (s != null)
            {
                try
                {
                    s.Close();
                }
                catch { }
            }
        }

#endregion
    }
}