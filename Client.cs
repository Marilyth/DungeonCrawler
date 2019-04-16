using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace DungeonCrawler
{
    public class Client
    {
        private TcpClient client;
        private static readonly string ServerIP = "127.0.0.1";

        public Client()
        {
            var ip = IPAddress.Parse("127.0.0.1");
            client = new TcpClient(ip.AddressFamily);
            client.ConnectAsync(ip, 11000).Wait();
        }

        public async Task SendToServer(string data)
        {
            var bytes = Encoding.ASCII.GetBytes(data);

            using(var stream = client.GetStream())
                await stream.WriteAsync(bytes);
        }

        public async Task<string> ObtainFromServer(string request)
        {
            await SendToServer(request);

            string str;
            using (NetworkStream stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                using (MemoryStream ms = new MemoryStream())
                {

                    int numBytesRead;
                    while ((numBytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, numBytesRead);


                    }
                    str = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
                }
            }

            return str;
        }
    }
}