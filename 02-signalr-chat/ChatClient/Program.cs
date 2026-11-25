using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/chat")
    .WithAutomaticReconnect()
    .Build();

connection.On<string, string>("ReceiveMessage", (user,text) =>
    Console.WriteLine($"{user}: {text}"));

await connection.StartAsync();