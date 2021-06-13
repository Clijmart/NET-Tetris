using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TetrisServer.Hubs
{
    public class TetrisHub : Hub
    {
        public override Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedUser.Ids.Remove(Context.ConnectionId);
            Clients.Others.SendAsync("OnLeave", Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public override Task OnConnectedAsync()
        {
            System.Diagnostics.Debug.WriteLine("Received Connection: " + Context.ConnectionId);
            ConnectedUser.Ids.Add(Context.ConnectionId);
            Clients.Client(Context.ConnectionId).SendAsync("OnJoin", new object[] { Context.ConnectionId, ConnectedUser.HubSeed, ConnectedUser.Ids.Count });
            Clients.Others.SendAsync("RequestStatus", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        public async Task SendStatus(string name, bool ready)
        {
            await Clients.Others.SendAsync("ReceiveStatus", new object[] { Context.ConnectionId, name, ready });
        }

        public async Task SendRequestedStatus(string connectionId, string name, bool ready)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveStatus", new object[] { Context.ConnectionId, name, ready });
        }

        public async Task SendGameInfo(object[] tetrisWell, long score)
        {
            await Clients.Others.SendAsync("ReceiveGameInfo", new object[] { Context.ConnectionId, tetrisWell, score });
        }
    }

    public static class ConnectedUser
    {
        public static List<string> Ids = new();
        public static int HubSeed = new Random().Next();
    }
}
