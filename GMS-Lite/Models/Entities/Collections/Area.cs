using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class Area
    {
        public Area()
        {
            ChildAreaIds = new List<string>();
            this.leavesIds = new List<string>();
        }
        [BsonId]
        public string Id { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("unit")]
        public string ManagedUnitId { get; set; }
        [BsonElement("poly")]
        public Polygon Polygon { get; set; }
        [BsonElement("center")]
        public Coordinate CenterCoord { get; set; }
        [BsonElement("farthest")]
        public double FarthestDistance { get; set; }
        [BsonElement("parent")]
        public string ParentAreaId { get; set; }
        [BsonElement("childs")]
        public List<string> ChildAreaIds { get; set; }
        [BsonElement("leaves")]
        public List<string> leavesIds { get; set; }
    }
}
