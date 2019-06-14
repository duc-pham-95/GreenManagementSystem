using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMS.Models.Entities.Collections
{
    public class Location
    {
        public Location(string provinceId, string districtId, string wardId, string unitId, string sectionId)
        {
            ProvinceId = provinceId;
            DistrictId = districtId;
            WardId = wardId;
            UnitId = unitId;
            SectionId = sectionId;
        }

        [BsonElement("pro")]
        public string ProvinceId { get; set; }
        [BsonElement("dis")]
        public string DistrictId { get; set; }
        [BsonElement("ward")]
        public string WardId { get; set; }
        [BsonElement("unit")]
        public string UnitId { get; set; }
        [BsonElement("sec")]
        public string SectionId { get; set; }

    }
}
