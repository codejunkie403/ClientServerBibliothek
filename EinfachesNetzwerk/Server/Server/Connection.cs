using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EinfachesNetzwerk
{
	public class Connection:
		Core
	{
		// Felder
		private TcpClient client;
		private bool call_remove_callback;

		// Events
		public event Action<string> ErrorOccured;
		public event Action<Connection> RemoveCallback;

		// Öffentliche Methoden
		public Connection(TcpClient client,
			Action<ConnectionInfo, string, string, string> ReceiveObject,
			Action<ConnectionInfo, string, string, long> ReceiveFileInfo,
			Action<ConnectionInfo, string, byte[], long, long> ReceiveFile)
		{
			this.client = client;
			this.call_remove_callback = true;

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
				this.ErrorOccured(exc.Message);
				this.RemoveCallback(this);
				return;
			}

			// Prozess zum Empfangen von Daten vom Client starten
			this.configReceiveBuffer(this.client.ReceiveBufferSize);
			this.startReceiving(this.client.GetStream(), () =>
			{
				if (this.call_remove_callback)
					this.RemoveCallback(this);
			});

			// Events weiterleiten
			this.ReceiveObject += (r, n, o) => ReceiveObject(this, r, n, o);
			this.ReceiveFileInfo += (r, p, s) => ReceiveFileInfo(this, r, p, s);
			this.ReceiveFile += (r, b, c, t) => ReceiveFile(this, r, b, c, t);
		}

		public void sendObject(string sender, string obj_name, object obj)
		{
			try
			{
				var objJson = new JObject {
					[obj_name] = JsonConvert.SerializeObject(obj),
					["Receiver"] = sender
				};
				var objString = objJson.ToString();
				var objectBytes = Encoding.UTF8.GetBytes(objString);
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
		public void sendFile(string sender, string path)
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
				var filePacket = new FilePacket
				{
					Size = fileInfo.Length,
					CreationTime = fileInfo.CreationTime,
					LastAccessTime = fileInfo.LastAccessTime,
					LastWriteTime = fileInfo.LastWriteTime,
					Name = fileInfo.Name,
					Receiver = sender
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
		public void forwardFilePacket(string receiver, byte[] buffer, long current_size, long total_size)
		{
			// TODO: Dateipakete an Empfänger weiterleiten
		}

		public void disconnect(bool call_remove_callback = true)
		{
			this.call_remove_callback = call_remove_callback;
			this.client.Close();
		}
	}
}
