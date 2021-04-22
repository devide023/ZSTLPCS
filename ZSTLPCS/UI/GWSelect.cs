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
    public partial class GWSelect : Form
    {
        public GWSelect()
        {
            //MessageBox.Show(Application.StartupPath);
            InitializeComponent();
            this.InitForm();
            SMethod.InitBackForeColor(this);

            dataGridView1.DataSource = SQL.SQLConnect.SelectSQL("select * from ZXJC_ZSTJPCS_CONFIG order by scx,gx,gongwei", out string err);
            dataGridView1.MultiSelect = false;
            if (dataGridView1.Rows.Count > 0 && dataGridView1.Columns.Count > 0)
            {
                dataGridView1.CurrentCell = dataGridView1[0, 0];
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        /// <summary>初始化选定的工位名称
        /// 
        /// </summary>
        public void Init(string initpcip)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                string gwPCIP = dataGridView1["PCIP", i].Value.ToType<string>();
                if (gwPCIP==initpcip)
                {
                    dataGridView1.CurrentCell = dataGridView1[0, i];
                    return;
                }
            }
        }

        /// <summary>当前选定的工位
        /// 
        /// </summary>
        public string SelectPCIP = "";

        private void bt_login_Click(object sender, EventArgs e)//登录
        {
            this.DialogResult = DialogResult.Yes;

            if (dataGridView1.CurrentCell != null)
            {
                SelectPCIP = dataGridView1["PCIP", dataGridView1.CurrentCell.RowIndex].Value.ToType<string>();
            }

            this.Close();
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    }
}