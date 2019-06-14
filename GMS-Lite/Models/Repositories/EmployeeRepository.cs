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
    public class EmployeeRepository : IRepository
    {
        private IMongoCollection<Employee> empCollection;

        public EmployeeRepository()
        {
            empCollection = db.GetCollection<Employee>("employee");
        }

        public async Task<List<Employee>> FindAll()
        {
            return await empCollection.AsQueryable().ToListAsync();
        }

        public async Task<Employee> Find(string id)
        {
            IAsyncCursor<Employee> cursor = await empCollection
            .FindAsync(Builders<Employee>.Filter.Eq("_id", ObjectId.Parse(id)));
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<List<Employee>> Find(string attr, string options)
        {
                IAsyncCursor<Employee> cursor;
                cursor = await empCollection
                .FindAsync(Builders<Employee>.Filter.Eq(options, attr));
                return await cursor.ToListAsync();          
        }
      

        //Create
        public async Task<Employee> Create(Employee emp)
        {
            await empCollection.InsertOneAsync(emp);
            return emp;
        }

        //Update
        public async Task<Boolean> Update(Employee emp)
        {
           UpdateResult result = await empCollection.UpdateOneAsync(
                Builders<Employee>.Filter.Eq("_id", ObjectId.Parse(emp.Id)),
                Builders<Employee>.Update
                .Set("name", emp.Name)
                .Set("gen", emp.Gender)
                .Set("address", emp.Address)
                .Set("phone", emp.Phone)
                .Set("pos", emp.Position)
                .Set("super", emp.SupervisorId)
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
           DeleteResult result = await empCollection
                .DeleteOneAsync(Builders<Employee>.Filter.Eq("_id", ObjectId.Parse(id)));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<Employee> cursor = await empCollection
            .FindAsync(Builders<Employee>.Filter.Eq("_id", ObjectId.Parse(id)));
            return await cursor.AnyAsync();
        }
    }
}
