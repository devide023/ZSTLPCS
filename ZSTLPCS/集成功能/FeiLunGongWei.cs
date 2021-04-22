using QLib;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ZSTJPCS
{
    public static class FeiLunGongWei
    {
        public delegate void GetJJH_Event(string jjh);

        private static string Dir = "";

        private static PLCConnect plcconnect;

        private static RFID_And_IO_Connect RFIDConnect;

        private static UdpClient udp_server = new UdpClient();

        private static UdpClient udp_client;

        private static string 服务端缓存的从RFID读取的夹具号 = "";

        public static bool 当前为服务端模式;

        public static event GetJJH_Event GetJJH;

        /// <summary>判断服务端或客户端模式,该函数必须调用
        /// 
        /// </summary>
        /// <returns>true=服务端模式,false=客户端模式</returns>
        public static bool CheckClientOrServerMode()
        {
            List<string> ips = QL_Net.GetHostIPs();
            当前为服务端模式 = ips.Exists((string ip) => ip == AppConfig.FEILUN_SERVERIP.Address.ToString());
            string[] msg = new string[]
            {
                "当前以" + (当前为服务端模式 ? "服务端" : "客户端") + "模式运行",
                "服务端:" + AppConfig.FEILUN_SERVERIP,
                "客户端:" + AppConfig.FEILUN_CLIENTIP,
            };
            ToolAlarm.ShowDialog(string.Join("\r\n", msg));
            return 当前为服务端模式;
        }

        public static void Init_Server(PLCConnect as_plcconnecct, RFID_And_IO_Connect as_rfidconnect)
        {
            //if (当前为服务端模式)
            //{
            plcconnect = as_plcconnecct;
            plcconnect.ReceData_Event += Plcconnect_ReceData_Event;
            RFIDConnect = as_rfidconnect;
            RFIDConnect.Open();
            RFIDConnect.RFID_Event += new RFID_And_IO_Connect.RFID_Dele(RFIDConnect_RFID_Event);
            //}
            //else
            //{
            //    new Thread(new ThreadStart(OpenClient)) { IsBackground = true }.Start();
            //}
        }

        /// <summary>收到线体主控PLC的 脚踏板信号 和 放行方向 后后即尝试 读取一次RFID
        /// 
        /// </summary>
        /// <param name="plcconnect"></param>
        /// <param name="b"></param>
        private static void Plcconnect_ReceData_Event(PLCConnect plcconnect, byte[] b)
        {
            Dir = Encoding.ASCII.GetString(b, 0, b.Length);
            AppMessage.Add("收到PLC消息:" + Dir, AppMessage.MsgType.飞轮岗位A1A3交互消息);
            //RFIDConnect.RFIDRead(AppConfig.RFIDSTARTINDEX, 4, AppConfig.RFIDREADTIMES);
            RFIDConnect.RFIDRead(AppConfig.RFIDSTARTINDEX, 4);
        }

        /// <summary>服务端:读取到一个夹具号信息后根据 放行方向 决定将夹具号自用或 提交给客户端
        /// 
        /// </summary>
        /// <param name="or"></param>
        /// <param name="context"></param>
        private static void RFIDConnect_RFID_Event(RFID_And_IO_Connect.OPerationResult or, string context)
        {
            if (or != RFID_And_IO_Connect.OPerationResult.RFID_读取成功)
            {
                AppMessage.Add("从RFID接收到错误消息:" + or.ToString(), AppMessage.MsgType.飞轮岗位A1A3交互消息, true);
            }
            else if (string.IsNullOrWhiteSpace(context))
            {
                AppMessage.Add("从RFID接收到空消息", AppMessage.MsgType.飞轮岗位A1A3交互消息, true);
            }
            else if (!SParms.RFIDEnable)
            {
                AppMessage.Add("从RFID接收到消息:" + context + ",但因为当前不属于RFID模式被抛弃", AppMessage.MsgType.飞轮岗位A1A3交互消息, true);
            }
            else if (context.Length < 10)
            {
                AppMessage.Add("从RFID接收到夹具号:" + context + ",因为长度不满10位被抛弃", AppMessage.MsgType.飞轮岗位A1A3交互消息, true);
            }
            else
            {
                string jjh = context/*.Substring(0, 10)*/;
                AppMessage.Add("接收到RFID夹具号:" + jjh, AppMessage.MsgType.飞轮岗位A1A3交互消息);
                if (服务端缓存的从RFID读取的夹具号 != jjh)
                {
                    服务端缓存的从RFID读取的夹具号 = jjh;
                    bool 主线 = Dir != "0";
                    if (主线)
                    {
                        try
                        {
                            byte[] arrMsg = Encoding.UTF8.GetBytes(jjh);
                            udp_server.Send(arrMsg, arrMsg.Length, AppConfig.FEILUN_CLIENTIP);
                            AppMessage.Add("已将条码" + jjh + "发送给客户端", AppMessage.MsgType.飞轮岗位A1A3交互消息);
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        GetJJH?.Invoke(jjh);
                    }
                }
                else
                {
                    AppMessage.Add("从RFID接收到数据因重复抛弃:" + context, AppMessage.MsgType.飞轮岗位A1A3交互消息, true);
                }
            }
        }

        #region 客户端

        /// <summary>以客户端模式初始化
        /// 
        /// </summary>
        public static void Init_Client()
        {
            new Thread(() =>
            {
                //IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(AppConfig.FEILUN_CLIENTIP), AppConfig.FEILUN_CLIENTPORT);
                //IPEndPoint endPoint2 = new IPEndPoint(IPAddress.Parse(AppConfig.FEILUN_CLIENTIP), AppConfig.FEILUN_CLIENTPORT);
                IPEndPoint endPoint = AppConfig.FEILUN_CLIENTIP;
                IPEndPoint endPoint2 = AppConfig.FEILUN_CLIENTIP;
                while (true)
                {
                    //try
                    //{
                        udp_client = new UdpClient(endPoint);
                    //}
                    //catch { }
                    while (true)
                    {
                        byte[] arrMsg;
                        try
                        {
                            arrMsg = udp_client.Receive(ref endPoint2);
                        }
                        catch
                        {
                            break;
                        }
                        string jjh = Encoding.UTF8.GetString(arrMsg, 0, arrMsg.Length);
                        AppMessage.Add("已从客户端接收到夹具号:" + jjh, AppMessage.MsgType.飞轮岗位A1A3交互消息);
                        GetJJH?.Invoke(jjh);
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        #endregion

        /// <summary>向PLC发送一次复位命令 & 清除当前缓存的避免重复影响事件的夹具号
        /// 
        /// </summary>
        public static void Reset()
        {
            if (当前为服务端模式)
            {
                plcconnect?.PLCSend(Encoding.UTF8.GetBytes("1"), "飞轮岗位复位");
                服务端缓存的从RFID读取的夹具号 = "";
            }
        }
    }
}
