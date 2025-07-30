using Microsoft.AspNetCore.Mvc;
using ChatAppApi.Models;
using ChatAppApi.Data;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using ChatAppApi.Hubs;

namespace ChatAppApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(MongoDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            var senderClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (senderClaim == null || string.IsNullOrEmpty(senderClaim.Value))
            {
                return Unauthorized("Sender identifier not found.");
            }
            message.SenderId = senderClaim.Value;
            message.Timestamp = DateTime.UtcNow;
            await _context.Messages.InsertOneAsync(message);
            await _hubContext.Clients.Group(message.ConversationId).SendAsync("ReceiveMessage", message.SenderId, message.Content);
            return Ok();
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var conversations = await _context.Conversations.Find(c => c.ParticipantIds.Contains(userId)).ToListAsync();

            var conversationResponses = new List<ConversationResponse>();
            foreach (var conv in conversations)
            {
                var participantUsernames = new string[2];
                for (int i = 0; i < conv.ParticipantIds.Length; i++)
                {
                    var user = await _context.Users.Find(u => u.Id == conv.ParticipantIds[i]).FirstOrDefaultAsync();
                    participantUsernames[i] = user?.Username ?? "Unknown";
                }

                conversationResponses.Add(new ConversationResponse
                {
                    Id = conv.Id,
                    ParticipantIds = conv.ParticipantIds,
                    ParticipantUsernames = participantUsernames
                });
            }

            return Ok(conversationResponses);
        }

        [HttpGet("messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(string conversationId)
        {
            var messages = await _context.Messages.Find(m => m.ConversationId == conversationId).ToListAsync();
            foreach ( var m in messages) {
                var user = await _context.Users.Find(u => u.Id == m.SenderId).FirstOrDefaultAsync();
                m.SenderName = user.Username;
            }
            return Ok(messages);
        }

        [HttpPost("conversation")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var conversation = new Conversation
            {
                ParticipantIds = new[] { userId!, request.OtherUserId! }
            };
            await _context.Conversations.InsertOneAsync(conversation);
            return Ok(conversation);
        }
    }

    public class CreateConversationRequest
    {
        public string? OtherUserId { get; set; }
    }
}