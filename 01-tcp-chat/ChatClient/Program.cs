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
            Task receiveTask = ReceiveLoopAsync(client, reader);
            string username = Console.ReadLine() ?? "Default";
            await writer.WriteLineAsync(username);
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
        static async Task ReceiveLoopAsync(TcpClient client, StreamReader receive)
        {
            try
            {
                string? line;
                while ((line = await receive.ReadLineAsync()) != null)
                {
                    if(line == "/shutdown")
                    {
                        Console.WriteLine("Server is shutting down. Client Disconnected.");
                        client.Client.Shutdown(SocketShutdown.Both);
                    }
                    else
                    {
                        Console.WriteLine($"{line}");
                    }
                }
            }
            catch (IOException) {
                Console.WriteLine("Connection to server ended. You can close this window.");
            }

        }
    }
}
