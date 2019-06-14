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
    public class PlantRepository : IRepository
    {
        private IMongoCollection<Tree> treeCollection;
        private IMongoCollection<Glass> glassCollection;

        public PlantRepository()
        {
            treeCollection = db.GetCollection<Tree>("tree");
            glassCollection = db.GetCollection<Glass>("glass");
        }

        //Find
        public async Task<List<Plant>> FindAll()
        {
            var trees = await treeCollection.AsQueryable().ToListAsync();
            var glass = await glassCollection.AsQueryable().ToListAsync();
            List<Plant> list = new List<Plant>();
            list.AddRange(trees);
            list.AddRange(glass);
            return list;
        }

        public async Task<Plant> Find(string id)
        {
                IAsyncCursor<Tree> treeCursor = await treeCollection
                .FindAsync(Builders<Tree>.Filter.Eq("_id", ObjectId.Parse(id)));             
                 var tree = await treeCursor.SingleOrDefaultAsync();
            if (tree != null)
                return tree;
                IAsyncCursor<Glass> glassCursor = await glassCollection
                .FindAsync(Builders<Glass>.Filter.Eq("_id", ObjectId.Parse(id)));
                return await glassCursor.SingleOrDefaultAsync();
        }

        public async Task<List<Plant>> Find(string attr, string options)
        {
                List<Plant> list = new List<Plant>();
            IAsyncCursor<Tree> treeCursor = await treeCollection
                .FindAsync(Builders<Tree>.Filter.Eq(options, attr));
                var list1 = await treeCursor.ToListAsync();
            IAsyncCursor<Glass> glassCursor = await glassCollection
                .FindAsync(Builders<Glass>.Filter.Eq(options, attr));
                var list2 = await glassCursor.ToListAsync();
                list.AddRange(list1);
                list.AddRange(list2);
                return list;
        }

        //Delete
        public async Task<Boolean> Delete(string id)
        {
            DeleteResult result = await treeCollection.DeleteOneAsync(Builders<Tree>.Filter.Eq("_id", ObjectId.Parse(id)));
            if (result.IsAcknowledged)
            {
                if (result.DeletedCount > 0)
                    return true;
            }
            result = await glassCollection.DeleteOneAsync(Builders<Glass>.Filter.Eq("_id", ObjectId.Parse(id)));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<Tree> treeCursor = await treeCollection
            .FindAsync(Builders<Tree>.Filter.Eq("_id", ObjectId.Parse(id)));
            if (await treeCursor.AnyAsync())
                return true;
            IAsyncCursor<Glass> glassCursor = await glassCollection
            .FindAsync(Builders<Glass>.Filter.Eq("_id", ObjectId.Parse(id)));
            return await glassCursor.AnyAsync();
        }
        
    }
}
