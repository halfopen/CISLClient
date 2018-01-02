using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CISLClient
{
	public class Form2 : Form
	{
		private infosysEntities entity = new infosysEntities();

		private int uId;

		private IContainer components = null;

		private Label label1;

		private Label label2;

		private TextBox textBox1;

		private TextBox textBox2;

		private Button button1;

		public Form2()
		{
			this.InitializeComponent();
		}

		public Form2(int uId)
		{
			this.uId = uId;
			this.InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string text = this.textBox1.Text.Trim();
			string b = this.textBox2.Text.Trim();
			if (text == "")
			{
				MessageBox.Show("密码不允许为空！");
			}
			else if (text != b)
			{
				MessageBox.Show("两次密码不一致！");
			}
			else
			{
				IQueryable<user> queryable = from item in this.entity.user
				where item.id == this.uId
				select item;
				foreach (user current in queryable)
				{
					current.password = text;
				}
				this.entity.SaveChanges();
				StreamReader streamReader = new StreamReader(Application.StartupPath + "\\config.txt", false);
				string text2 = streamReader.ReadLine();
				string[] array = text2.Split(new char[]
				{
					' '
				});
				string value = array[0] + " " + text;
				streamReader.Close();
				FileStream fileStream = new FileStream(Application.StartupPath + "\\config.txt", FileMode.Open, FileAccess.Write);
				StreamWriter streamWriter = new StreamWriter(fileStream);
				streamWriter.Write(value);
				streamWriter.Close();
				fileStream.Close();
				MessageBox.Show("修改成功！");
				Environment.Exit(0);
			}
		}

		private void Form2_Load(object sender, EventArgs e)
		{
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
			this.label1 = new Label();
			this.label2 = new Label();
			this.textBox1 = new TextBox();
			this.textBox2 = new TextBox();
			this.button1 = new Button();
			base.SuspendLayout();
			this.label1.AutoSize = true;
			this.label1.Location = new Point(72, 67);
			this.label1.Name = "label1";
			this.label1.Size = new Size(53, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "新密码：";
			this.label2.AutoSize = true;
			this.label2.Location = new Point(48, 100);
			this.label2.Name = "label2";
			this.label2.Size = new Size(77, 12);
			this.label2.TabIndex = 1;
			this.label2.Text = "重复新密码：";
			this.textBox1.Location = new Point(120, 62);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new Size(100, 21);
			this.textBox1.TabIndex = 2;
			this.textBox2.Location = new Point(120, 97);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new Size(100, 21);
			this.textBox2.TabIndex = 3;
			this.button1.Location = new Point(120, 144);
			this.button1.Name = "button1";
			this.button1.Size = new Size(75, 23);
			this.button1.TabIndex = 4;
			this.button1.Text = "确定";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new EventHandler(this.button1_Click);
			base.AutoScaleDimensions = new SizeF(6f, 12f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.ClientSize = new Size(292, 273);
			base.Controls.Add(this.button1);
			base.Controls.Add(this.textBox2);
			base.Controls.Add(this.textBox1);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label1);
			base.Name = "Form2";
			this.Text = "修改密码";
			base.Load += new EventHandler(this.Form2_Load);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}
