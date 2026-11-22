using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace ChatClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            using var client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5000);
            Console.WriteLine("Connected to Server...");
            using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };
            Task receiveTask = ReceiveLoopAsync(reader);
            try
            {
                string? line;
                while(!string.IsNullOrEmpty(line = Console.ReadLine()))
                {
                    await writer.WriteLineAsync(line);
                }
                client.Client.Shutdown(SocketShutdown.Send);
                await receiveTask;
            }
            catch (IOException)
            {
                Console.WriteLine("Connection to server ended abruptly...");
            }
        }
        static async Task ReceiveLoopAsync(StreamReader receive)
        {
            try
            {
                string? line;
                while ((line = await receive.ReadLineAsync()) != null)
                {
                    Console.WriteLine($"Got {line}");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error..{ex}");
            }

        }
    }
}
