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
		private List<Connection> clientConnections;

		// Eigenschaften
		public ushort Port { get => this.port; }
		public bool Running { get => this.running; }

		// Events
		public event Action<string> ErrorOccured;
		public event Action<List<ConnectionInfo>> ConnectionsChanged;
		public event Action<ConnectionInfo, string, string, string> ReceiveObject;
		public event Action<ConnectionInfo, string, string, long> ReceiveFileInfo;
		public event Action<ConnectionInfo, string, byte[], long, long> ReceiveFile;

		// Öffentliche Methoden
		public Server()
		{
			this.listener = null;
			this.port = 0;
			this.running = false;

			this.clientConnections = new List<Connection>();
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
					this.ErrorOccured(exc.Message);
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
					this.ErrorOccured(exc.Message);
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

		// Private Methoden
		private void ReceiveObject_Internal(ConnectionInfo connection_info, string receiver, string name, string obj)
		{
			var connection = (Connection)connection_info;
			if (connection.Name == null)
			{
				if (name == "ConnectionInfoName")
				{
					string client_name = Core.ToObject<string>(obj);

					// Überprüfung ob Name schon verbunden ist
					foreach (Connection client in this.clientConnections)
					{
						if (client.Name == client_name || client_name == "Server" || client_name == "Admin")
						{
							connection.sendObject("Server", "Error", "Der Name ist bereits beim Server angemeldet oder ungültig!");
							connection.disconnect(false);
							return;
						}
					}

					connection.Name = client_name;
					Console.WriteLine("{0} hat sich verbunden", connection.Name);

					// Namen senden
					this.clientConnections.Add(connection);
					this.ConnectionsChanged(this.getClientInfoList());
				}
				else
				{
					Console.WriteLine("Zuerst muss ein Name gesendet werden!");
					connection.sendObject("Server", "Error", "Zuerst muss ein Name gesendet werden!");
					connection.disconnect(false);
				}
			}
			else
			{
				this.ReceiveObject(connection_info, receiver, name, obj);
			}
		}
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
			var connection = new Connection(client,
				this.ReceiveObject_Internal,
				this.ReceiveFileInfo,
				this.ReceiveFile);
			connection.RemoveCallback += this.removeClientConnection;
			connection.ErrorOccured += this.ErrorOccured;
			//this.clientConnections.Add(connection);
			//this.ConnectionsChanged(this.getClientInfoList());

			// Weiter nach Client-Verbindungen lauschen
			try
			{
				this.listen();
			}
			catch (SocketException exc)
			{
				this.running = false;
				this.ErrorOccured(exc.Message);
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
