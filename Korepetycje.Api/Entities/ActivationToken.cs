using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Korepetycje.Api.Entities
{
    public class ActivationToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserId { get; set; }

        [BsonElement("tokenHash")]
        public required string TokenHash { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [BsonElement("usedAt")]
        public DateTime? UsedAt { get; set; }
    }
}
