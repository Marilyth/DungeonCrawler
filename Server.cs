using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using DungeonCrawler.Objects;
using Newtonsoft.Json;

namespace DungeonCrawler
{
    public class Server
    {
        private Dictionary<TcpClient, Player> clients;
        private static readonly int ServerPort = 11000;

        public Server()
        {
            clients = new Dictionary<TcpClient, Player>();
        }

        public async Task StartServer()
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            var listener = new TcpListener(localAddr, ServerPort);
            listener.Start();

            while (true)
            {
                Console.WriteLine("Waiting for clients to connect...");
                var client = await listener.AcceptTcpClientAsync();
                ClientConversation(client);
            }
        }

        public async Task ClientConversation(TcpClient client)
        {
            try
            {
                await SendToClient("Available commands:\nWorldChanged>>\nDownloadMap>>\nDownloadPlayer>>", client);
                while (true)
                {
                    var info = (await ObtainFromClient(null, client)).Split(">>");
                    Console.WriteLine("Received command: " + info[0]);
                    if (info[0].StartsWith("WorldChanged"))
                    {
                        object[] parsed = JsonConvert.DeserializeObject<object[]>(info[1]);
                        MapChange test = Enum.GetValues(typeof(MapChange)).Cast<MapChange>().ToList()[Convert.ToInt32(parsed[0])];
                        Program.map.ServerWorldChanged(test, parsed[1], parsed[2]);
                        await SendToAllClientsAsync(info[1], client);
                    }
                    else if (info[0].StartsWith("DownloadMap"))
                    {
                        await SendToClient(JsonConvert.SerializeObject(Program.map), client);
                    }
                    else if (info[0].StartsWith("DownloadPlayer"))
                    {
                        clients[client] = new Player(Program.map.Fields.GetLength(1) / 2, Program.map.Fields.GetLength(0) / 2, info[1]);
                        await SendToClient(JsonConvert.SerializeObject(clients[client]), client);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Connection failed, {e}");
                clients.Remove(client);
                client.Dispose();
            }
        }

        public async Task SendToAllClientsAsync(string data, TcpClient exclude)
        {
            foreach (var client in clients)
            {
                if (!client.Key.Equals(exclude))
                    await SendToClient(data, client.Key);
            }
        }

        public async Task SendToClient(string data, TcpClient client)
        {
            var bytes = Encoding.ASCII.GetBytes(data);

            var stream = client.GetStream();
            await stream.WriteAsync(bytes);
        }

        public async Task<string> ObtainFromClient(string request, TcpClient client)
        {
            if(request != null)
                await SendToClient(request, client);

            string str;
            NetworkStream stream = client.GetStream();

            while(!stream.DataAvailable){
                await Task.Delay(100);
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