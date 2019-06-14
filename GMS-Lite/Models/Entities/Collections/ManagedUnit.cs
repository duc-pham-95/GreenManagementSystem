using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class ManagedUnit
    {
        [BsonId]
        public string Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }       
    }
}
