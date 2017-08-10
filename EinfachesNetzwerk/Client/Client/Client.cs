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
	public class Client
	{
		// Felder
		private TcpClient client;
		private string host;
		private ushort port;
		private bool connected;
		private Action<byte[], int> receiveCallback;
		private Action<string> errorCallback;
		//private byte[] receiveBuffer;
		private Core core;

		// Eigenschaften
		public string Host { get => this.host; }
		public ushort Port { get => this.port; }
		public bool Connected { get => this.connected; }

		// Öffentliche Methoden
		public Client(Action<byte[], int> receiveCallback, Action<string> errorCallback = null)
		{
			this.client = null;
			this.host = null;
			this.port = 0;
			this.connected = false;
			this.receiveCallback = receiveCallback;
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
		//public void send(byte[] data)
		//{
		//	if (this.connected)
		//	{
		//		// Nachricht an Client senden
		//		this.client.GetStream().BeginWrite(data, 0, data.Length, ar =>
		//		{
		//			try
		//			{
		//				this.client.GetStream().EndWrite(ar);
		//				Console.WriteLine("Nachricht gesendet");
		//			}
		//			catch (IOException exc)
		//			{
		//				this?.errorCallback(exc.Message);
		//				this.setDisconnected();
		//			}
		//			catch (ObjectDisposedException exc)
		//			{
		//				this.setDisconnected();
		//			}

		//		}, null);
		//	}
		//}
		public void sendObject(object obj)
		{
			if (!this.connected)
			{
				Console.WriteLine("Objekt kann nicht gesendet werden, da keine Verbindung zum Server besteht");
				return;
			}

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

		/////////////////////////////////////////////////////////////////////

		//private class Packet
		//{
		//	public enum PacketType : byte
		//	{
		//		Object,
		//		File
		//	}
		//	public PacketType Type { get; set; }
		//}
		//private class ObjectPacket:
		//	Packet
		//{
		//	public object Object;

		//	public ObjectPacket()
		//	{
		//		this.Type = PacketType.Object;
		//	}
		//}

		//private class FilePacket:
		//	Packet
		//{
		//	public long Size { get; set; }
		//	public DateTime CreationTime { get; set; }
		//	public DateTime LastAccessTime { get; set; }
		//	public DateTime LastWriteTime { get; set; }
		//	public string Name { get; set; }

		//	public FilePacket()
		//	{
		//		this.Type = PacketType.File;
		//	}
		//}

		//private byte[] serializeObject(object obj)
		//{
		//	return Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
		//}

		/////////////////////////////////////////////////////////////////////

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
			this.core = new Core(this.client.ReceiveBufferSize);
			this.core.startReceiving(this.client.GetStream(), this.setDisconnected);
		}
		//private void receive()
		//{
		//	try
		//	{
		//		this.client.GetStream().BeginRead(this.core.receivedBuffer, this.core.receivedBufferOffset, this.core.receivedBuffer.Length - this.core.receivedBufferOffset, this.processReceiveBuffer, null);
		//	}
		//	catch (IOException exc)
		//	{
		//		//this?.errorCallback(exc.Message);
		//		this.setDisconnected();
		//	}
		//}
		//private void processReceiveBuffer(IAsyncResult ar)
		//{
		//	int receivedSize = 0;
		//	try
		//	{
		//		receivedSize = this.client.GetStream().EndRead(ar);
		//		if (receivedSize == 0)
		//		{
		//			this.setDisconnected();
		//			return;
		//		}
		//	}
		//	catch (Exception exc)
		//	{
		//		//this?.errorCallback(exc.Message);
		//		this.setDisconnected();
		//		return;
		//	}

		//	//this.receiveCallback(this.receiveBuffer, receivedSize);
		//	Console.WriteLine("{0} Bytes empfangen", receivedSize);
		//	this.core.parseReceivedBuffer(receivedSize);

		//	this.receive();
		//}
	}
}
