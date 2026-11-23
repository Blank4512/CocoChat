using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
namespace ChatServer
{
    internal class Program
    {
        static ConcurrentDictionary<int, (StreamWriter swriter, string name)> writers = new();
        static async Task Main()
        {

            var listener = new TcpListener(IPAddress.Loopback, 5000);
            listener.Start();
            Console.WriteLine("Server Started.");
            
            int nextID = 0;
            while (true) {
                TcpClient tcp = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(tcp, nextID);
                Interlocked.Increment(ref nextID);
            }
        }
        static async Task HandleClientAsync(TcpClient tcp, int ID)
        {
            using (tcp)
            {
                
                using NetworkStream stream = tcp.GetStream();
                using var reader = new StreamReader(stream);
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                string username = await reader.ReadLineAsync() ?? "Default";
                Console.WriteLine($"{username} Connected.");
                writers.TryAdd(ID,(writer,username));
                try
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        Console.WriteLine($"From: {username}: {line}");
                        foreach (var kvp in writers)
                        {
                            try
                            {
                                await kvp.Value.swriter.WriteLineAsync($"{username}: {line}");
                            }
                            catch (IOException)
                            {
                                Console.WriteLine($"{username} Disconnected.");
                            }
                        }
                    }
                    Console.WriteLine($"{username} Disconnected.");
                }
                catch (IOException)
                {
                    Console.WriteLine($"{username} Disconnected abruptly.");
                }
                finally { writers.TryRemove(ID, out _); }
            }
           
        }
    }
}
