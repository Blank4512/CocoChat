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
                writers.TryAdd(ID,(writer,username));
                await BroadcastAsync($"{username} Connected.");
                try
                {
                    string? line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        //Console.WriteLine($"From: {username}: {line}");
                        if (line.StartsWith('/'))
                        {
                            var cmd = line.Split(' ', 3);
                            string? message;
                            switch (cmd[0]) {
                                case "/list":
                                    message = "Connected Users: \n";
                                    message += string.Join("\n", writers.Select(u => $"{u.Value.name}#{u.Key}"));
                                    foreach (var u in writers)
                                    {
                                        message += $"{u.Value.name}#{u.Key} \n";
                                    }
                                    await writer.WriteLineAsync(message);
                                    break;
                                case "/u":
                                    if (cmd.Length < 3 || !TryParseTarget(cmd[1], out int targetId))
                                    {
                                        await writer.WriteLineAsync("Usage: /u Name#ID message");
                                        break;
                                    }
                                    if (!await BroadcastPrivateAsync(targetId, username, cmd[2]))
                                        await writer.WriteLineAsync("User not found.");
                                    break;
                                case "/help":
                                    message = "Available Commands: \n /list\n/u username#id message\n/quit";
                                    await writer.WriteLineAsync(message);
                                    break;
                                case "/quit":
                                    tcp.Client.Shutdown(SocketShutdown.Send);
                                    break;
                            }
                        }
                        else
                        {
                            await BroadcastAsync($"{username}:{line}");
                        }
                    }
                    await BroadcastAsync($"{username} Disconnected.");
                }
                catch (IOException)
                {
                    Console.WriteLine($"{username} Disconnected abruptly.");
                }
                finally { writers.TryRemove(ID, out _); }
            }
           
        }

        static async Task<bool> BroadcastPrivateAsync(int targetId, string from, string text)
        {
            if (!writers.TryGetValue(targetId, out var target)) return false;
            await target.swriter.WriteLineAsync($"[Private Message]: {from} -> {text}");
            return true;
        }
        static async Task BroadcastAsync(string line)
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
        static bool TryParseTarget(string text, out int id)
        {
            id = 0;
            int hash = text.LastIndexOf('#');
            return hash >= 0 && int.TryParse(text[(hash + 1)..], out id);
        }
    }
}
