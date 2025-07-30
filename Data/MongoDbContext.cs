using MongoDB.Driver;
using ChatAppApi.Models; 
using Microsoft.Extensions.Configuration;

namespace ChatAppApi.Data { public class MongoDbContext {
        private readonly IMongoDatabase _database;
        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            _database = client.GetDatabase("ChatAppDB");
        }
        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Message> Messages => _database.GetCollection<Message>("Messages");
        public IMongoCollection<Conversation> Conversations => _database.GetCollection<Conversation>("Conversations");}
}
