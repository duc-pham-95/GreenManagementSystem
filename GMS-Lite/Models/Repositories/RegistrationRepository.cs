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
    public class RegistrationRepository : IRepository
    {
        private IMongoCollection<Registration> regisCollection;

        public RegistrationRepository()
        {
            regisCollection = db.GetCollection<Registration>("registration");
        }

        //Find
        public async Task<List<Registration>> FindAll()
        {
            return await regisCollection.AsQueryable<Registration>().ToListAsync();
        }

        public async Task<List<Registration>> Find(string userId)
        {
            IAsyncCursor<Registration> cursor = await regisCollection
                .FindAsync(Builders<Registration>.Filter.Eq("user", userId));
            return await cursor.ToListAsync();
        }

        //Insert
        public async Task<Registration> Create(Registration regis)
        {
           await regisCollection.InsertOneAsync(regis);
           return regis;

        }

        //Delete
        public async Task<Boolean> Delete(string id)
        {
            DeleteResult result = await regisCollection
                .DeleteOneAsync(Builders<Registration>.Filter.Eq("_id", ObjectId.Parse(id)));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<Registration> cursor = await regisCollection
            .FindAsync(Builders<Registration>.Filter.Eq("_id", ObjectId.Parse(id)));
            return await cursor.AnyAsync();
        }
    }
}
