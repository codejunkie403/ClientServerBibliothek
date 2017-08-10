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
		private Action<ServerClientConnectionInfo, byte[], int> receiveCallback;
		private Action<List<ServerClientConnectionInfo>> clientConnectionsCallback;
		private Action<string> errorCallback;
		private List<ServerClientConnection> clientConnections;

		// Eigenschaften
		public ushort Port { get => this.port; }
		public bool Running { get => this.running; }

		// Öffentliche Methoden
		public Server(Action<ServerClientConnectionInfo, byte[], int> receiveCallback, Action<List<ServerClientConnectionInfo>> clientConnectionsCallback = null, Action<string> errorCallback = null)
		{
			this.listener = null;
			this.port = 0;
			this.running = false;

			this.receiveCallback = receiveCallback;
			this.clientConnectionsCallback = clientConnectionsCallback;
			this.errorCallback = errorCallback;

			this.clientConnections = new List<ServerClientConnection>();
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
		public List<ServerClientConnectionInfo> getClientInfoList()
		{
			var clientInfos = new List<ServerClientConnectionInfo>();
			foreach (var clientConnection in this.clientConnections)
			{
				clientInfos.Add(clientConnection);
			}
			return clientInfos;
		}
		public void send(ServerClientConnectionInfo clientInfo, byte[] data)
		{
			if (this.clientConnections.Contains(clientInfo))
			{
				((ServerClientConnection)clientInfo).send(data);
			}
			else
			{
				this?.errorCallback("Nachricht kann nicht gesendet werden! - Keine Verbindung");
			}
		}
		public void send(ServerClientConnectionInfo clientInfo, object obj)
		{
			try
			{
				string objStr = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
				this.send(clientInfo, Encoding.UTF8.GetBytes(objStr));
			}
			catch (Exception exc)
			{
				Console.WriteLine("Fehler beim Serialisieren des Objekts: {0}", exc.Message);
			}
		}
		public void sendObject(object obj)
		{

		}
		public void sendFile(string path)
		{

		}
		public void kick(ServerClientConnectionInfo clientInfo)
		{
			if (this.clientConnections.Contains(clientInfo))
			{
				((ServerClientConnection)clientInfo).disconnect();
			}
		}
		public void kick(string host, ushort port)
		{
			ServerClientConnectionInfo clientInfo = null;
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
			this.clientConnections.Add(new ServerClientConnection(client, this.receiveCallback, this.errorCallback, this.removeClientConnection));
			this?.clientConnectionsCallback(this.getClientInfoList());

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
		private void removeClientConnection(ServerClientConnection clientConnection)
		{
			lock (this.clientConnections)
			{
				clientConnection.disconnect();
				this.clientConnections.Remove(clientConnection);
				Console.WriteLine("Verbindung zum Client getrennt");
				this?.clientConnectionsCallback(this.getClientInfoList());
			}
		}
	}
}
