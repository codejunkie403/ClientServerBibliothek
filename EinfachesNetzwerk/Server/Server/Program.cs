using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Server
{
	class Program
	{
		static EinfachesNetzwerk.Server server;

		static void Main(string[] args)
		{
			server = new EinfachesNetzwerk.Server(receiveCallback, clientConnectionsChanged, errorCallback);
			server.start(port: 9876);
			Console.ReadKey();

			//var clients = server.getClientInfoList();
			//if (clients.Count > 0)
			//	server.kick(clients[0]);
			//Console.ReadKey();

			if (server.Running)
			{
				server.stop();
				Console.ReadKey();
			}
		}

		static void errorCallback(string message)
		{
			Console.WriteLine("Fehler: {0}", message);
		}

		static void receiveCallback(EinfachesNetzwerk.ServerClientConnectionInfo clientInfo, byte[] data, int size)
		{
			//try
			//{
			//	var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(Encoding.UTF8.GetString(data, 0, size));
			//	if (jsonObj["Name"] != null)
			//	{
			//		clientInfo.Name = (string)jsonObj["Name"];
			//	}
			//}
			//catch { }

			byte[] filePacketSizeBytes = new byte[sizeof(int)];
			byte[] filePacketBytes;

			using (var memoryStream = new MemoryStream(data))
			{
				memoryStream.Read(filePacketSizeBytes, 0, sizeof(int));
				filePacketBytes = new byte[BitConverter.ToInt32(filePacketSizeBytes, 0)];
				memoryStream.Read(filePacketBytes, 0, filePacketBytes.Length);
			}


			Console.WriteLine("{0} Bytes empfangen von {1}:{2}:{3}", size, clientInfo.Host, clientInfo.Port, clientInfo.Name);
			Console.WriteLine("Nachricht: {0}", Encoding.UTF8.GetString(filePacketBytes));
		}

		static void clientConnectionsChanged(List<EinfachesNetzwerk.ServerClientConnectionInfo> clientInfoList)
		{
			Console.WriteLine("Verbundene Clients:");
			foreach (var client in clientInfoList)
			{
				Console.WriteLine("\t{0}:{1}:{2}", client.Host, client.Port, client.Name);

				//var data = new Newtonsoft.Json.Linq.JObject();
				//data["Nachricht"] = "Hallo, wie gehts, wie stets?";

				//server.send(client, data);

				((EinfachesNetzwerk.ServerClientConnection)client).core.ReceiveObject += Core_ReceiveObject;
				((EinfachesNetzwerk.ServerClientConnection)client).core.ReceiveFileInfo += Core_ReceiveFileInfo;
				((EinfachesNetzwerk.ServerClientConnection)client).core.ReceiveFile += Core_ReceiveFile;
			}
		}

		private static void Core_ReceiveObject(object obj)
		{
			Console.WriteLine("Objekt empfangen");
		}

		private static void Core_ReceiveFileInfo(string fileName, long fileSize)
		{
			Console.WriteLine("Empfange Datei '{0}' - {1} Bytes", fileName, fileSize);
		}

		private static void Core_ReceiveFile(byte[] fileBuffer, long currentFileSize, long totalFileSize)
		{
			float percentage = ((float)currentFileSize / totalFileSize) * 100;
			Console.WriteLine("{0:0.00} Prozent heruntergeladen...", percentage);
			if (currentFileSize == totalFileSize)
			{
				Console.WriteLine("Datei heruntergeladen");
			}
		}
	}
}
