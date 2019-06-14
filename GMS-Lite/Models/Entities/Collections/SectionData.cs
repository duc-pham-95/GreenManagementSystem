using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class SectionData
    {
        public SectionData(string id)
        {
            this.id = id;
            TreeIds = new List<string>();
            this.glassIds = new List<string>();
        }

        [BsonId]
        public string id { get; set; }
        [BsonElement("trees")]
        public List<string> TreeIds { get; set;}
        [BsonElement("glass")]
        public List<string> glassIds { get; set; }
    }
}
