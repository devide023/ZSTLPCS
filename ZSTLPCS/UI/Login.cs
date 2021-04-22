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
    public partial class Login : Form
    {
        #region 初始化
        public Login()
        {
            //MessageBox.Show(Application.StartupPath);
            InitializeComponent();
            this.InitForm();
            SMethod.InitBackForeColor(this);
            label_title.Text += string.Format("({0})", ApplicationUpdate.GetCurrentVersion());
            Task.Run(() =>
            {
                ApplicationUpdate.Init();
                this.BeginInvoke_CheckHandle(() =>
                {
                    button_login.Enabled = true;
                    label_ServerVersion.Text = ApplicationUpdate.CheckForUpdate_Result ? "当前有可用更新" : "当前无可用更新";
                    label_ServerVersion.ForeColor = ApplicationUpdate.CheckForUpdate_Result ? SParms.AlarmColor : SParms.NomalColor;
                    label_ServerVersion.Visible = true;
                });
            });
            //if ( string.IsNullOrWhiteSpace(  ApplicationUpdate.ServerVewsion))
            //{
            //    label_ServerVersion.Text = "服务器上的最新版本为:" + ApplicationUpdate.ServerVewsion;
            //}
            //else
            //{
            //}
        }

        private void Login_Load(object sender, EventArgs e)
        {
            button1_Click(null, null);
            tb_user.Focus();
            //Lang.ChangeLang(this);
        }
        #endregion
        #region 登录

        private void bt_login_Click(object sender, EventArgs e)//登录
        {
            Button bt = button_login;
            var str_temp = bt.Text;
            bt.Text = "登录并检查更新";
            bt.Enabled = false;
            TryLogin();
            bt.Text = str_temp;
            bt.Enabled = true;
        }

        private void tb_password_KeyUp(object sender, KeyEventArgs e)//在密码录入后按下回车登录
        {
            if (e.KeyCode == Keys.Enter)
            {
                bt_login_Click(button_login, null);
            }
        }

        public void TryLogin()
        {
            bool mysuperid = tb_user.Text == "331";//匹配岗位的超级权限
            bool mysuperid_configwindow = tb_user.Text == "332";//不匹配岗位的超级权限,且还能弹出窗口选择相应的岗位
            bool bingding_id = tb_user.Text == "999";//人工绑定岗位专用ID

            if (!mysuperid_configwindow && !IsGetConfig)//获取程序配置信息
            {
                button1_Click(null, null);
                if (!IsGetConfig)
                {
                    return;
                }
            }

            if (!mysuperid && !mysuperid_configwindow && !bingding_id && (tb_user.Text == "" || tb_password.Text == ""))
            {
                MessageBox.Show("用户名和密码均不能为空", "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string loginResult = SQL.Login(tb_user.Text, tb_password.Text, out string err);

            if (mysuperid_configwindow)
            {
                GWSelect gwselect = new GWSelect();
                gwselect.Init(AppConfig.PCIP);
                if (gwselect.ShowDialog() == DialogResult.Yes)
                {
                    SQL.GetConfig(new List<string> { gwselect.SelectPCIP }, out err);
                    AppConfig.GetDynmaicConfigImm();
                }
                else
                {
                    return;
                }
            }

            if (mysuperid || mysuperid_configwindow || (bingding_id && AppConfig.GXType_Enum== AppConfig.GXType_EnumType.绑定) || (!string.IsNullOrEmpty(loginResult) && loginResult.Substring(0, 1) == "1"))
            {
                AppConfig.Init();

                //登陆前的ClickOnce自动更新判定
                if (ApplicationUpdate.CheckForUpdate(AppConfig.ALLOWLOWVERSION))
                {
                    //if (MessageBox.Show(string.Format("使用该版本登陆将自动更新\r\n最新版:{0}\r\n当前版本:{1}\r\n最低允许版本:{2}\r\n你确定要这么做吗?更新无法回退!", ApplicationUpdate.ServerVewsion, ApplicationUpdate.GetCurrentVersion(), AppConfig.ALLOWLOWVERSION), "警告", MessageBoxButtons.YesNo)
                    if (MessageBox.Show(string.Format("使用该版本登陆将自动更新\r\n当前版本:{0}\r\n最低允许版本:{1}\r\n你确定要这么做吗?更新无法回退!", ApplicationUpdate.GetCurrentVersion(), AppConfig.ALLOWLOWVERSION), "警告", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        ApplicationUpdate.Update();
                    }
                }

                switch (AppConfig.GXType_Enum)
                {
                    case AppConfig.GXType_EnumType.未定义:
                    default:
                        MessageBox.Show("你的工序类型不正确,请联系管理员", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    case AppConfig.GXType_EnumType.人工:
                    case AppConfig.GXType_EnumType.扭力:
                    case AppConfig.GXType_EnumType.干检:
                    case AppConfig.GXType_EnumType.绑定:
                        SParms.User = tb_user.Text;
                        this.Visible = false;
                        Main main = new Main(mysuperid || mysuperid_configwindow);
                        main.ShowDialog();
                        break;
                }
                this.Close();
            }
            else
            {
                MessageBox.Show(loginResult.Substring(1), "输入错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //tb_user.Text = "";
                tb_password.Text = "";
                tb_password.Focus();
                return;
            }
        }

        /// <summary>已经从数据库中获取所有配置信息
        /// 
        /// </summary>
        bool IsGetConfig;

        private async void button1_Click(object sender, EventArgs e)//重新获取本地网络地址 和 配置项 和 缓存机号
        {
            label_IPMsg.Text = "...";
            label_IPMsg.ForeColor = this.ForeColor;
            await Task.Run(() => System.Threading.Thread.Sleep(200));

            #region 检测和数据库的网络关系,一般来说,使用ClickOnce后,必须是联网状态才能显示登陆窗体,这个检测意义并不是很大
            if (!QL_Ping.PingOnce(SQL.DBIP))
            {
                label_IPMsg.Text = "您未和数据库服务器" + SQL.DBIP + "建立网络连接.\r\n如果您刚打开计算机,请稍等后再尝试.若长时间无法解决,请通知网管";
                label_IPMsg.ForeColor = Color.Red;
                return;
            }
            #endregion
            #region 获取有效IP地址
            var enableip = QL_Net.GetEnableNetHosts("172.16");
            if (enableip.Count == 0)
            {
                label_IPMsg.Text = "您当前计算机的网卡中没有有效的IP地址,请检查网络配置后再次尝试.\r\n您当前计算机中检测到的网络地址为:" + QL_Net.GetHostIPs().Join();
                label_IPMsg.ForeColor = Color.Red;
                return;
            }
            #endregion

            #region 获取当前有效IP地址并显示 注入配置信息
            SQL.GetConfig(enableip, out string err);
            if (err != "")
            {
                label_IPMsg.Text = err;
                label_IPMsg.ForeColor = Color.Red;
                return;
            }

            #endregion

            label_IPMsg.Text = string.Format(
                "当前有效的网络地址为{0}\r\n岗位信息如下:\r\n生产线:{1} | 工位:{2} | 工序:{3} | 工位名称:{4}\r\n端口:{5} | 驱动类型:{6}",
                AppConfig.PCIP, AppConfig.SCX, AppConfig.GWName, AppConfig.GX, AppConfig.GWName, AppConfig.ComPort, AppConfig.TIGHTENCONTROLERTYPE);
            label_IPMsg.ForeColor = this.ForeColor;

            IsGetConfig = true;
        }

        #endregion
        #region 软键盘

        /// <summary>1等于选中用户框,2=选中密码框
        /// 
        /// </summary>
        int ClickTextBox = 1;

        private void tb_user_Click(object sender, EventArgs e)
        {
            ClickTextBox = 1;//选中用户框
        }

        private void tb_password_Click(object sender, EventArgs e)
        {
            ClickTextBox = 2;//选中密码框
        }

        private void bt_n_Click(object sender, EventArgs e)
        {
            Button bt = (Button)sender;

            switch (bt.Name)
            {
                case "bt_n0":
                    if (ClickTextBox == 1)
                    {
                        tb_user.Text += 0;
                    }
                    else if (ClickTextBox == 2)
                    {
                        tb_password.Text += 0;
                    }

                    break;

                case "bt_n1":
                    if (ClickTextBox == 1)
                    {
                        tb_user.Text += 1;
                    }
                    else if (ClickTextBox == 2)
                    {
                        tb_password.Text += 1;
                    }
                    break;
                case "bt_n2":
                    if (ClickTextBox == 1)
                    {
                        tb_user.Text += 2;
                    }
                    else if (ClickTextBox == 2)
                    {
                        tb_password.Text += 2;
                    }
                    break;
                case "bt_n3":
                    if (ClickTextBox == 1)
                    {
                        tb_user.Text += 3;
                    }
                    else if (ClickTextBox == 2)
                    {
                        tb_password.Text += 3;
                    }
                    break;
                case "bt_n4":
                    if (ClickTextBox == 1)
                    {
                        tb_user.Text += 4;
                    }
                    else if (ClickTextBox == 2)
                    {
                        tb_password.Text += 4;
                    }
                    break;

                case "bt_n5":
                    if (ClickTextBox == 1)
                    {
                        tb_user.Text += 5;
                    }
                    else if (ClickTextBox == 2)
                    {
                        tb_password.Text += 5;
                    }
                    break;
                case "bt_n6":
                    if (ClickTextBox == 1)
                    {
                        tb_user.Text += 6;
                    }
                    else if (ClickTextBox == 2)
                    {
                        tb_password.Text += 6;
                    }
                    break;
                case "bt_n7":
                    if (ClickTextBox == 1)
                    {
                        tb_user.Text += 7;
                    }
                    else if (ClickTextBox == 2)
                    {
                        tb_password.Text += 7;
                    }
                    break;
                case "bt_n8":
                    if (ClickTextBox == 1)
                    {
                        tb_user.Text += 8;
                    }
                    else if (ClickTextBox == 2)
                    {
                        tb_password.Text += 8;
                    }
                    break;
                case "bt_n9":
                    if (ClickTextBox == 1)
                    {
                        tb_user.Text += 9;
                    }
                    else if (ClickTextBox == 2)
                    {
                        tb_password.Text += 9;
                    }
                    break;
            }
        }

        private void bt_clean_Click(object sender, EventArgs e)
        {
            if (ClickTextBox == 1)
            {
                tb_user.Text = "";
            }
            else if (ClickTextBox == 2)
            {
                tb_password.Text = "";
            }
        }

        private void bt_changepassword_Click(object sender, EventArgs e)
        {
            if (ClickTextBox == 1)
            {
                if (tb_user.Text != "" && tb_user.Text != null)
                {
                    tb_user.Text = tb_user.Text.Substring(0, tb_user.Text.Length - 1);
                }
            }
            else if (ClickTextBox == 2)
            {
                if (tb_password.Text != "" && tb_password.Text != null)
                {
                    tb_password.Text = tb_password.Text.Substring(0, tb_password.Text.Length - 1);
                }
            }
        }

        #endregion
        #region 关闭窗体

        private void button_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UserLogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
            Application.Exit();
            System.Environment.Exit(0);
        }
        #endregion
    }
}