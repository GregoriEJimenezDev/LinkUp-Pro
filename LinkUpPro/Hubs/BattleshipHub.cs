using Microsoft.AspNetCore.SignalR;

namespace LinkUpPro.Hubs
{
    public class BattleshipHub : Hub
    {
        public async Task JoinGameGroup(string gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        }

        public async Task LeaveGameGroup(string gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
        }
    }
}
