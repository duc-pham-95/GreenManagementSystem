using GMS.Models.Entities.Collections;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMS.Models.Repositories
{
    public class UserPlantingRepository : IRepository
    {
        private IMongoCollection<UserPlanting> userPlantingCollection;

        public UserPlantingRepository()
        {
            userPlantingCollection = db.GetCollection<UserPlanting>("userPlants");
        }

        public async Task<List<UserPlanting>> FindAll()
        {
            return await userPlantingCollection.AsQueryable().ToListAsync();
        }

        public async Task<UserPlanting> Find(string id)
        {
            IAsyncCursor<UserPlanting> cursor = await userPlantingCollection
            .FindAsync(Builders<UserPlanting>.Filter.Eq("_id", id));
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<List<UserPlanting>> Find(string attr, string options)
        {
            IAsyncCursor<UserPlanting> cursor;
            cursor = await userPlantingCollection
            .FindAsync(Builders<UserPlanting>.Filter.Eq(options, attr));
            return await cursor.ToListAsync();
        }

        //Create
        public async Task<UserPlanting> Create(UserPlanting userPlating)
        {
            await userPlantingCollection.InsertOneAsync(userPlating);
            return userPlating;
        }

        //Update
        public async Task<Boolean> Update(UserPlanting userPlating)
        {
            UpdateResult result = await userPlantingCollection.UpdateOneAsync(
                 Builders<UserPlanting>.Filter.Eq("_id", userPlating.userId),
                 Builders<UserPlanting>.Update
                 .Set("plants", userPlating.plants)
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
            DeleteResult result = await userPlantingCollection
                 .DeleteOneAsync(Builders<UserPlanting>.Filter.Eq("_id", id));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<UserPlanting> cursor = await userPlantingCollection
            .FindAsync(Builders<UserPlanting>.Filter.Eq("_id", id));
            return await cursor.AnyAsync();
        }
    }
}
