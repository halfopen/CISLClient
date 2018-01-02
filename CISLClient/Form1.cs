using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;

namespace CISLClient
{
	public class Form1 : Form
	{
        // 自动签入出
        private string autosignin;  
        private string autosignout;

        private string[] array;


        private string uname;

		private string pwd;

		private string zh_name;

		private bool flag;

		private int uid = 0;

		private infosysEntities entity = new infosysEntities();

		private System.Timers.Timer timer = new System.Timers.Timer(1000.0);

		private System.Timers.Timer timer2 = new System.Timers.Timer(60000.0);

		private System.Timers.Timer timer3 = new System.Timers.Timer(3600000.0);

		private DateTime beginTime;

		private IContainer components = null;

		private NotifyIcon notifyIcon1;

		private ContextMenuStrip contextMenuStrip1;

		private ToolStripMenuItem toolStripMenuItem1;

		private ToolStripMenuItem toolStripMenuItem2;

		private ToolStripMenuItem ToolStripMenuItem3;
        private System.Windows.Forms.Timer timer1;
        private ToolStripMenuItem 上次检查时间ToolStripMenuItem;
        private ToolStripMenuItem ToolStripMenuItem4;

		public void theout(object source, ElapsedEventArgs e)
		{
			MessageBox.Show("OK!");
		}

		public Form1()
		{
			this.InitializeComponent();
			this.timer.Elapsed += new ElapsedEventHandler(this.timer_Elapsed);
			this.timer.Enabled = true;
			this.timer2.Elapsed += new ElapsedEventHandler(this.timer_Elapsed2);
			this.timer2.Enabled = true;
			this.timer3.Elapsed += new ElapsedEventHandler(this.timer_Elapsed3);
			this.timer3.Enabled = true;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			Process[] processesByName = Process.GetProcessesByName("CISLClient");
			if (processesByName.Length > 1)
			{
				MessageBox.Show("程序已运行！");
				Application.Exit();
			}
			Control.CheckForIllegalCrossThreadCalls = false;
			this.beginTime = this.entity.Database.SqlQuery<DateTime>("select now()", new object[0]).FirstOrDefault<DateTime>();
			this.notifyIcon1.ShowBalloonTip(5000, "", "CISL签到客户端", ToolTipIcon.Info);
			StreamReader streamReader = new StreamReader(Application.StartupPath + "\\config.txt", false);
			string text = streamReader.ReadLine();
			if (text != null && text != "")
			{
				this.array = text.Split(new char[]
				{
					' '
				});
				this.uname = array[0];
				this.pwd = array[1];
                if (this.array.Length > 2)
                {
                    this.autosignin = array[2];
                    this.autosignout = array[3];

                    int inteval = int.Parse(array[4]);
                    this.timer1.Interval = inteval;
                    MessageBox.Show("签入:" + this.autosignin + ",签出:" + this.autosignout +",间隔时间:"+inteval, "配置信息");
                }


				streamReader.Close();
				IQueryable<user> source = from item in this.entity.user
				where item.username == this.uname && item.password == this.pwd
				select item;
				if (source.Count<user>() == 0)
				{
					MessageBox.Show("用户名或密码错误，请到配置文件修改！");
				}
				else
				{
					this.contextMenuStrip1.Items[0].Text = "当前用户：" + this.uname;
					this.uid = source.First<user>().id;
					this.zh_name = source.First<user>().zh_name;
					IOrderedQueryable<signrecord> source2 = from r in this.entity.signrecord
					where r.userId == this.uid
					orderby r.signTimestamp descending
					select r;
					if (source2.Count<signrecord>() == 0)
					{
						this.flag = false;
						this.contextMenuStrip1.Items[1].Text = "签入 " + this.beginTime;
					}
					else if (source2.First<signrecord>().in_out)
					{
						this.flag = true;
						this.contextMenuStrip1.Items[1].Text = "签出 " + this.beginTime;
					}
					else
					{
						this.flag = false;
						this.contextMenuStrip1.Items[1].Text = "签入 " + this.beginTime;
					}
				}
			}
			else
			{
				MessageBox.Show("请先配置用户名及密码！");
			}
		}

		private void toolStripMenuItem1_Click(object sender, EventArgs e)
		{
            signRecord();
		}

        private void signRecord()
        {
            if (this.contextMenuStrip1.Items[1].Text.Contains("签入"))
            {
                if (this.uid == 0)
                {
                    MessageBox.Show("请先配置用户名及密码！");
                }
                else
                {
                    this.flag = false;
                    this.contextMenuStrip1.Items[1].Text = "签出 " + this.beginTime;
                    signrecord signrecord = new signrecord();
                    signrecord.userId = this.uid;
                    UTF8Encoding uTF8Encoding = new UTF8Encoding();
                    byte[] bytes = uTF8Encoding.GetBytes(this.zh_name);
                    signrecord.zh_name = uTF8Encoding.GetString(bytes);
                    signrecord.in_out = true;
                    signrecord.valid = 1;
                    signrecord.signTimestamp = this.entity.Database.SqlQuery<DateTime>("select now()", new object[0]).FirstOrDefault<DateTime>();
                    this.entity.signrecord.Add(signrecord);
                    this.entity.SaveChanges();
                    this.notifyIcon1.ShowBalloonTip(5000, "", "已签入", ToolTipIcon.Info);
                }
            }
            else if (this.uid == 0)
            {
                MessageBox.Show("请先配置用户名及密码！");
            }
            else
            {
                this.flag = true;
                this.contextMenuStrip1.Items[1].Text = "签入 " + this.beginTime;
                signrecord signrecord = new signrecord();
                signrecord.userId = this.uid;
                signrecord.zh_name = this.zh_name;
                signrecord.in_out = false;
                signrecord.valid = 1;
                signrecord.signTimestamp = this.entity.Database.SqlQuery<DateTime>("select now()", new object[0]).FirstOrDefault<DateTime>();
                this.entity.signrecord.Add(signrecord);
                this.entity.SaveChanges();
                DateTime signTimestamp = (from q in this.entity.signrecord
                                          where q.userId == this.uid && q.in_out
                                          orderby q.signTimestamp descending
                                          select q).First<signrecord>().signTimestamp;
                history history = new history();
                history.userId = this.uid;
                history.zh_name = this.zh_name;
                history.in_timestamp = signTimestamp;
                history.out_timestamp = signrecord.signTimestamp;
                history.duration = (int)(history.out_timestamp - history.in_timestamp).TotalSeconds;
                history.valid_time = 0;
                history.allowance = 0;
                string text = history.in_timestamp.Date.ToString();
                string a = history.out_timestamp.DayOfWeek.ToString();
                if (a != "Saturday" && a != "Sunday")
                {
                    DateTime t = history.in_timestamp.Date.AddHours(5.5);
                    DateTime dateTime = history.in_timestamp.Date.AddHours(9.0);
                    DateTime t2 = history.in_timestamp.Date.AddHours(9.5);
                    DateTime dateTime2 = history.in_timestamp.Date.AddHours(11.0);
                    DateTime t3 = history.in_timestamp.Date.AddHours(11.5);
                    DateTime dateTime3 = history.in_timestamp.Date.AddHours(13.0);
                    DateTime dateTime4 = history.in_timestamp.Date.AddHours(17.0);
                    if (history.in_timestamp >= dateTime && history.in_timestamp <= dateTime2 && history.out_timestamp >= dateTime2 && history.out_timestamp < dateTime3)
                    {
                        history.valid_time = (int)(dateTime2 - history.in_timestamp).TotalSeconds;
                        if (history.in_timestamp <= t2)
                        {
                            history.allowance = 1;
                        }
                    }
                    else if (history.in_timestamp <= dateTime && history.in_timestamp > t && history.out_timestamp > dateTime && history.out_timestamp <= dateTime2)
                    {
                        history.valid_time = (int)(history.out_timestamp - dateTime).TotalSeconds;
                    }
                    else if (history.in_timestamp <= dateTime && history.in_timestamp > t && history.out_timestamp > dateTime2 && history.out_timestamp < dateTime3)
                    {
                        history.valid_time = (int)(dateTime2 - dateTime).TotalSeconds;
                        history.allowance = 1;
                    }
                    else if (history.in_timestamp > dateTime && history.out_timestamp <= dateTime2)
                    {
                        history.valid_time = (int)(history.out_timestamp - dateTime).TotalSeconds;
                    }
                    else if (history.in_timestamp >= dateTime3 && history.in_timestamp < dateTime4 && history.out_timestamp >= dateTime4)
                    {
                        history.valid_time = (int)(dateTime4 - history.in_timestamp).TotalSeconds;
                    }
                    else if (history.in_timestamp >= t3 && history.in_timestamp <= dateTime3 && history.out_timestamp > dateTime3 && history.out_timestamp <= dateTime4)
                    {
                        history.valid_time = (int)(history.out_timestamp - dateTime3).TotalSeconds;
                    }
                    else if (history.in_timestamp >= t3 && history.in_timestamp <= dateTime3 && history.out_timestamp > dateTime4)
                    {
                        history.valid_time = (int)(dateTime4 - dateTime3).TotalSeconds;
                    }
                    else if (history.in_timestamp > dateTime3 && history.in_timestamp < dateTime4 && history.out_timestamp <= dateTime4)
                    {
                        history.valid_time = (int)(history.out_timestamp - dateTime3).TotalSeconds;
                    }
                }
                this.entity.history.Add(history);
                this.entity.SaveChanges();
                this.notifyIcon1.ShowBalloonTip(5000, "", "已签出", ToolTipIcon.Info);
            }
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			this.beginTime = this.beginTime.AddMilliseconds(1000.0);
			string text = this.contextMenuStrip1.Items[1].Text;
			if (text.Contains("签入"))
			{
				this.contextMenuStrip1.Items[1].Text = "签入 " + this.beginTime;
			}
			else
			{
				this.contextMenuStrip1.Items[1].Text = "签出 " + this.beginTime;
			}
		}

		private void timer_Elapsed2(object sender, ElapsedEventArgs e)
		{
			if ((this.beginTime.TimeOfDay.Hours == 11 && this.beginTime.TimeOfDay.Minutes == 0) || (this.beginTime.TimeOfDay.Hours == 17 && this.beginTime.TimeOfDay.Minutes == 0) || (this.beginTime.TimeOfDay.Hours == 22 && this.beginTime.TimeOfDay.Minutes == 0))
			{
				this.notifyIcon1.ShowBalloonTip(10000, "", "请及时签出！", ToolTipIcon.Info);
			}
			if ((this.beginTime.TimeOfDay.Hours == 12 && this.beginTime.TimeOfDay.Minutes == 30) || (this.beginTime.TimeOfDay.Hours == 18 && this.beginTime.TimeOfDay.Minutes == 30) || (this.beginTime.TimeOfDay.Hours == 23 && this.beginTime.TimeOfDay.Minutes == 30))
			{
				IOrderedQueryable<signrecord> source = from item in this.entity.signrecord
				where item.userId == this.uid
				orderby item.signTimestamp descending
				select item;
				if (source.Count<signrecord>() > 0)
				{
					if (source.First<signrecord>().in_out)
					{
						this.contextMenuStrip1.Items[1].Text = "签出 " + this.beginTime;
					}
					else
					{
						this.contextMenuStrip1.Items[1].Text = "签入 " + this.beginTime;
					}
				}
			}
		}

		private void timer_Elapsed3(object sender, ElapsedEventArgs e)
		{
			string text = this.contextMenuStrip1.Items[1].Text;
			this.beginTime = this.entity.Database.SqlQuery<DateTime>("select now()", new object[0]).FirstOrDefault<DateTime>();
			if (text.Contains("签入"))
			{
				this.contextMenuStrip1.Items[1].Text = "签入 " + this.beginTime;
			}
			else
			{
				this.contextMenuStrip1.Items[1].Text = "签出 " + this.beginTime;
			}
		}

		private void toolStripMenuItem2_Click(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		private void ToolStripMenuItem4_Click(object sender, EventArgs e)
		{
			Form2 form = new Form2(this.uid);
			form.ShowDialog();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.上次检查时间ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "CISL";
            this.notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem3,
            this.toolStripMenuItem1,
            this.ToolStripMenuItem4,
            this.toolStripMenuItem2,
            this.上次检查时间ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(193, 114);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // ToolStripMenuItem3
            // 
            this.ToolStripMenuItem3.Name = "ToolStripMenuItem3";
            this.ToolStripMenuItem3.Size = new System.Drawing.Size(192, 22);
            this.ToolStripMenuItem3.Text = "当前用户：";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(192, 22);
            this.toolStripMenuItem1.Text = "toolStripMenuItem1";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // ToolStripMenuItem4
            // 
            this.ToolStripMenuItem4.Name = "ToolStripMenuItem4";
            this.ToolStripMenuItem4.Size = new System.Drawing.Size(192, 22);
            this.ToolStripMenuItem4.Text = "修改密码";
            this.ToolStripMenuItem4.Click += new System.EventHandler(this.ToolStripMenuItem4_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(192, 22);
            this.toolStripMenuItem2.Text = "退出程序";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // 上次检查时间ToolStripMenuItem
            // 
            this.上次检查时间ToolStripMenuItem.Name = "上次检查时间ToolStripMenuItem";
            this.上次检查时间ToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.上次检查时间ToolStripMenuItem.Text = "上次检查时间";
            this.上次检查时间ToolStripMenuItem.Click += new System.EventHandler(this.上次检查时间ToolStripMenuItem_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1100000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Name = "Form1";
            this.ShowInTaskbar = false;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

		}

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int h = DateTime.Now.Hour;



            this.上次检查时间ToolStripMenuItem.Text = "上次检查时间" + DateTime.Now.Hour + ":" + DateTime.Now.Minute;



            string text = this.contextMenuStrip1.Items[1].Text;

            // 如果在文件中有配置
            if (this.array.Length > 2)
            {
                if(this.autosignin.IndexOf(h+",")>=0 && text.Contains("签入") || this.autosignout.IndexOf(h+",")>=0 && text.Contains("签出"))
                {
                    signRecord();
                }
            }
            else
            {
                switch (h)
                {
                    case 17:case 22:case 11:
                        // 如果还没有签出
                        if (text.Contains("签出")){
                            signRecord();
                        }
                        break;

                    case 9:case 12:case 18:
                        // 如果还没有签入
                        if (text.Contains("签入"))
                        {
                            signRecord();
                        }
                        break;
                    default:
                        break;

                }
            }

            
        }

        private void 上次检查时间ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
