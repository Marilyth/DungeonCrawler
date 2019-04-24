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
        public StringBuilder Log;

        public Server()
        {
            Log = new StringBuilder();
            clients = new Dictionary<TcpClient, Player>();
        }

        public async Task StartServer()
        {
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            var listener = new TcpListener(localAddr, ServerPort);
            listener.Start();

            while (true)
            {
                Console.Write("Waiting for client to connect...");
                var client = await listener.AcceptTcpClientAsync();
                ClientConversation(client);
                Console.WriteLine($"{(client.Client.RemoteEndPoint as IPEndPoint).Address}:{(client.Client.RemoteEndPoint as IPEndPoint).Port} connected!");
            }
        }

        public async Task ClientConversation(TcpClient client)
        {
            try
            {
                var clientIP = $"{(client.Client.RemoteEndPoint as IPEndPoint).Address}:{(client.Client.RemoteEndPoint as IPEndPoint).Port}";
                while (true)
                {
                    foreach (var command in (await ObtainFromClient(null, client)).Split("<<"))
                    {
                        var info = command.Split(">>");
                        Console.WriteLine("Received command: " + info[0]);
                        if (info[0].StartsWith("DownloadMap"))
                        {
                            await SendToClient(JsonConvert.SerializeObject(Program.map), client);
                        }
                        if (info[0].StartsWith("SaveMap"))
                        {
                            Program.map.SaveMap();
                        }
                        else if (info[0].StartsWith("DownloadField"))
                        {
                            var field = JsonConvert.DeserializeObject<Tuple<int, int>>(info[1]);
                            Player player = player = clients.Values.FirstOrDefault(x => x.XAxis == field.Item1 && x.YAxis == field.Item2);
                            Program.map.GetBaseObjectDict().TryGetValue(field, out BaseObject o);
                            await SendToClient(JsonConvert.SerializeObject(Tuple.Create(Program.map.Fields[field.Item2, field.Item1], o, player)), client);
                        }
                        else if (info[0].StartsWith("DownloadPlayer"))
                        {
                            clients[client] = new Player(Program.map.Fields.GetLength(1) / 2, Program.map.Fields.GetLength(0) / 2, info[1]);
                            Log.AppendLine($"{DateTime.UtcNow}>>{clientIP}, Change at|{clients[client].XAxis},{clients[client].YAxis}");
                            await SendToClient(JsonConvert.SerializeObject(clients[client]), client);
                        }
                        else if (info[0].StartsWith("DownloadLog"))
                        {
                            var since = new DateTime(long.Parse(info[1]));
                            StringBuilder log = new StringBuilder("Start");
                            foreach (var b in Log.ToString().Split("\n"))
                            {
                                if (!String.Empty.Equals(b) && !b.Split(">>")[1].Split(",")[0].Equals(clientIP) && DateTime.Parse(b.Split(">>")[0]) > since)
                                    log.AppendLine(b);
                            }
                            await SendToClient(log.ToString(), client);
                        }
                        else if (info[0].StartsWith("FieldChanged"))
                        {
                            //x, y, fieldtype
                            Tuple<int, int, int> change = JsonConvert.DeserializeObject<Tuple<int, int, int>>(info[1]);
                            Program.map.SetField((FieldType)change.Item3, change.Item1, change.Item2);
                            Log.AppendLine($"{DateTime.UtcNow}>>{clientIP}, Change at|{change.Item1},{change.Item2}");
                        }
                        else if (info[0].StartsWith("ObjectChanged"))
                        {
                            //x, y, Object
                            Tuple<int, int, BaseObject> change = JsonConvert.DeserializeObject<Tuple<int, int, BaseObject>>(info[1]);
                            Program.map.AddBaseObject(change.Item3);
                            Program.map.RemoveBaseObject(change.Item1, change.Item2);
                            Log.AppendLine($"{DateTime.UtcNow}>>{clientIP}, Change at|{change.Item1},{change.Item2}");
                            Log.AppendLine($"{DateTime.UtcNow}>>{clientIP}, Change at|{change.Item3.XAxis},{change.Item3.YAxis}");
                        }
                        else if (info[0].StartsWith("ObjectAppeared"))
                        {
                            //Object
                            BaseObject obj = JsonConvert.DeserializeObject<BaseObject>(info[1]);
                            Program.map.AddBaseObject(obj);
                            Log.AppendLine($"{DateTime.UtcNow}>>{clientIP}, Change at|{obj.XAxis},{obj.YAxis}");
                        }
                        else if (info[0].StartsWith("ObjectDisappeared"))
                        {
                            //x, y
                            Tuple<int, int> coor = JsonConvert.DeserializeObject<Tuple<int, int>>(info[1]);
                            Program.map.RemoveBaseObject(coor.Item1, coor.Item2);
                            Log.AppendLine($"{DateTime.UtcNow}>>{clientIP}, Change at|{coor.Item1},{coor.Item2}");
                        }
                        else if (info[0].StartsWith("StatsChanged"))
                        {
                            //x, y
                            Player updatedPlayer = JsonConvert.DeserializeObject<Player>(info[1]);
                            Log.AppendLine($"{DateTime.UtcNow}>>{clientIP}, Change at|{clients[client].XAxis},{clients[client].YAxis}");
                            Log.AppendLine($"{DateTime.UtcNow}>>{clientIP}, Change at|{updatedPlayer.XAxis},{updatedPlayer.YAxis}");
                            clients[client] = updatedPlayer;
                        }
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
            if (request != null)
                await SendToClient(request, client);

            string str;
            NetworkStream stream = client.GetStream();

            while (!stream.DataAvailable)
            {
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