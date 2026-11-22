using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace ChatClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {           
            string? line;
            var listener = new TcpListener(IPAddress.Loopback,5000);
            listener.Start();
            Console.WriteLine("Awaiting Message...");

            using TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Client Connected.");

            
                using NetworkStream stream = client.GetStream();
                using var reader = new StreamReader(stream);
                using var writer = new StreamWriter(stream) { AutoFlush = true };
            try
            {
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    Console.WriteLine($"Got: {line}");
                    await writer.WriteLineAsync($"Echo: {line}");
                }
            }catch(Exception e) {
                Console.WriteLine("Client Disconnected.");

                    }


        }
    }
}
