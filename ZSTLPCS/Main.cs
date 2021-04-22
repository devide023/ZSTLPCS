using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QLib;

namespace ZSTJPCS
{
    public partial class Main : Form
    {
        #region 构造函数和Load初始化

        public Main(bool usesuperid = false)
        {
            InitializeComponent();
            ToolStripMenuItem_测试模式.Visible = usesuperid;
            SMethod.InitBackForeColor(this);//前后景色
            SParms.DefultColor = this.BackColor;
            //时间同步
            时间同步ToolStripMenuItem.Visible = TimeCheck.CheckTime();
            时间同步ToolStripMenuItem.ForeColor = Color.Yellow;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            string err;

            #region 引用日志消息

            AppMessage.HaveShowLog_Event += AppMessage_HaveShowLog_Event;

            #endregion

            toolStripStatusLabel_ShowAppConfig.Text = string.Format("操作员:{0} | 工位:{1}", SParms.User, AppConfig.GWName);

            #region 根据岗位性质不同初始化界面

            switch (AppConfig.GXType_Enum)
            {
                case AppConfig.GXType_EnumType.未定义:
                default:
                    break;
                case AppConfig.GXType_EnumType.扭力:

                    if (AppConfig.TIGHTENCONTROLERTYPE == 1 || AppConfig.TIGHTENCONTROLERTYPE == 0)
                    {
                        toolStripMenuItem_ATLS.Visible = true;
                    }

                    tabControl1.TabPages.Remove(tabPage_ScanBinding);
                    tabControl1.TabPages.Remove(tabPage_Hand);
                    tabControl1.TabPages[0].Text = AppConfig.GX_Enum.ToString();

                    break;
                case AppConfig.GXType_EnumType.干检:

                    干检ToolStripMenuItem.Visible = true;

                    tabControl1.TabPages.Remove(tabPage_ScanBinding);
                    tabControl1.TabPages.Remove(tabPage_Hand);
                    tabControl1.TabPages[0].Text = AppConfig.GX_Enum.ToString();

                    label_NJJS.Text = "充气时间";
                    label_NJZS.Text = "允许泄露";
                    label_NLSX.Text = "压力上限";
                    label_NLXX.Text = "压力下限";
                    label_DQNL.Text = "当前泄露";

                    break;
                case AppConfig.GXType_EnumType.人工:

                    label_NLSX.Visible = false;
                    label_NLXX.Visible = false;
                    label_DQCX.Visible = false;
                    textBox_NLSX.Visible = false;
                    textBox_NLXX.Visible = false;
                    textBox_DQCX.Visible = false;

                    tabControl1.TabPages.Remove(tabPage_ScanBinding);
                    tabControl1.TabPages.Remove(tabPage_NL);

                    tb_HandJCXM.Text = SQL.SQLConnect.SelectResult<string>(string.Format("select jcxm from ZJC_XM where work_no='{0}' and scx='{1}'", AppConfig.GX, AppConfig.SCX), out err);

                    var remark_temp = "";
                    switch (AppConfig.GX_Enum)
                    {
                        case AppConfig.GX_EnumType.气门:
                            remark_temp = "气门岗位";
                            break;
                        case AppConfig.GX_EnumType.点火器:
                            remark_temp = "点火器岗位";
                            break;
                    }
                    cb_HandReson.Items.AddRange(SQL.SQLConnect.SelectListResult<string>(string.Format("select fault_name from ZXJC_FAULT_EDIT where REMARK='{0}'", remark_temp), out err).ToArray());
                    cb_HandReson.DropDown += (sender_tmp, e_tmp) => { cb_HandReson.Tag = checkBox_LockScan.Checked; checkBox_LockScan.Checked = false; };//下拉时暂停自动对焦到扫码区
                    cb_HandReson.DropDownClosed += (sender_tmp, e_tmp) => { checkBox_LockScan.Checked = (bool)cb_HandReson.Tag; };//下拉完成后恢复自动对焦到扫码区
                    SetTextControl(button_HandOK, "合格", SParms.GoodColor);
                    break;
                case AppConfig.GXType_EnumType.绑定:

                    tabControl1.TabPages.Remove(tabPage_NL);
                    tabControl1.TabPages.Remove(tabPage_Hand);

                    label_HC.Visible = false;
                    label_NLSX.Visible = false;
                    label_NLXX.Visible = false;
                    label_DQCX.Visible = false;
                    //label_DQJX.Visible = false;
                    //label_ZTBM.Visible = false;
                    label_QDZT.Visible = false;
                    //label_SJSC.Visible = false;

                    textBox_HC.Visible = false;
                    textBox_NLSX.Visible = false;
                    textBox_NLXX.Visible = false;
                    textBox_DQCX.Visible = false;
                    //textBox_DQJX.Visible = false;
                    //textBox_ZTBM.Visible = false;
                    textBox_QDZT.Visible = false;
                    //textBox_SJSC.Visible = false;

                    label_QDZTBHG.Visible = false;

                    button_ClearAll.Visible = false;
                    button_ClearOne.Visible = false;

                    ToolStripMenuItem_RFID.Visible = false;
                    //ToolStripMenuItem_RFID复位.Enabled=

                    JHBinding.BindingOver_Event += JHBinding_BindingOver_Event;

                    break;
            }

            #endregion

            #region 是否缓存时的界面变化

            if (!AppConfig.ISCACHE)
            {
                button_ClearAll.Visible = false;
                textBox_HC.Visible = false;
                label_HC.Visible = false;
                button_ClearOne.Location = new Point(button_ClearOne.Location.X, textBox_DQJX.Location.Y);
            }

            #endregion

            #region PLCConnect初始化

            if (!string.IsNullOrWhiteSpace(AppConfig.FEILUN_PLCIP))
            {
                pLCConnect = new PLCConnect(new System.Net.IPEndPoint(AppConfig.FEILUN_PLCIP.ToIPAddress(), AppConfig.FEILUN_PLCPORT));
            }

            #endregion

            #region RFID模式初始化

            #region 是否绑定RFID

            if (AppConfig.RFIDRECIVETYPE == 2)
            {
                checkBox_SMBDJH.Checked = true;
                checkBox_SMBDJH.Enabled = true;
            }
            else
            {
                checkBox_SMBDJH.Checked = false;
                checkBox_SMBDJH.Visible = false;
            }

            #endregion

            if (AppConfig.RFIDRECIVETYPE == 3)
            {
                #region 飞轮岗位RFID初始化

                if (FeiLunGongWei.CheckClientOrServerMode())
                {
                    rFIDConnect = new RFID_And_IO_Connect(AppConfig.RFIDIP, AppConfig.RFIDRECIVETYPE);
                    PLCConnect feilun_pLCConnect = new PLCConnect(AppConfig.FeiLun_PLCIP2);
                    FeiLunGongWei.Init_Server(feilun_pLCConnect, rFIDConnect);
                }
                else
                {
                    FeiLunGongWei.Init_Client();
                }

                FeiLunGongWei.GetJJH += FeiLunGongWei_GetJJH;
                AppMessage.Add("程序已按双飞轮岗位模式初始化", AppMessage.MsgType.RFID, false);

                #endregion
            }
            else
            {
                #region 一般岗位RFID初始化

                rFIDConnect = new RFID_And_IO_Connect(AppConfig.RFIDIP, AppConfig.RFIDRECIVETYPE);

                if (AppConfig.RFIDRECIVETYPE == 1)
                {
                    rFIDConnect.AutoAskIOStatusInterval = 200;
                    rFIDConnect.AutoAskIOStatus = true;
                    rFIDConnect.IOUpDown_Event += connect_IOUpDown_Event;
                    AppMessage.Add("程序已按按钮触发或扫描触发模式初始化", AppMessage.MsgType.RFID, false);
                }
                bool RFIDOpend = rFIDConnect.Open();
                AppMessage.Add(RFIDOpend ? "RFID连接成功,已自动切换为RFID模式" : "RFID连接失败,已自动切换为扫描模式", AppMessage.MsgType.RFID,/*false*/ !RFIDOpend, RFIDOpend ? AppMessage.ImportantEnum.Normal : AppMessage.ImportantEnum.Err);
                SParms.RFIDEnable = RFIDOpend;
                ShowRFIDEnable();
                rFIDConnect.RFID_Event += connect_RFID_Event;
                rFIDConnect.SocketEvent += connect_SocketEvent;

                #region 自动读取模式单线程自动读取
                if (AppConfig.RFIDRECIVETYPE == 0)
                {
                    new System.Threading.Thread(AutoReadRFID) { IsBackground = true }.Start();
                    AppMessage.Add("程序已按自动模式初始化", AppMessage.MsgType.RFID, false);
                }
                #endregion
                #endregion
            }
            label_KJJ.Text = "";

            #endregion

            #region 绑定工位初始化

            JHBinding.Init(rFIDConnect, pLCConnect);
            checkBox1_CheckedChanged(checkBox_JJHSourceWhenBinding, null);
            checkBox_LockScan.Checked = true;

            #endregion

            #region 数据采集模块初始化

            switch (AppConfig.GXType_Enum)
            {
                case AppConfig.GXType_EnumType.扭力:
                    switch (AppConfig.TIGHTENCONTROLERTYPE)
                    {
                        case 0:
                            AtlasConnecter_Com atlasConnecter_com = new AtlasConnecter_Com();
                            atlasConnecter_com.Init(AppConfig.ComPort);
                            atlasConnecter_com.RetrunAtlsCommand_Event += AtlasConnecter_com_RetrunAtlsCommand_Event;
                            dc = atlasConnecter_com;
                            break;
                        case 1:
                            AtlasConnecter atlasConnecter = new AtlasConnecter();
                            atlasConnecter.Init(AppConfig.TIGHTENCONTROLERIP);
                            atlasConnecter.RetrunAtlsCommand_Event += AtlasConnecter_RetrunAtlsCommand_Event;
                            dc = atlasConnecter;
                            break;
                        case 2:
                            IngersollRandConnecter ingersollRandConnecter = new IngersollRandConnecter(AppConfig.TIGHTENCONTROLERIP, AppConfig.TIGHTENCONTROLERBINDINGIP);
                            dc = ingersollRandConnecter;
                            break;
                        case 3:

                            break;
                        case 5:
                            AtlasConnecter atlasConnecter5 = new AtlasConnecter(AtlasConnecter.AtlasControlerType.Multiple);
                            atlasConnecter5.Init(AppConfig.TIGHTENCONTROLERIP);
                            atlasConnecter5.RetrunAtlsCommand_Event += AtlasConnecter_RetrunAtlsCommand_Event;
                            dc = atlasConnecter5;
                            break;
                    }
                    break;
                case AppConfig.GXType_EnumType.干检:
                    Cosmo cosmo = new Cosmo(AppConfig.ComPort);
                    if (!cosmo.Init(out err))
                    {
                        ToolAlarm.ShowDialog("串口打开失败:" + err);
                    }
                    dc = cosmo;
                    break;
            }
            if (dc != null)
            {
                dc.GetData_Event += Dc_GetData;
                dc.Msg_Event += Dc_Msg_Event;
            }

            #endregion

            #region 取出上次退出程序的缓存机号

            JHQueue.CacheJHCountChanged += () => ShowJHCacheCount();
            var cachesn = JHQueue.GetVin();
            if (cachesn != null)
            {
                DealNowVin(cachesn, out err);//程序启动时取缓存机号
                //textBox_HC.Text = JHQueue.GetCount().ToString();
            }

            #endregion

            #region 设备、PLC、RFID灯初始化

            ucButton_CommcationStateShow_PLC.Visible = pLCConnect != null;
            ucButton_CommcationStateShow_SB.Visible = dc != null;

            ucButton_CommcationStateShow_PLC.OKColor = new SolidBrush(SParms.GoodColor);
            ucButton_CommcationStateShow_PLC.ErrColor = new SolidBrush(SParms.ErrColor);

            ucButton_CommcationStateShow_RFID.OKColor = ucButton_CommcationStateShow_PLC.OKColor;
            ucButton_CommcationStateShow_RFID.ErrColor = ucButton_CommcationStateShow_PLC.ErrColor;

            ucButton_CommcationStateShow_SB.OKColor = ucButton_CommcationStateShow_PLC.OKColor;
            ucButton_CommcationStateShow_SB.ErrColor = ucButton_CommcationStateShow_PLC.ErrColor;

            #endregion

            this.Text = string.Format("通机生产过程管理系统,版本:{0}", ApplicationUpdate.GetCurrentVersion());
            AppMessage.Add("应用程序登录,版本:" + ApplicationUpdate.GetCurrentVersion(), AppMessage.MsgType.一般, isDBImportant: true);//兰浩
        }

        private void ShowJHCacheCount()
        {
            textBox_HC.Text = (JHQueue.GetCount() + (NowVinIsGoing ? 1 : 0)).ToString();
        }

        #endregion
        #region 字段和属性

        /// <summary>程序当前正在处理的机号信息
        /// 
        /// </summary>
        public static VinMsg vinmsg { get; set; }

        /// <summary>表示当前有一个发动机机号处于未完成状态
        /// 
        /// </summary>
        public static bool NowVinIsGoing { get {return vinmsg!= null && !vinmsg.IsEnd && vinmsg.QDZT; } }

        RFID_And_IO_Connect rFIDConnect;

        DataCollecter dc;

        PLCConnect pLCConnect;

        #endregion
        #region RFID相关

        private void FeiLunGongWei_GetJJH(string jjh)//具有两个飞轮自动拧紧机的飞轮岗位RFID获取到机号
        {
            connect_RFID_Event(RFID_And_IO_Connect.OPerationResult.RFID_读取成功, jjh);
            //this.BeginInvoke_CheckHandle(() =>
            //{
            //    if (string.IsNullOrWhiteSpace(jjh))
            //    {
            //        AppMessage.Add("获取到的夹具号为空", AppMessage.MsgType.飞轮岗位A1A3交互消息, true);
            //    }
            //    else
            //    {
            //        string codeByRfid = SQL.SearchCodeByRFID(jjh);
            //        if (string.IsNullOrWhiteSpace(codeByRfid))
            //        {
            //            string msg = string.Format("夹具号{0}未绑定任何机号", jjh);
            //            ToolAlarm.ShowDialog(msg);
            //            AppMessage.Add(msg, AppMessage.MsgType.飞轮岗位A1A3交互消息, true);
            //        }
            //        else
            //        {
            //            AppMessage.Add("已在数据库中查询到机号为:" + codeByRfid, AppMessage.MsgType.飞轮岗位A1A3交互消息, true);
            //            DealNewVIN(codeByRfid, out string err);//A1A3飞轮岗位RFID
            //        }
            //    }
            //});
        }

        private void connect_SocketEvent(RFID_And_IO_Connect.SocketEventType obj)
        {
            this.BeginInvoke_CheckHandle(() =>
            {

            });
        }

        public void AutoReadRFID()//自动一直读取RFID中的数据
        {
            while (true)
            {
                try
                {
                    System.Threading.Thread.Sleep(AppConfig.RFIDAUTOSCANINTERVAL);
                    if (rFIDConnect != null)
                    {
                        rFIDConnect.RFIDRead(AppConfig.RFIDSTARTINDEX, 4);
                    }
                }
                catch (System.Exception)
                {

                }
            }
        }

        void connect_IOUpDown_Event(int Index, bool Dir)//RFID中的IO信号
        {
            this.BeginInvoke_CheckHandle(() =>
            {
                if (Dir && AppConfig.RFIDRECIVETYPE == 1)
                {
                    rFIDConnect.AutoAskIOStatus = false;
                    rFIDConnect.RFIDRead(AppConfig.RFIDSTARTINDEX, 4, AppConfig.RFIDREADTIMES);
                    AppMessage.Add("接受到IO变化信号", AppMessage.MsgType.RFID);
                }
            });
        }

        public string LastReadJJH = "";
        public DateTime LastReadJJHTime = DateTime.Now;

        void connect_RFID_Event(RFID_And_IO_Connect.OPerationResult or, string context)//RFID数据处理
        {
            this.BeginInvoke_CheckHandle((() =>
            {
                string err;
                #region 按钮触发模式下打开按钮触发开关
                if (AppConfig.RFIDRECIVETYPE == 1)
                {
                    if (!checkBox_LockScan.Checked)
                    {
                        rFIDConnect.AutoAskIOStatus = true;
                        AppMessage.Add("开启读取IO信号", AppMessage.MsgType.RFID);
                    }
                }
                #endregion
                #region 提示信息 - RFID读取不成功
                if (or != RFID_And_IO_Connect.OPerationResult.RFID_读取成功)
                {
                    AppMessage.Add("RFID读取不成功:" + or.ToString(), AppMessage.MsgType.RFID);
                    return;
                }
                #endregion
                #region 提示信息 - RFID读取结果
                AppMessage.Add("收到RFID消息:" + or.ToString() + " | [长度" + context.Length + "]" + context + " 内容:" + Encoding.ASCII.GetBytes(context).ToStandardString(","), AppMessage.MsgType.RFID);
                #endregion
                #region 中止本函数 - 不在RFID模式
                if (!SParms.RFIDEnable)
                {
                    AppMessage.Add("因不在RFID模式放弃夹具号:" + context, AppMessage.MsgType.RFID);
                    return;
                }
                #endregion
                #region 中止本函数 - RFID读取到的内容长度不正确 或 内容为空
                if (context.Length < 11 || string.IsNullOrWhiteSpace(context.Trim()))
                {
                    //textBox_Scan.Text = "";
                    AppMessage.Add("RFID读取到的内容长度不正确或内容为空:" + context + "长度[" + context.Length + "]", AppMessage.MsgType.RFID);
                    return;
                }
                #endregion
                string jjh = context.Trim();
                #region 根据线别计算正确的夹具号信息
                if (AppConfig.RFIDSTARTINDEX == 1)
                {
                    //2，4线芯片倒序
                    jjh = context.Trim();
                    string result = jjh[7].ToString();
                    result += jjh[6].ToString();
                    result += jjh[5].ToString();
                    result += jjh[4].ToString();
                    result += jjh[3].ToString();
                    result += jjh[2].ToString();
                    result += jjh[1].ToString();
                    result += jjh[0].ToString();
                    result += jjh[15].ToString();
                    result += jjh[14].ToString();
                    jjh = result;
                }
                jjh = jjh.Replace('\0'.ToString(), "");
                if (jjh.Length > 10)
                {
                    jjh = jjh.Substring(0, 10);
                }
                #endregion
                #region 装夹绑定岗位处理
                if (AppConfig.GXType_Enum == AppConfig.GXType_EnumType.绑定)
                {
                    textBox_JJHInRFID .Text= jjh;
                    JHBinding.jjhInRFID = jjh;
                    return;
                }
                #endregion
                #region 扫描绑定岗位模式
                if (checkBox_LockScan.Checked && vinmsg != null && string.IsNullOrWhiteSpace(vinmsg.vin))
                {
                    string vin_temp = vinmsg.vin;
                    //#region 中止本函数 - 当前输入的机号为空
                    //if (string.IsNullOrWhiteSpace(vin_temp))
                    //{
                    //    AppMessage.Add("当前获取到的机号为空退出函数", AppMessage.MsgType.RFID);
                    //    return;
                    //}
                    //#endregion
                    var sqlcount = SQL.BindingVINToJJH(vin_temp, jjh, out err);
                    if (err != "")
                    {
                        string bindre = string.Format("绑定机号{0}到夹具{1}失败", vin_temp, jjh) + err;
                        ToolAlarm.ShowDialog(bindre);
                        AppMessage.Add(bindre, AppMessage.MsgType.RFID);
                    }
                    else
                    {
                        AppMessage.Add(string.Format("绑定机号{0}到夹具{1}成功", vin_temp, jjh), AppMessage.MsgType.RFID);
                    }
                }
                #endregion
                string vinByRfid = SQL.SearchCodeByRFID(jjh);//在数据库中根据夹具号查询机号
                #region 避免重复扫描到相同的夹具号
                //只在RFID自动模式才生效，因为其他方式都被认为是人工触发的，因此允许多次触发相同的机号，但RFID自动模式则不允许
                if (LastReadJJH == vinByRfid && !string.IsNullOrWhiteSpace(LastReadJJH))
                {
                    bool IsAutoAskMode_Temp = AppConfig.RFIDRECIVETYPE == 0;
                    bool IsMircoInterval = (DateTime.Now - LastReadJJHTime).TotalSeconds < 2;
                    if (IsAutoAskMode_Temp || IsMircoInterval)
                    {
                        AppMessage.Add("当前获取到的机号和上一次处理的机号重复退出函数:" + vinByRfid, AppMessage.MsgType.RFID);
                        return;
                    }
                }
                LastReadJJHTime = DateTime.Now;
                LastReadJJH = vinByRfid;
                #endregion
                #region 空夹具提示和处理
                if (string.IsNullOrWhiteSpace(vinByRfid))
                {
                    SetTextControl(label_KJJ, "空夹具" + jjh, SParms.ErrColor);
                    string 空夹具提示消息 = string.Format("夹具号{0}[长度{1}]未绑定任何机号", jjh, jjh.Length);
                    AppMessage.Add(空夹具提示消息, AppMessage.MsgType.空夹具提示);

                    #region A1缸头复检岗位特殊处理

                    if (AppConfig.SCX == 1 && AppConfig.GX == 22)
                    {
                        pLCConnect?.PLCSend(new byte[] { 1 }, "A1缸头空夹具放行");
                    }

                    #endregion

                    AppMessage.Add("空夹具退出函数:" + jjh, AppMessage.MsgType.RFID);
                    return;
                }
                #endregion
                SetTextControl(label_KJJ, "空夹具" + jjh, SParms.DefultColor);
                DealNewVIN(vinByRfid, out err);//RFID
            }));
        }

        #endregion
        #region 处理机号

        private void textBox_Scan_KeyDown(object sender, KeyEventArgs e)//扫描机号并按下回车键
        {
            TextBox tb = sender as TextBox;
            if (e.KeyCode == Keys.Enter)
            {
                //#region 清除上次可能的条码 兰浩2020-02-22
                ////如果处于RFID模式且RFID未向计算机回复响应，可能造成tb_ddtm控件中可能还有上次扫描的机号【因为未调用dealCode方法清除控件Text属性】
                ////导致在计算机下一次识别机号时可能同时识别两个机号【中间以回车换行区分】，最后要么存入数据库时两个机号叠加，再取出时界面报错
                ////扫码错误，请重新扫码！！！
                ////因此在每次触发条码时均需要清除上次可能的条码
                //ClearLastCodeIn_tb_ddtm();
                //#endregion
                //string err;
                string vin_temp = ((TextBox)sender).Text.Replace("\r", "").Replace("\n", "");
                tb.Text = "";
                bool isvinorjjh = (vin_temp.Length > 10);//是机号还是夹具号,true=机号,false=夹具号
                switch (AppConfig.GXType_Enum)
                {
                    case AppConfig.GXType_EnumType.扭力:
                    case AppConfig.GXType_EnumType.干检:
                    case AppConfig.GXType_EnumType.人工:
                        if (SParms.RFIDEnable)
                        {
                            if (AppConfig.RFIDRECIVETYPE == 2)
                            {
                                //this.connect.AutoAskIOStatus = false;
                                this.rFIDConnect.RFIDRead(AppConfig.RFIDSTARTINDEX, 4, AppConfig.RFIDREADTIMES);
                                AppMessage.Add("人工扫描条码并尝试读取RFID", AppMessage.MsgType.RFID);

                                DealNewVIN(vin_temp, out _);//扫描绑定
                            }
                            else
                            {
                                ToolAlarm.ShowDialog("您当前处于RFID模式且不处于扫描绑定模式,扫描的条码无效!\r\n可切换为扫描模式重新扫描!");
                            }
                        }
                        else
                        {
                            DealNewVIN(vin_temp, out _);//正常扫描
                        }
                        break;
                    case AppConfig.GXType_EnumType.绑定:
                        if (isvinorjjh)
                        {
                            DealNewVIN(vin_temp, out _);
                        }
                        else
                        {
                            DealNewJJH(vin_temp, out _);
                        }
                        break;
                    default:
                        break;
                }
                //return;
            }
        }

        /// <summary>处理一个新进入的机号,可能是移入缓存内
        /// 
        /// </summary>
        bool DealNewVIN(string vin, out string err)
        {
            if (string.IsNullOrWhiteSpace(vin))
            {
                err = "您录入的机号为空";
                AppMessage.Add(err, AppMessage.MsgType.机号, true, AppMessage.ImportantEnum.Err);
                return false;
            }

            var vinmsg_temp = VinMsg.Create(vin, out err);
            if (err != "")
            {
                AppMessage.Add("机号:" + vin + "存在错误:" + err, AppMessage.MsgType.机号, true, AppMessage.ImportantEnum.Alarm);
                return false;
            }

            switch (AppConfig.GXType_Enum)
            {
                case AppConfig.GXType_EnumType.扭力:
                case AppConfig.GXType_EnumType.干检:

                    if (vinmsg != null && vinmsg.vin == vin && !vinmsg.IsEnd)//和当前正在操作的机号重复
                    {
                        err = "您录入的机号和当前正在装配的机号重复";
                        AppMessage.Add(err, AppMessage.MsgType.机号, true, AppMessage.ImportantEnum.Err);
                        return false;
                    }

                    if (AppConfig.RFIDRECIVETYPE == 3)
                    {
                        pLCConnect.PLCSend(PLCConnect.HandUnLock1, "飞轮放行");
                    }

                    if (AppConfig.ISCACHE && vinmsg != null && !vinmsg.IsEnd)//尝试移入到缓存,只有在缓存开启且上一个机号不为空且未结束的情况下才移入缓存
                    {
                        /*textBox_HC.Text =*/
                        JHQueue.SetVin(vinmsg_temp, out _).ToString();
                    }
                    else
                    {
                        var re_temp = DealNowVin(vin, out err);//扭力、干检岗位机号立即处理
                        ShowJHCacheCount();
                        return re_temp;
                    }

                    //if (vinmsg == null || vinmsg.IsEnd)//上一个产品已经处理完毕 或 刚打开应用程序
                    //{
                    //    return DealNowVin(vin, out err);
                    //}
                    ////尝试移入到缓存
                    //if (!AppConfig.ISCACHE)
                    //{
                    //    err = "当前机号还未处理结束,新录入的机号" + vin + "已丢弃";
                    //    AppMessage.Add(err, AppMessage.MsgType.机号, true, AppMessage.ImportantEnum.Err);
                    //    return false;
                    //}
                    //textBox_HC.Text = JHQueue.SetVin(vin, out err).ToString();
                    //if (err != "")
                    //{
                    //    AppMessage.Add(err, AppMessage.MsgType.机号, true, AppMessage.ImportantEnum.Err);
                    //    return false;
                    //}
                    err = "机号:" + vin + "已存入缓存";
                    return true;
                case AppConfig.GXType_EnumType.人工:
                case AppConfig.GXType_EnumType.绑定:
                    return DealNowVin(vinmsg_temp, out err);//人工和绑定岗位的机号必须立刻处理
                default:
                    err = "当前岗位类型未知";
                    AppMessage.Add(err, AppMessage.MsgType.机号, true, AppMessage.ImportantEnum.Err);
                    return false;
            }
        }

        /// <summary>当前立刻处理一个机号为当前正在操作的产品机号
        /// 
        /// </summary>
        bool DealNowVin(string vinmsg_temp, out string err)
        {
            var vin = VinMsg.Create(vinmsg_temp, out err);
            if (err == "")
            {
                return DealNowVin(vin, out err);//重载
            }
            return false;
        }

        /// <summary>当前立刻处理一个机号为当前正在操作的产品机号
        /// 
        /// </summary>
        bool DealNowVin(VinMsg vinmsg_temp, out string err)
        {
            #region 换产确认界面弹出
            VinChange.GetChange(vinmsg_temp);
            VinChange.SetChange(vinmsg_temp);
            #endregion
            vinmsg = vinmsg_temp;
            #region 空机号调整界面处理
            if (vinmsg_temp == null)
            {
                #region 界面驱动
                //左侧
                textBox_JH.Text = SParms.NothingStr;
                textBox_DQJX.Text = SParms.NothingStr;
                textBox_ZTBM.Text = SParms.NothingStr;
                SetTextControl(textBox_QDZT, SParms.NothingStr, SParms.DefultColor);
                //textBox_QDZT.BackColor = SParms.NomalColor;
                label_QDZTBHG.Visible = false;
                SetTextControl(textBox_SJSC, SParms.NothingStr, SParms.DefultColor);

                //中间
                textBox_NLSX.Text = SParms.NothingStr;
                textBox_NLXX.Text = SParms.NothingStr;
                textBox_DQCX.Text = SParms.NothingStr;

                //右侧
                textBox_NJJS.Text = SParms.NothingStr;
                textBox_NJZS.Text = SParms.NothingStr;
                SetTextControl(textBox_DCJG, SParms.NothingStr, SParms.DefultColor);
                textBox_DQNL.Text = SParms.NothingStr;
                textBox_DQJD.Text = SParms.NothingStr;

                //err = "机号信息错误:" + err;
                //textBox_Log.Text = err;
                //return false;
                #endregion
                err = "机号信息为空";
                return false;
            }
            #endregion
            AppMessage.Add("当前机号:" + vinmsg.vin, AppMessage.MsgType.机号, true, AppMessage.ImportantEnum.Normal);
            #region 界面驱动

            //左侧
            textBox_JH.Text = vinmsg_temp.vin;
            textBox_DQJX.Text = vinmsg_temp.JX;
            textBox_ZTBM.Text = vinmsg_temp.ZTBM;
            SetTextControl(textBox_QDZT, vinmsg_temp.QDZT ? "前端合格" : "前端不合格", vinmsg_temp.QDZT ? SParms.GoodColor : SParms.ErrColor);
            //textBox_QDZT.Tag = vinmsg_temp.QDBHGGX;
            if (vinmsg_temp.QDBHGGX != null && vinmsg_temp.QDBHGGX.Count != 0)
            {
                SetTextControl(label_QDZTBHG, vinmsg_temp.QDBHGGX.Join(), SParms.ErrColor);
                label_QDZTBHG.Visible = true;
            }
            else
            {
                label_QDZTBHG.Visible = false;
            }
            SetTextControl(textBox_SJSC, SParms.NothingStr, SParms.DefultColor);

            //结果数据
            SetTextControl(textBox_DCJG, SParms.NothingStr, SParms.DefultColor);
            textBox_DQNL.Text = SParms.NothingStr;
            textBox_DQJD.Text = SParms.NothingStr;

            switch (AppConfig.GXType_Enum)
            {
                case AppConfig.GXType_EnumType.扭力:
                    //中间
                    textBox_NLSX.Text = vinmsg_temp.NLSX.ToString() + "N·m";
                    textBox_NLXX.Text = vinmsg_temp.NLXX.ToString() + "N·m";

                    //右侧
                    textBox_NJJS.Text = (vinmsg.NowCount - 1).ToString();
                    textBox_NJZS.Text = vinmsg.TotalCount.ToString();
                    break;
                case AppConfig.GXType_EnumType.干检:
                    //中间
                    textBox_NLSX.Text = vinmsg_temp.NLSX.ToString() + "KPa";
                    textBox_NLXX.Text = vinmsg_temp.NLXX.ToString() + "KPa";
                    break;
                case AppConfig.GXType_EnumType.人工:

                    break;
            }
            textBox_DQCX.Text = vinmsg_temp.DQCXH;

            #endregion
            #region 设备控制
            switch (AppConfig.GXType_Enum)
            {
                case AppConfig.GXType_EnumType.扭力:

                    if (!vinmsg.IsEnd && vinmsg.QDZT)
                    {
                        dc.DC_Channel(vinmsg.DQCXH);//下发程序号到机台
                        dc.DC_Lock(false);//解锁
                    }

                    #region A1缸头复检岗位特殊处理

                    if (AppConfig.SCX == 1 && AppConfig.GX == 22)
                    {
                        if (!vinmsg.QDZT)
                        {
                            //因为通机A1线部分机型工艺数据维护不合理必须增加这行代码，比如1T6FQG004状态，该状态不在自动化岗位打螺栓扭力，只在人工岗位处理，按照我们的逻辑应当是49、50岗位维护互锁标志为N，人工缸头岗位维护为Y，但其实际维护是49、50岗位是Y，人工缸头扭力岗位是N，所以每一台发动机到此岗位都是前端状态不合格，必须在前端状态不合格时也发送设备切换频道和解锁指令，让其正常操作。
                            dc.DC_Channel(vinmsg.DQCXH);
                            dc.DC_Lock(false);

                            pLCConnect?.PLCSend(new byte[] { 1 }, "A1缸头前端不合格放行");
                        }
                    }

                    #endregion

                    break;
                case AppConfig.GXType_EnumType.干检:

                    textBox_NJJS.Text = vinmsg.充气时间_干检.ToString() + "S";
                    textBox_NJZS.Text = vinmsg.允许泄漏_干检.ToString() + "KPa";

                    if (!vinmsg.IsEnd && vinmsg.QDZT)
                    {
                        //开始干检,注意:当前使用该程序的只有1、3、6线，但1、3、6线只生产小机型，而且所有干检使用都同一个频道且充气时间都为7秒,因此此处并不需要切换频道或调整充气时间,如果后期要增加该功能,可考虑使用函数dc.Start(vinmsg_temp.DQCXH)切换频道并开始作业
                        //dc.Stop();
                        //Task.Run(() => { System.Threading.Thread.Sleep(1200); });
                        //dc.Start(vinmsg_temp.DQCXH);
                        //dc.Channel(vinmsg_temp.DQCXH);
                        //Task.Run(() => { System.Threading.Thread.Sleep(300); });
                        dc.DC_Start();
                    }

                    break;
                case AppConfig.GXType_EnumType.人工:
                    break;
            }

            #endregion
            #region 人工和绑定工位得到机号后要立刻处理完成

            switch (AppConfig.GXType_Enum)
            {
                case AppConfig.GXType_EnumType.人工:

                    if (vinmsg_temp.QDZT)
                    {
                        switch (AppConfig.GXType_Enum)
                        {
                            case AppConfig.GXType_EnumType.人工:
                                bool IsOK = button_HandOK.Text == "合格";
                                SQL.Insert_ZXJC_Data_List(vinmsg_temp, IsOK, out err);

                                //结果显示及错误数据上传至数据库
                                bool UpdateOK_Temp = err == "";
                                if (!UpdateOK_Temp)
                                {
                                    AppMessage.Add("上传数据失败", AppMessage.MsgType.应用程序错误, false, AppMessage.ImportantEnum.Alarm, err);
                                }
                                SetTextControl(textBox_SJSC, UpdateOK_Temp ? "上传成功" : "上传失败", UpdateOK_Temp ? SParms.GoodColor : SParms.ErrColor);

                                //给PLC走主线或支线的命令
                                if (IsOK && UpdateOK_Temp)
                                {
                                    pLCConnect?.PLCSend(PLCConnect.HandUnLock1, "人工放行主线");
                                }
                                else if (!IsOK && UpdateOK_Temp)
                                {
                                    pLCConnect?.PLCSend(PLCConnect.HandUnLock2, "人工放行支线");
                                }

                                //处理完毕后要关闭界面中的 不合格记录 和 下拉选单
                                SetTextControl(button_HandOK, "合格", SParms.GoodColor);
                                cb_HandReson.SelectedItem = null;

                                return true;
                        }
                    }
                    else
                    {
                        pLCConnect.PLCSend(PLCConnect.HandUnLock2, "人工放行支线");
                    }
                    button_HandOK.Text = "合格";
                    vinmsg.IsEnd = true;
                    err = "";
                    return true;
                case AppConfig.GXType_EnumType.绑定:

                    JHBinding.vinInScan = vinmsg_temp.vin;
                    BindingShow();

                    break;
            }
            #endregion
            err = "";
            return true;
        }

        /// <summary>绑定岗位专用,处理夹具号
        /// 
        /// </summary>
        /// <param name="jjh_temp"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        bool DealNewJJH(string jjh_temp, out string err)
        {
            if (jjh_temp.Length == 10)
            {
                textBox_JJHInScan .Text= jjh_temp;
                JHBinding.jjhInScan = jjh_temp;
                err = "";
                return true;
            }
            err = "夹具号长度错误:" + jjh_temp.Length;
            return false;
        }

        #endregion
        #region 夹具绑定

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox cb)
            {
                cb.Text = cb.Checked ? "夹具号来源：RFID" : "夹具号来源：扫描";
                JHBinding.WorkMode = cb.Checked ? JHBinding.WorkModeEnum.以RFID作为夹具号来源 : JHBinding.WorkModeEnum.以扫描作为夹具号来源;
                BindingShow();
            }
        }

        private void JHBinding_BindingOver_Event(JHBinding.BindingResultEnum bindingResultEnum, DateTime bindingtime, string jjh, string vin, string err)
        {
            string str_temp = string.Format("时间:{0}\r\n机号:{1}\r\n夹具号:{2}\r\n结果:{3}\r\n{4}", bindingtime.ToStandardString(), vin, jjh, bindingResultEnum, err);
            this.BeginInvoke_CheckHandle(() =>
            {
                SetTextControl(textBox_SJSC, bindingResultEnum.ToString(), bindingResultEnum == JHBinding.BindingResultEnum.绑定成功 ? SParms.GoodColor : SParms.ErrColor);
                textBox_SJSC.Tag = err;
            });
        }

        private void button_Binding_Help_Click(object sender, EventArgs e)
        {
            string s_tmep = "绑定岗位工作模式分两种:\r\n1、夹具号来源：RFID，即正常工作模式，员工待夹具流转到RFID位置时，扫描机号条码即可绑定。\r\n2、夹具号来源：扫描，非正常工作模式，员工待夹具流转到RFID位置时，先扫描机号条码，后扫描夹具号，即可实现绑定，同时程序会将机号、扫描夹具号、RFID中的夹具号上传到数据库中备存，以便判断芯片中的夹具号和条码纸的夹具号是否不同，或检查芯片中的夹具号是否有重复，或检查条码纸的夹具号是否有重复，一般来说，使用非正常工作模式只需要持续夹具在生产线上流转一圈的时间即可。\r\n\r\n特殊情况处理：\r\n1、空夹具：直接踩脚踏板放行。\r\n2、当需要在非正常工作模式下工作且条码纸污损导致无法扫描：可临时切换回正常工作模式，处理该夹具号再切换回非正常工作模式。\r\n\r\n注意：\r\n1、非正常工作模式必须先扫描机号再扫描夹具号，因为扫描机号时会清除系统中缓存的扫描夹具号。\r\n2、若绑定不成功，可点击\"数据上传\"的文本框查看未成功原因";
            MessageBox.Show(s_tmep);
        }

        private void textBox_SJSC_Click(object sender, EventArgs e)//显示数据上传的错误
        {
            if (sender is TextBox tb)
            {
                if (tb.Tag != null)
                {
                    var s_temp = tb.Tag.ToString();
                    if (string.IsNullOrWhiteSpace(s_temp))
                    {
                        MessageBox.Show(s_temp);
                    }
                }
            }
        }

        /// <summary>绑定岗位显示相关信息
        /// 
        /// </summary>
        void BindingShow()
        {
            textBox_JJHInRFID.Text = JHBinding.jjhInRFID;
            textBox_JJHInScan.Text = JHBinding.jjhInScan;
        }

        #endregion
        #region 设备数据采集

        private void AtlasConnecter_com_RetrunAtlsCommand_Event(AtlasConnecter_Com.AtlsCommand a)
        {
            this.BeginInvoke_CheckHandle(() =>
            {
                switch (a.命令号)
                {
                    case 35:
                        AppMessage.Add("35收到工作结果", AppMessage.MsgType.阿特拉斯网络指令, false);
                        break;
                    case 4:
                        AppMessage.Add("4拒绝," + a.附加解析类.ToString(), AppMessage.MsgType.阿特拉斯网络指令, true);
                        break;
                    case 9999:
                        AppMessage.Add("9999心跳", AppMessage.MsgType.阿特拉斯网络指令, false);
                        break;
                    case 11:
                        AppMessage.Add("同意切换PSet", AppMessage.MsgType.阿特拉斯网络指令, false);
                        break;
                    case 31:
                        AppMessage.Add("同意切换Job", AppMessage.MsgType.阿特拉斯网络指令, false);
                        break;
                    default:
                        break;
                }
            });
        }

        private void AtlasConnecter_RetrunAtlsCommand_Event(AtlasConnecter.AtlsCommand a)
        {
            this.BeginInvoke_CheckHandle(() =>
            {
                switch (a.命令号)
                {
                    case 35:
                        AppMessage.Add("35收到工作结果", AppMessage.MsgType.阿特拉斯网络指令, false);
                        break;
                    case 4:
                        AppMessage.Add("4拒绝", AppMessage.MsgType.阿特拉斯网络指令, true);
                        break;
                    case 9999:
                        AppMessage.Add("9999心跳", AppMessage.MsgType.阿特拉斯网络指令, false);
                        break;
                    case 11:
                        AppMessage.Add("同意切换PSet", AppMessage.MsgType.阿特拉斯网络指令, false);
                        break;
                    case 31:
                        AppMessage.Add("同意切换Job", AppMessage.MsgType.阿特拉斯网络指令, false);
                        break;
                    default:
                        break;
                }
            });
        }

        /// <summary>从设备数据采集器返回了一个过程控制数据
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void Dc_GetData(object[] obj)
        {
            this.BeginInvoke_CheckHandle(() =>
            {
                DealReceData(obj);
            });
        }

        /// <summary>从设备数据采集器返回了一个消息
        /// 
        /// </summary>
        /// <param name="obj"></param>
        private void Dc_Msg_Event(object[] obj)
        {
            if (obj[0] is string s)
            {
                AppMessage.Add(s, AppMessage.MsgType.数据采集, false, AppMessage.ImportantEnum.Err);
            }
            else if (obj[0] is AtlasConnecter.AtlsCommand a)
            {
                AppMessage.Add(a.Name, AppMessage.MsgType.数据采集, false, AppMessage.ImportantEnum.Err);
            }
        }

        private void timer1_DealReceData_Tick(object sender, EventArgs e)
        {
            var recedata = dc?.GetReceData();
            if (recedata == null)
            {
                return;
            }
            return;
            DealReceData(recedata);
        }

        private void DealReceData(object[] obj)
        {
            //string err;
            if (vinmsg == null)
            {
                AppMessage.Add("获取到设备数据但无机号,数据已丢弃", AppMessage.MsgType.一般, true, AppMessage.ImportantEnum.Alarm);
                return;
            }
            switch (AppConfig.GXType_Enum)
            {
                case AppConfig.GXType_EnumType.扭力:

                    if (obj[0] is TightResult tr)
                    {
                        //注意:博世拧紧枪返回数据中的Y/N是不准确的,无法用来计算是否正确,必须要按照设定的扭力进行对比,但目前博世拧紧枪未使用可不考虑此逻辑
                        //tr.TorqueStatus = vinmsg.NLSX >= tr.Torque && vinmsg.NLSX <= tr.Torque;

                        //AppMessage.Add("接收数据 | 命令号:" + a.命令号.ToString() + " | 长度:" + a.命令_String.Length + " | 内容:" + a.命令_String, AppMessage.MsgType.阿特拉斯网络指令);

                        textBox_DQNL.Text = tr.Torque.ToString();
                        textBox_DQJD.Text = tr.Angle.ToString();
                        SetTextControl(textBox_DCJG, tr.TorqueStatus ? "OK" : "NOK", tr.TorqueStatus ? SParms.GoodColor : SParms.ErrColor);
                        bool update_nl_temp = SQL.Insert_BOSCHPDC(vinmsg, tr.Torque, tr.TorqueStatus, out _);
                        if (update_nl_temp && tr.TorqueStatus)
                        {
                            textBox_NJJS.Text = vinmsg.NowCount++.ToString();
                        }
                        vinmsg.IsEnd = (vinmsg.TotalCount <= vinmsg.NowCount - 1);
                        if (vinmsg.IsEnd)
                        {
                            AppMessage.Add("拧紧结束", AppMessage.MsgType.一般, true, AppMessage.ImportantEnum.Normal);
                        }
                        else
                        {
                            AppMessage.Add("请您继续拧紧螺栓" + vinmsg.NowCount, AppMessage.MsgType.一般, true, AppMessage.ImportantEnum.Normal);
                        }

                        //螺栓全部完成时上传全部数据
                        if (vinmsg.IsEnd)
                        {
                            var updatere_nl = SQL.Insert_ZXJC_Data_List(vinmsg, true, out _);
                            SetTextControl(textBox_SJSC, updatere_nl ? "上传完成" : "上传失败", updatere_nl ? SParms.GoodColor : SParms.ErrColor);
                            if (updatere_nl)
                            {
                                dc.DC_ByPass();
                                dc.DC_Lock(true);

                                #region A1缸头复检岗位特殊处理

                                if (AppConfig.SCX == 1 && AppConfig.GX == 22)
                                {
                                    pLCConnect?.PLCSend(new byte[] { 1 }, "A1缸头合格放行");
                                }

                                #endregion
                            }

                            //缓存工位准备取出数据
                            var vinmsg_temp = JHQueue.GetVin();
                            //textBox_HC.Text = JHQueue.GetCount().ToString();
                            textBox_HC.Text = (JHQueue.GetCount() + (NowVinIsGoing ? 1 : 0)).ToString();
                            if (vinmsg_temp != null)
                            {
                                DealNowVin(vinmsg_temp, out _);
                            }
                            else
                            {
                                vinmsg = null;
                            }
                        }
                        ShowJHCacheCount();
                    }
                    break;
                case AppConfig.GXType_EnumType.干检:
                    Cosmo.GJResult gjre = obj[0] as Cosmo.GJResult;

                    //是否合格
                    SetTextControl(textBox_DCJG, gjre.OKRe, gjre.OK ? SParms.GoodColor : SParms.ErrColor);
                    if (!gjre.OK)
                    {
                        AppMessage.Add("干检不合格", AppMessage.MsgType.数据采集, true, AppMessage.ImportantEnum.Err);
                    }

                    //泄漏值 高 低
                    textBox_DQNL.Text = gjre.XLZ + "Pa";

                    //textBox_NLSX.Text = gjre.Hi.ToString();
                    //textBox_NLXX.Text = gjre.Lo.ToString();

                    //上传数据
                    var updatere_gj_1 = SQL.Insert_ZXJC_QMD(vinmsg, gjre, out _);
                    var updatere_gj_2 = SQL.Insert_ZXJC_Data_List(vinmsg, gjre.OK, out _);
                    var updatere_gj = updatere_gj_1 && updatere_gj_2;
                    SetTextControl(textBox_SJSC, updatere_gj ? "上传完成" : "上传失败", updatere_gj ? SParms.GoodColor : SParms.ErrColor);

                    //PLC允许通行指令
                    if (updatere_gj && gjre.OK)
                    {
                        pLCConnect?.PLCSend(PLCConnect.HandUnLock1, "人工放行主线");
                    }
                    vinmsg.IsEnd = true;

                    //缓存工位准备取出数据
                    //vinmsg = JHQueue.GetVin();
                    //if (vinmsg != null)
                    //{
                    //    DealNowVin(vinmsg, out err);//扭力岗位上一个完成后尝试取出机号
                    //}

                    break;
                default:
                    return;
            }
        }

        #endregion
        #region 界面交互

        #region 菜单栏和按钮

        private void ToolStripMenuItem_RFIDEnable_Click(object sender, EventArgs e)
        {
            SParms.RFIDEnable = !SParms.RFIDEnable;
            ShowRFIDEnable();
        }

        void ShowRFIDEnable()
        {
            if (SParms.RFIDEnable)
            {
                ToolStripMenuItem_RFID.Text = "RFID";
                //ToolAlarm.ShowDialog("当前模式为RFID模式,您无法扫描机号!");
                AppMessage.Add("当前模式为RFID模式,您无法扫描机号!", AppMessage.MsgType.RFID, true, AppMessage.ImportantEnum.Normal);
            }
            else
            {
                ToolStripMenuItem_RFID.Text = "扫描";
                //ToolAlarm.ShowDialog("当前模式为扫描模式,您无法通过RFID自动获取机号!");
                AppMessage.Add("当前模式为扫描模式,您无法通过RFID自动获取机号!", AppMessage.MsgType.RFID, true, AppMessage.ImportantEnum.Normal);
            }
            checkBox_LockScan.Checked = !SParms.RFIDEnable;
            ToolStripMenuItem_RFID复位.Visible = SParms.RFIDEnable;
        }

        private void 测试模式ToolStripMenuItem_Click(object sender, EventArgs e)//正式测试模式切换
        {
            SParms.DebugMode = !SParms.DebugMode;
            var s = sender as ToolStripMenuItem;
            if (SParms.DebugMode)
            {
                s.Text = "测试模式";
                AppMessage.Add("进入测试模式", AppMessage.MsgType.一般, true);
            }
            else
            {
                s.Text = "正常模式";
                AppMessage.Add("进入正常模式", AppMessage.MsgType.一般, true);
            }
        }

        private void button2_Click(object sender, EventArgs e)//清除一个
        {
            //var sw = System.Diagnostics.Stopwatch.StartNew();
            var vinmsg_temp = JHQueue.GetVin();
            //System.Diagnostics.Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
            //textBox_HC.Text = JHQueue.GetCount().ToString();
            //System.Diagnostics.Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
            DealNowVin(vinmsg_temp, out string _);//清除一个
            //System.Diagnostics.Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
            AppMessage.Add("已清除一个缓存的机号", AppMessage.MsgType.机号, true, AppMessage.ImportantEnum.Alarm);
            //System.Diagnostics.Trace.WriteLine(sw.ElapsedMilliseconds.ToString());
        }

        private void button1_Click(object sender, EventArgs e)//清除全部
        {
            JHQueue.ClearAll();
            //textBox_HC.Text = JHQueue.GetCount().ToString();
            VinMsg vinmsg_temp = null;
            DealNowVin(vinmsg_temp, out _);//清除全部
            AppMessage.Add("已清除全部缓存的机号", AppMessage.MsgType.机号, true, AppMessage.ImportantEnum.Alarm);
        }

        private void 解锁ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dc != null)
            {
                dc.DC_Lock(false);
            }
        }

        private void 复位ToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            switch (AppConfig.RFIDRECIVETYPE)
            {
                case 1:
                    if (rFIDConnect != null)
                    {
                        rFIDConnect.AutoAskIOStatus = true;
                    }
                    break;
                case 3:
                    FeiLunGongWei.Reset();
                    break;
            }
            LastReadJJH = "";
        }

        private void toolStripMenuItem17_Click(object sender, EventArgs e)//Atlas手动操作
        {
            if (sender is ToolStripMenuItem tsm)
            {
                if (AppConfig.TIGHTENCONTROLERTYPE == 0 || AppConfig.TIGHTENCONTROLERTYPE == 1)
                {
                    switch (tsm.Text)
                    {
                        case "锁定":
                            dc.DC_Lock(true);
                            break;
                        case "解锁":
                            dc.DC_Lock(false);
                            break;
                        case "P1":
                        case "P2":
                        case "P3":
                        case "P4":
                        case "P5":
                        case "P6":
                        case "P7":
                        case "P8":
                        case "P9":
                        case "P10":
                        case "P11":
                        case "P12":
                        case "P13":
                        case "P14":
                        case "P15":
                            dc.DC_Channel(tsm.Text);
                            break;
                    }
                }
            }
        }

        private void 时间同步ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TimeCheck.SyncTime(out _))//每次打开程序只能同步一次，不成功的话以后也不会成功的，没有继续尝试的必要
            {
                (sender as ToolStripItem).Visible = false;
            }
        }

        #endregion
        #region 底部状态栏

        private void timer_ShowTime_Tick(object sender, EventArgs e)//显示时间 锁定控件
        {
            toolStripStatusLabel_ShowTime.Text = DateTime.Now.ToStandardString();

            if (checkBox_LockScan.Checked)
            {
                textBox_Scan.Focus();
            }

            #region 设备和PLC连接灯

            if (pLCConnect != null && ucButton_CommcationStateShow_PLC.Visible)
            {
                ucButton_CommcationStateShow_PLC.IsConnected = pLCConnect.IsConnected;
                ucButton_CommcationStateShow_PLC.IsCommcation = pLCConnect.IsCommcation;
            }

            if (dc != null && ucButton_CommcationStateShow_SB.Visible)
            {
                ucButton_CommcationStateShow_SB.IsConnected = dc.DC_IsConnected;
                ucButton_CommcationStateShow_SB.IsCommcation = dc.DC_IsCommcation;
            }

            bool A1A3双飞轮客户端模式 = (AppConfig.RFIDRECIVETYPE == 3) && !FeiLunGongWei.当前为服务端模式;
            ucButton_CommcationStateShow_RFID.Visible = SParms.RFIDEnable && !A1A3双飞轮客户端模式;
            if (rFIDConnect != null /*&& ucButton_CommcationStateShow_RFID.Visible*/)
            {
                ucButton_CommcationStateShow_RFID.IsConnected = rFIDConnect.IsConnected;
                ucButton_CommcationStateShow_RFID.IsCommcation = rFIDConnect.IsCommcation;
            }

            #endregion
        }

        private void toolStripStatusLabel_ShowAppConfig_Click(object sender, EventArgs e)//点击显示AppConfig详情
        {
            var ps = typeof(AppConfig).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            string s = "";
            //var o = Activator.CreateInstance(typeof(AppConfig));
            foreach (var p in ps)
            {
                string pname = p.Name.ToUpper();
                string pvalue = p.GetValue(null).ToType<string>();
                //p.GetValue()
                s += pname + ":" + pvalue + "\r\n";
            }
            MessageBox.Show(s);
        }

        #endregion
        #region 显示日志

        int ShowLogCount = 1;
        const int MaxShowLogCount = 99;

        private void AppMessage_HaveShowLog_Event(AppMessage.Log log)
        {
            this.BeginInvoke_CheckHandle(/*async*/ () =>
            {
                Color color_temp = SParms.NomalColor;
                switch (log.importantEnum)
                {
                    case AppMessage.ImportantEnum.Err:
                        color_temp = SParms.ErrColor;
                        break;
                    case AppMessage.ImportantEnum.Alarm:
                        color_temp = SParms.AlarmColor;
                        break;
                    case AppMessage.ImportantEnum.Normal:
                        color_temp = SParms.GoodColor;
                        break;
                    default:
                        color_temp = SParms.GoodColor;
                        break;
                }
                textBox_Log.Text = ShowLogCount.ToString().PadLeft(2, '0') + ":" + log.LSXX1;
                textBox_Log.ForeColor = color_temp;
                ShowLogCount++;
                if (ShowLogCount > MaxShowLogCount)
                {
                    ShowLogCount = 1;
                }
            });
        }

        #endregion
        #region 点击

        //private void textBox_QDZT_Click(object sender, EventArgs e)//前端状态不合格时显示详细信息
        //{
        //    if (sender is TextBox tb)
        //    {
        //        if (tb.Tag is List<string> ls)
        //        {
        //            ToolAlarm.ShowDialog(ls.Join("\r\n"));
        //        }
        //    }
        //}

        private void button_HandOK_Click(object sender, EventArgs e)//人工岗位合格与不合格切换
        {
            if (sender is Button tb)
            {
                SetTextControl(tb, (tb.Text == "合格") ? "不合格" : "合格", (tb.Text == "合格") ? SParms.GoodColor : SParms.ErrColor);
            }
        }

        private void textBox_DQCX_Click(object sender, EventArgs e)//飞轮岗位程序号含义提示
        {
            ToolAlarm.ShowDialog("飞轮岗位程序号含义提示:\r\n以M11D为例,11代表11#程序\r\nD代表螺栓位置:S=水平 G=垂直-高位 D=垂直-低位");
        }

        private void textBox_HCJH_Click(object sender, EventArgs e)//查询缓存机号的详细内容
        {
            if (JHQueue.GetCount() > 0)
            {
                MessageBox.Show("当前缓存机号:" + JHQueue.GetAllCache().ToList().Join("\r\n"));
            }
        }

        #endregion
        #region 一个带前景或背景色的文本框显示

        void SetTextControl(Control c, string text, Color color)
        {
            c.Text = text;
            c.BackColor = color;
        }

        #endregion

        #endregion
        #region 干检手动控制

        private void 启动ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (dc != null)
            //{
            dc?.DC_Start();
            //}
        }

        private void 停止ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (dc != null)
            //{
            dc?.DC_Stop();
            //}
        }

        #endregion
        #region 程序关闭

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            string nowvin = "";
            if (vinmsg != null && !vinmsg.IsEnd)
            {
                nowvin = vinmsg.vin;
            }
            JHQueue.SaveToDB(nowvin);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)//退出
        {
            if (MessageBox.Show("你确定要退出程序吗?", "警告", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        #endregion
    }
}