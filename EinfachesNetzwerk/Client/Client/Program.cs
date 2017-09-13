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
			client = new EinfachesNetzwerk.Client(errorCallback);
			client.ReceiveObject += Client_ReceiveObject;
			client.ReceiveFileInfo += Client_ReceiveFileInfo;
			client.ReceiveFile += Client_ReceiveFile;

			client.connect(host: "localhost", port: 9876);

			Console.ReadKey();


			client.sendObject("HALLLOOOO");
			Console.ReadKey();

			if (client.Connected)
			{
				client.disconnect();
				Console.ReadKey();
			}
		}

		private static void Client_ReceiveFile(byte[] arg1, long arg2, long arg3)
		{
			Console.WriteLine("Dateipaket empfangen");
		}

		private static void Client_ReceiveFileInfo(string arg1, long arg2)
		{
			Console.WriteLine("Dateiinfo empfangen");
		}

		private static void Client_ReceiveObject(object obj)
		{
			Console.WriteLine("Objekt empfangen");
		}

		static void errorCallback(string message)
		{
			Console.WriteLine("Fehler: {0}", message);
		}
	}
}
