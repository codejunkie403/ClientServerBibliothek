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
			client = new EinfachesNetzwerk.Client(receiveCallback, errorCallback);
			client.connect(host: "localhost", port: 9876);
			Console.ReadKey();

			//var obj = new Newtonsoft.Json.Linq.JObject();
			//obj["Name"] = "Hans Peter!";

			//client.sendObject(obj);
			//client.sendObject(obj);

			client.sendFile("C:\\Users\\Marcel\\Downloads\\FileZilla_3.26.2_win64-setup.exe");
			//client.sendObject(obj);
			//client.sendObject(obj);
			//client.sendObject(obj);
			//client.sendFile("C:\\Users\\Marcel\\Downloads\\FileZilla_3.26.2_win64-setup.exe");
			//client.sendObject(obj);

			Console.ReadKey();

			if (client.Connected)
			{
				client.disconnect();
				Console.ReadKey();
			}
		}

		static void errorCallback(string message)
		{
			Console.WriteLine("Fehler: {0}", message);
		}

		static void receiveCallback(byte[] data, int size)
		{
			Console.WriteLine("{0} Bytes empfangen", size);
			Console.WriteLine("Nachricht: {0}", Encoding.UTF8.GetString(data, 0, size));
			//client.send(Encoding.UTF8.GetBytes("Jo, alles klar!"));
		}
	}
}
