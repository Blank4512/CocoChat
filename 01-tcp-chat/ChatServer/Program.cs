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

            using var cts = new CancellationTokenSource();

            _ = Task.Run(() => { Console.ReadKey(); cts.Cancel(); });

            var listener = new TcpListener(IPAddress.Loopback, 5000);
            listener.Start();
            Console.WriteLine("Server Started. Press any key to stop.");
            
            int nextID = 0;
            try
            {
                while (true)
                {
                    TcpClient tcp = await listener.AcceptTcpClientAsync(cts.Token);
                    _ = HandleClientAsync(tcp, nextID);
                    Interlocked.Increment(ref nextID);
                }
            }
            catch (OperationCanceledException){
            }

            await BroadcastAsync("/shutdown");
            listener.Stop();

        }
        static async Task HandleClientAsync(TcpClient tcp, int ID)
        {
            using (tcp)
            {
                
                using NetworkStream stream = tcp.GetStream();
                using var reader = new StreamReader(stream);
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                string username = await reader.ReadLineAsync() ?? "Default";
                _ = BroadcastAsync($"{username} Connected.");
                writers.TryAdd(ID,(writer,username));
                try
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        //Console.WriteLine($"From: {username}: {line}");
                        if (line[0] == '/')
                        {
                            var cmd = line.Split(' ', 3);
                            string? message;
                            switch (cmd[0]) {
                                case "/list":
                                    message = "Connected Users: \n";
                                    foreach(var u in writers)
                                    {
                                        message += $"{u.Value.name}#{u.Key} \n";
                                    }
                                    _ = writer.WriteLineAsync(message);
                                    break;
                                case "/u":
                                    int.TryParse(cmd[1].Split('#')[1], out int res);
                                    _ = BroadcastAsync(cmd[2], res, username);
                                    break;
                                case "/help":
                                    message = "Available Commands: \n /list\n/u username#id message\n/quit";
                                    _ = writer.WriteLineAsync(message);
                                    break;
                                case "/quit":
                                    tcp.Client.Shutdown(SocketShutdown.Send);
                                    break;
                            }
                        }
                        else
                        {
                            _ = BroadcastAsync($"{username}:{line}");
                        }
                    }
                    _ = BroadcastAsync($"{username} Disconnected.");
                }
                catch (IOException)
                {
                    Console.WriteLine($"{username} Disconnected abruptly.");
                }
                finally { writers.TryRemove(ID, out _); }
            }
           
        }

        static async Task BroadcastAsync(string line, int id = 0, string user ="")
        {
            if(id == 0)
            {
                foreach (var kvp in writers)
                {
                    try
                    {
                        await kvp.Value.swriter.WriteLineAsync($"{line}");
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"{kvp.Value.name} Disconnected.");
                    }
                }
            }
            else
            {
                _ = writers[id].swriter.WriteLineAsync($"[Private Message]: {user} -> {line}");
            }

        }
    }
}
