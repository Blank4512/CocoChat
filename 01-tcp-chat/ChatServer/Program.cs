using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
namespace ChatServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string? line;
            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5000);
            Console.WriteLine("Connected to Server...");
            Console.WriteLine("Press any key to Exit.");
            using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            do
            {
                line = Console.ReadLine();
                Console.WriteLine($"Sent: {line}");
                await writer.WriteLineAsync(line);
            } while (line != "");

        }
    }
}
