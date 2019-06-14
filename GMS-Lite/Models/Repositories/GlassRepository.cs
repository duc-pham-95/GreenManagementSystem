using GMS.Models.Entities.Collections;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMS.Models.Repositories
{
    public class GlassRepository : IRepository
    {
        private IMongoCollection<Glass> glassCollection;

        public GlassRepository()
        {
            glassCollection = db.GetCollection<Glass>("glass");
        }
        //Find
        public async Task<List<Glass>> FindAll()
        {
            return await glassCollection.AsQueryable<Glass>().ToListAsync();
        }

        public async Task<Glass> Find(string id)
        {
            IAsyncCursor<Glass> cursor = await glassCollection
            .FindAsync(Builders<Glass>.Filter.Eq("_id", ObjectId.Parse(id)));
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<List<Glass>> Find(string attr, string options)
        {
            IAsyncCursor<Glass> cursor;
                cursor = await glassCollection
                .FindAsync(Builders<Glass>.Filter.Eq(options, attr));
                return await cursor.ToListAsync();
        }


        //Create
        public async Task<Glass> Create(Glass glass)
        {
           await glassCollection.InsertOneAsync(glass);
           return glass;
        }

        //Update
        public async Task<Boolean> Update(Glass glass)
        {
            UpdateResult result = await glassCollection.UpdateOneAsync(
                Builders<Glass>.Filter.Eq("_id", ObjectId.Parse(glass.Id)),
                Builders<Glass>.Update
                .Set("name", glass.Name)
                .Set("family", glass.Family)
                .Set("date", glass.RegisDate)
                .Set("status", glass.Status)
                .Set("poly", glass.Polygon)
                .Set("surface", glass.SurfaceArea)
                .Set("empl", glass.EmployeeId)
                .Set("location", glass.Location)
                .Set("type", glass.Type)
                .Set("unit", glass.ManagedUnitId)
                .Set("user", glass.UserId)
                );
            if (result.IsAcknowledged)
            {
                return result.MatchedCount > 0 && result.ModifiedCount > 0;
            }
            return false;
        }

        //Delete
        public async Task<Boolean> Delete(string id)
        {
           DeleteResult result = await glassCollection.DeleteOneAsync(Builders<Glass>.Filter.Eq("_id", ObjectId.Parse(id)));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<Glass> cursor = await glassCollection
            .FindAsync(Builders<Glass>.Filter.Eq("_id", ObjectId.Parse(id)));
            return await cursor.AnyAsync();
        }
    }
}
