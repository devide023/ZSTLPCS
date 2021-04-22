namespace ZSTJPCS
{
    partial class Login
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            this.tb_user = new System.Windows.Forms.TextBox();
            this.tb_password = new System.Windows.Forms.TextBox();
            this.button_login = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label_title = new System.Windows.Forms.Label();
            this.bt_n9 = new System.Windows.Forms.Button();
            this.bt_n6 = new System.Windows.Forms.Button();
            this.bt_n3 = new System.Windows.Forms.Button();
            this.bt_n8 = new System.Windows.Forms.Button();
            this.bt_n7 = new System.Windows.Forms.Button();
            this.bt_n5 = new System.Windows.Forms.Button();
            this.bt_n4 = new System.Windows.Forms.Button();
            this.bt_n2 = new System.Windows.Forms.Button();
            this.bt_n1 = new System.Windows.Forms.Button();
            this.bt_backspace = new System.Windows.Forms.Button();
            this.bt_clear = new System.Windows.Forms.Button();
            this.bt_n0 = new System.Windows.Forms.Button();
            this.label_IPMsg = new System.Windows.Forms.Label();
            this.button_getconfig = new System.Windows.Forms.Button();
            this.button_exit = new System.Windows.Forms.Button();
            this.label_ServerVersion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tb_user
            // 
            this.tb_user.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb_user.Location = new System.Drawing.Point(238, 112);
            this.tb_user.Margin = new System.Windows.Forms.Padding(4);
            this.tb_user.Name = "tb_user";
            this.tb_user.Size = new System.Drawing.Size(316, 49);
            this.tb_user.TabIndex = 0;
            this.tb_user.Click += new System.EventHandler(this.tb_user_Click);
            // 
            // tb_password
            // 
            this.tb_password.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb_password.Location = new System.Drawing.Point(238, 219);
            this.tb_password.Margin = new System.Windows.Forms.Padding(4);
            this.tb_password.Name = "tb_password";
            this.tb_password.PasswordChar = '*';
            this.tb_password.Size = new System.Drawing.Size(316, 49);
            this.tb_password.TabIndex = 1;
            this.tb_password.Click += new System.EventHandler(this.tb_password_Click);
            this.tb_password.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tb_password_KeyUp);
            // 
            // button_login
            // 
            this.button_login.Enabled = false;
            this.button_login.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold);
            this.button_login.Location = new System.Drawing.Point(238, 312);
            this.button_login.Margin = new System.Windows.Forms.Padding(4);
            this.button_login.Name = "button_login";
            this.button_login.Size = new System.Drawing.Size(316, 65);
            this.button_login.TabIndex = 2;
            this.button_login.Text = "登陆";
            this.button_login.UseVisualStyleBackColor = false;
            this.button_login.Click += new System.EventHandler(this.bt_login_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(34, 115);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 37);
            this.label1.TabIndex = 2;
            this.label1.Text = "账号:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(34, 219);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 37);
            this.label2.TabIndex = 2;
            this.label2.Text = "密码:";
            // 
            // label_title
            // 
            this.label_title.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_title.Location = new System.Drawing.Point(-4, 9);
            this.label_title.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_title.Name = "label_title";
            this.label_title.Size = new System.Drawing.Size(1021, 85);
            this.label_title.TabIndex = 2;
            this.label_title.Text = "宗申通机生产过程数据追溯系统";
            this.label_title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bt_n9
            // 
            this.bt_n9.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_n9.Location = new System.Drawing.Point(790, 112);
            this.bt_n9.Margin = new System.Windows.Forms.Padding(4);
            this.bt_n9.Name = "bt_n9";
            this.bt_n9.Size = new System.Drawing.Size(91, 85);
            this.bt_n9.TabIndex = 4;
            this.bt_n9.Text = "9";
            this.bt_n9.UseVisualStyleBackColor = false;
            this.bt_n9.Click += new System.EventHandler(this.bt_n_Click);
            // 
            // bt_n6
            // 
            this.bt_n6.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_n6.Location = new System.Drawing.Point(790, 204);
            this.bt_n6.Margin = new System.Windows.Forms.Padding(4);
            this.bt_n6.Name = "bt_n6";
            this.bt_n6.Size = new System.Drawing.Size(91, 85);
            this.bt_n6.TabIndex = 5;
            this.bt_n6.Text = "6";
            this.bt_n6.UseVisualStyleBackColor = false;
            this.bt_n6.Click += new System.EventHandler(this.bt_n_Click);
            // 
            // bt_n3
            // 
            this.bt_n3.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_n3.Location = new System.Drawing.Point(790, 297);
            this.bt_n3.Margin = new System.Windows.Forms.Padding(4);
            this.bt_n3.Name = "bt_n3";
            this.bt_n3.Size = new System.Drawing.Size(91, 85);
            this.bt_n3.TabIndex = 6;
            this.bt_n3.Text = "3";
            this.bt_n3.UseVisualStyleBackColor = false;
            this.bt_n3.Click += new System.EventHandler(this.bt_n_Click);
            // 
            // bt_n8
            // 
            this.bt_n8.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_n8.Location = new System.Drawing.Point(691, 112);
            this.bt_n8.Margin = new System.Windows.Forms.Padding(4);
            this.bt_n8.Name = "bt_n8";
            this.bt_n8.Size = new System.Drawing.Size(91, 85);
            this.bt_n8.TabIndex = 7;
            this.bt_n8.Text = "8";
            this.bt_n8.UseVisualStyleBackColor = false;
            this.bt_n8.Click += new System.EventHandler(this.bt_n_Click);
            // 
            // bt_n7
            // 
            this.bt_n7.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_n7.Location = new System.Drawing.Point(593, 112);
            this.bt_n7.Margin = new System.Windows.Forms.Padding(4);
            this.bt_n7.Name = "bt_n7";
            this.bt_n7.Size = new System.Drawing.Size(91, 85);
            this.bt_n7.TabIndex = 8;
            this.bt_n7.Text = "7";
            this.bt_n7.UseVisualStyleBackColor = false;
            this.bt_n7.Click += new System.EventHandler(this.bt_n_Click);
            // 
            // bt_n5
            // 
            this.bt_n5.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_n5.Location = new System.Drawing.Point(691, 204);
            this.bt_n5.Margin = new System.Windows.Forms.Padding(4);
            this.bt_n5.Name = "bt_n5";
            this.bt_n5.Size = new System.Drawing.Size(91, 85);
            this.bt_n5.TabIndex = 9;
            this.bt_n5.Text = "5";
            this.bt_n5.UseVisualStyleBackColor = false;
            this.bt_n5.Click += new System.EventHandler(this.bt_n_Click);
            // 
            // bt_n4
            // 
            this.bt_n4.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_n4.Location = new System.Drawing.Point(593, 204);
            this.bt_n4.Margin = new System.Windows.Forms.Padding(4);
            this.bt_n4.Name = "bt_n4";
            this.bt_n4.Size = new System.Drawing.Size(91, 85);
            this.bt_n4.TabIndex = 10;
            this.bt_n4.Text = "4";
            this.bt_n4.UseVisualStyleBackColor = false;
            this.bt_n4.Click += new System.EventHandler(this.bt_n_Click);
            // 
            // bt_n2
            // 
            this.bt_n2.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_n2.Location = new System.Drawing.Point(691, 297);
            this.bt_n2.Margin = new System.Windows.Forms.Padding(4);
            this.bt_n2.Name = "bt_n2";
            this.bt_n2.Size = new System.Drawing.Size(91, 85);
            this.bt_n2.TabIndex = 11;
            this.bt_n2.Text = "2";
            this.bt_n2.UseVisualStyleBackColor = false;
            this.bt_n2.Click += new System.EventHandler(this.bt_n_Click);
            // 
            // bt_n1
            // 
            this.bt_n1.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_n1.Location = new System.Drawing.Point(593, 297);
            this.bt_n1.Margin = new System.Windows.Forms.Padding(4);
            this.bt_n1.Name = "bt_n1";
            this.bt_n1.Size = new System.Drawing.Size(91, 85);
            this.bt_n1.TabIndex = 12;
            this.bt_n1.Text = "1";
            this.bt_n1.UseVisualStyleBackColor = false;
            this.bt_n1.Click += new System.EventHandler(this.bt_n_Click);
            // 
            // bt_backspace
            // 
            this.bt_backspace.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_backspace.Location = new System.Drawing.Point(889, 112);
            this.bt_backspace.Margin = new System.Windows.Forms.Padding(4);
            this.bt_backspace.Name = "bt_backspace";
            this.bt_backspace.Size = new System.Drawing.Size(132, 85);
            this.bt_backspace.TabIndex = 15;
            this.bt_backspace.Text = "回删";
            this.bt_backspace.UseVisualStyleBackColor = false;
            this.bt_backspace.Click += new System.EventHandler(this.bt_changepassword_Click);
            // 
            // bt_clear
            // 
            this.bt_clear.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_clear.Location = new System.Drawing.Point(889, 205);
            this.bt_clear.Margin = new System.Windows.Forms.Padding(4);
            this.bt_clear.Name = "bt_clear";
            this.bt_clear.Size = new System.Drawing.Size(132, 85);
            this.bt_clear.TabIndex = 16;
            this.bt_clear.Text = "清除";
            this.bt_clear.UseVisualStyleBackColor = false;
            this.bt_clear.Click += new System.EventHandler(this.bt_clean_Click);
            // 
            // bt_n0
            // 
            this.bt_n0.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.bt_n0.Location = new System.Drawing.Point(889, 296);
            this.bt_n0.Margin = new System.Windows.Forms.Padding(4);
            this.bt_n0.Name = "bt_n0";
            this.bt_n0.Size = new System.Drawing.Size(132, 86);
            this.bt_n0.TabIndex = 18;
            this.bt_n0.Text = "0";
            this.bt_n0.UseVisualStyleBackColor = false;
            this.bt_n0.Click += new System.EventHandler(this.bt_n_Click);
            // 
            // label_IPMsg
            // 
            this.label_IPMsg.Font = new System.Drawing.Font("宋体", 10F);
            this.label_IPMsg.Location = new System.Drawing.Point(25, 411);
            this.label_IPMsg.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_IPMsg.Name = "label_IPMsg";
            this.label_IPMsg.Size = new System.Drawing.Size(529, 100);
            this.label_IPMsg.TabIndex = 20;
            this.label_IPMsg.Text = "当前计算机有效网络地址为:172.16.147.220\r\n\r\n当前岗位配置信息为：";
            this.label_IPMsg.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_getconfig
            // 
            this.button_getconfig.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_getconfig.Location = new System.Drawing.Point(593, 411);
            this.button_getconfig.Margin = new System.Windows.Forms.Padding(4);
            this.button_getconfig.Name = "button_getconfig";
            this.button_getconfig.Size = new System.Drawing.Size(424, 100);
            this.button_getconfig.TabIndex = 21;
            this.button_getconfig.Text = "重新获取网络地址";
            this.button_getconfig.UseVisualStyleBackColor = false;
            this.button_getconfig.Click += new System.EventHandler(this.button1_Click);
            // 
            // button_exit
            // 
            this.button_exit.Font = new System.Drawing.Font("宋体", 21.75F, System.Drawing.FontStyle.Bold);
            this.button_exit.Location = new System.Drawing.Point(28, 312);
            this.button_exit.Margin = new System.Windows.Forms.Padding(4);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(202, 65);
            this.button_exit.TabIndex = 22;
            this.button_exit.Text = "退出";
            this.button_exit.UseVisualStyleBackColor = false;
            this.button_exit.Click += new System.EventHandler(this.button_exit_Click);
            // 
            // label_ServerVersion
            // 
            this.label_ServerVersion.Location = new System.Drawing.Point(0, -1);
            this.label_ServerVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_ServerVersion.Name = "label_ServerVersion";
            this.label_ServerVersion.Size = new System.Drawing.Size(438, 34);
            this.label_ServerVersion.TabIndex = 23;
            this.label_ServerVersion.Text = "服务器上的最新版本为:";
            this.label_ServerVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(51)))), ((int)(((byte)(88)))));
            this.ClientSize = new System.Drawing.Size(1034, 520);
            this.Controls.Add(this.label_ServerVersion);
            this.Controls.Add(this.button_exit);
            this.Controls.Add(this.button_getconfig);
            this.Controls.Add(this.label_IPMsg);
            this.Controls.Add(this.bt_n9);
            this.Controls.Add(this.bt_n6);
            this.Controls.Add(this.bt_n3);
            this.Controls.Add(this.bt_n8);
            this.Controls.Add(this.bt_n7);
            this.Controls.Add(this.bt_n5);
            this.Controls.Add(this.bt_n4);
            this.Controls.Add(this.bt_n2);
            this.Controls.Add(this.bt_n1);
            this.Controls.Add(this.bt_backspace);
            this.Controls.Add(this.bt_clear);
            this.Controls.Add(this.bt_n0);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label_title);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_login);
            this.Controls.Add(this.tb_password);
            this.Controls.Add(this.tb_user);
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "登陆";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.UserLogin_FormClosed);
            this.Load += new System.EventHandler(this.Login_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tb_user;
        private System.Windows.Forms.TextBox tb_password;
        private System.Windows.Forms.Button button_login;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_title;
        private System.Windows.Forms.Button bt_n9;
        private System.Windows.Forms.Button bt_n6;
        private System.Windows.Forms.Button bt_n3;
        private System.Windows.Forms.Button bt_n8;
        private System.Windows.Forms.Button bt_n7;
        private System.Windows.Forms.Button bt_n5;
        private System.Windows.Forms.Button bt_n4;
        private System.Windows.Forms.Button bt_n2;
        private System.Windows.Forms.Button bt_n1;
        private System.Windows.Forms.Button bt_backspace;
        private System.Windows.Forms.Button bt_clear;
        private System.Windows.Forms.Button bt_n0;
        private System.Windows.Forms.Label label_IPMsg;
        private System.Windows.Forms.Button button_getconfig;
        private System.Windows.Forms.Button button_exit;
        private System.Windows.Forms.Label label_ServerVersion;
    }
}