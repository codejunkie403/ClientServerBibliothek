using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EinfachesNetzwerk
{
	public class Server
	{
		// Felder
		private TcpListener listener;
		private ushort port;
		private bool running;
		private Action<string> errorCallback;
		private List<Connection> clientConnections;

		// Eigenschaften
		public ushort Port { get => this.port; }
		public bool Running { get => this.running; }

		// Events
		public event Action<List<ConnectionInfo>> ConnectionsChanged;
		public event Action<ConnectionInfo, object> ReceiveObject;
		public event Action<ConnectionInfo, string, long> ReceiveFileInfo;
		public event Action<ConnectionInfo, byte[], long, long> ReceiveFile;

		// Öffentliche Methoden
		public Server()
		{
			this.listener = null;
			this.port = 0;
			this.running = false;

			this.clientConnections = new List<Connection>();
		}
		public Server(Action<string> errorCallback = null):
			this()
		{
			this.errorCallback = errorCallback;
		}

		public void start(ushort port)
		{
			if (!this.running)
			{
				this.listener = new TcpListener(IPAddress.Any, port);

				try
				{
					// Server starten
					Console.WriteLine("Server wird gestartet...");
					this.listener.Start();
					Console.WriteLine("Server läuft");

					this.listen();

					this.port = port;
					this.running = true;
				}
				catch (SocketException exc)
				{
					this?.errorCallback(exc.Message);
				}
			}
			else
			{
				// Server läuft schon
			}
		}
		public void stop()
		{
			if (this.running)
			{
				// Server wird gestoppt
				try
				{
					Console.WriteLine("Server wird gestoppt...");
					this.listener.Stop();

					foreach (var clientConnection in this.clientConnections)
					{
						clientConnection.disconnect();
					}
					this.clientConnections.Clear();
					Console.WriteLine("Server wurde gestoppt");
				}
				catch (SocketException exc)
				{
					this?.errorCallback(exc.Message);
				}
				this.running = false;
			}
		}
		public List<ConnectionInfo> getClientInfoList()
		{
			var clientInfos = new List<ConnectionInfo>();
			foreach (var clientConnection in this.clientConnections)
			{
				clientInfos.Add(clientConnection);
			}
			return clientInfos;
		}
		
		public void kick(ConnectionInfo clientInfo)
		{
			if (this.clientConnections.Contains(clientInfo))
			{
				((Connection)clientInfo).disconnect();
			}
		}
		public void kick(string host, ushort port)
		{
			ConnectionInfo clientInfo = null;
			foreach (var clientConnection in this.clientConnections)
			{
				if (clientConnection.Host == host && clientConnection.Port == port)
				{
					clientInfo = clientConnection;
					break;
				}
			}

			if (clientInfo != null)
			{
				this.kick(clientInfo);
			}
		}

		// Private Methodenayy
		private void listen()
		{
			Console.WriteLine("Warte auf eingehende Verbindungen...");
			this.listener.BeginAcceptTcpClient(this.acceptTcpClient, null);
		}
		private void acceptTcpClient(IAsyncResult ar)
		{
			// Verbindung eines Clients annehmen
			TcpClient client;
			try
			{
				client = this.listener.EndAcceptTcpClient(ar);
			}
			catch
			{
				return;
			}

			// Verbindung halten
			this.clientConnections.Add(new Connection(client,
				this.ReceiveObject,
				this.ReceiveFileInfo,
				this.ReceiveFile,
				this.errorCallback, this.removeClientConnection));
			this.ConnectionsChanged(this.getClientInfoList());

			// Weiter nach Client-Verbindungen lauschen
			try
			{
				this.listen();
			}
			catch (SocketException exc)
			{
				this.running = false;
				this?.errorCallback(exc.Message);
			}
		}
		private void removeClientConnection(Connection clientConnection)
		{
			lock (this.clientConnections)
			{
				clientConnection.disconnect();
				this.clientConnections.Remove(clientConnection);
				Console.WriteLine("Verbindung zum Client getrennt");
				this.ConnectionsChanged(this.getClientInfoList());
			}
		}
	}
}
