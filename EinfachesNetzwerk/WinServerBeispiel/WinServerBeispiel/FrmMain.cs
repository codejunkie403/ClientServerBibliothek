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

namespace WinServerBeispiel
{
	public partial class FrmMain : Form
	{
		private Server server;

		public FrmMain()
		{
			InitializeComponent();

			this.server = new Server(this.receiveCallback, this.clientConnectionsChanged, this.errorCallback);
		}

		private void receiveCallback(ServerClientConnectionInfo clientInfo, byte[] data, int size)
		{
			string dataStr = Encoding.UTF8.GetString(data, 0, size);

			MessageBox.Show(string.Format("Nachricht von {0}:{1} erhalten:\n{2}", clientInfo.Host, clientInfo.Port, dataStr), "Nachricht");
		}

		private void clientConnectionsChanged(List<ServerClientConnectionInfo> clientInfoList)
		{
			if (this.InvokeRequired)
			{
				this.Invoke(new Action(() => this.clientConnectionsChanged(clientInfoList)));
			}
			else
			{
				this.lvwClients.Items.Clear();

				foreach (var clientInfo in clientInfoList)
				{
					this.lvwClients.Items.Add(new ListViewItem(new string[] { clientInfo.Host, clientInfo.Port.ToString() }));
				}
			}
		}

		private void errorCallback(string message)
		{
			MessageBox.Show(message, "Error");
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			ushort port;
			if (ushort.TryParse(this.txtPort.Text, out port))
			{
				this.server.start(port);
			}
			else
			{
				MessageBox.Show("Ungültiger Port!");
			}
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			this.server.stop();
		}

		private void btnKick_Click(object sender, EventArgs e)
		{
			if (this.lvwClients.SelectedItems.Count > 0)
			{
				var clientsToKick = new Dictionary<string, ushort>();
				foreach (ListViewItem item in this.lvwClients.SelectedItems)
				{
					clientsToKick[item.SubItems[0].Text] = Convert.ToUInt16(item.SubItems[1].Text);
				}

				foreach (var client in clientsToKick)
				{
					this.server.kick(client.Key, client.Value);
				}
			}
		}
	}
}
