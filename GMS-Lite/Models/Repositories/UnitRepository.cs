using GMS.Models.Entities.Collections;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Repositories
{
    public class UnitRepository : IRepository
    {
        private IMongoCollection<ManagedUnit> unitCollection;

        public UnitRepository()
        {
            unitCollection = db.GetCollection<ManagedUnit>("unit");
        }

        public async Task<List<ManagedUnit>> FindAll()
        {
            return await unitCollection.AsQueryable().ToListAsync();
        }

        public async Task<ManagedUnit> Find(string id)
        {
            IAsyncCursor<ManagedUnit> cursor = await unitCollection
            .FindAsync(Builders<ManagedUnit>.Filter.Eq("_id", id));
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<List<ManagedUnit>> Find(string attr, string options)
        {
            IAsyncCursor<ManagedUnit> cursor;
            cursor = await unitCollection
            .FindAsync(Builders<ManagedUnit>.Filter.Eq(options, attr));
            return await cursor.ToListAsync();
        }

        //Create
        public async Task<ManagedUnit> Create(ManagedUnit unit)
        {
            await unitCollection.InsertOneAsync(unit);
            return unit;
        }

        //Update
        public async Task<Boolean> Update(ManagedUnit unit)
        {
            UpdateResult result = await unitCollection.UpdateOneAsync(
                 Builders<ManagedUnit>.Filter.Eq("_id", unit.Id),
                 Builders<ManagedUnit>.Update
                 .Set("name", unit.Name)
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
            DeleteResult result = await unitCollection
                 .DeleteOneAsync(Builders<ManagedUnit>.Filter.Eq("_id", id));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<ManagedUnit> cursor = await unitCollection
            .FindAsync(Builders<ManagedUnit>.Filter.Eq("_id", id));
            return await cursor.AnyAsync();
        }
    }
}
