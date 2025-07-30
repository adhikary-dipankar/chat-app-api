using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAppApi.Models
{
    public class ConversationResponse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string[] ParticipantIds { get; set; } = new string[2];
        public string[] ParticipantUsernames { get; set; } = new string[2];
    }
}