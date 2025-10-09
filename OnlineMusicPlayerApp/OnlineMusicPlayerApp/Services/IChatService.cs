using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace OnlineMusicPlayerApp.Services
{
    public interface IChatService
    {
        Task<HubConnectionState> Connect();
        Task Disconnect();
        void ReceiveMessage(Action<string, string> GetMessageAndUser);
        Task SendMessage(string userId, string message);
        Task SendMessage(string message);
    }
}