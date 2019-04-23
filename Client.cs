using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using DungeonCrawler.Objects;

namespace DungeonCrawler
{
    public class Client
    {
        private TcpClient client;
        private static readonly string ServerIP = "127.0.0.1";

        public Client()
        {
        }

        public async Task Connect(){
            Console.WriteLine("Connecting to server...");
            var ip = IPAddress.Parse("127.0.0.1");
            client = new TcpClient(ip.AddressFamily);
            client.Connect(ip, 11000);
            Console.WriteLine("Connected to server!");
            Console.WriteLine(await ObtainFromServer());
        }

        public async Task<WorldMap> DownloadMap(){
            Console.WriteLine("Downloading Map...");
            var mapJSON = await ObtainFromServer("DownloadMap>>");
            return JsonConvert.DeserializeObject<WorldMap>(mapJSON);
        }

        public async Task<Player> DownloadPlayer(string name){
            Console.WriteLine("Downloading Player...");
            var playerJSON = await ObtainFromServer("DownloadPlayer>>"+name);
            return JsonConvert.DeserializeObject<Player>(playerJSON);
        }

        public async Task SendToServer(string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);

            var stream = client.GetStream();
                await stream.WriteAsync(bytes);
        }

        public async Task<string> ObtainFromServer(string request = null)
        {
            if(request != null){
                client.GetStream().Flush();
                await SendToServer(request);
            }

            string str;
            NetworkStream stream = client.GetStream();

            int timedOut = 0;
            while(!stream.DataAvailable){
                await Task.Delay(100);
                timedOut++;
                if(timedOut > 20) return "";
            }
            
                byte[] buffer = new byte[1024];
                using (MemoryStream ms = new MemoryStream())
                {

                    int numBytesRead;
                    while (stream.DataAvailable && (numBytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, numBytesRead);


                    }
                    str = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
                }
            

            return str;
        }
    }
}