
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public abstract class Plant
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("family")]
        public string Family { get; set; }
        [BsonElement("date")]
        public DateTime RegisDate { get; set; }
        [BsonElement("empl")]
        public string EmployeeId { get; set; }
        [BsonElement("status")]
        public string Status { get; set; }
        [BsonElement("location")]
        public Location Location { get; set; }
        [BsonElement("type")]
        public string Type { get; set; }
        [BsonElement("unit")]
        public string ManagedUnitId { get; set; }
        [BsonElement("user")]
        public string UserId { get; set; }
    }
}
