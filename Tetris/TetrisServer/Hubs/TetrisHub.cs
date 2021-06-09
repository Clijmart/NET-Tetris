using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TetrisServer.Hubs
{
    public class TetrisHub : Hub
    {
        public async Task UpdateWell(object TetrisWell)
        {
            await Clients.Others.SendAsync("UpdateWell", TetrisWell);
        }
        
        public async Task ReadyUp(int seed)
        {
            await Clients.Others.SendAsync("ReadyUp", seed);
        }
    }
}
