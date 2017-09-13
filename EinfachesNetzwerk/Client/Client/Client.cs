using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Security;
using System.IO;

namespace EinfachesNetzwerk
{
	public class Client: Core
	{
		// Felder
		private TcpClient client;
		private string host;
		private ushort port;
		private bool connected;
		private Action<string> errorCallback;
		//private byte[] receiveBuffer;

		// Eigenschaften
		public string Host { get => this.host; }
		public ushort Port { get => this.port; }
		public bool Connected { get => this.connected; }

		// Öffentliche Methoden
		public Client(Action<string> errorCallback = null)
		{
			this.client = null;
			this.host = null;
			this.port = 0;
			this.connected = false;
			this.errorCallback = errorCallback;
		}
		public void connect(string host, ushort port)
		{
			if (!this.connected)
			{
				// Verbindung zum Server aufbauen
				this.client = new TcpClient();

				this.host = host;
				this.port = port;

				try
				{
					Console.WriteLine("Verbindungsaufbau läuft...");
					this.client.BeginConnect(this.host, this.port, this.acceptConnection, null);
				}
				catch (SocketException exc)
				{
					this?.errorCallback(exc.Message);
				}
				catch (SecurityException exc)
				{
					this?.errorCallback(exc.Message);
				}
			}
			else
			{
				// Verbindung schon vorhanden
				Console.WriteLine("Eine Verbindung besteht bereits!");
			}
		}
		public void disconnect()
		{
			if (this.connected)
			{
				// Verbindung trennen
				Console.WriteLine("Verbindung wird getrennt...");
				this.client.Close();
			}
			else
			{
				// Verbindung schon getrennt
				Console.WriteLine("Es besteht keine Verbindung, die getrennt werden kann!");
			}
		}

		public void sendObject(object obj)
		{
			if (!this.connected)
			{
				Console.WriteLine("Objekt kann nicht gesendet werden, da keine Verbindung zum Server besteht");
				return;
			}

			try
			{
				var objectBytes = this.serialize(obj);
				var objectSizeBytes = BitConverter.GetBytes(objectBytes.Length);

				var clientStream = this.client.GetStream();
				using (var memoryStream = new MemoryStream())
				{
					memoryStream.Write(objectSizeBytes, 0, objectSizeBytes.Length);
					memoryStream.WriteByte((byte)Core.PacketType.Object);
					memoryStream.Write(objectBytes, 0, objectBytes.Length);

					memoryStream.WriteTo(clientStream);
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine("Fehler beim Serialisieren des Objekts: {0}", exc.Message);
			}
		}
		public void sendFile(string path)
		{
			if (!this.connected)
			{
				Console.WriteLine("Datei kann nicht gesendet werden, da keine Verbindung zum Server besteht");
				return;
			}

			FileInfo fileInfo = null;
			try
			{
				fileInfo = new FileInfo(path);
			}
			catch (Exception exc)
			{
				Console.WriteLine("Fehler beim Lesen der Dateiinfo! - {0}", exc.Message);
				return;
			}

			if (fileInfo.Exists)
			{
				var filePacket = new Core.FilePacket
				{
					Size = fileInfo.Length,
					CreationTime = fileInfo.CreationTime,
					LastAccessTime = fileInfo.LastAccessTime,
					LastWriteTime = fileInfo.LastWriteTime,
					Name = fileInfo.Name
				};

				var filePacketBytes = this.serialize(filePacket);
				var filePacketSizeBytes = BitConverter.GetBytes(filePacketBytes.Length);

				var clientStream = this.client.GetStream();

				using (var memoryStream = new MemoryStream())
				{
					memoryStream.Write(filePacketSizeBytes, 0, filePacketSizeBytes.Length);
					memoryStream.WriteByte((byte)Core.PacketType.File);
					memoryStream.Write(filePacketBytes, 0, filePacketBytes.Length);

					memoryStream.WriteTo(clientStream);
				}

				byte[] buffer = new byte[this.client.SendBufferSize];
				using (var fileStream = new FileStream(path, FileMode.Open))
				{
					fileStream.Seek(0, SeekOrigin.Begin);

					int size = 0;
					while ((size = fileStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						clientStream.Write(buffer, 0, size);
					}
				}
			}
			else
			{
				Console.WriteLine("Die Datei '{0}' existiert nicht!", fileInfo.FullName);
			}
		}

		// Private Methoden
		private void setDisconnected()
		{
			this.connected = false;
			Console.WriteLine("Verbindung getrennt");
		}
		private void acceptConnection(IAsyncResult ar)
		{
			try
			{
				// Verbindung annehmen
				this.client.EndConnect(ar);

				Console.WriteLine("Verbindung hergestellt");
				this.connected = true;
			}
			catch (SocketException exc)
			{
				this?.errorCallback(exc.Message);
				return;
			}

			// TODO: Callback für Verbindung hergestellt
			//var data = new Newtonsoft.Json.Linq.JObject();
			//data["Name"] = "Hans Dieter";
			//this.send(data);

			// Prozess zum Empfangen vom Server starten
			//this.receiveBuffer = new byte[this.client.ReceiveBufferSize];

			this.configReceiveBuffer(this.client.ReceiveBufferSize);
			this.startReceiving(this.client.GetStream(), this.setDisconnected);
		}
	}
}
