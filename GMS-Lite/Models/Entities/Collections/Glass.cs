using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class Glass : Plant
    {
       
        [BsonElement("surface")]
        public double SurfaceArea { get; set; }
        [BsonElement("poly")]
        public Polygon Polygon { get; set; }
    }
}
