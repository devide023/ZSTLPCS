using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using QLib;

namespace ZSTJPCS
{
    /// <summary>食用方法:1.使用构造函数初始化类,给定透传模块的IP地址和端口号.2.使用Open方法打开连接器.3.使用完毕后使用Close方法关闭连接器
    /// 4.使用SocketEvent事件获取Socket在连接、发送、接收数据中的错误信息;
    /// 5.使用RFID_Event事件获取RFID读写头返回的信息,第1个参数包含基本信息,第2个参数在读取成功时包含读取的内容
    /// 6.使用RFIDRead读取指定区块中的RFID内容,内容在RFID_Event事件中回复;7.使用RFIDWrite写入指定区块中的RFID内容,内容在RFID_Event事件中回复;
    /// IO模块有两种工作模式:
    /// 1、自动读取模式:当AutoAskIOStatus为true时[默认为false],AutoAskIOStatusInterval表示默认自动读取的频率[默认为200],此时可使用IO_Event和IOUpDown_Event事件获取信息
    /// 2、手动读取模式[暂时不需要使用]:外部调用IORead方法读取并通过IO_Event和IOUpDown_Event事件获取信息
    /// 3、在通机按钮驱动模式下,必须先打开AutoAskIOStatus当读取到IOUpDown_Event事件后,将AutoAskIOStatus=false,调用ReadRFID方法,
    /// 等待RFID_Event事件后将AutoAskIOStatus=true[注意ReadRFID方法不一定会必定触发RFID_Event事件]
    /// 事件解析:
    /// IO_Event事件是每次IO模块返回信息后都会触发
    /// IOUpDown_Event事件是每次IO中有状态
    /// 注意:所有事件均为异步
    /// </summary>
    public partial class RFID_And_IO_Connect
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host">数据透传模块的IP地址</param>
        /// <param name="port">数据透传模块的TCP连接端口</param>
        public RFID_And_IO_Connect(string host, int port, int as_RFIDRECIVETYPE)
        {
            Host = host;
            Port = port;
            RFIDRECIVETYPE = as_RFIDRECIVETYPE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip">数据透传模块的IP地址</param>
        public RFID_And_IO_Connect(System.Net.IPEndPoint ip, int as_RFIDRECIVETYPE)
        {
            Host = ip.Address.ToString();
            Port = ip.Port;
            RFIDRECIVETYPE = as_RFIDRECIVETYPE;
        }
        #region 字段和属性

        /// <summary>发送数据缓冲区
        /// 
        /// </summary>
        List<byte[]> SendDataCache = new List<byte[]>();

        #endregion
        #region Socket封装

        System.Net.Sockets.Socket socket;

        string Host;

        int Port;

        System.Threading.Thread SendData_Thread;

        System.Threading.Thread ReceData_Thread;

        public bool IsConnected { get { return socket == null ? false : socket.Connected; } }

        public bool IsCommcation { get; set; } = true;

        /// <summary>Socket相关事件
        /// 
        /// </summary>
        public enum SocketEventType
        {
            连接成功,
            连接失败,
            发送数据成功,
            发送数据失败,
            接收数据成功,
            接收数据失败,
            接收到未知数据,
        }

        public event Action<SocketEventType> SocketEvent;

        /// <summary>初始化Socket收发线程
        /// 
        /// </summary>
        void SocketInit()
        {
            ReceData_Thread = new Thread(SocketReceData) { IsBackground = true };
            ReceData_Thread.Start();
            SendData_Thread = new Thread(SocketSendData) { IsBackground = true };
            SendData_Thread.Start();
            //= new System.Threading.Thread(new System.Threading.ThreadStart(this.SocketSendData));
            //this.ReceData_Thread.IsBackground = true;
            //this.SendData_Thread.Start();
        }

        /// <summary>Socket重新实例化并连接至目标地址
        /// 
        /// </summary>
        /// <returns></returns>
        bool SocketConnect()
        {
            this.SocketDisConnect();
            #region 修改超时之前的代码
            //this.socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //bool result;
            //try
            //{
            //    this.socket.Connect(this.Host, this.Port);
            //}
            //catch// (System.Exception exc_32)
            //{
            //    if (this.SocketEvent != null)
            //    {
            //        this.SocketEvent(RFID_And_IO_Connect.SocketEventType.连接失败);
            //    }
            //    result = false;
            //    return result;
            //}
            //if (this.SocketEvent != null)
            //{
            //    this.SocketEvent(RFID_And_IO_Connect.SocketEventType.连接成功);
            //}
            //result = true;
            //return result;
            #endregion
            #region 修改超时之后的代码
            Socket socket_temp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult connResult = socket_temp.BeginConnect(this.Host, this.Port, null, null);
            connResult.AsyncWaitHandle.WaitOne(2000, true);  //等待2秒
            if (!connResult.IsCompleted)
            {
                socket_temp.Close();
                //处理连接不成功的动作
                this.SocketEvent?.Invoke(SocketEventType.连接失败);
                return false;
            }
            else
            {
                //处理连接成功的动作
                this.socket = socket_temp;
                this.SocketEvent?.Invoke(SocketEventType.连接成功);
                return true;
            }
            #endregion
        }


        void SocketDisConnect()
        {
            if (this.socket != null)
            {
                try
                {
                    this.socket.Disconnect(false);
                }
                catch
                {

                }
            }
        }

        void SocketReceData()
        {
            byte[] b = new byte[1024];
            while (true)
            {
                System.Threading.Thread.Sleep(1);
                if (this.socket != null && this.socket.Available != 0)
                {
                    int count_rece = 0;
                    try
                    {
                        count_rece = this.socket.Receive(b);
                    }
                    catch
                    {
                        this.SocketEvent?.Invoke(RFID_And_IO_Connect.SocketEventType.接收数据失败);
                        continue;
                    }
                    this.SocketEvent?.Invoke(RFID_And_IO_Connect.SocketEventType.接收数据成功);
                    byte b2 = b[0];
                    switch (b2)
                    {
                        case 0x30:
                        case 0x32:
                        case 0x34:
                        case 0x35:
                        case 0x36:
                            DealReceRFIDData(b, 0, count_rece);
                            break;
                        case 1:
                            if (count_rece == 6)
                            {
                                //AppMessage.Add("接受到IO读取数据:"+b.ToStandardString(","), AppMessage.MsgType.RFID);
                                DealReceIOData(b, 0, count_rece);
                            }
                            else
                            {
                                SocketEvent?.Invoke(SocketEventType.接收到未知数据);
                            }
                            break;
                        default:
                            SocketEvent?.Invoke(SocketEventType.接收到未知数据);
                            break;
                    }

                }
            }
        }

        void SocketSendData()
        {
            byte[] sendbytes = null;
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                lock (SendDataCache)
                {
                    if (SendDataCache.Count != 0)
                    {
                        sendbytes = SendDataCache[0];
                        SendDataCache.RemoveAt(0);
                    }
                    else
                    {
                        sendbytes = null;
                    }
                }
                if (/*socket.Connected && */  sendbytes != null && socket!=null)
                {
                    try
                    {
                        socket.Send(sendbytes);
                        IsCommcation = true;
                    }
                    catch// (Exception exc)
                    {
                        IsCommcation = false;
                        SocketEvent?.Invoke(SocketEventType.发送数据失败);
                        SocketConnect();
                    }
                    SocketEvent?.Invoke(SocketEventType.发送数据成功);
                }
            }
        }

        #endregion
        #region RFID封装

        public enum OPerationResult
        {
            RFID_读取成功,
            RFID_写入成功,
            RFID_读取或写入失败_Switch_On_Message,
            RFID_读取或写入失败_发送命令错误,
            RFID_读取或写入失败_没有可读写的芯片,
            RFID_读取或写入失败_硬件错误,
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="or">结果</param>
        /// <param name="context">读取数据时返回相关内容,写入数据时为空</param>
        public delegate void RFID_Dele(OPerationResult or, string context);

        public event RFID_Dele RFID_Event;

        /// <summary>尝试读取RFID的芯片数据,结果在RFIDRead_Event事件
        /// 
        /// </summary>
        /// <param name="StartBlockIndex"></param>
        /// <param name="BlockCount"></param>
        /// <param name="Times">尝试读取的次数,默认为1</param>
        public void RFIDRead(int StartBlockIndex, int BlockCount, int Times = 1)
        {
            string s = "SR" + StartBlockIndex.ToString().PadLeft(4, '0').Substring(0, 4) + BlockCount.ToString().PadLeft(2, '0').Substring(0, 2);
            byte[] bs = System.Text.Encoding.ASCII.GetBytes(s);
            byte[] b = new byte[bs.Length + 2];
            bs.CopyTo(b, 0);
            b[b.Length - 2] = this.CheckAdd(b, 0, b.Length - 2);
            b[b.Length - 1] = 3;
            lock (this.SendDataCache)
            {
                if (this.SendDataCache.Count > 10)//如果RFID连接不上不加这个会爆炸的
                {
                    this.SendDataCache.Clear();
                }
                for (int i = 0; i < Times; i++)
                {
                    this.SendDataCache.Add(b);
                }
            }
        }

        /// <summary>尝试写入RFID的芯片数据,结果在RFIDRead_Event事件
        /// 
        /// </summary>
        /// <param name="StartBlockIndex"></param>
        /// <param name="BlockCount"></param>
        public void RFIDWrite(int StartBlockIndex, int BlockCount, string Context)
        {
            int len = BlockCount * 4;
            string s = "SW" + StartBlockIndex.ToString().PadLeft(4, '0').Substring(0, 4) + BlockCount.ToString().PadLeft(2, '0').Substring(0, 2) + Context.PadRight(len, ' ').Substring(0, len);
            byte[] bs = System.Text.Encoding.ASCII.GetBytes(s);
            byte[] b = new byte[bs.Length + 2];
            bs.CopyTo(b, 0);
            b[b.Length - 2] = this.CheckAdd(b, 0, b.Length - 2);
            b[b.Length - 1] = 3;
            lock (this.SendDataCache)
            {
                if (this.SendDataCache.Count > 10)//如果RFID连接不上不加这个会爆炸的
                {
                    this.SendDataCache.Clear();
                }
                this.SendDataCache.Add(b);
            }
        }

        void DealReceRFIDData(byte[] b, int startindex, int count_rece)
        {
            string s = Encoding.ASCII.GetString(b, 0, count_rece);
            switch (b[0])
            {
                case 0x30:
                    if (count_rece >= 3)//读取数据
                    {
                        string context = Encoding.ASCII.GetString(b, 1, count_rece - 3);
                        RFID_Event?.Invoke(OPerationResult.RFID_读取成功, context);
                    }
                    else
                    {
                        RFID_Event?.Invoke(OPerationResult.RFID_写入成功, "");
                    }
                    break;
                case 0x32:
                    RFID_Event?.Invoke(OPerationResult.RFID_读取或写入失败_Switch_On_Message, "");
                    break;
                case 0x34:
                    RFID_Event?.Invoke(OPerationResult.RFID_读取或写入失败_发送命令错误, "");
                    break;
                case 0x35:
                    RFID_Event?.Invoke(OPerationResult.RFID_读取或写入失败_没有可读写的芯片, "");
                    break;
                case 0x36:
                    RFID_Event?.Invoke(OPerationResult.RFID_读取或写入失败_硬件错误, "");
                    break;
            }
        }

        byte CheckAdd(byte[] b, int start, int len)
        {
            byte re = 0;
            for (int i = start; i < len; i++)
            {
                re += b[i];
            }
            return re;
        }

        #endregion
        #region IO封装

        /// <summary>
        /// 
        /// </summary>
        /// <param name="or">结果</param>
        /// <param name="context">读取数据时返回相关内容,写入数据时为空</param>
        public delegate void IO_Dele(bool[] bools);

        public event IO_Dele IO_Event;

        /// <summary>DI状态
        /// 
        /// </summary>
        bool[] DIStatus = new bool[7];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Index">DIIndex号,比如DI51=1</param>
        /// <param name="Dir">true=上升沿,false=下降沿</param>
        public delegate void IOUpDown_Dele(int Index, bool Dir);

        public event IOUpDown_Dele IOUpDown_Event;

        /// <summary>是否自动读取IO状态
        /// 
        /// </summary>
        public bool AutoAskIOStatus { get; set; }

        /// <summary>自动读取IO状态的间隔,单位:毫秒
        /// 
        /// </summary>
        public int AutoAskIOStatusInterval { get; set; }

        /// <summary>IO模块自动读取的线程
        /// 
        /// </summary>
        System.Threading.Thread AutoReadIO_Thread;

        /// <summary>RFID触发方式,0=自动触发模式,1=按钮触发模式,2=扫描绑定模式
        /// 【从2020-06-02开始,原来1=按钮触发[包括扫描枪触发]被分解为1=按钮触发,2=扫描绑定模式】
        /// </summary>
        int RFIDRECIVETYPE = 0;

        void IOInit()
        {
            AutoReadIO_Thread = new Thread(IOAutoRead) { IsBackground = true };
            AutoReadIO_Thread.Start();
        }

        void IOAutoRead()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(this.AutoAskIOStatusInterval);
                if (this.AutoAskIOStatus)
                {
                    this.IORead();
                }
            }
        }

        public void IORead()
        {
            byte[] b = new byte[] { 0x01, 0x02, 0x00, 0x00, 0x00, 0x08, 0x79, 0xCC };
            lock (SendDataCache)
            {
                if (this.SendDataCache.Count > 10)//如果RFID连接不上不加这个会爆炸的
                {
                    this.SendDataCache.Clear();
                }
                SendDataCache.Add(b);
            }
        }

        public void SleepOpenIO()
        {
            System.Threading.Thread.Sleep(500);
            if (RFIDRECIVETYPE == 1)
            {
                if (!this.AutoAskIOStatus)
                {
                    this.AutoAskIOStatus = true;
                }
            }
        }

        void DealReceIOData(byte[] b, int startindex, int count_rece)
        {
            string s = Encoding.ASCII.GetString(b, startindex, count_rece);
            bool[] DIStatus_Old = new bool[7];
            this.DIStatus.CopyTo(DIStatus_Old, 0);
            this.DIStatus = new bool[7];
            for (int i = 0; i < this.DIStatus.Length; i++)
            {
                this.DIStatus[i] = (b[3] >> i == 1);
            }
            this.IO_Event?.Invoke(this.DIStatus);
            for (int i = 0; i < DIStatus_Old.Length; i++)
            {
                if (DIStatus_Old[i] != this.DIStatus[i])
                {
                    this.IOUpDown_Event?.Invoke(i, this.DIStatus[i]);
                }
            }
        }

        #endregion
        #region 公开方法

        public bool Open()
        {
            this.SocketInit();
            bool re = this.SocketConnect();
            this.IOInit();
            return re;
        }

        /// <summary>关闭并不再使用连接器
        /// 
        /// </summary>
        public void Close()
        {
            this.SocketDisConnect();
            if (this.ReceData_Thread != null)
            {
                try
                {
                    this.ReceData_Thread.Abort();
                }
                catch
                {
                }
            }
            if (this.SendData_Thread != null)
            {
                try
                {
                    this.SendData_Thread.Abort();
                }
                catch
                {
                }
            }
            if (this.AutoReadIO_Thread != null)
            {
                try
                {
                    this.AutoReadIO_Thread.Abort();
                }
                catch
                {
                }
            }
        }

        #endregion
    }
}
