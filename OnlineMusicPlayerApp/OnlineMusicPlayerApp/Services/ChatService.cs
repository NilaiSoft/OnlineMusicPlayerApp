

using OnlineMusicPlayerApp.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Plugin.Toast;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(ChatService))]
namespace OnlineMusicPlayerApp.Services
{
    public class ChatService : IChatService
    {
        private readonly HubConnection hubConnection;
        public ChatService()
        {
            //hubConnection = new HubConnectionBuilder().WithUrl("https://10.0.2.2:2777/chathub").Build();
            hubConnection = new HubConnectionBuilder()
            //.WithUrl($"https://Linqe.ir/movehub", (opts) =>
            .WithUrl($"http://192.168.43.14/movehub", (opts) =>
            {
                opts.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is System.Net.Http.HttpClientHandler clientHandler)
                        // bypass SSL certificate
                        clientHandler.ServerCertificateCustomValidationCallback +=
                            (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    return message;
                };
            }).Build();
        }

        public async Task<HubConnectionState> Connect()
        {
            if (hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    if (Utility.CheckConnection())
                        await hubConnection.StartAsync();
                }
                catch (System.Exception ex)
                {

                }
            }
            return hubConnection.State;
        }

        public async Task Disconnect()
        {
            await hubConnection.StopAsync();
        }

        public async Task SendMessage(string userId, string message)
        {
            await hubConnection.InvokeAsync("ReceiveNewPosition", userId, message);
        }
        public async Task SendMessage(string message)
        {
            if (await Connect() == HubConnectionState.Connected)
                await hubConnection.SendAsync("ReceiveNewPosition", message);
        }

        public void ReceiveMessage(Action<string, string> GetMessageAndUser)
        {
            hubConnection.On("ReceiveNewPosition", GetMessageAndUser);
        }
    }
}
