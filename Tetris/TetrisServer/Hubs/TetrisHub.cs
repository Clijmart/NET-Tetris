using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TetrisServer.Hubs
{
    public class TetrisHub : Hub
    {
        /// <summary>
        /// When a client disconnects remove them from the list and send a message to all other clients.
        /// </summary>
        /// <param name="exception">The exception that caused the client to disconnect.</param>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedUser.Ids.Remove(Context.ConnectionId);
            Clients.Others.SendAsync("OnLeave", Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// When a client connects add them to the list, send them their id, the seed and the amount of connected clients.
        /// Then request the status of all other clients.
        /// </summary>
        public override Task OnConnectedAsync()
        {
            System.Diagnostics.Debug.WriteLine("Received Connection: " + Context.ConnectionId);
            ConnectedUser.Ids.Add(Context.ConnectionId);
            Clients.Client(Context.ConnectionId).SendAsync("OnJoin", new object[] { Context.ConnectionId, ConnectedUser.HubSeed, ConnectedUser.Ids.Count });
            Clients.Others.SendAsync("RequestStatus", Context.ConnectionId);
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// Send the lobby status of the client to all other clients.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <param name="ready">Whether the client is ready.</param>
        public async Task SendStatus(string name, bool ready)
        {
            await Clients.Others.SendAsync("ReceiveStatus", new object[] { Context.ConnectionId, name, ready });
        }

        /// <summary>
        /// Sends a single client to a lobby status.
        /// </summary>
        /// <param name="connectionId">The client to send the status to.</param>
        /// <param name="name">The name of the client.</param>
        /// <param name="ready">Whether the client is ready.</param>
        public async Task SendRequestedStatus(string connectionId, string name, bool ready)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveStatus", new object[] { Context.ConnectionId, name, ready });
        }

        /// <summary>
        /// Sends the game info of the client to all other clients.
        /// </summary>
        /// <param name="tetrisWell">The tetris well of the client.</param>
        /// <param name="score">The score of the client.</param>
        public async Task SendGameInfo(object[] tetrisWell, long score)
        {
            await Clients.Others.SendAsync("ReceiveGameInfo", new object[] { Context.ConnectionId, tetrisWell, score });
        }
    }

    public static class ConnectedUser
    {
        // A list of all connected clients.
        public static List<string> Ids = new();

        // The generated seed to send to the clients.
        public static int HubSeed = new Random().Next();
    }
}
