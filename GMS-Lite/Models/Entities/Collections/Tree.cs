using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class Tree : Plant
    {
        
        [BsonElement("height")]
        public double Height { get; set; }
        [BsonElement("width")]
        public double Width { get; set; }
        [BsonElement("coord")]
        public Coordinate Coord { get; set; }   
    }
}
