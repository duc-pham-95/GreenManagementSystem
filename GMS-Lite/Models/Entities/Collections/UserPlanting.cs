using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class UserPlanting
    {
        public UserPlanting(string userId)
        {
            this.userId = userId;
            this.plants = new List<string>();
        }

        [BsonId]
        public string userId { get; set; }
        [BsonElement("plants")]
        public List<string> plants { get; set; }

    }

}
