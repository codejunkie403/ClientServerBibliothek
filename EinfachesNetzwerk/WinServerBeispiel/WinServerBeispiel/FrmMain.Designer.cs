namespace WinServerBeispiel
{
	partial class FrmMain
	{
		/// <summary>
		/// Erforderliche Designervariable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Verwendete Ressourcen bereinigen.
		/// </summary>
		/// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Vom Windows Form-Designer generierter Code

		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung.
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblPort = new System.Windows.Forms.Label();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.lvwClients = new System.Windows.Forms.ListView();
			this.colHost = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colPort = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.btnKick = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblPort
			// 
			this.lblPort.AutoSize = true;
			this.lblPort.Location = new System.Drawing.Point(51, 39);
			this.lblPort.Name = "lblPort";
			this.lblPort.Size = new System.Drawing.Size(26, 13);
			this.lblPort.TabIndex = 0;
			this.lblPort.Text = "Port";
			// 
			// txtPort
			// 
			this.txtPort.Location = new System.Drawing.Point(54, 55);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(100, 20);
			this.txtPort.TabIndex = 1;
			this.txtPort.Text = "9876";
			// 
			// btnStart
			// 
			this.btnStart.Location = new System.Drawing.Point(54, 122);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 2;
			this.btnStart.Text = "Starten";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// btnStop
			// 
			this.btnStop.Location = new System.Drawing.Point(135, 122);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(75, 23);
			this.btnStop.TabIndex = 3;
			this.btnStop.Text = "Stoppen";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// lvwClients
			// 
			this.lvwClients.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colHost,
            this.colPort});
			this.lvwClients.Location = new System.Drawing.Point(54, 173);
			this.lvwClients.Name = "lvwClients";
			this.lvwClients.Size = new System.Drawing.Size(485, 173);
			this.lvwClients.TabIndex = 5;
			this.lvwClients.UseCompatibleStateImageBehavior = false;
			this.lvwClients.View = System.Windows.Forms.View.Details;
			// 
			// colHost
			// 
			this.colHost.Text = "Host";
			// 
			// colPort
			// 
			this.colPort.Text = "Port";
			// 
			// btnKick
			// 
			this.btnKick.Location = new System.Drawing.Point(464, 122);
			this.btnKick.Name = "btnKick";
			this.btnKick.Size = new System.Drawing.Size(75, 23);
			this.btnKick.TabIndex = 4;
			this.btnKick.Text = "Kicken";
			this.btnKick.UseVisualStyleBackColor = true;
			this.btnKick.Click += new System.EventHandler(this.btnKick_Click);
			// 
			// FrmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(611, 467);
			this.Controls.Add(this.btnKick);
			this.Controls.Add(this.lvwClients);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.txtPort);
			this.Controls.Add(this.lblPort);
			this.Name = "FrmMain";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblPort;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.ListView lvwClients;
		private System.Windows.Forms.ColumnHeader colHost;
		private System.Windows.Forms.ColumnHeader colPort;
		private System.Windows.Forms.Button btnKick;
	}
}

