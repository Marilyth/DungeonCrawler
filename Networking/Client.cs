using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32.SafeHandles;
using DungeonCrawler.Objects;

namespace DungeonCrawler.Networking
{
    public class Client
    {
        private TcpClient client;
        private string key = "";
        private DateTime lastSync;
        public event Action PlayerDownloaded;

        public Client()
        {
            lastSync = DateTime.UtcNow;
        }

        public async Task Connect(string IP)
        {
            Console.WriteLine("Connecting to server...");
            var ip = IPAddress.Parse(IP);
            client = new TcpClient(ip.AddressFamily);
            client.Connect(ip, 11000);
            Console.WriteLine("Connected to server!");
        }

        public async Task DownloadMap()
        {
            Console.WriteLine("Downloading Map...");
            await SendToServer("DownloadMap>><<");
        }

        public async Task SaveMap()
        {
            await SendToServer("SaveMap>><<");
        }

        public async Task DownloadPlayer(string name)
        {
            Console.WriteLine("Downloading Player...");
            await SendToServer("DownloadPlayer>>" + name + "<<");
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

        public async Task DownloadField(int x, int y)
        {
            await SendToServer($"DownloadField>>{JsonConvert.SerializeObject(Tuple.Create(x, y))}<<");
        }

        public async Task ReceivedField(Tuple<Tuple<int, int>, FieldType, BaseObject, Player> field){
            Program.map.SetField(field.Item2, field.Item1.Item1, field.Item1.Item2);
            Program.map.RemoveBaseObject(field.Item1.Item1, field.Item1.Item2);
            if (field.Item3 != null)
            {
                Program.map.AddBaseObject(field.Item3);
            }
            if (field.Item4 != null)
            {
                Program.map.AddBaseObject(field.Item4);
            }

            Program.map.DrawField(field.Item1.Item1, field.Item1.Item2);
        }

        public async Task ReceivedLog(string entry)
        {
            var coordinates = entry.Split("|")[1].Split(",");
            await DownloadField(int.Parse(coordinates[0]), int.Parse(coordinates[1]));
        }

        public async Task StartReceiver(){
            while(true){
                await HandleCommand(await ObtainFromServer());
            }
        }

        public async Task HandleCommand(string commandString)
        {
            var commands = commandString.Split("<<");
            foreach (var command in commands)
            {
                var info = command.Split(">>");

                switch (info[0])
                {
                    case "DownloadLog":
                        ReceivedLog(info.Last());
                        break;
                    case "DownloadField":
                        var field = JsonConvert.DeserializeObject<Tuple<Tuple<int, int>, FieldType, BaseObject, Player>>(info.Last());
                        ReceivedField(field);
                        break;
                    case "DownloadMap":
                        Program.map = JsonConvert.DeserializeObject<WorldMap>(info.Last());
                        break;
                    case "DownloadPlayer":
                        Program.map.SetPlayer(JsonConvert.DeserializeObject<Player>(info.Last()));
                        PlayerDownloaded.Invoke();
                        break;
                    default:
                        break;
                }
            }
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

                while (!stream.DataAvailable)
                {
                    Task.Delay(100).Wait();
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