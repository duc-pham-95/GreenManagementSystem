using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class Registration
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("user")]
        public string UserId { get; set; }
        [BsonElement("date")]
        public DateTime SubmitDate { get; set; }
        [BsonElement("tree")]
        public Tree RegisTree { get; set; }
        [BsonElement("glass")]
        public Glass RegisGlass { get; set; }
    }
}
