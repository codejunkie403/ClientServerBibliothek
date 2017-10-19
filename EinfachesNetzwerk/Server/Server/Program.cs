using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using EinfachesNetzwerk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
	class Program
	{
		static EinfachesNetzwerk.Server server;

		static void Main(string[] args)
		{
			server = new EinfachesNetzwerk.Server();
			server.ErrorOccured += Server_ErrorOccured;
			server.ConnectionsChanged += Server_ConnectionsChanged;
			server.ReceiveObject += Server_ReceiveObject;
			server.ReceiveFileInfo += Server_ReceiveFileInfo;
			server.ReceiveFile += Server_ReceiveFile;

			server.start(port: 9876);
			Console.ReadKey();

			if (server.Running)
			{
				server.stop();
				Console.ReadKey();
			}
		}

		#region Events
		// Event, das aufgerufen wird wenn ein Fehler auftritt
		private static void Server_ErrorOccured(string message)
		{
			Console.WriteLine("Fehler: {0}", message);
		}
		// Event, das aufgerufen wird wenn sich die Verbindungen ändern
		private static void Server_ConnectionsChanged(List<ConnectionInfo> clients)
		{
			Console.WriteLine("Verbundene Clients haben sich geändert");

			// Sobald sich ein neuer Client verbindet/trennt werden die verbunden Clientnamen an alle noch verbundenen Clients gesendet
			if (clients.Count > 0)
			{
				var names = new List<string>();
				for (int i = 0; i < clients.Count; i++)
				{
					names.Add(clients[i].Name);
				}

				foreach (var client in clients)
				{
					Console.WriteLine("Sende Client-Namen an {0}", client.Name);
					((Connection)client).sendObject("Server", "ClientNamen", names);
				}
			}
		}
		// Event, das aufgerufen wird wenn ein Objekt empfangen wird
		private static void Server_ReceiveObject(ConnectionInfo sender_info, string receiver, string obj_name, string obj_str)
		{
			Console.WriteLine("Objekt von {0} empfangen, das an {1} addressiert ist.\n\tObjekt-Name: {2}\n\tObjekt-String: {3}", sender_info.Name, receiver, obj_name, obj_str);

			// Überprüfen, ob Empfänger verbunden ist
			foreach (var client in server.getClientInfoList())
			{
				if (client.Name == receiver)
				{
					// Objekt an Empfänger weiterleiten
					((Connection)client).sendObject(sender_info.Name, obj_name, obj_str);
					Console.WriteLine("Objekt an {0} weitergeleitet", receiver);
					return;
				}
			}

			Console.WriteLine("Empfänger ist nicht verbunden!");
		}
		// Event, das aufgerufen wird wenn eine Dateiinfo empfangen wird
		private static void Server_ReceiveFileInfo(ConnectionInfo sender_info, string receiver, string filename, long size)
		{
			Console.WriteLine("Dateiinfo von {0} empfangen, die an {1} addressiert ist.\n\tDateiname: {2}\n\tDateigröße: {3} Bytes", sender_info.Name, receiver, filename, size);
		}
		// Event, das aufgerufen wird wenn ein Dateipaket empfangen wird
		private static void Server_ReceiveFile(ConnectionInfo sender_info, string receiver, byte[] buffer, long current_size, long total_size)
		{
			float progress = ((float)current_size / total_size) * 100;
			Console.WriteLine("Dateipaket von {0} empfangen, das an {1} addressiert ist.\n\tFortschritt: {2:0.00} %", sender_info.Name, receiver, progress);
		}
		#endregion
	}
}
