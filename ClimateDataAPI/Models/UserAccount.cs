using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClimateDataAPI.Models
{
    public class UserAccount
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        [BsonElement("Last Access")]
        public DateTime LastAccess  { get; set; }
        public string ApiKey { get; set; }
    }
}
