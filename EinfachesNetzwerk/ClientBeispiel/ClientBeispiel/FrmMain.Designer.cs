namespace ClientBeispiel
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
			this.lstClients = new System.Windows.Forms.ListBox();
			this.txtServer = new System.Windows.Forms.TextBox();
			this.btnDisConnect = new System.Windows.Forms.Button();
			this.lblServer = new System.Windows.Forms.Label();
			this.txtName = new System.Windows.Forms.TextBox();
			this.lblName = new System.Windows.Forms.Label();
			this.btnSendHallo = new System.Windows.Forms.Button();
			this.btnSendFile = new System.Windows.Forms.Button();
			this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// lstClients
			// 
			this.lstClients.Enabled = false;
			this.lstClients.FormattingEnabled = true;
			this.lstClients.Location = new System.Drawing.Point(352, 12);
			this.lstClients.Name = "lstClients";
			this.lstClients.Size = new System.Drawing.Size(143, 173);
			this.lstClients.TabIndex = 0;
			this.lstClients.SelectedIndexChanged += new System.EventHandler(this.lstClients_SelectedIndexChanged);
			// 
			// txtServer
			// 
			this.txtServer.Location = new System.Drawing.Point(12, 28);
			this.txtServer.Name = "txtServer";
			this.txtServer.Size = new System.Drawing.Size(128, 20);
			this.txtServer.TabIndex = 2;
			// 
			// btnDisConnect
			// 
			this.btnDisConnect.Location = new System.Drawing.Point(12, 100);
			this.btnDisConnect.Name = "btnDisConnect";
			this.btnDisConnect.Size = new System.Drawing.Size(128, 23);
			this.btnDisConnect.TabIndex = 3;
			this.btnDisConnect.Text = "Verbinden";
			this.btnDisConnect.UseVisualStyleBackColor = true;
			this.btnDisConnect.Click += new System.EventHandler(this.btnDisConnect_Click);
			// 
			// lblServer
			// 
			this.lblServer.AutoSize = true;
			this.lblServer.Location = new System.Drawing.Point(12, 12);
			this.lblServer.Name = "lblServer";
			this.lblServer.Size = new System.Drawing.Size(38, 13);
			this.lblServer.TabIndex = 4;
			this.lblServer.Text = "Server";
			// 
			// txtName
			// 
			this.txtName.Location = new System.Drawing.Point(12, 74);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(128, 20);
			this.txtName.TabIndex = 5;
			// 
			// lblName
			// 
			this.lblName.AutoSize = true;
			this.lblName.Location = new System.Drawing.Point(12, 58);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(35, 13);
			this.lblName.TabIndex = 6;
			this.lblName.Text = "Name";
			// 
			// btnSendHallo
			// 
			this.btnSendHallo.Enabled = false;
			this.btnSendHallo.Location = new System.Drawing.Point(15, 295);
			this.btnSendHallo.Name = "btnSendHallo";
			this.btnSendHallo.Size = new System.Drawing.Size(108, 23);
			this.btnSendHallo.TabIndex = 7;
			this.btnSendHallo.Text = "\"Hallo\" senden";
			this.btnSendHallo.UseVisualStyleBackColor = true;
			this.btnSendHallo.Click += new System.EventHandler(this.btnSendHallo_Click);
			// 
			// btnSendFile
			// 
			this.btnSendFile.Enabled = false;
			this.btnSendFile.Location = new System.Drawing.Point(129, 295);
			this.btnSendFile.Name = "btnSendFile";
			this.btnSendFile.Size = new System.Drawing.Size(120, 23);
			this.btnSendFile.TabIndex = 8;
			this.btnSendFile.Text = "Datei senden";
			this.btnSendFile.UseVisualStyleBackColor = true;
			this.btnSendFile.Visible = false;
			this.btnSendFile.Click += new System.EventHandler(this.btnSendFile_Click);
			// 
			// openFileDialog
			// 
			this.openFileDialog.FileName = "openFileDialog";
			// 
			// FrmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(507, 330);
			this.Controls.Add(this.btnSendFile);
			this.Controls.Add(this.btnSendHallo);
			this.Controls.Add(this.lblName);
			this.Controls.Add(this.txtName);
			this.Controls.Add(this.lblServer);
			this.Controls.Add(this.btnDisConnect);
			this.Controls.Add(this.txtServer);
			this.Controls.Add(this.lstClients);
			this.Name = "FrmMain";
			this.Text = "Form1";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox lstClients;
		private System.Windows.Forms.TextBox txtServer;
		private System.Windows.Forms.Button btnDisConnect;
		private System.Windows.Forms.Label lblServer;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.Label lblName;
		private System.Windows.Forms.Button btnSendHallo;
		private System.Windows.Forms.Button btnSendFile;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
	}
}

