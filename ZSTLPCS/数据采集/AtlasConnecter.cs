using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Net;

namespace ZSTJPCS
{
    /// <summary>阿特拉斯控制器连接器
    /// 
    /// </summary>
    public class AtlasConnecter : DataCollecter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="as_IsDropReceFirstData">是否要丢弃第一个接收到的扭力结果、角度曲线和扭力曲线,默认为true,对于航发IRC Focus型号的扭力枪应该为false</param>
        public AtlasConnecter(AtlasControlerType as_AtlasControlerType = AtlasControlerType.PF4000)
        {
            atlasControlerType = as_AtlasControlerType;
            switch (atlasControlerType)
            {
                case AtlasControlerType.PF4000:
                    break;
                case AtlasControlerType.PF6000:
                    break;
                case AtlasControlerType.IRC_Focus:
                    IsDropReceFirstData = false;
                    break;
                case AtlasControlerType.Multiple:
                    break;
                default:
                    break;
            }
            #region 初始化控制命令
            for (int i = 0; i < 100; i++)//序号从0开始没错，因为第一个是不用的
            {
                //byte[] b = PackCommand_Socket(18, 0, i.ToString().PadLeft(3, '0'));
                //Cmd_ChangePSetIndex.Add(new AtlsCommand(b));
                Cmd_ChangePSetIndex.Add(new AtlsCommand(18, 0, i.ToString().PadLeft(3, '0')));
                Cmd_LoadPSetIndex.Add(new AtlsCommand(12, 1, i.ToString().PadLeft(3, '0')));
            }
            for (int i = 0; i < 100; i++)
            {
                //byte[] b = PackCommand_Socket(38, 0, i.ToString().PadLeft(2, '0'));
                //Cmd_ChangeJSetIndex.Add(new AtlsCommand(b));
                Cmd_ChangeJSetIndex.Add(new AtlsCommand(38, 1, i.ToString().PadLeft(2, '0')));
                Cmd_LoadJSetIndex.Add(new AtlsCommand(32, 0, i.ToString().PadLeft(2, '0')));
            }
            for (int i = 0; i < 50; i++)
            {
                Cmd_ChangeMSetIndex.Add(new AtlsCommand(2606, 0, i.ToString().PadLeft(4, '0')));
            }

            #endregion
        }

        #region 控制器基础命令

        /// <summary>建立连接,进行通讯前必须先建立连接,否则控制器不响应任何数据
        /// 
        /// </summary>
        AtlsCommand Cmd_Connect = new AtlsCommand(1, 0, "");

        /// <summary>关闭连接,程序退出时使用
        /// 
        /// </summary>
        AtlsCommand Cmd_DisConnect = new AtlsCommand(3, 0, "");

        /// <summary>拒绝请求
        /// 
        /// </summary>
        AtlsCommand Cmd_Refuse = new AtlsCommand(4, 0, "");

        /// <summary>接收请求
        /// 
        /// </summary>
        AtlsCommand Cmd_GetAccpetCmd(int mid)
        {
            return new AtlsCommand(5, 0, mid.ToString().PadLeft(4, '0'));
        }

        /// <summary>请求曲线数据_角度和扭力
        /// 
        /// </summary>
        AtlsCommand Cmd_CurveSub = new AtlsCommand(8, 0, "0900001350                             02001002");

        /// <summary>请求曲线参数
        /// 
        /// </summary>
        //public static AtlsCommand CurveSub = PackCommand_SerialPort(8, 0, "0900001350                             01001");
        AtlsCommand Cmd_TracePlottingParameter = new AtlsCommand(8, 0, "090100100");

        /// <summary>设置控制器内部时间
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        AtlsCommand Cmd_SetTime(DateTime dt)
        {
            return new AtlsCommand(82, 0, dt.ToString("yyyy-MM-dd:HH:mm:ss"));
        }

        /// <summary>请求程序索引号PSet
        /// 
        /// </summary>
        AtlsCommand Cmd_AskPSetIndex = new AtlsCommand(10, 0, "");

        /// <summary>切换程序PSet,集合索引代表要切换的程序号
        /// 
        /// </summary>    
        List<AtlsCommand> Cmd_ChangePSetIndex = new List<AtlsCommand>();

        /// <summary>切换程序JSet,集合索引代表要切换的程序号
        /// 
        /// </summary>
        List<AtlsCommand> Cmd_ChangeJSetIndex = new List<AtlsCommand>();

        /// <summary>切换程序MSet,集合索引代表要切换的程序号
        /// 
        /// </summary>
        List<AtlsCommand> Cmd_ChangeMSetIndex = new List<AtlsCommand>();

        /// <summary>读取PSet相关信息,集合索引代表要读取的程序号
        /// 
        /// </summary>
        List<AtlsCommand> Cmd_LoadPSetIndex = new List<AtlsCommand>();

        /// <summary>请求程序索引号JSet,集合索引代表要读取的程序号
        /// 
        /// </summary>
        AtlsCommand Cmd_AskJSetIndex = new AtlsCommand(30, 0, "");

        /// <summary>读取JSet相关信息,集合索引代表要读取的程序号
        /// 
        /// </summary>
        List<AtlsCommand> Cmd_LoadJSetIndex = new List<AtlsCommand>();

        /// <summary>锁定扭矩枪,不允许枪动作
        /// 
        /// </summary>
        AtlsCommand Cmd_Lock = new AtlsCommand(42, 0, "");

        /// <summary>解锁扭矩枪,允许枪动作
        /// 
        /// </summary>
        AtlsCommand Cmd_UnLock = new AtlsCommand(43, 0, "");

        /// <summary>订阅拧紧结果
        /// 
        /// </summary>
        AtlsCommand Cmd_ResultSub = new AtlsCommand(60, 0, "");

        /// <summary>拧紧结果确认
        /// 
        /// </summary>
        AtlsCommand Cmd_ResultACK = new AtlsCommand(62, 0, "");

        /// <summary>多轴结果订阅
        /// 
        /// </summary>
        AtlsCommand Cmd_MultiResultSub = new AtlsCommand(100, 0, "");

        /// <summary>多轴结果确认
        /// 
        /// </summary>
        AtlsCommand Cmd_MultiResultACK = new AtlsCommand(102, 0, "");

        /// <summary>心跳,至少每15秒发送一次,否则控制器会断开连接
        /// 
        /// </summary>
        AtlsCommand Cmd_Heart = new AtlsCommand(9999, 0, "");

        /// <summary>订阅Job结果
        /// 
        /// </summary>
        AtlsCommand Cmd_JobInfoSubscribe = new AtlsCommand(34, 1, "");

        /// <summary>Job结果确认
        /// 
        /// </summary>
        AtlsCommand Cmd_JobInfoAcknowledge = new AtlsCommand(36, 1, "");

        /// <summary>关闭job任务
        /// 
        /// </summary>
        AtlsCommand Cmd_abortJob = new AtlsCommand(127, 1, "");

        #endregion

        #region 字段和属性

        AtlasControlerType atlasControlerType;

        public enum AtlasControlerType
        {
            PF4000,
            PF6000,
            IRC_Focus,
            /// <summary>多轴拧紧控制器
            /// 
            /// </summary>
            Multiple,
        }

        public enum TypePro
        {
            UnKown,
            Para,
            Mode,
            Job
        }

        /// <summary>IP和Port已经初始化
        /// 
        /// </summary>
        bool IsInit = false;

        /// <summary>已经和Socket重新建立连接
        /// 
        /// </summary>
        bool IsSocetConnect = false;

        /// <summary>已经和控制器使用Cmd1建立连接
        /// 
        /// </summary>
        bool IsCmd1Connect = false;

        /// <summary>是否要丢弃第一个接收到的扭力结果数据、角度曲线、扭力曲线
        /// 
        /// </summary>
        bool IsDropReceFirstData = true;

        /// <summary>已经接收到要丢弃的扭力结果数据
        /// 
        /// </summary>
        bool IsReceFirstResult = false;

        /// <summary>已经接收到要丢弃的曲线结果数据-角度
        /// 
        /// </summary>
        bool IsReceFirstCurve_Angle = false;

        /// <summary>已经接收到要丢弃的曲线结果数据-扭力
        /// 
        /// </summary>
        bool IsReceFirstCurve_Tight = false;

        /// <summary>最后一次接收到心跳的时间,如果15秒未接收到心跳数据认为连接已经断开
        /// 
        /// </summary>
        DateTime LastReceHeartTime = DateTime.MinValue;

        System.Threading.Thread SocketSendDataThread;

        System.Threading.Thread SocketReceiveDataThread;

        System.Threading.Thread HeartThread;

        System.Net.Sockets.Socket socket;

        /// <summary>控制器IP
        /// 
        /// </summary>
        String ControlerIP;

        /// <summary>控制器端口号
        /// 
        /// </summary>
        int ControlerPort;

        /// <summary>接收数据缓冲区,只保存上一帧未能正确处理的数据
        /// 
        /// </summary>
        List<byte> ReceiveDataCache = new List<byte>();

        /// <summary>发送数据缓冲区
        /// 
        /// </summary>
        List<AtlsCommand> SendDataCache = new List<AtlsCommand>();

        #endregion

        #region 公开接口

        /// <summary>
        /// 向指定IP和端口号的控制器请求并开始自动连
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="Port">默认4545</param>
        public void Init(string ip, int Port)
        {
            ControlerIP = ip;
            ControlerPort = Port;

            #region 初始化多线程用于数据收发

            if (SocketSendDataThread != null)
            {
                SocketSendDataThread.Abort();
            }
            if (SocketReceiveDataThread != null)
            {
                SocketReceiveDataThread.Abort();
            }
            if (HeartThread != null)
            {
                HeartThread.Abort();
            }

            SocketSendDataThread = new System.Threading.Thread(SocketSendData) { IsBackground = true };
            SocketSendDataThread.Start();

            SocketReceiveDataThread = new System.Threading.Thread(SocketReceiveData) { IsBackground = true };
            SocketReceiveDataThread.Start();

            HeartThread = new System.Threading.Thread(HeartThread_Method) { IsBackground = true };
            HeartThread.Start();

            #endregion

            IsInit = true;
        }

        /// <summary>
        /// 向指定IP和端口号的控制器请求并开始自动连
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="Port">默认4545</param>
        public void Init(IPEndPoint ip)
        {
            Init(ip.Address.ToString(), ip.Port);
        }

        /// <summary>
        /// 查询PSetID号
        /// </summary>
        public void AskPSetIndex()
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(Cmd_AskPSetIndex);
            }
        }

        /// <summary>
        /// 读取指定序号的程序的相关信息
        /// </summary>
        /// <param name="index"></param>
        public void LoadPSetIndex(int index)
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(Cmd_LoadPSetIndex[index]);
            }
        }

        ///// <summary>
        ///// 设置PSet
        ///// </summary>
        ///// <param name="index">index=1代表设定为Pset1</param>
        ///// <returns></returns>
        //public void ChangePSet(int index)
        //{
        //    lock (SendDataCache)
        //    {
        //        //SendDataCache.Add();
        //        SendDataCache.Add(Cmd_ChangePSetIndex[index]);
        //    }
        //}

        /// <summary>
        /// 查询JSetID号
        /// </summary>
        public void AskJSetIndex()
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(Cmd_AskJSetIndex);
            }
        }
        /// <summary>
        /// 关闭Job程式
        /// </summary>
        /// <returns></returns>
        public void AbortJob()
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(Cmd_abortJob);
            }
        }

        ///// <summary>
        ///// 设置PSet
        ///// </summary>
        ///// <param name="index">index=1代表设定为Pset1</param>
        ///// <returns></returns>
        //public void ChangeJSet(int index)
        //{
        //    lock (SendDataCache)
        //    {
        //        SendDataCache.Add(Cmd_ChangeJSetIndex[index]);
        //    }
        //}

        /// <summary>设定程序号
        /// 
        /// </summary>
        /// <param name="typePro"></param>
        /// <param name="id"></param>
        public void SelectPro(TypePro typePro, int id)
        {
            lock (SendDataCache)
            {
                switch (typePro)
                {
                    case TypePro.Para:
                        if (id >= 0 && id <= Cmd_ChangePSetIndex.Count - 1)
                        {
                            SendDataCache.Add(Cmd_ChangePSetIndex[id]);
                        }
                        break;
                    //case TypePro.Mode:

                    //    break;
                    case TypePro.Job:
                        if (id >= 0 && id <= Cmd_ChangeJSetIndex.Count - 1)
                        {
                            SendDataCache.Add(Cmd_ChangeJSetIndex[id]);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>专门用于MSet下载程序号的命令
        /// 
        /// </summary>
        /// <param name="prostr"></param>
        public void SelectPro(string prostr)
        {
            var cmd_temp = new AtlsCommand(150, 0, prostr);
            SendDataCache.Add(cmd_temp);
        }

        /// <summary>
        /// 读取指定序号的Job相关信息
        /// </summary>
        /// <param name="index"></param>
        public void LoadJSetIndex(int index)
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(Cmd_LoadJSetIndex[index]);
            }
        }

        /// <summary>
        /// 锁定或解锁扭矩枪
        /// </summary>
        /// <param name="Lock">true-->lock false-->unlock</param>
        public void Lock(bool Lock)
        {
            if (Lock)
            {
                lock (SendDataCache)
                {
                    SendDataCache.Add(Cmd_Lock);
                }
            }
            else
            {
                lock (SendDataCache)
                {
                    SendDataCache.Add(Cmd_UnLock);
                }
            }
        }

        /// <summary>
        /// 设置控制器时间
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public void SetTime(DateTime dt)
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(Cmd_SetTime(dt));
            }
        }

        /// <summary>设定控制器DO状态(打开或关闭)
        /// 
        /// </summary>
        /// <param name="index">DO号，范围1-10</param>
        /// <param name="enable">设定为打开</param>
        public void SetDO(int index, bool enable)
        {
            string message = "333333333".Insert(index - 1, enable ? "1" : "0");
            //string message = "333333333".Insert(index - 1, "2");
            AtlsCommand a = new AtlsCommand(200, 1, message);
            //string message = "11111111";
            //AtlsCommand a = new AtlsCommand(211, 1, message);
            lock (SendDataCache)
            {
                SendDataCache.Add(a);
            }
        }

        /// <summary>
        /// 向控制器发送条码
        /// </summary>
        /// <param name="vin"></param>
        public void SetVin(String vin)
        {
            AtlsCommand a = new AtlsCommand(50, 1, vin);
            lock (SendDataCache)
            {
                SendDataCache.Add(a);
            }
        }

        /// <summary>发送一个指定的ATLS命令
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void SendCmd(AtlsCommand a)
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(a);
            }
        }

        public delegate void RetrunAtlsCommand_Dele(AtlsCommand a);

        /// <summary>命令结果
        /// 
        /// </summary>
        public event RetrunAtlsCommand_Dele RetrunAtlsCommand_Event;

        /// <summary>接收到心跳数据
        /// 
        /// </summary>
        public event Action HeartBreak_Event;

        public delegate void RetrunMessage_Dele(string s);

        /// <summary>一般消息事件
        /// 
        /// </summary>
        public event RetrunMessage_Dele RetrunMessage;

        /// <summary>
        /// 关闭与控制器的连接,关闭通讯资源，在程序退出时使用
        /// </summary>
        public void Close()
        {
            if (SocketSendDataThread != null)
            {
                SocketSendDataThread.Abort();
            }
            if (SocketReceiveDataThread != null)
            {
                SocketReceiveDataThread.Abort();
            }
            if (socket.Connected)
            {
                socket.Disconnect(true);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 连接
        /// </summary>
        void Connect()
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(Cmd_Connect);
            }
        }

        /// <summary>
        /// 向控制器订阅拧紧结果
        /// </summary>
        void SubResult()
        {
            lock (SendDataCache)
            {
                switch (atlasControlerType)
                {
                    case AtlasControlerType.Multiple:
                        SendDataCache.Add(Cmd_MultiResultSub);
                        break;
                    default:
                        SendDataCache.Add(Cmd_ResultSub);
                        break;
                }
            }
        }

        /// <summary>
        /// 向控制器订阅拧紧曲线_角度和扭力
        /// </summary>
        void SubCruve()
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(Cmd_CurveSub);
            }
        }

        /// <summary>发送心跳
        /// 
        /// </summary>
        /// <returns></returns>
        void Heart()
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(Cmd_Heart);
            }
        }

        /// <summary>订阅Job信息
        /// 
        /// </summary>
        /// <returns></returns>
        void SubJobInfo()
        {
            lock (SendDataCache)
            {
                SendDataCache.Add(Cmd_JobInfoSubscribe);
            }
        }

        #endregion

        void SocketSendData()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(100);

                if (!IsInit)//检测程序有没有初始化
                {
                    continue;
                }
                if (socket == null || !socket.Connected)//Socket重连接
                {
                    try
                    {
                        if (socket != null)
                        {
                            try
                            {
                                socket.Disconnect(true);
                            }
                            catch (Exception)
                            {

                            }
                        }
                        socket = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
                            System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                        socket.Connect(ControlerIP, ControlerPort);
                        TraceWrite("Socket连接成功");
                        IsSocetConnect = true;
                        IsCmd1Connect = false;
                    }
                    catch /*(Exception exc)*/
                    {
                        // TraceWrite("Socket连接失败" + exc.Message);
                        IsSocetConnect = false;
                        System.Threading.Thread.Sleep(5000);
                        continue;
                    }
                }

                //已经15秒没有接收到心跳数据了 或 Socket重新建立了一次连接 需要重新使用Cmd1建立连接
                if ((DateTime.Now - LastReceHeartTime).TotalSeconds >= 15 || !IsCmd1Connect)
                {
                    try
                    {
                        socket.Send(Cmd_Connect.命令_ByteList);
                        TraceWrite("Socket发送数据成功,命令号:" + Cmd_Connect.命令号);
                        System.Threading.Thread.Sleep(2000);
                    }
                    catch { }
                }

                if (!IsCmd1Connect)
                {
                    continue;
                }

                //if (!IsSubCurve)//还未订阅数据
                //{
                //    socket.Send(Cmd_CurveSub.命令_ByteList);
                //    System.Threading.Thread.Sleep(1000);
                //}

                //if (!IsSubCurve)//还未订阅数据
                //{
                //    socket.Send(Cmd_ResultSub.命令_ByteList);
                //    System.Threading.Thread.Sleep(1000);
                //}

                //发送数据
                AtlsCommand cmd = null;
                lock (SendDataCache)
                {
                    if (SendDataCache.Count > 0)
                    {
                        cmd = SendDataCache[0];
                        SendDataCache.RemoveAt(0);
                    }
                }
                if (cmd == null)
                {
                    continue;
                }
                try
                {
                    socket.Send(cmd.命令_ByteList);
                    if (cmd.命令号 != 9999)
                    {
                        TraceWrite("Socket发送数据成功,命令号:" + cmd.命令号 + ",长度 " + cmd.命令_ByteList.Length);
                        AppMessage.Add("Socket发送数据成功,命令号:" + cmd.命令号 + ",长度 " + cmd.命令_ByteList.Length, AppMessage.MsgType.阿特拉斯网络指令);
                    }
                }
                catch /*(Exception e)*/
                {
                    TraceWrite("Socket发送数据失败,命令号:" + cmd.命令号 + ",长度 " + cmd.命令_ByteList.Length);
                    AppMessage.Add("Socket发送数据失败,命令号:" + cmd.命令号 + ",长度 " + cmd.命令_ByteList.Length, AppMessage.MsgType.阿特拉斯网络指令);
                }
                //System.Threading.Thread.Sleep(150);//2021-03-29修改,阿特拉斯控制器要求两个字符之间间隔不超过100ms
            }
        }

        void SocketReceiveData()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(1);
                DateTime BeginTime = DateTime.Now;
                if (socket != null && socket.Connected && socket.Available != 0)
                {
                    byte[] b = new byte[socket.Available];
                    try
                    {
                        socket.Receive(b);
                        TraceWrite("接收数据长度:" + b.Length + "个");
                    }
                    catch
                    {
                        continue;
                    }
                    //byte[] b_OK = null;
                    //byte[] b_OK = b;
                    AtlsCommand a = null;
                    #region 数据缓存处理
                    a = CheckByteListIsCmd(b, out int outlen, out string err);//看新数据能不能处理成正确的Cmd
                    if (a != null)//解析成功了
                    {
                        ReceiveDataCache.Clear();
                    }
                    else//解析不成功
                    {
                        ReceiveDataCache.AddRange(b);
                        a = CheckByteListIsCmd(ReceiveDataCache, out outlen, out err);//看组合数据能不能处理成正确的Cmd
                        if (a != null)//解析成功了
                        {
                            ReceiveDataCache.Clear();
                        }
                        else
                        {
                            ReceiveDataCache.Clear();//这个地方的隐患是基于认知为任何数据最多只分包两次即可完成,如果实际上在三次以上则无法解析成功,比如使用串口通讯时
                            ReceiveDataCache.AddRange(b);
                        }
                    }

                    //if (ReceiveDataCache.Count != 0)
                    //{
                    //if (CheckByteListIsCmd(b,out outlen))//新数据能承诺更为
                    //{
                    //}
                    //ReceiveDataCache.AddRange(b);//组合起来看有没有可能
                    //if (CheckByteListIsCmd(ReceiveDataCache, out outlen))//组合起来可以成新数据
                    //{
                    //    a = new AtlsCommand(ReceiveDataCache.ToArray());
                    //}
                    //else//如果不能,看新数据能不能成为Cmd
                    //{
                    //    if (outlen == 0)//数据无法被识别并且有错误
                    //    {
                    //        ReceiveDataCache.Clear();
                    //    }
                    //    if (CheckByteListIsCmd(b))
                    //    {
                    //        a = new AtlsCommand(b);
                    //        ReceiveDataCache.Clear();
                    //    }
                    //}
                    if (a == null)
                    {
                        continue;
                    }
                    //else//新的数据可以形成一个正确的Cmd
                    //{
                    //    ReceiveDataCache.Clear();
                    //    AtlsCommand cmd = new AtlsCommand(b);
                    //}
                    //ReceiveDataCache.AddRange(b);
                    //byte[] b_len_temp = new byte[4] { ReceiveDataCache[0], ReceiveDataCache[1], ReceiveDataCache[2], ReceiveDataCache[3] };
                    //int len = 0;
                    //try
                    //{
                    //    //Console.WriteLine("数据缓存处理：" + Encoding.ASCII.GetString(b_len_temp));
                    //    len = Convert.ToInt32(Encoding.ASCII.GetString(b_len_temp));
                    //}
                    //catch
                    //{
                    //    ReceiveDataCache.Clear();
                    //    len = 0;
                    //}
                    //if (len > 5000)//5000最大的数据是不太可能出现的
                    //{
                    //    continue;
                    //}
                    //if (len != 0 && len <= ReceiveDataCache.Count)//ReceiveDataCache构成一组完整的数据
                    //{
                    //    int 实际长度 = Math.Min(len + 1, ReceiveDataCache.Count);
                    //    b_OK = new byte[实际长度];
                    //    ReceiveDataCache.CopyTo(0, b_OK, 0, b_OK.Length);
                    //    ReceiveDataCache.RemoveRange(0, b_OK.Length);
                    //}
                    #endregion
                    //if (b_OK == null)
                    //{
                    //    continue;
                    //}
                    //AtlsCommand a = new AtlsCommand(b_OK);
                    #region 解析可能存在的特殊的MID900和61等
                    if (a.命令号 != 9999)
                    {
                        TraceWrite("接收数据,命令号为:" + a.命令号);
                    }
                    if (a.命令号 == 900)
                    {
                        lock (SendDataCache)
                        {
                            SendDataCache.Add(Cmd_GetAccpetCmd(900));
                        }
                        TraceCurveDataMessage t = a.附加解析类 as TraceCurveDataMessage;
                        if (t.TraceType == 1)
                        {
                            a.附加解析类 = t;
                            TraceWrite("解析为角度曲线数据");
                        }
                        else if (t.TraceType == 2)
                        {
                            a.附加解析类 = t;
                            TraceWrite("解析为扭力曲线数据");
                        }
                    }
                    else if (a.命令号 == 61)
                    {
                        lock (SendDataCache)
                        {
                            SendDataCache.Add(Cmd_ResultACK);
                        }
                    }
                    else if (a.命令号 == 101)//多轴拧紧
                    {
                        lock (SendDataCache)
                        {
                            SendDataCache.Add(Cmd_MultiResultACK);
                        }
                    }
                    else if (a.命令号 == 2)//已经建立好连接
                    {
                        IsCmd1Connect = true;
                        //IsSocetConnect = false;
                        LastReceHeartTime = DateTime.Now;
                        lock (SendDataCache)//清理待发数据
                        {
                            SendDataCache.Clear();
                        }
                        IsReceFirstCurve_Angle = IsDropReceFirstData;
                        IsReceFirstCurve_Tight = IsDropReceFirstData;
                        IsReceFirstResult = IsDropReceFirstData;

                        SubResult();


                        //SubCruve();
                        //SubJobInfo();
                        //System.Threading.Thread.Sleep(500)
                        //AtlasConnect.AskPSetIndex();
                        //AtlasConnect.AskJSetIndex();
                    }
                    else if (a.命令号 == 9999)
                    {
                        LastReceHeartTime = DateTime.Now;
                    }
                    else if (a.命令号 == 35)
                    {
                        JobInfo ji = a.附加解析类 as JobInfo;
                        System.Diagnostics.Trace.WriteLine("JobInfo | JobBatchCounter:" + ji.JobBatchCounter + " | JobStatus:" + ji.JobStatus);
                        lock (SendDataCache)
                        {
                            SendDataCache.Add(Cmd_JobInfoAcknowledge);
                        }
                    }
                    #endregion
                    #region 引发事件
                    if (a.命令号 == 9999)
                    {
                        HeartBreak_Event?.Invoke();
                    }
                    if (RetrunAtlsCommand_Event != null)
                    {
                        switch (a.命令号)
                        {
                            case 9999:

                                break;
                            case 61:
                                if (!IsReceFirstResult)
                                {
                                    IsReceFirstResult = true;
                                }
                                else
                                {
                                    RetrunAtlsCommand_Event(a);
                                    if (a.附加解析类 is TighteningResult tr)
                                    {
                                        OnGetData_Event(new TightResult(tr.TighteningStatus == 1, tr.Torque, tr.Angle));
                                        //AddReceData(new TightResult(tr.TighteningStatus == 1, tr.Torque, tr.Angle));
                                    }
                                }
                                break;
                            case 101:
                                if (!IsReceFirstResult)
                                {
                                    IsReceFirstResult = true;
                                }
                                else
                                {
                                    RetrunAtlsCommand_Event(a);
                                    if (a.附加解析类 is MultiSpindleResult tr)
                                    {
                                        if (tr.spindleStatuss.Count > 0)
                                        {
                                            var ss_temp = tr.spindleStatuss[0];
                                            OnGetData_Event(new TightResult(ss_temp.AllOK == 1, ss_temp.Torque, ss_temp.Angle));
                                            //AddReceData(new TightResult(ss_temp.AllOK == 1, ss_temp.Torque, ss_temp.Angle));
                                        }
                                    }
                                }
                                break;
                            case 900:
                                TraceCurveDataMessage tcm = a.附加解析类 as TraceCurveDataMessage;
                                switch (tcm.TraceType)
                                {
                                    case 1://角度
                                        if (!IsReceFirstCurve_Angle)
                                        {
                                            IsReceFirstCurve_Angle = true;
                                        }
                                        else
                                        {
                                            RetrunAtlsCommand_Event(a);
                                            OnMsg_Event(a);
                                        }
                                        break;
                                    case 2://扭力
                                        if (!IsReceFirstCurve_Tight)
                                        {
                                            IsReceFirstCurve_Tight = true;
                                        }
                                        else
                                        {
                                            RetrunAtlsCommand_Event(a);
                                            OnMsg_Event(a);
                                        }
                                        break;
                                }
                                break;
                            default:
                                RetrunAtlsCommand_Event(a);
                                OnMsg_Event(a);
                                break;
                        }
                    }
                    #endregion
                    //ReceiveDataCache.Clear();
                }
            }
        }

        void HeartThread_Method()
        {
            while (true)
            {
                if (IsCmd1Connect)
                {
                    lock (SendDataCache)
                    {
                        SendDataCache.Add(Cmd_Heart);
                    }
                }
                System.Threading.Thread.Sleep(5000);
            }
        }

        #region 附加属性类

        /// <summary>MID=11时返回PSET数据
        /// 
        /// </summary>
        public class PsetResult
        {
            public PsetResult(byte[] b, string s)
            {
                int startIndex = 1;
                GetValue(s, 20 - startIndex, 4, out int PNum);
                if (PNum > 0)
                {
                    int psetNo;
                    for (int i = 0; i < PNum; i++)
                    {
                        GetValue(s, 24 + 3 * i - startIndex, 3, out psetNo);
                        psetList.Add(psetNo);
                    }
                }
            }

            /// <summary>编号列表
            /// 
            /// </summary>
            public List<int> psetList = new List<int>();
        }

        /// <summary>MID=13时返回PSET详情
        /// 
        /// </summary>
        public class PsetDetail
        {
            public PsetDetail(byte[] b, string s)
            {
                int startIndex = 1;
                GetValue(s, 23 - startIndex, 3, out PID);
                GetValue(s, 28 - startIndex, 25, out PIDName);
                GetValue(s, 62 - startIndex, 6, out torqueMin);
                GetValue(s, 70 - startIndex, 6, out torqueMax);
                GetValue(s, 86 - startIndex, 5, out angleMin);
                GetValue(s, 93 - startIndex, 5, out angleMax);
            }

            /// <summary>当前PID
            /// 
            /// </summary>
            public int PID;

            /// <summary>当前PID名称
            /// 
            /// </summary>
            public string PIDName;

            /// <summary>扭力下限
            /// 
            /// </summary>
            public decimal torqueMin;

            /// <summary>扭力上限
            /// 
            /// </summary>
            public decimal torqueMax;

            /// <summary>角度上限
            /// 
            /// </summary>
            public int angleMin;

            /// <summary>角度下限
            /// 
            /// </summary>
            public int angleMax;
        }

        /// <summary>MID=31时返回PSET数据
        /// 
        /// </summary>
        public class JsetResult
        {
            public JsetResult(byte[] b, string s)
            {
                int startIndex = 1; int JNum;
                GetValue(s, 21 - startIndex, 2, out JNum);
                if (JNum > 0)
                {
                    int JsetNo;
                    for (int i = 0; i < JNum; i++)
                    {
                        GetValue(s, 23 + 2 * i - startIndex, 2, out JsetNo);
                        jsetList.Add(JsetNo);
                    }
                }
            }

            /// <summary>编号列表
            /// 
            /// </summary>
            public List<int> jsetList = new List<int>();
        }

        /// <summary>MID=33时返回JSET详情
        /// 
        /// </summary>
        public class JsetDetail
        {
            public JsetDetail(byte[] b, string s)
            {
                int startIndex = 1;
                GetValue(s, 23 - startIndex, 2, out JID);
                GetValue(s, 28 - startIndex, 51 - 27 + 1, out JIDName);
                GetValue(s, 88 - startIndex, 2, out NumberOfPID);
                for (int i = 0; i < NumberOfPID; i++)
                {
                    //int id;
                    //  00:001:1:01;
                    //PID
                    PTest pt = new PTest();
                    string s_temp = s.Substring(92 - startIndex + 12 * i, 12);
                    pt.ChannelID = s_temp.Substring(0, 2);
                    pt.TypeID = s_temp.Substring(3, 3);
                    pt.AutoValue = s_temp.Substring(7, 1);
                    pt.BatchSize = s_temp.Substring(9, 2);
                    PIDs.Add(pt);
                }

            }

            /// <summary>当前JID
            /// 
            /// </summary>
            public int JID;

            /// <summary>当前JID名称
            /// 
            /// </summary>
            public string JIDName;

            public int NumberOfPID;

            public List<PTest> PIDs = new List<PTest>();

            public class PTest
            {
                public string ChannelID = "";

                public string TypeID = "";

                public string AutoValue = "";

                public string BatchSize = "";
            }

        }

        /// <summary>拧紧结果
        /// 
        /// </summary>
        public class TighteningResult
        {
            /// <summary>从接收命令消息体构造拧紧结果
            /// 
            /// </summary>
            /// <param name="s"></param>
            public TighteningResult(byte[] b, string s)
            {
                int startIndex = 1;
                GetValue(s, 23 - startIndex, 4, out CellId);
                GetValue(s, 29 - startIndex, 2, out ChannelId);
                GetValue(s, 33 - startIndex, 25, out TorqueControllerName);
                GetValue(s, 60 - startIndex, 25, out VinNumber);
                GetValue(s, 87 - startIndex, 2, out JsetId);
                GetValue(s, 91 - startIndex, 3, out PsetId);
                GetValue(s, 96 - startIndex, 4, out BatchSize);
                GetValue(s, 102 - startIndex, 4, out BatchCounter);

                GetValue(s, 108 - startIndex, 1, out TighteningStatus);
                GetValue(s, 111 - startIndex, 1, out TorqueStatus);
                GetValue(s, 114 - startIndex, 1, out AngleStatus);
                GetValue(s, 117 - startIndex, 6, out TorqueMinLimit);
                GetValue(s, 125 - startIndex, 6, out TorqueMaxLimit);
                GetValue(s, 133 - startIndex, 6, out TorqueFinalTarget);
                GetValue(s, 141 - startIndex, 6, out Torque);
                GetValue(s, 149 - startIndex, 5, out AngleMin);
                GetValue(s, 156 - startIndex, 5, out AngleMax);
                GetValue(s, 163 - startIndex, 5, out FinalAngleTarget);
                GetValue(s, 170 - startIndex, 5, out Angle);
                GetValue(s, 177 - startIndex, 19, out TimeStamp);
                GetValue(s, 198 - startIndex, 19, out DatetimeOfLastChangeInParameterSetSettings);
                GetValue(s, 219 - startIndex, 1, out BatchStatus);
                GetValue(s, 222 - startIndex, 10, out TighteningId);
            }

            public int CellId;
            public int ChannelId;
            public string TorqueControllerName;
            public string VinNumber;
            public int JsetId;
            public int PsetId;
            //public int Strategy;
            //public int StrategyOption;
            public int BatchSize;
            public int BatchCounter;
            /// <summary>
            ///拧紧结果 0=tightening NOK, 1=tightening OK
            /// </summary>
            public int TighteningStatus;
            /// <summary>0=Low, 1=OK, 2=High
            /// 
            /// </summary>
            public int TorqueStatus;
            /// <summary>0=Low, 1=OK, 2=High
            /// 
            /// </summary>
            public int AngleStatus;
            public decimal TorqueMinLimit;
            public decimal TorqueMaxLimit;
            public decimal TorqueFinalTarget;
            public decimal Torque;
            public int AngleMin;
            public int AngleMax;
            public int FinalAngleTarget;
            public int Angle;
            public DateTime TimeStamp;
            public DateTime DatetimeOfLastChangeInParameterSetSettings;
            /// <summary>0=batch NOK (batch not completed), 1=batch OK, 2=batch not used
            /// 
            /// </summary>
            public int BatchStatus;
            public int TighteningId;
            //public int AngleStatus;
            //public int RundownAngleStatus;
            //public int CurrentMonitoringStatus;
            //public int Self_TapStatus;
            //public int PrevailTorqueMonitoringStatus;
            //public int PrevailTorqueCompensateStatus;
            //public int TighteningErrorStatus;
            //public int TorqueStatus;
            //public int TorqueStatus;
            //public int TorqueStatus;
        }

        public class MultiSpindleResult
        {
            public MultiSpindleResult(byte[] b, string s)
            {
                int offset = -1;
                GetValue(s, offset + 09, 03, out Version);
                GetValue(s, offset + 23, 02, out NumOfSpindles);
                GetValue(s, offset + 27, 25, out VinNumber);
                GetValue(s, offset + 54, 02, out JsetId);
                GetValue(s, offset + 58, 03, out PsetId);
                GetValue(s, offset + 75, 01, out BatchStatus);
                GetValue(s, offset + 165, 05, out SyncTighteningID);
                GetValue(s, offset + 172, 01, out SyncOverallStatus);

                if (s.Length >= 174 + 18 * NumOfSpindles)
                {
                    for (int i = 0; i < NumOfSpindles; i++)
                    {
                        spindleStatuss.Add(new SpindleStatus(s.Substring(174 + i * 18, 18)));
                    }
                }
                else
                {
                    Console.WriteLine(DateTime.Now + "-->多轴拧紧数据长度异常-->" + s.Length + "-->" + s);
                }
            }
            public int Version;//修订版本号
            public int NumOfSpindles;//拧紧枪数量
            public string VinNumber;
            public int JsetId;
            public int PsetId;
            public int BatchStatus;
            public int SyncTighteningID;//The sync tightening ID is a unique ID for each sync tightening result.
            public int SyncOverallStatus;//是否全部OK   1-->OK 2-->Nok
            public List<SpindleStatus> spindleStatuss = new List<SpindleStatus>();

            public class SpindleStatus
            {
                public SpindleStatus(string s)
                {
                    int offset = -1;
                    GetValue(s, offset + 01, 2, out SpindleNumber);
                    GetValue(s, offset + 05, 1, out AllOK);
                    GetValue(s, offset + 06, 1, out TorqueStatus);
                    GetValue(s, offset + 07, 6, out Torque);
                    GetValue(s, offset + 13, 1, out AngleStatus);
                    GetValue(s, offset + 14, 5, out Angle);
                }
                public int SpindleNumber;
                public int AllOK;
                public int TorqueStatus;// 0=Low, 1=OK, 2 =High
                public decimal Torque;//扭力
                public int AngleStatus;//angle status of each spindle. 0=NOK, 1=OK
                public int Angle;
            }
        }

        /// <summary>曲线结果
        /// 
        /// </summary>
        public class TraceCurveDataMessage
        {
            public TraceCurveDataMessage(byte[] b, string s)
            {
                GetValue(s, 20, 10, out ResultDataIdentifier);
                GetValue(s, 30, 19, out TimeStamp);
                GetValue(s, 49, 3, out NumberOfPid);

                int index = 52;
                for (int i = 0; i < NumberOfPid; i++)
                {
                    PID pidTemp = new PID();
                    GetValue(s, index, 5, out pidTemp.ParameterId);
                    GetValue(s, index + 5, 3, out pidTemp.Length);
                    GetValue(s, index + 8, 2, out pidTemp.DataType);
                    GetValue(s, index + 10, 3, out pidTemp.Unit);
                    GetValue(s, index + 13, 4, out pidTemp.StepNo);
                    GetValue(s, index + 17, pidTemp.Length, out pidTemp.DataValue);
                    index += 17 + pidTemp.Length;
                    PIDs.Add(pidTemp);
                }

                GetValue(s, index, 2, out TraceType); index += 2;
                GetValue(s, index, 2, out TransducerType); index += 2;
                GetValue(s, index, 3, out Unit); index += 3;
                GetValue(s, index, 3, out NumberOfParameterDataFields); index += 3;

                for (int i = 0; i < NumberOfParameterDataFields; i++)
                {
                    PID parameterTemp = new PID();
                    GetValue(s, index, 5, out parameterTemp.ParameterId);
                    GetValue(s, index + 5, 3, out parameterTemp.Length);
                    GetValue(s, index + 8, 2, out parameterTemp.DataType);
                    GetValue(s, index + 10, 3, out parameterTemp.Unit);
                    GetValue(s, index + 13, 4, out parameterTemp.StepNo);
                    GetValue(s, index + 17, parameterTemp.Length, out parameterTemp.DataValue);
                    index += 17 + parameterTemp.Length;
                    Parameters.Add(parameterTemp);
                }

                GetValue(s, index, 3, out NumberOfResolutionDataFields); index += 3;
                for (int i = 0; i < NumberOfResolutionDataFields; i++)
                {
                    ResolutionField resolutionFieldTemp = new ResolutionField();
                    GetValue(s, index, 5, out resolutionFieldTemp.FirstIndex);
                    GetValue(s, index + 5, 5, out resolutionFieldTemp.LastIndex);
                    GetValue(s, index + 10, 3, out resolutionFieldTemp.Length);
                    GetValue(s, index + 13, 2, out resolutionFieldTemp.DataType);
                    GetValue(s, index + 15, 3, out resolutionFieldTemp.Unit);
                    GetValue(s, index + 18, resolutionFieldTemp.Length, out resolutionFieldTemp.TimeValue);
                    index += 18 + resolutionFieldTemp.Length;
                    ResolutionFields.Add(resolutionFieldTemp);
                }
                GetValue(s, index, 5, out NumberOfTraceSamples); index += 5;
                index++;
                TraceSample = new short[NumberOfTraceSamples];
                for (int i = 0; i < NumberOfTraceSamples; i++)
                {
                    Int16 i_Temp = (short)(b[index + 2 * i] * 256 + b[index + 1 + 2 * i]);
                    TraceSample[i] = i_Temp;
                }

                TraceSample_ByteList = new byte[TraceSample.Length * 2];
                for (int i = 0; i < TraceSample.Length; i++)
                {
                    //byte[] b_temp = BitConverter.GetBytes(TraceSample[i]);
                    TraceSample_ByteList[i * 2 + 0] = (byte)(TraceSample[i] >> 8);
                    TraceSample_ByteList[i * 2 + 1] = (byte)TraceSample[i];
                }
            }

            /// <summary>唯一标识符
            /// 
            /// </summary>
            public string ResultDataIdentifier;

            public DateTime TimeStamp;

            public int NumberOfPid;

            public List<PID> PIDs = new List<PID>();

            public class PID
            {
                public string ParameterId;

                public int Length;

                public int DataType;

                public int Unit;

                public int StepNo;

                public string DataValue;
            }

            public int TraceType;

            public int TransducerType;

            public int Unit;

            public int NumberOfParameterDataFields;

            public List<PID> Parameters = new List<PID>();

            public int NumberOfResolutionDataFields;

            public List<ResolutionField> ResolutionFields = new List<ResolutionField>();

            public class ResolutionField
            {
                public int FirstIndex;

                public int LastIndex;

                public int Length;

                public int DataType;

                public int Unit;

                public string TimeValue;
            }

            public int NumberOfTraceSamples;

            public Int16[] TraceSample;

            public byte[] TraceSample_ByteList;
        }

        /// <summary>曲线参数
        /// 
        /// </summary>
        public class TracePlottingParameterMessage
        {
            public TracePlottingParameterMessage(byte[] b, string s)
            {
                GetValue(s, 20, 10, out ResultDataIdentifier);
                GetValue(s, 30, 19, out TimeStamp);
                GetValue(s, 49, 3, out NumberOfPID);

                int index = 52;
                for (int i = 0; i < NumberOfPID; i++)
                {
                    PID pid_temp = new PID();
                    GetValue(s, index, 5, out pid_temp.ParameterID);
                    GetValue(s, index + 5, 3, out pid_temp.Length);
                    GetValue(s, index + 8, 2, out pid_temp.DataType);
                    GetValue(s, index + 10, 3, out pid_temp.Unit);
                    GetValue(s, index + 13, 4, out pid_temp.StepNo);
                    GetValue(s, index + 17, pid_temp.Length, out pid_temp.DataValue);
                    index += 17 + pid_temp.Length;
                    PIDs.Add(pid_temp);
                }
            }

            /// <summary>唯一标识符
            /// 
            /// </summary>
            public string ResultDataIdentifier;

            public DateTime TimeStamp;

            public int NumberOfPID;

            public List<PID> PIDs = new List<PID>();

            public class PID
            {
                public string ParameterID;

                public int Length;

                public int DataType;

                public int Unit;

                public int StepNo;

                public string DataValue;
            }
        }

        public class JobInfo
        {
            public JobInfo(byte[] b, string s)
            {
                GetValue(s, 23 - 1, 2, out JobID);
                GetValue(s, 27 - 1, 1, out JobStatus);
                GetValue(s, 30 - 1, 1, out JobBatchMode);
                GetValue(s, 33 - 1, 4, out JobBatchSize);
                GetValue(s, 39 - 1, 4, out JobBatchCounter);

                ////GetValue(s, 45 - 1, 19, out TimeStamp);
            }

            public int JobID;

            /// <summary>0=Job not completed, 1=Job OK, 2=Job NOK
            /// 
            /// </summary>
            public int JobStatus;

            /// <summary>0= only the OK tightenings are counted
            ///1= both the OK and NOK tightenings are counted            
            /// </summary>
            public int JobBatchMode;

            /// <summary>Job最大螺丝数目
            /// 
            /// </summary>
            public int JobBatchSize;

            /// <summary>当前螺丝计数
            /// 
            /// </summary>
            public int JobBatchCounter;

            public DateTime TimeStamp;
        }

        /// <summary>04命令返回的拒绝原因
        /// 
        /// </summary>
        public class RefuseCommand
        {
            public RefuseCommand(byte[] b, string s)
            {
                GetValue(s, 21 - 1, 4, out RefuseMID);
                GetValue(s, 25 - 1, 2, out RefuseReasonID);
                if (RefuseReason_Dict.TryGetValue(RefuseReasonID, out string RefuseReason_temp))
                {
                    RefuseReason = RefuseReason_temp;
                }
                else
                {
                    RefuseReason = "未知的拒绝原因ID";
                }
            }

            /// <summary>拒绝原因ID和描述的对照表
            /// 
            /// </summary>
            public static Dictionary<int, string> RefuseReason_Dict = new Dictionary<int, string> {
                {00,"No Error" },
                {01,"Invalid data" },
                {02,"Parameter set ID not present" },
                {03,"Parameter set can not be set." },
                {04,"Parameter set not running" },
                {06,"VIN upload subscription already exists" },
                {07,"VIN upload subscription does not exists" },
                {08,"VIN input source not granted" },
                {09,"Last tightening result subscription already exists" },
                {10,"Last tightening result subscription does not exist" },
                {11,"Alarm subscription already exists" },
                {12,"Alarm subscription does not exist" },
                {13,"Parameter set selection subscription already exists" },
                {14,"Parameter set selection subscription does not exist" },
                {15,"Tightening ID requested not found" },
                {16,"Connection rejected protocol busy" },
                {17,"Job ID not present" },
                {18,"Job info subscription already exists" },
                {19,"Job info subscription does not exist" },
                {20,"Job can not be set" },
                {21,"Job not running" },
                {22,"Not possible to execute dynamic Job request" },
                {23,"Job batch decrement failed" },
                {24,"Not possible to create Pset" },
                {25,"Programming control not granted" },
                {26,"Wrong tool type to Pset download connected" },
                {27,"Tool is inaccessible" },
                {28,"Job abortion is in progress" },
                {30,"Controller is not a sync Master/station controller" },
                {31,"Multi-spindle status subscription already exists" },
                {32,"Multi-spindle status subscription does not exist" },
                {33,"Multi-spindle result subscription already exists" },
                {34,"Multi-spindle result subscription does not exist" },
                {40,"Job line control info subscription already exists" },
                {41,"Job line control info subscription does not exist" },
                {42,"Identifier input source not granted" },
                {43,"Multiple identifiers work order subscription already exists" },
                {44,"Multiple identifiers work order subscription does not exist" },
                {50,"Status external monitored inputs subscription already exists" },
                {51,"Status external monitored inputs subscription does not exist" },
                {52,"IO device not connected" },
                {53,"Faulty IO device ID" },
                {54,"Tool Tag ID unknown" },
                {55,"Tool Tag ID subscription already exists" },
                {56,"Tool Tag ID subscription does not exist" },
                {57,"Tool Motor tuning failed" },
                {58,"No alarm present" },
                {59,"Tool currently in use" },
                {60,"No histogram available" },
                {61,"Pairing failed" },
                {62,"Pairing denied" },
                {63,"Pairing or Pairing abortion attempt on wrong tooltype" },
                {64,"Pairing abortion denied" },
                {65,"Pairing abortion failed" },
                {66,"Pairing disconnection failed" },
                {67,"Pairing in progress or already done" },
                {68,"Pairing denied. No Program Control" },
                {69,"Unsupported extra data revision" },
                {70,"Calibration failed" },
                {71,"Subscription already exists" },
                {72,"Subscription does not exists" },
                {73,"Subscribed MID unsupported,-answer if trying to subscribe on a non-existing MID" },
                {74,"Subscribed MID Revision unsupported,-answer if trying to subscribe on unsupported MID Revision." },
                {75,"Requested MID unsupported-answer if trying to request on a non-existing MID" },
                {76,"Requested MID Revision unsupported-response when trying to request unsupported MID Revision" },
                {77,"Requested on specific data not supported-response when trying to request data that is not supported" },
                {78,"Subscription on specific data not supported-answer if trying to subscribe for unsupported data" },
                {79,"Command failed" },
                {80,"Audi emergency status subscription exists" },
                {81,"Audi emergency status subscription does not exist" },
                {82,"Automatic/Manual mode subscribe already exist" },
                {83,"Automatic/Manual mode subscribe does not exist" },
                {84,"The relay function subscription already exists" },
                {85,"The relay function subscription does not exist" },
                {86,"The selector socket info subscription already exist" },
                {87,"The selector socket info subscription does not exist" },
                {88,"The digin info subscription already exist" },
                {89,"The digin info subscription does not exist" },
                {90,"Lock at batch done subscription already exist" },
                {91,"Lock at batch done subscription does not exist" },
                {92,"Open protocol commands disabled" },
                {93,"Open protocol commands disabled subscription already exists" },
                {94,"Open protocol commands disabled subscription does not exist" },
                {95,"Reject request, Power MACS is in manual mode" },
                {96,"Reject connection, Client already connected" },
                {97,"MID revision unsupported" },
                {98,"Controller internal request timeout" },
                {99,"Unknown MID" },
                 };

            /// <summary>拒绝Command号
            /// 
            /// </summary>
            public int RefuseMID;

            /// <summary>拒绝原因ID
            /// 
            /// </summary>
            public int RefuseReasonID;

            /// <summary>拒绝原因
            /// 
            /// </summary>
            public string RefuseReason = "";

            public override string ToString()
            {
                return string.Format("RefuseMID={0},RefuseReasonID='{1}',RefuseReason={2}", RefuseMID, RefuseReasonID, RefuseReason);
            }
        }

        #endregion

        #region AtlsCommand基类,它是所有命令的基类

        /// <summary>阿特拉斯控制命令
        /// 
        /// </summary>
        public class AtlsCommand
        {
            /// <summary>创建一个命令
            /// 
            /// </summary>
            /// <param name="as_命令号"></param>
            /// <param name="as_命令版本"></param>
            /// <param name="as_命令消息"></param>
            public AtlsCommand(int as_命令号, int as_命令版本, string as_命令消息体)
            {
                命令号 = as_命令号;
                命令版本 = as_命令版本;
                命令消息体 = as_命令消息体;

                命令_ByteList = PackCommand_Socket(命令号, 命令版本, 命令消息体);
                命令_String = Encoding.ASCII.GetString(命令_ByteList, 0, 命令_ByteList.Length);
                Name =
                    "MID=" + 命令号 + "\r\n" +
                    "MID版本=" + 命令版本 + "\r\n" +
                    "消息体=" + 命令消息体 + "\r\n" +
                    "时间=" + DateTime.Now;
            }

            /// <summary>解析一个命令
            /// 
            /// </summary>
            /// <param name="b"></param>
            public AtlsCommand(byte[] b)
            {
                命令_ByteList = b;
                命令_String = Encoding.ASCII.GetString(命令_ByteList, 0, 命令_ByteList.Length);
                UnPackCommand_Socket(b, out 命令号, out 命令版本, out 命令消息体);
                Name =
                    "MID=" + 命令号 + "\r\n" +
                    "MID版本=" + 命令版本 + "\r\n" +
                    "消息体=" + 命令消息体 + "\r\n" +
                    "时间=" + DateTime.Now;

                switch (命令号)
                {
                    case 4://拒绝
                        附加解析类 = new RefuseCommand(b, Encoding.ASCII.GetString(b));
                        break;
                    case 900://拧紧曲线
                        附加解析类 = new TraceCurveDataMessage(b, Encoding.ASCII.GetString(b));
                        break;
                    case 61://收到拧紧结果-->需要回复0062
                        附加解析类 = new TighteningResult(b, Encoding.ASCII.GetString(b));
                        break;
                    case 11:
                        附加解析类 = new PsetResult(b, Encoding.ASCII.GetString(b));
                        break;
                    case 13:
                        附加解析类 = new PsetDetail(b, Encoding.ASCII.GetString(b));
                        break;
                    case 31:
                        附加解析类 = new JsetResult(b, Encoding.ASCII.GetString(b));
                        break;
                    case 33:
                        附加解析类 = new JsetDetail(b, Encoding.ASCII.GetString(b));
                        break;
                    case 35://收到JOB结果-->需要回复0036
                        附加解析类 = new JobInfo(b, Encoding.ASCII.GetString(b));
                        break;
                    case 101://多轴拧紧事件结果
                        附加解析类 = new MultiSpindleResult(b, Encoding.ASCII.GetString(b));
                        break;
                }
            }

            public string Name { get; set; }

            public byte[] 命令_ByteList;

            public string 命令_String;

            public int 命令号;

            public int 命令版本;

            public string 命令消息体;

            public object 附加解析类;

            ///// <summary>MID=61时返回扭力结果
            ///// 
            ///// </summary>
            //public TighteningResult tighteningResult = null;

            ///// <summary>MID=900时返回曲线结果_角度
            ///// 
            ///// </summary>
            //public TraceCurveDataMessage TraceCurveDataMessage_Angle = null;

            ///// <summary>MID=900时返回曲线结果_扭力
            ///// 
            ///// </summary>
            //public TraceCurveDataMessage TraceCurveDataMessage_Truning = null;
            ///// <summary>MID=11获取PSET列表
            ///// 
            ///// </summary>
            //public PsetResult psetResult = null;
            ///// <summary>MID=13获取PSET详情
            ///// 
            ///// </summary>
            //public PsetDetail psetDetail = null;

            public override string ToString()
            {
                return 命令号.ToString();
            }
        }

        #endregion

        #region 静态方法,用于原始数据的解析和封装

        static void GetValue(string s, int index, int length, out int value)
        {
            string s_ = s.Substring(index, length);
            int.TryParse(s_, out value);
        }

        static void GetValue(byte[] b, int index, int length, out int value)
        {
            string s = Encoding.ASCII.GetString(b);
            GetValue(s, index, length, out value);
        }

        static void GetValue(string s, int index, int length, out string value)
        {
            string s_ = s.Substring(index, length);
            value = s_;
        }

        static void GetValue(byte[] b, int index, int length, out string value)
        {
            string s = Encoding.ASCII.GetString(b);
            GetValue(s, index, length, out value);
        }

        static void GetValue(string s, int index, int length, out DateTime value)
        {
            string s_dt = s.Substring(index, length).Remove(10, 1);
            s_dt = s_dt.Insert(10, " ");
            //string s_date = s.Substring(index, 10);
            //string s_time = s.Substring(index+11, 8);
            DateTime.TryParse(s_dt, out value);
        }

        static void GetValue(byte[] b, int index, int length, out DateTime value)
        {
            string s = Encoding.ASCII.GetString(b);
            GetValue(s, index, length, out value);
        }

        static void GetValue(string s, int index, int length, out decimal value)
        {
            string s_ = s.Substring(index, length);
            Decimal.TryParse(s_, out value);
            value /= 100M;
            //value = Convert.ToDecimal(s_) / 100M;
        }

        static void GetValue(byte[] b, int index, int length, out decimal value)
        {
            string s = Encoding.ASCII.GetString(b);
            GetValue(s, index, length, out value);
        }

        /// <summary>使用串口通讯时封装命令
        /// 
        /// </summary>
        /// <param name="mid">MID</param>
        /// <param name="vission">版本</param>
        /// <param name="message_data">消息体</param>
        /// <returns></returns>
        static byte[] PackCommand_SerialPort(int mid, int vission, string message_data)
        {
            string message = ((char)7).ToString() + ((char)9).ToString() +
                ((char)7).ToString() + ((char)9).ToString() + ((char)2).ToString();
            int messagelen = message_data.Length + 20;
            message += messagelen.ToString().PadLeft(4, '0');
            message += mid.ToString().PadLeft(4, '0');
            message += (vission == 0 ? "" : vission.ToString()).PadLeft(3, '0');
            message += "".PadLeft(9);
            message += message_data;
            message += ((char)0).ToString();
            message += ((char)3).ToString();
            return Encoding.ASCII.GetBytes(message);
        }

        /// <summary>使用网络通讯时封装命令
        /// 
        /// </summary>
        /// <param name="mid">MID</param>
        /// <param name="vission">MID版本</param>
        /// <param name="message_data">消息体</param>
        /// <returns></returns>
        static byte[] PackCommand_Socket(int mid, int vission, string message_data)
        {
            string message = "";
            int messagelen = message_data.Length + 20;
            message += messagelen.ToString().PadLeft(4, '0');
            message += mid.ToString().PadLeft(4, '0');
            message += (vission == 0 ? "" : vission.ToString()).PadLeft(3, '0');
            message += "".PadLeft(9);
            message += message_data;
            message += ((char)0).ToString();
            return Encoding.ASCII.GetBytes(message);
        }

        /// <summary>使用串口通讯时解析数据
        /// 
        /// </summary>
        /// <param name="b">数据</param>
        /// <param name="mid">MID</param>
        /// <param name="vission">MID版本</param>
        /// <param name="message_data">消息体</param>
        static void UnPackCommand_SerialPort(byte[] b, out int mid, out int vission, out string message_data)
        {
            string s = Encoding.ASCII.GetString(b);
            mid = Convert.ToInt32(s.Substring(5, 4));
            string vission_str = s.Substring(9, 4);
            if (vission_str.Trim() == "")
            {
                vission = 0;
            }
            else
            {
                vission = Convert.ToInt32(vission_str);
            }
            message_data = s.Substring(21, s.Length - 22);
        }

        /// <summary>使网络通讯时解析数据
        /// 
        /// </summary>
        /// <param name="b">数据</param>
        /// <param name="mid">MID</param>
        /// <param name="vission">MID版本</param>
        /// <param name="message_data">消息体</param>
        static bool UnPackCommand_Socket(byte[] b, out int mid, out int vission, out string message_data)
        {
            mid = 0; vission = 0; message_data = "";
            int length = 0;
            try
            {
                string s = Encoding.ASCII.GetString(b);
                //   mid = Convert.ToInt32(s.Substring(4, 4));
                Int32.TryParse(s.Substring(4, 4), out mid);
                string vission_str = s.Substring(8, 4);
                if (vission_str.Trim() == "")
                {
                    vission = 0;
                }
                else
                {
                    int.TryParse(vission_str, out vission);
                }
                message_data = s.Substring(20, s.Length - 21);

                int.TryParse(s.Substring(0, 4), out length);
            }
            catch { return false; }
            if (b.Length < length)
            {
                return false;
            }
            return true;
        }

        void TraceWrite(string s, bool Trace = true)
        {
            if (Trace)
            {
                System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":" + s);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ":" + s);
            }
            if (RetrunMessage != null)
            {
                if (!s.Contains("失败"))//连接失败的情况下不发送此类信息
                {
                    RetrunMessage(s);
                }
            }
        }

        /// <summary>尝试解析一个Byte数组,将其解析为正确的AtlsCommand
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        static AtlsCommand CheckByteListIsCmd(List<byte> b1, out int len, out string err)
        {
            byte[] b = b1.ToArray();
            len = 0; err = "";
            if (b.Length < 20)
            {
                err = "数据长度小于20";
                return null;//肯定不小于20
            }
            try
            {
                string s = ((char)b[0]).ToString() + ((char)b[1]).ToString() + ((char)b[2]).ToString() + ((char)b[3]).ToString();
                len = Convert.ToInt32(s);
                //int.TryParse(s, out len);
            }
            catch
            {
                err = "头4个字节无法识别为数据";
                return null;//无法识别为头
            }
            if (b.Length != len + 1 && b.Length != len)//长度无法匹配
            {
                err = "头长度和实际长度无法匹配";
                return null;
            }

            //尝试开始解析Cmd
            int mid = 0;
            int vission = 0;
            string message_data = "";

            string bs = Encoding.ASCII.GetString(b);
            try
            {
                mid = Convert.ToInt32(bs.Substring(4, 4));
            }
            catch
            {
                err = "MID无法识别";
                return null;
            }
            string vission_str = bs.Substring(8, 4);
            if (vission_str.Trim() == "")
            {
                vission = 0;
            }
            else
            {
                try
                {
                    vission = Convert.ToInt32(vission_str);
                }
                catch
                {
                    err = "Vission无法识别";
                    return null;
                }
            }
            message_data = bs.Substring(20, bs.Length - 21);
            return new AtlsCommand(b);
        }

        /// <summary>尝试解析一个Byte数组,将其解析为正确的AtlsCommand
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        static AtlsCommand CheckByteListIsCmd(byte[] b, out int len, out string err)
        {
            len = 0; err = "";
            if (b.Length < 20)
            {
                err = "数据长度小于20";
                return null;//肯定不小于20
            }
            try
            {
                string s = ((char)b[0]).ToString() + ((char)b[1]).ToString() + ((char)b[2]).ToString() + ((char)b[3]).ToString();
                len = Convert.ToInt32(s);
            }
            catch
            {
                err = "头4个字节无法识别为数据";
                return null;//无法识别为头
            }
            if (b.Length != len + 1 && b.Length != len)//长度无法匹配
            {
                err = "头长度和实际长度无法匹配";
                return null;
            }

            //尝试开始解析Cmd
            int mid = 0;
            int vission = 0;
            string message_data = "";

            string bs = Encoding.ASCII.GetString(b);
            try
            {
                mid = Convert.ToInt32(bs.Substring(4, 4));
            }
            catch
            {
                err = "MID无法识别";
                return null;
            }
            string vission_str = bs.Substring(8, 4);
            if (vission_str.Trim() == "")
            {
                vission = 0;
            }
            else
            {
                try
                {
                    vission = Convert.ToInt32(vission_str);
                }
                catch
                {
                    err = "Vission无法识别";
                    return null;
                }
            }
            message_data = bs.Substring(20, bs.Length - 21);
            return new AtlsCommand(b);
        }

        #endregion

        #region 继承的字段、属性、方法

        public override bool DC_IsConnected { get => socket == null ? false : socket.Connected; }

        public override bool DC_IsCommcation { get => (DateTime.Now - LastReceHeartTime).TotalSeconds < 15; }

        public override void DC_Channel(string channel)
        {
            if (channel.Length < 2)
            {
                return;
            }
            string id_str = channel.Substring(1, channel.Length - 1);
            char type_str = channel[0];
            TypePro typePro = TypePro.Para;
            switch (type_str)
            {
                case 'P':
                    typePro = TypePro.Para;
                    break;
                case 'J':
                    typePro = TypePro.Job;
                    break;
                case 'M':
                    typePro = TypePro.Mode;
                    break;
                default:
                    typePro = TypePro.UnKown;
                    break;
            }

            switch (typePro)
            {
                case TypePro.UnKown:
                    break;
                case TypePro.Mode:
                    this.SelectPro(id_str);
                    break;
                case TypePro.Para:
                case TypePro.Job:
                    if (!int.TryParse(id_str, out int id))
                    {
                        return;
                    }
                    this.SelectPro(typePro, id);
                    break;
                default:
                    break;
            }
        }


        public override void DC_Start()
        {
            return;
        }

        public override void DC_Start(string channel)
        {
            return;
        }

        public override void DC_Lock(bool as_Enable)
        {
            this.Lock(as_Enable);
        }

        public override void DC_ByPass()
        {
            this.SetDO(1, true);
            this.SetDO(1, false);

            //serial.com_send("07 09 07 09 02 30 30 33 30 30 32 30 30 30 30 31 20 20 20 20 20 20 20 20 20 31 33 33 33 33 33 33 33 33 33 00 03");
            ////serial.com_send("00300200001         1333333333\0");
            //Thread.Sleep(1000);
            //serial.com_send("07 09 07 09 02 30 30 33 30 30 32 30 30 30 30 31 20 20 20 20 20 20 20 20 20 31 33 33 33 33 33 33 33 33 33 00 03");
            ////serial.com_send("00300200001         1333333333\0");
            //Thread.Sleep(1500);
            //serial.com_send("07 09 07 09 02 30 30 33 30 30 32 30 30 30 30 31 20 20 20 20 20 20 20 20 20 30 33 33 33 33 33 33 33 33 33 00 03");
            //Thread.Sleep(1000);
            //serial.com_send("07 09 07 09 02 30 30 33 30 30 32 30 30 30 30 31 20 20 20 20 20 20 20 20 20 30 33 33 33 33 33 33 33 33 33 00 03");
            ////serial.com_send("00300200001         0333333333\0");
        }

        public override void DC_Stop()
        {
            return;
        }

        public override void DC_Close()
        {
            this.Close();
        }

        #endregion
    }
}