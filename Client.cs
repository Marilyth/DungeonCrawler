using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32.SafeHandles;
using DungeonCrawler.Objects;

namespace DungeonCrawler
{
    public class Client
    {
        private TcpClient client;
        private string key = "";
        private static readonly string ServerIP = "127.0.0.1";
        private DateTime lastSync;

        public Client()
        {
            lastSync = DateTime.UtcNow;
        }

        public async Task Connect()
        {
            Console.WriteLine("Connecting to server...");
            var ip = IPAddress.Parse("127.0.0.1");
            client = new TcpClient(ip.AddressFamily);
            client.Connect(ip, 11000);
            Console.WriteLine("Connected to server!");
            Console.WriteLine(await ObtainFromServer());
        }

        public async Task<WorldMap> DownloadMap()
        {
            Console.WriteLine("Downloading Map...");
            var mapJSON = await ObtainFromServer("DownloadMap>><<");
            return JsonConvert.DeserializeObject<WorldMap>(mapJSON);
        }

        public async Task<Player> DownloadPlayer(string name)
        {
            Console.WriteLine("Downloading Player...");
            var playerJSON = await ObtainFromServer("DownloadPlayer>>" + name + "<<");
            return JsonConvert.DeserializeObject<Player>(playerJSON);
        }

        public async Task FieldChanged(int x, int y, FieldType type)
        {
            string changeJSON = JsonConvert.SerializeObject(Tuple.Create(
                x, y, type
            ));

            await Program.Client.SendToServer("FieldChanged>>" + changeJSON + "<<");
        }

        public async Task ObjectChanged(int x, int y, Object o)
        {
            string changeJSON = JsonConvert.SerializeObject(Tuple.Create(
                x, y, o
            ));

            await Program.Client.SendToServer("ObjectChanged>>" + changeJSON + "<<");
        }

        public async Task ObjectDisappeared(int x, int y)
        {
            string changeJSON = JsonConvert.SerializeObject(new object[]{
                x, y
            });

            await Program.Client.SendToServer("ObjectDisappeared>>" + changeJSON + "<<");
        }

        public async Task ObjectAppeared(Object o)
        {
            string changeJSON = JsonConvert.SerializeObject(o);

            await Program.Client.SendToServer("ObjectAppeared>>" + changeJSON + "<<");
        }

        public async Task StatsChanged(Player user)
        {
            string changeJSON = JsonConvert.SerializeObject(user);

            await Program.Client.SendToServer("StatsChanged>>" + changeJSON + "<<");
        }

        public async Task DownloadPlayers()
        {

        }

        public async Task DownloadField(int x, int y)
        {
            var response = await ObtainFromServer($"DownloadField>>{JsonConvert.SerializeObject(Tuple.Create(x, y))}<<");
            Tuple<FieldType, BaseObject, Player> field = JsonConvert.DeserializeObject<Tuple<FieldType, BaseObject, Player>>(response);

            Program.map.SetField(field.Item1, x, y);
            Program.map.RemoveBaseObject(x, y);
            if (field.Item2 != null)
            {
                Program.map.AddBaseObject(field.Item2);
            }
            if (field.Item3 != null)
            {
                Program.map.AddBaseObject(field.Item3);
            }
        }

        public async Task<bool> InterpretLog()
        {
            var logTime = DateTime.UtcNow;
            var log = await ObtainFromServer($"DownloadLog>>{lastSync.Ticks + 1}<<");
            lastSync = logTime;
            bool hasChanged = false;
            
            foreach (var entry in log.Split("\n"))
            {
                if (!string.Empty.Equals(entry) && !entry.Equals("Start"))
                {
                    var coordinates = entry.Split("|")[1].Split(",");
                    await DownloadField(int.Parse(coordinates[0]), int.Parse(coordinates[1]));
                    hasChanged = true;
                }
            }

            return hasChanged;
        }

        public async Task SendToServer(string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);

            var stream = client.GetStream();
            await stream.WriteAsync(bytes);
        }

        public async Task<string> ObtainFromServer(string request = null)
        {
            lock (key)
            {
                if (request != null)
                {
                    SendToServer(request).Wait();
                }

                string str;
                NetworkStream stream = client.GetStream();

                int timedOut = 0;
                while (!stream.DataAvailable)
                {
                    Task.Delay(100).Wait();
                    timedOut++;
                    if (timedOut > 20) return "";
                }

                byte[] buffer = new byte[1024];
                using (MemoryStream ms = new MemoryStream())
                {

                    int numBytesRead;
                    while (stream.DataAvailable && (numBytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, numBytesRead);


                    }
                    str = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
                }

                return str;
            }
        }
    }
}