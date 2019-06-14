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
    public class TreeRepository : IRepository
    {
        private IMongoCollection<Tree> treeCollection;

        public TreeRepository()
        {
            treeCollection = db.GetCollection<Tree>("tree");
        }
        //Find
        public async Task<List<Tree>> FindAll()
        {
            return await treeCollection.AsQueryable<Tree>().ToListAsync();
        }

        public async Task<Tree> Find(string id)
        {
            IAsyncCursor<Tree> cursor = await treeCollection
            .FindAsync(Builders<Tree>.Filter.Eq("_id", ObjectId.Parse(id)));
            return await cursor.SingleOrDefaultAsync();
        }

        
        public async Task<List<Tree>> Find(string attr, string options)
        {
            IAsyncCursor<Tree> cursor;
                cursor = await treeCollection
                .FindAsync(Builders<Tree>.Filter.Eq(options, attr));
                return await cursor.ToListAsync();     
        }


        //Create
        public async Task<Tree> Create(Tree tree)
        {
            await treeCollection.InsertOneAsync(tree);
            return tree;
        }

        //Update
        public async Task<Boolean> Update(Tree tree)
        {
            UpdateResult result = await treeCollection.UpdateOneAsync(
                Builders<Tree>.Filter.Eq("_id", ObjectId.Parse(tree.Id)),
                Builders<Tree>.Update
                .Set("name", tree.Name)
                .Set("family", tree.Family)
                .Set("height", tree.Height)
                .Set("width", tree.Width)
                .Set("status", tree.Status)
                .Set("date", tree.RegisDate)
                .Set("coord", tree.Coord)
                .Set("empl", tree.EmployeeId)
                .Set("location", tree.Location)
                .Set("type", tree.Type)
                .Set("unit", tree.ManagedUnitId)
                .Set("user", tree.UserId)
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
            DeleteResult result = await treeCollection.DeleteOneAsync(Builders<Tree>.Filter.Eq("_id", ObjectId.Parse(id)));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<Tree> cursor = await treeCollection
            .FindAsync(Builders<Tree>.Filter.Eq("_id", ObjectId.Parse(id)));
            return await cursor.AnyAsync();
        }
    }
}
