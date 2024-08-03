using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace Live.Hubs
{
    public class GameHub : Hub
    {
        public async Task UpdateScore(int player1Score, int player2Score)
        {
            await Clients.All.SendAsync("ReceiveScoreUpdate", player1Score, player2Score);
        }

        public async Task FinalResult(int player1Score, int player2Score)
        {
            await Clients.All.SendAsync("ReceiveFinalResult", player1Score, player2Score);
        }
    }
}
