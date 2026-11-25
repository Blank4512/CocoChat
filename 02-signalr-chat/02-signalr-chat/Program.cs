using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<ChatHub>("/chat");
app.Run();

public class ChatHub() : Hub
{
    public async Task SendMessage(string user, string text)
    => await Clients.All.SendAsync("ReceiveMessage",user, text);
}
