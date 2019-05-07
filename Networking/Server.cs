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

namespace DungeonCrawler.Networking
{
    public class Server
    {
        private Dictionary<TcpClient, Player> clients;
        private static readonly int ServerPort = 11000;
        public List<string> Log;

        public Server()
        {
            Log = new List<string>();
            clients = new Dictionary<TcpClient, Player>();
        }

        public async Task StartServer()
        {
            IPAddress localAddr = IPAddress.Parse("0.0.0.0");
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
                        await HandleCommand(command, client, clientIP);
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

        public async Task HandleCommand(string command, TcpClient client, string clientIP)
        {
            var info = command.Split(">>");
            Console.WriteLine("Received command: " + info[0]);
            if (info[0].StartsWith("DownloadMap"))
            {
                await SendToClient(info[0] + ">>" + JsonConvert.SerializeObject(Program.map), client);
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
                await SendToClient(info[0] + ">>" + JsonConvert.SerializeObject(Tuple.Create(field, Program.map.Fields[field.Item2, field.Item1], o, player)), client);
            }
            else if (info[0].StartsWith("DownloadPlayer"))
            {
                clients[client] = Player.GetPlayer(info[1]);
                Log.Add($"{DateTime.UtcNow}>>{clientIP}, Change at|{clients[client].XAxis},{clients[client].YAxis}");
                await SendToClient(info[0] + ">>" + JsonConvert.SerializeObject(clients[client]), client);
                foreach(var curClient in clients){
                    if(curClient.Key != client)
                        await ReloadField(curClient.Value.XAxis, curClient.Value.YAxis, curClient.Key, clientIP);
                }
                await ReloadField(clients[client].XAxis, clients[client].YAxis, client, clientIP);
            }
            else if (info[0].StartsWith("FieldChanged"))
            {
                //x, y, fieldtype
                Tuple<int, int, int> change = JsonConvert.DeserializeObject<Tuple<int, int, int>>(info[1]);
                Program.map.SetField((FieldType)change.Item3, change.Item1, change.Item2);
                await ReloadField(change.Item1, change.Item2, client, clientIP);
            }
            else if (info[0].StartsWith("ObjectChanged"))
            {
                //x, y, Object
                Tuple<int, int, BaseObject> change = JsonConvert.DeserializeObject<Tuple<int, int, BaseObject>>(info[1]);
                Program.map.AddBaseObject(change.Item3);
                Program.map.RemoveBaseObject(change.Item1, change.Item2);
                await ReloadField(change.Item1, change.Item2, client, clientIP);
                await ReloadField(change.Item3.XAxis, change.Item3.YAxis, client, clientIP);
            }
            else if (info[0].StartsWith("ObjectAppeared"))
            {
                //Object
                BaseObject obj = JsonConvert.DeserializeObject<BaseObject>(info[1]);
                Program.map.AddBaseObject(obj);
                await ReloadField(obj.XAxis, obj.YAxis, client, clientIP);
            }
            else if (info[0].StartsWith("ObjectDisappeared"))
            {
                //x, y
                Tuple<int, int> coor = JsonConvert.DeserializeObject<Tuple<int, int>>(info[1]);
                Program.map.RemoveBaseObject(coor.Item1, coor.Item2);
                
                await ReloadField(coor.Item1, coor.Item2, client, clientIP);
            }
            else if (info[0].StartsWith("StatsChanged"))
            {
                //x, y
                Player updatedPlayer = JsonConvert.DeserializeObject<Player>(info[1]);
                await ReloadField(clients[client].XAxis, clients[client].YAxis, client, clientIP);
                await ReloadField(updatedPlayer.XAxis, updatedPlayer.YAxis, client, clientIP);
                clients[client] = updatedPlayer;
                updatedPlayer.SavePlayer();
            }
        }

        public async Task ReloadField(int x, int y, TcpClient sender, string clientIP){
            await SendToAllClientsAsync($"{DateTime.UtcNow}>>{clientIP}, StatsChanged|{x},{y}", sender);
        }

        public async Task SendToAllClientsAsync(string data, TcpClient exclude)
        {
            foreach (var client in clients.ToList())
            {
                if (!client.Key.Equals(exclude))
                try{
                    await SendToClient("DownloadLog>>"+data, client.Key);
                } catch(System.IO.IOException e){
                    Console.WriteLine("Client didn't respond, closing connection.");
                    clients.Remove(client.Key);
                    await ReloadField(client.Value.XAxis, client.Value.YAxis, client.Key, "");
                }
            }
        }

        public async Task SendToClient(string data, TcpClient client)
        {
            var bytes = Encoding.ASCII.GetBytes(data + "<<");

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