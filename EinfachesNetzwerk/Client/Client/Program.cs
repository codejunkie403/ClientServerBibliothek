using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	class Program
	{
		static EinfachesNetzwerk.Client client;

		static void Main(string[] args)
		{
			client = new EinfachesNetzwerk.Client();
			client.ErrorOccured += Client_ErrorOccured;
			client.ConnectionStateChanged += Client_ConnectionStateChanged;
			client.ReceiveObject += Client_ReceiveObject;
			client.ReceiveFileInfo += Client_ReceiveFileInfo;
			client.ReceiveFile += Client_ReceiveFile;

			Console.WriteLine("Stelle Verbindung zum Server her...");
			client.connect(host: "localhost", port: 9876, name: "Hannes");
			Console.ReadKey();


			client.sendObject("Hans", "Nachricht", "ACHTUNG");
			client.sendFile("Peter", "C:\\Users\\Marcel Weski\\Downloads\\VulkanSDK-1.0.61.1-Installer.exe");
			Console.ReadKey();

			if (client.Connected)
			{
				client.disconnect();
				Console.ReadKey();
			}
		}

		#region Events
		// Event, das aufgerufen wird wenn sich die Verbindung zum Server ändern
		private static void Client_ConnectionStateChanged(bool connected)
		{
			Console.WriteLine(connected ? "Verbunden" : "Nicht verbunden");
		}
		// Event, das aufgerufen wird wenn ein Fehler aufgetreten ist
		private static void Client_ErrorOccured(string obj)
		{
			Console.WriteLine("Fehler: {0}", obj);
		}
		// Event, das aufgerufen wird wenn ein Objekt empfangen wird
		private static void Client_ReceiveObject(string sender, string obj_name, string obj_str)
		{
			Console.WriteLine("Objekt von {0} empfangen.\n\tObjekt-Name: {1}\n\tObjekt-String: {2}", sender, obj_name, obj_str);
		}
		// Event, das aufgerufen wird wenn eine Dateiinfo empfangen wird
		private static void Client_ReceiveFileInfo(string sender, string file_name, long file_size)
		{
			Console.WriteLine("Dateiinfo von {0} empfangen.\n\tDateiname: {1}\n\tDateigröße: {2} Bytes", sender, file_name, file_size);
		}
		// Event, das aufgerufen wird wenn ein Dateipaket empfangen wird
		private static void Client_ReceiveFile(string sender, byte[] buffer, long current_size, long total_size)
		{
			float progress = ((float)current_size / total_size) * 100;
			Console.WriteLine("Dateipaket von {0} empfangen.\n\tFortschritt: {1:0.00} %", sender, progress);
		}
		#endregion
	}
}
