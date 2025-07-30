using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChatAppApi.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string conversationId, string senderId, string content)
        {
            await Clients.Group(conversationId).SendAsync("ReceiveMessage", senderId, content);
        }

        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }
    }
}