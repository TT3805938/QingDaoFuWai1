
partial class PrintHYDForm
{
	/// <summary>
	/// 必需的设计器变量。
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary>
	/// 清理所有正在使用的资源。
	/// </summary>
	/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
	protected override void Dispose(bool disposing)
	{
		if (disposing && (components != null))
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	#region Windows 窗体设计器生成的代码

	/// <summary>
	/// 设计器支持所需的方法 - 不要
	/// 使用代码编辑器修改此方法的内容。
	/// </summary>
	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.Button_Close = new System.Windows.Forms.Label();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.progressBar1 = new System.Windows.Forms.ProgressBar();
		this.Caption = new System.Windows.Forms.Label();
		this.timer1 = new System.Windows.Forms.Timer(this.components);
		this.groupBox1.SuspendLayout();
		this.SuspendLayout();
		// 
		// ModuleNameLab
		// 
		this.ModuleNameLab.Size = new System.Drawing.Size(0, 42);
		this.ModuleNameLab.Text = "";
		// 
		// NameLeft
		// 
		this.NameLeft.Size = new System.Drawing.Size(0, 42);
		this.NameLeft.Text = "";
		// 
		// NameRight
		// 
		this.NameRight.Size = new System.Drawing.Size(0, 42);
		this.NameRight.Text = "";
		// 
		// CardNoLeft
		// 
		this.CardNoLeft.Size = new System.Drawing.Size(0, 42);
		this.CardNoLeft.Text = "";
		// 
		// CardNoRight
		// 
		this.CardNoRight.Size = new System.Drawing.Size(0, 42);
		this.CardNoRight.Text = "";
		// 
		// YERight
		// 
		this.YERight.Size = new System.Drawing.Size(0, 42);
		this.YERight.Text = "";
		// 
		// YELeft
		// 
		this.YELeft.Size = new System.Drawing.Size(0, 42);
		this.YELeft.Text = "";
		// 
		// Button_Close
		// 
		this.Button_Close.BackColor = System.Drawing.Color.Transparent;
		this.Button_Close.Font = new System.Drawing.Font("微软雅黑", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
		this.Button_Close.Location = new System.Drawing.Point(1006, 723);
		this.Button_Close.Name = "Button_Close";
		this.Button_Close.Size = new System.Drawing.Size(262, 121);
		this.Button_Close.TabIndex = 116;
		this.Button_Close.Tag = "关闭按钮";
		this.Button_Close.Text = "关闭";
		this.Button_Close.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.Button_Close.Click += new System.EventHandler(this.Button_Close_Click);
		// 
		// groupBox1
		// 
		this.groupBox1.BackColor = System.Drawing.Color.Transparent;
		this.groupBox1.Controls.Add(this.progressBar1);
		this.groupBox1.Controls.Add(this.Caption);
		this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBox1.Location = new System.Drawing.Point(0, 0);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(1280, 853);
		this.groupBox1.TabIndex = 119;
		this.groupBox1.TabStop = false;
		this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
		// 
		// progressBar1
		// 
		this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
		this.progressBar1.Location = new System.Drawing.Point(119, 431);
		this.progressBar1.Name = "progressBar1";
		this.progressBar1.Size = new System.Drawing.Size(1043, 73);
		this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
		this.progressBar1.TabIndex = 121;
		// 
		// Caption
		// 
		this.Caption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
								| System.Windows.Forms.AnchorStyles.Right)));
		this.Caption.BackColor = System.Drawing.Color.Transparent;
		this.Caption.Font = new System.Drawing.Font("黑体", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
		this.Caption.ForeColor = System.Drawing.Color.Orange;
		this.Caption.Location = new System.Drawing.Point(12, 201);
		this.Caption.Name = "Caption";
		this.Caption.Size = new System.Drawing.Size(1246, 152);
		this.Caption.TabIndex = 119;
		this.Caption.Text = "正在打印您的化验单\r\n\r\n";
		this.Caption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.Caption.Click += new System.EventHandler(this.Caption_Click);
		// 
		// timer1
		// 
		this.timer1.Enabled = true;
		this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
		// 
		// PrintHYDForm
		// 
		this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.ClientSize = new System.Drawing.Size(1280, 853);
		this.Controls.Add(this.groupBox1);
		this.Controls.Add(this.Button_Close);
		this.Name = "PrintHYDForm";
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Load += new System.EventHandler(this.PrintHYDForm_Load);
		this.Controls.SetChildIndex(this.YELeft, 0);
		this.Controls.SetChildIndex(this.YERight, 0);
		this.Controls.SetChildIndex(this.Button_Close, 0);
		this.Controls.SetChildIndex(this.groupBox1, 0);
		this.Controls.SetChildIndex(this.TYDJSLab, 0);
		this.Controls.SetChildIndex(this.ModuleNameLab, 0);
		this.Controls.SetChildIndex(this.NameLeft, 0);
		this.Controls.SetChildIndex(this.NameRight, 0);
		this.Controls.SetChildIndex(this.CardNoLeft, 0);
		this.Controls.SetChildIndex(this.CardNoRight, 0);
		this.groupBox1.ResumeLayout(false);
		this.ResumeLayout(false);
		this.PerformLayout();

	}

	#endregion

	private System.Windows.Forms.Label Button_Close;
	private System.Windows.Forms.GroupBox groupBox1;
	private System.Windows.Forms.Label Caption;
	private System.Windows.Forms.Timer timer1;
	public System.Windows.Forms.ProgressBar progressBar1;
}
