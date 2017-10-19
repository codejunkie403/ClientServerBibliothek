using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EinfachesNetzwerk
{
	public class ConnectionInfo
	{
		protected string host;
		protected ushort port;
		protected string name;

		public string Host { get => this.host; }
		public ushort Port { get => this.port; }
		
		//
		public string Name { get => this.name; set => this.name = value; }
	}
	public class Core:
		ConnectionInfo
	{
		// Datentypen
		public enum PacketType: byte
		{
			Invalid,
			Object,
			File,
			FileData
		}
		public class FilePacket
		{
			public long Size { get; set; }
			public DateTime CreationTime { get; set; }
			public DateTime LastAccessTime { get; set; }
			public DateTime LastWriteTime { get; set; }
			public string Name { get; set; }
			public string Receiver;
		}

		// Felder
		public byte[] receivedBuffer;
		public int receivedBufferOffset;
		private int currentPacketSize;
		private PacketType currentPacketType;
		private MemoryStream packetStream;
		private FilePacket currentFilePacket;
		private long receivedFileSize;

		// Events
		public event Action<string, string, string> ReceiveObject;
		public event Action<string, string, long> ReceiveFileInfo;
		public event Action<string, byte[], long, long> ReceiveFile;

		// Methoden
		public Core()
		{
			this.receivedBufferOffset = 0;
			this.currentPacketSize = 0;
			this.currentPacketType = PacketType.Invalid;
			this.packetStream = new MemoryStream();
			this.currentFilePacket = null;
			this.receivedFileSize = 0;
		}
		public void configReceiveBuffer(int size)
		{
			this.receivedBuffer = new byte[size];
		}
		public byte[] serialize(object obj)
		{
			return Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj));
		}
		public object deserialize(byte[] bytes)
		{
			return Newtonsoft.Json.JsonConvert.DeserializeObject(Encoding.UTF8.GetString(bytes));
		}
		public static T ToObject<T>(string obj)
		{
			return JsonConvert.DeserializeObject<T>(obj);
		}
		private void parseReceivedBuffer(int receivedSize)
		{
			//////////////////////////////////////////////////////
			// ------------------------------------------------ //
			// | Paketgröße | Pakettyp |  Paket  | Dateibytes | //
			// ------------------------------------------------ //
			// |   4 Bytes  |  1 Byte  | X Bytes |  X Bytes   | //
			// ------------------------------------------------ //
			//////////////////////////////////////////////////////

			// Wenn gerade kein Paket gelesen wird
			if (this.currentPacketType == PacketType.Invalid)
			{
				// Wenn die empfangen Bytes nicht ausreichen, um die Paketgröße und den Pakettyp zu lesen
				if (receivedSize < sizeof(int) + sizeof(byte))
				{
					this.receivedBufferOffset = receivedSize;
					// Weiter empfangen...
				}
				// Paketgröße und Pakettyp lesen
				else
				{
					this.currentPacketSize = BitConverter.ToInt32(this.receivedBuffer, this.receivedBufferOffset);
					if (Enum.IsDefined(typeof(PacketType), this.receivedBuffer[this.receivedBufferOffset + sizeof(int)]))
					{
						this.currentPacketType = (PacketType)this.receivedBuffer[this.receivedBufferOffset + sizeof(int)];

						int offset = sizeof(int) + sizeof(byte);

						// Wenn noch mehr gelesen werden kann
						if (receivedSize - offset > 0)
						{
							this.receivedBufferOffset += offset;

							this.parseReceivedBuffer(receivedSize - offset);
						}
						else
						{
							this.receivedBufferOffset = 0;
							// Weiter empfangen...
						}
					}
					else
					{
						this.currentPacketSize = 0;
						Console.WriteLine("Empfangener Pakettyp '{0}' ist ungültig!", this.receivedBuffer[this.receivedBufferOffset + sizeof(int)]);

						this.receivedBufferOffset = 0;
						// Weiter empfangen...
					}
				}
			}
			// Dateipaket lesen
			else if (this.currentPacketType == PacketType.File)
			{
				// Wenn das ganze Paket gelesen werden kann
				if (receivedSize >= this.currentPacketSize)
				{
					// Alle Bytes des Pakets in den Stream schreiben
					this.packetStream.Write(this.receivedBuffer, this.receivedBufferOffset, this.currentPacketSize);
					// Paket ist fertig!
					// Wenn gültiges Paket: Datei empfangen, sonst zurück setzen
					try
					{
						this.currentFilePacket = Newtonsoft.Json.JsonConvert.DeserializeObject<FilePacket>(Encoding.UTF8.GetString(this.packetStream.ToArray()));
						this.currentPacketType = PacketType.FileData;
						//Console.WriteLine(Encoding.UTF8.GetString(this.packetStream.ToArray()));
						//Console.WriteLine("Dateipaket empfangen");
						this.ReceiveFileInfo(this.currentFilePacket.Receiver, this.currentFilePacket.Name, this.currentFilePacket.Size);
					}
					catch
					{
						this.currentPacketType = PacketType.Invalid;
						Console.WriteLine("Dateipaket fehlerhaft!");
					}

					// Paketstream für nächstes Paket zurücksetzen
					this.packetStream.Position = 0;
					this.packetStream.SetLength(0);

					// Wenn noch mehr Bytes gelesen werden können
					if (receivedSize > this.currentPacketSize)
					{
						this.receivedBufferOffset += this.currentPacketSize;

						this.parseReceivedBuffer(receivedSize - this.currentPacketSize);
					}
					else
					{
						this.receivedBufferOffset = 0;
						// Weiter empfangen...
					}

					this.currentPacketSize = 0;
				}
				// Wenn ein Teil des Pakets gelesen werden kann
				else
				{
					// Verfügbare Bytes in den Paketstream schreiben
					this.packetStream.Write(this.receivedBuffer, this.receivedBufferOffset, receivedSize);

					this.receivedBufferOffset = 0;
					// Weiter empfangen...
				}
			}
			// Datei lesen
			else if (this.currentPacketType == PacketType.FileData)
			{
				// Wenn die Datei noch empfangen wird
				if (this.receivedFileSize < this.currentFilePacket.Size)
				{
					long restSize = this.currentFilePacket.Size - this.receivedFileSize;

					// Wenn die fehlenden Bytes vorhanden sind
					if (receivedSize >= restSize)
					{
						byte[] fileBuffer = new byte[restSize];
						Array.Copy(this.receivedBuffer, this.receivedBufferOffset, fileBuffer, 0, restSize);
						//Console.WriteLine("{0} Bytes der Datei empfangen", restSize);
						//Console.WriteLine("Datei komplett empfangen");
						// Callback für Dateiverarbeitung
						this.receivedFileSize += restSize;
						this?.ReceiveFile(this.currentFilePacket.Receiver, fileBuffer, this.receivedFileSize, this.currentFilePacket.Size);

						this.currentPacketSize = 0;
						this.currentPacketType = PacketType.Invalid;
						this.currentFilePacket = null;
						this.receivedFileSize = 0;

						// Wenn noch mehr Bytes gelesen werden können
						if (receivedSize > restSize)
						{
							this.receivedBufferOffset += (int)restSize;

							this.parseReceivedBuffer(receivedSize - (int)restSize);
						}
						else
						{
							this.receivedBufferOffset = 0;
							// Weiter empfangen...
						}
					}
					else
					{
						byte[] fileBuffer = new byte[receivedSize];
						Array.Copy(this.receivedBuffer, this.receivedBufferOffset, fileBuffer, 0, receivedSize);
						//Console.WriteLine("{0} Bytes der Datei empfangen", receivedSize);
						// Callback für Dateiverarbeitung
						this?.ReceiveFile(this.currentFilePacket.Receiver, fileBuffer, this.receivedFileSize, this.currentFilePacket.Size);

						this.receivedFileSize += receivedSize;

						this.receivedBufferOffset = 0;
						// Weiter empfangen...
					}
				}
				else
				{
					Console.WriteLine("WIE BIN ICH HIER HIN GEKOMMEN???");
					System.Diagnostics.Debugger.Break();
				}
			}
			// Objekt lesen
			else if (this.currentPacketType == PacketType.Object)
			{
				// Wenn das ganze Paket gelesen werden kann
				if (receivedSize >= this.currentPacketSize)
				{
					// Alle Bytes des Pakets in den Stream schreiben
					this.packetStream.Write(this.receivedBuffer, this.receivedBufferOffset, this.currentPacketSize);
					// Paket ist fertig!
					//Console.WriteLine("Dateipaket empfangen");
					//if (this.name == null)
					//{
					//	try
					//	{
					//		var data = (Newtonsoft.Json.Linq.JObject)this.deserialize(this.packetStream.ToArray());
					//		if (data["$ConnectionInfoName"] != null)
					//		{
					//			this.name = (string)data["$ConnectionInfoName"];
					//			Console.WriteLine("{0} hat sich verbunden", this.name);
					//		}
					//		else
					//		{
					//			throw new Exception("Es sollte erst ein Name an den Server geschickt werden!");
					//		}
					//	}
					//	catch
					//	{

					//	}
					//}
					//else
					{
						var data = (JObject)this.deserialize(this.packetStream.ToArray());
						foreach (var pair in data)
						{
							//this.ReceiveObject?.Invoke(pair.Key, JsonConvert.DeserializeObject(pair.Value.ToString()));
							if (pair.Key != "Receiver")
								this.ReceiveObject?.Invoke(data["Receiver"].ToString(), pair.Key, pair.Value.ToString());
						}

					}

					// Pakettyp und Paketstream für nächstes Paket zurücksetzen
					this.currentPacketType = PacketType.Invalid;
					this.packetStream.Position = 0;
					this.packetStream.SetLength(0);

					// Wenn noch mehr Bytes gelesen werden können
					if (receivedSize > this.currentPacketSize)
					{
						this.receivedBufferOffset += this.currentPacketSize;

						this.parseReceivedBuffer(receivedSize - this.currentPacketSize);
					}
					else
					{
						this.receivedBufferOffset = 0;
						// Weiter empfangen...
					}

					this.currentPacketSize = 0;
				}
				// Wenn ein Teil des Pakets gelesen werden kann
				else
				{
					// Verfügbare Bytes in den Paketstream schreiben
					this.packetStream.Write(this.receivedBuffer, this.receivedBufferOffset, receivedSize);

					this.receivedBufferOffset = 0;
					// Weiter empfangen...
				}
			}
		}
		public void startReceiving(NetworkStream stream, Action callback)
		{
			try
			{
				stream.BeginRead(this.receivedBuffer, this.receivedBufferOffset, this.receivedBuffer.Length - this.receivedBufferOffset, ar =>
				{
					int receivedSize = 0;
					try
					{
						receivedSize = stream.EndRead(ar);
						if (receivedSize == 0)
						{
							callback();
							return;
						}
					}
					catch
					{
						callback();
						return;
					}

					this.parseReceivedBuffer(receivedSize);

					this.startReceiving(stream, callback);

				}, null);
			}
			catch
			{
				callback();
			}
		}

	}
}
