using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;

namespace EinfachesNetzwerk
{
	public class ConnectionInfo
	{
		protected string host;
		protected ushort port;

		public string Host { get => this.host; }
		public ushort Port { get => this.port; }

		//
		public string Name { get; set; }
	}

	public class Connection:
		ConnectionInfo
	{
		// Felder
		private TcpClient client;
		private Action<string> errorCallback;
		private Action<Connection> removeCallback;
		public Core core;

		// Öffentliche Methoden
		public Connection(TcpClient client,
			Action<ConnectionInfo, object> ReceiveObject,
			Action<ConnectionInfo, string, long> ReceiveFileInfo,
			Action<ConnectionInfo, byte[], long, long> ReceiveFile,
			Action<string> errorCallback,
			Action<Connection> removeCallback)
		{
			this.client = client;
			this.errorCallback = errorCallback;
			this.removeCallback = removeCallback;

			// Verbindungsinfos des Clients ausgeben
			try
			{
				IPEndPoint endPoint = (IPEndPoint)client.Client.RemoteEndPoint;

				this.host = endPoint.Address.ToString();
				this.port = Convert.ToUInt16(endPoint.Port);
				Console.WriteLine("Client {0}:{1} hat sich verbunden", this.host, this.port);
			}
			catch (SocketException exc)
			{
				this?.errorCallback(exc.Message);
				this.removeCallback(this);
				return;
			}

			// Prozess zum Empfangen von Daten vom Client starten
			this.core = new Core();
			this.core.configReceiveBuffer(this.client.ReceiveBufferSize);
			this.core.startReceiving(this.client.GetStream(), () => this.removeCallback(this));

			// Events weiterleiten
			this.core.ReceiveObject += (o) => ReceiveObject(this, o);
			this.core.ReceiveFileInfo += (p, s) => ReceiveFileInfo(this, p, s);
			this.core.ReceiveFile += (b, c, t) => ReceiveFile(this, b, c, t);
		}

		public void sendObject(object obj)
		{
			try
			{
				var objectBytes = this.core.serialize(obj);
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

				var filePacketBytes = this.core.serialize(filePacket);
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

		public void disconnect()
		{
			this.client.Close();
		}
	}
}
