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
	public class ServerClientConnectionInfo
	{
		protected string host;
		protected ushort port;

		public string Host { get => this.host; }
		public ushort Port { get => this.port; }

		//
		public string Name { get; set; }
	}

	public class ServerClientConnection:
		ServerClientConnectionInfo
	{
		// Felder
		private TcpClient client;
		private Action<ServerClientConnectionInfo, byte[], int> receiveCallback;
		private Action<string> errorCallback;
		private Action<ServerClientConnection> removeCallback;
		//private byte[] receiveBuffer;
		public Core core;

		// Eigenschaften

		// Öffentliche Methoden
		public ServerClientConnection(TcpClient client, Action<ServerClientConnectionInfo, byte[], int> receiveCallback, Action<string> errorCallback, Action<ServerClientConnection> removeCallback)
		{
			this.client = client;
			this.receiveCallback = receiveCallback;
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
			//this.receiveBuffer = new byte[this.client.ReceiveBufferSize];
			this.core = new Core(this.client.ReceiveBufferSize);
			this.core.startReceiving(this.client.GetStream(), () => this.removeCallback(this));
		}
		public void send(byte[] data)
		{
			// Nachricht an Client senden
			this.client.GetStream().BeginWrite(data, 0, data.Length, ar =>
			{
				try
				{
					this.client.GetStream().EndWrite(ar);
					Console.WriteLine("Nachricht gesendet");
				}
				catch (IOException exc)
				{
					this?.errorCallback(exc.Message);
					this.removeCallback(this);
				}
				catch (ObjectDisposedException exc)
				{
					this.removeCallback(this);
				}

			}, null);
		}
		public void sendObject()
		{

		}
		public void sendFile()
		{

		}
		public void disconnect()
		{
			this.client.Close();
		}

		// Private Methoden
		//private void receive()
		//{
		//	try
		//	{
		//		this.client.GetStream().BeginRead(this.core.receivedBuffer, this.core.receivedBufferOffset, this.core.receivedBuffer.Length - this.core.receivedBufferOffset, this.processReceiveBuffer, null);
		//	}
		//	catch (IOException exc)
		//	{
		//		//this?.errorCallback(exc.Message);
		//		this.removeCallback(this);
		//	}
		//}

		//////////////////////////////////////////////////////////////////////
		//private MemoryStream memoryStream = new MemoryStream();
		//private bool readPacket = false;
		//private bool readFile = false;
		//private int packetSize = 0;
		//private FilePacket currentFilePacket;
		//private FileStream fileStream;
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
		//private class ObjectPacket :
		//	Packet
		//{
		//	public object Object;
		//}

		//private class FilePacket :
		//	Packet
		//{
		//	public long Size { get; set; }
		//	public DateTime CreationTime { get; set; }
		//	public DateTime LastAccessTime { get; set; }
		//	public DateTime LastWriteTime { get; set; }
		//	public string Name { get; set; }
		//}

		//private byte[] serializeObject(object obj)
		//{
		//	return Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
		//}

		/////////////////////////////////////////////////////////////////////

		//private void parse(int receivedSize, int bufferOffset = 0)
		//{
		//	//// Paket weiter einlesen
		//	//if (this.readPacket)
		//	//{
		//	//	int bytesAvailable = receivedSize - bufferOffset;
		//	//	if (bytesAvailable >= packetSize)
		//	//	{
		//	//		this.memoryStream.Write(this.receiveBuffer, bufferOffset, packetSize);
		//	//		// Callback

		//	//		if (bytesAvailable > packetSize)
		//	//		{
		//	//			this.parse(receivedSize, bufferOffset + 6);
		//	//		}
		//	//	}
		//	//	else
		//	//	{
		//	//		this.memoryStream.Write(this.receiveBuffer, bufferOffset, receivedSize - bufferOffset);
		//	//	}
		//	//}
		//	//else
		//	//{
		//	//	// Paketgröße einlesen
		//	//	if (receivedSize >= sizeof(int))
		//	//	{
		//	//		this.packetSize = BitConverter.ToInt32(this.receiveBuffer, 0);
		//	//		this.readPacket = true;

		//	//		if (receivedSize > sizeof(int))
		//	//		{
		//	//			this.parse(receivedSize, sizeof(int));
		//	//		}
		//	//	}
		//	//	else // Puffer füllen bis Paketgröße gelesen werden kann
		//	//	{
					
		//	//	}
		//	//}

		//	if (this.readPacket)
		//	{
		//		// Paket einlesen
		//		int bytesAvailable = receivedSize - bufferOffset;
		//		int restBytes = this.packetSize - (int)this.memoryStream.Length;

		//		if (bytesAvailable >= restBytes)
		//		{
		//			this.memoryStream.Write(this.receiveBuffer, bufferOffset, this.packetSize);
		//			// Paket empfangen!
		//			Console.WriteLine("Paket empfangen!");

		//			try
		//			{
		//				var filePacket = Newtonsoft.Json.JsonConvert.DeserializeObject<FilePacket>(Encoding.UTF8.GetString(this.memoryStream.ToArray()));

		//				Console.WriteLine("Dateigröße: {0}", filePacket.Size);
		//				this.readFile = true;

		//				this.currentFilePacket = filePacket;
		//			}
		//			catch
		//			{
		//				var objectPacket = Newtonsoft.Json.JsonConvert.DeserializeObject<ObjectPacket>(Encoding.UTF8.GetString(this.memoryStream.ToArray()));

		//			}
		//			this.readPacket = false;
		//			this.memoryStream.Position = 0;
		//			this.memoryStream.SetLength(0);

		//			if (bytesAvailable > restBytes)
		//			{
		//				this.parse(receivedSize, bufferOffset + this.packetSize);
		//			}
		//		}
		//		else
		//		{
		//			this.memoryStream.Write(this.receiveBuffer, bufferOffset, bytesAvailable);

		//			// Empfangen... (Springe zum Ende der Funktion)
		//		}
		//	}
		//	else if (this.readFile)
		//	{
		//		// ...

		//		if (this.fileStream == null)
		//		{
		//			File.Delete(this.currentFilePacket.Name);
		//			this.fileStream = File.Create(this.currentFilePacket.Name);
		//		}

		//		fileStream.Write(this.receiveBuffer, 0, receivedSize);

		//		if (fileStream.Length == this.currentFilePacket.Size)
		//		{
		//			fileStream.Close();

		//			File.SetCreationTime(this.currentFilePacket.Name, this.currentFilePacket.CreationTime);
		//			File.SetLastAccessTime(this.currentFilePacket.Name, this.currentFilePacket.LastAccessTime);
		//			File.SetLastWriteTime(this.currentFilePacket.Name, this.currentFilePacket.LastWriteTime);

		//			this.fileStream = null;
		//			this.readFile = false;
		//			this.currentFilePacket = null;
		//			Console.WriteLine("Datei heruntergeladen");
		//		}
		//	}
		//	else
		//	{
		//		// Paketgröße einlesen
		//		this.packetSize = BitConverter.ToInt32(this.receiveBuffer, 0);
		//		bufferOffset = sizeof(int);
		//		this.readPacket = true;
		//		Console.WriteLine("Paketgröße: {0} Bytes", this.packetSize);

		//		if (receivedSize > sizeof(int))
		//		{
		//			this.parse(receivedSize, bufferOffset);
		//		}
		//		else
		//		{
		//			// Empfangen... (Springe zum Ende der Funktion)
		//		}
		//	}
		//}
		//////////////////////////////////////////////////////////////////////

		//private void processReceiveBuffer(IAsyncResult ar)
		//{
		//	int receivedSize = 0;
		//	try
		//	{
		//		receivedSize = this.client.GetStream().EndRead(ar);
		//		if (receivedSize == 0)
		//		{
		//			this.removeCallback(this);
		//			return;
		//		}
		//	}
		//	catch (Exception exc)
		//	{
		//		//this?.errorCallback(exc.Message);
		//		this.removeCallback(this);
		//		return;
		//	}

		//	//////////////////////////////////////////////////////////////////////
		//	//Console.WriteLine("{0} Bytes empfangen", receivedSize);
		//	//this.parse(receivedSize);
		//	this.core.parseReceivedBuffer(receivedSize);
		//	//////////////////////////////////////////////////////////////////////

		//	//this.receiveCallback(this, this.receiveBuffer, receivedSize);

		//	this.receive();
		//}
	}
}
