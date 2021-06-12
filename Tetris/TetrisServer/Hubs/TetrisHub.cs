using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TetrisServer.Hubs
{
    public class TetrisHub : Hub
    {
        public async Task Join(Guid PlayerID)
        {
            await Clients.Others.SendAsync("Join", PlayerID);
        }

        public async Task SendStatus(object[] Message)
        {
            await Clients.Others.SendAsync("SendStatus", Message);
        }

        public async Task UpdatePlayer(object[] Message)
        {
            await Clients.Others.SendAsync("UpdatePlayer", Message);
        }
        
        public async Task StartGame()
        {
            await Clients.All.SendAsync("StartGame");
        }
        public async Task EndGame()
        {
            await Clients.Others.SendAsync("EndGame");
        }
    }
}
