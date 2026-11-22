using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
namespace ChatServer
{
    internal class Program
    {
        static async Task Main()
        {
            var listener = new TcpListener(IPAddress.Loopback, 5000);
            listener.Start();
            Console.WriteLine("Server Started.");
            var writers = new ConcurrentDictionary<int, StreamWriter>();
            int nextID = 0;
            while (true) {
                TcpClient tcp = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(tcp, writers, nextID);
                Interlocked.Increment(ref nextID);
            }
        }
        static async Task HandleClientAsync(TcpClient tcp, ConcurrentDictionary<int, StreamWriter> writers, int ID)
        {
            using (tcp)
            {
                Console.WriteLine("Client Connected.");
                using NetworkStream stream = tcp.GetStream();
                using var reader = new StreamReader(stream);
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                writers.TryAdd(ID, writer);
                try
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        Console.WriteLine($"From: Client {ID}: {line}");
                        foreach (var kvp in writers)
                        {
                            try
                            {
                                await kvp.Value.WriteLineAsync($"{ID}: {line}");
                            }
                            catch (IOException)
                            {
                                Console.WriteLine($"Client {kvp.Key} Disconnected.");
                            }
                        }
                    }
                    Console.WriteLine($"Client {ID} Disconnected.");
                }
                catch (IOException)
                {
                    Console.WriteLine($"Client {ID} Disconnected abruptly.");
                }
                finally { writers.TryRemove(ID, out _); }
            }
           
        }
    }
}
