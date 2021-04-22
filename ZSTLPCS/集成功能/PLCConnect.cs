using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using QLib;

namespace ZSTJPCS
{
    /// <summary>一个只向PLC发送数据而不接收数据的对象,一般用于向线体PLC发送放行信号
    /// 
    /// </summary>
    public class PLCConnect
    {
        public PLCConnect(IPEndPoint ip)
        {
            IP = ip.Address.ToString();
            Port = ip.Port;
            SocketReconnect();

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
                        IsCommcation = true;
                    }
                    catch { SocketReconnect(); System.Threading.Thread.Sleep(5000); }
                    senddata = null;
                }
            })
            { IsBackground = true }.Start();

            //接收数据
            new System.Threading.Thread(() =>
            {
                byte[] b = new byte[1024];
                while (true)
                {
                    if (s == null)
                    {
                        continue;
                    }
                    if (s.Available == 0)
                    {
                        continue;
                    }
                    int len = 0;
                    try
                    {
                        len = s.Receive(b);
                    }
                    catch
                    {

                    }
                    if (len == 0)
                    {
                        continue;
                    }
                    byte[] recedata = b.Take(len).ToArray();
                    ReceData_Event?.Invoke(this, recedata);
                }
            })
            { IsBackground = true }.Start();
        }

        //各岗位中自动放行的通讯配置:1线IP = "172.16.147.228";3线IP = "172.16.147.227";端口:Gan11 Gan31 = 506,Gan12 Gan32 = 507,Gan13 Gan33 = 508,Gan14 Gan34 = 509,Fei11 Fei31 = 510,Fei12 Fei32 = 511,合箱 = 501,气门 = 502,点火器 = 503,加机油 = 504

        #region 字段和属性

        /// <summary>干检 & 手动岗位解锁指令1_走主线
        /// 
        /// </summary>
        public static byte[] HandUnLock1 = { 0x43, 0x50, 0x43, 0x2E, 0x00, 0x06, 0x01, 0x06, 0x00, 0x00, 0x00, 0x01 };

        /// <summary>干检 & 手动岗位解锁指令2_走支线
        /// 
        /// </summary>
        public static byte[] HandUnLock2 = { 0x43, 0x50, 0x43, 0x2E, 0x00, 0x06, 0x01, 0x06, 0x00, 0x00, 0x00, 0x02 };

        /// <summary>干检 & 手动岗位心跳指令
        /// 
        /// </summary>
        public static byte[] HandAlive = { 0x43, 0x50, 0x43, 0x2e, 0x00, 0x06, 0x01, 0x06, 0x00, 0x01, 0x00, 0x02 };

        string IP { get; set; }

        public int Port { get; set; }

        /// <summary>是否和PLC连接上Socket
        /// 
        /// </summary>
        public bool IsConnected { get { return s == null ? false : s.Connected; } }

        /// <summary>最后一次向PLC发送数据是否成功
        /// 
        /// </summary>
        public bool IsCommcation { get; set; } = true;

        Socket s;

        //public static PLCConnect plcConnect;

        List<byte[]> SendCache = new List<byte[]>();

        #endregion

        /// <summary>向PLC发送数据
        /// 
        /// </summary>
        /// <param name="b"></param>
        public void PLCSend(byte[] b, string context = "")
        {
            AppMessage.Add(context, AppMessage.MsgType.线体PLC通讯);
            lock (SendCache)
            {
                SendCache.Add(b);
            }
        }

        public delegate void ReceData_Dele(PLCConnect plcconnect, byte[] b);
        public event ReceData_Dele ReceData_Event;

        /// <summary>Socket重新实例化并连接至目标地址
        /// 
        /// </summary>
        /// <returns></returns>
         bool SocketReconnect()
        {
            //释放连接
            if (s != null)
            {
                try
                {
                    s.Disconnect(false);
                }
                catch
                {
                }
            }

            //尝试连接Socket,成功后赋值
            Socket socket_temp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
                s = socket_temp;
                //if (this.SocketEvent != null)
                //{
                //    this.SocketEvent(RFID_And_IO_Connect.SocketEventType.连接成功);
                //}
                return true;
            }
        }

        public void Close()
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
    }
}
