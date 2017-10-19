using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EinfachesNetzwerk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClientBeispiel
{
	public partial class FrmMain : Form
	{
		private Client client;

		public FrmMain()
		{
			InitializeComponent();
			this.client = new Client();
			this.client.ConnectionStateChanged += Client_ConnectionStateChanged;
			this.client.ErrorOccured += Client_ErrorOccured;
			this.client.ReceiveObject += Client_ReceiveObject;
			this.client.ReceiveFileInfo += Client_ReceiveFileInfo;
			this.client.ReceiveFile += Client_ReceiveFile;

			this.txtServer.Text = "localhost:9876";
		}

		private void Client_ReceiveFile(string arg1, byte[] arg2, long arg3, long arg4)
		{

		}

		private void Client_ReceiveFileInfo(string sender, string filename, long filesize)
		{
			MessageBox.Show("Dateiinfo empfangen", string.Format("von {0}", sender));
		}

		private void Client_ReceiveObject(string sender, string obj_name, string obj_str)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new Action(() => this.Client_ReceiveObject(sender, obj_name, obj_str)));
			}
			else
			{
				//var json = (JObject)obj;
				//if (json["ClientNamen"] != null)
				//{
				//	var namen = JsonConvert.DeserializeObject<List<string>>((string)json["ClientNamen"]);
				//	foreach (var name in namen)
				//	{
				//		this.lstClients.Items.Add(name);
				//	}
				//}
				if (sender == "Server")
				{
					if (obj_name == "ClientNamen")
					{
						var client_namen = Core.ToObject<List<string>>(obj_str);

						this.lstClients.Items.Clear();
						foreach (var client_name in client_namen)
						{
							if (client_name != this.txtName.Text)
								this.lstClients.Items.Add(client_name);
						}
						//Console.WriteLine(obj);
					}
					else
					{
						// ...
					}
				}
				else
				{
					if (obj_name == "Nachricht")
					{
						string message = Core.ToObject<string>(obj_str);
						MessageBox.Show(string.Format("Nachricht von {0} erhalten:\n{1}", sender, message), "Nachricht erhalten");
					}
					else
					{
						MessageBox.Show(string.Format("Objekt-Name: {0}\nObjekt-String: {1}", obj_name, obj_str), sender);
					}
				}
				
			}
		}

		private void Client_ErrorOccured(string error_message)
		{
			MessageBox.Show(error_message);
		}

		private void Client_ConnectionStateChanged(bool connected)
		{
			if (this.InvokeRequired)
			{
				this.BeginInvoke(new Action(() => this.Client_ConnectionStateChanged(connected) ));
			}
			else
			{
				this.txtServer.Enabled = !connected;
				this.txtName.Enabled = !connected;
				this.btnDisConnect.Text = connected ? "Trennen" : "Verbinden";
				this.btnDisConnect.Enabled = true;
				if (!connected)
					this.lstClients.Items.Clear();
				this.lstClients.Enabled = connected;

				this.btnSendHallo.Enabled = connected && this.lstClients.SelectedItems.Count > 0;
				this.btnSendFile.Enabled = connected && this.lstClients.SelectedItems.Count > 0;
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void btnDisConnect_Click(object sender, EventArgs e)
		{
			if (!this.client.Connected)
			{
				string server_address = this.txtServer.Text;
				if (server_address != "" && server_address.Contains(":"))
				{
					if (this.txtName.Text == "")
					{
						MessageBox.Show("Bitte einen gültigen Namen angeben!");
						return;
					}
					else
					{
						var name_and_port = server_address.Split(':');

						ushort port;
						if (ushort.TryParse(name_and_port[1], out port))
						{
							this.btnDisConnect.Text = "wird hergestellt...";
							this.btnDisConnect.Enabled = false;
							this.txtServer.Enabled = false;

							this.client.connect(name_and_port[0], port, this.txtName.Text);
							return;
						}
					}
				}

				MessageBox.Show("Bitte eine gültige Server-Adresse angeben!");
			}
			else
			{
				this.client.disconnect();
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.client.Connected)
			{
				this.client.disconnect();
			}
		}

		private void lstClients_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.btnSendHallo.Enabled = this.lstClients.SelectedItem != null;
			this.btnSendFile.Enabled = this.lstClients.SelectedItem != null;
		}

		private void btnSendHallo_Click(object sender, EventArgs e)
		{
			if (this.lstClients.SelectedItem != null)
			{
				string selected_client_name = this.lstClients.SelectedItem.ToString();

				this.client.sendObject(selected_client_name, "Nachricht", "Hallo!");
			}
		}

		private void btnSendFile_Click(object sender, EventArgs e)
		{
			var result = this.openFileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				string receiver = this.lstClients.SelectedItem.ToString();
				string path = this.openFileDialog.FileName;

				this.client.sendFile(receiver, path);
			}
		}
	}
}
