using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class Employee
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("gen")]
        public string Gender { get; set; }
        [BsonElement("address")]
        public string Address { get; set; }
        [BsonElement("phone")]
        public string Phone { get; set; }
        [BsonElement("pos")]
        public string Position { get; set; }
        [BsonElement("super")]
        public string SupervisorId { get; set; }

    }
}
