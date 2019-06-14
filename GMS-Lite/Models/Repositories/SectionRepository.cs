using GMS.Models.Entities.Collections;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GMS.Models.Repositories
{
    public class SectionRepository : IRepository
    {
        private IMongoCollection<SectionData> sectionCollection;

        public SectionRepository()
        {
            sectionCollection = db.GetCollection<SectionData>("section");
        }

        //Find

        public async Task<SectionData> Find(string id)
        {
            IAsyncCursor<SectionData> cursor =  await sectionCollection
                .FindAsync(Builders<SectionData>.Filter.Eq("_id", id));          
            return await cursor.SingleOrDefaultAsync();
        }

        //Insert
        public async Task<SectionData> Create(SectionData section)
        {
            await sectionCollection.InsertOneAsync(section);
            return section;
        }
        //Update
        public async Task<Boolean> Update(SectionData section)
        {
            UpdateResult result = await sectionCollection.UpdateOneAsync(
                Builders<SectionData>.Filter.Eq("_id", section.id),
                Builders<SectionData>.Update
                .Set("trees", section.TreeIds)
                .Set("glass", section.glassIds)
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
            DeleteResult result = await sectionCollection
                .DeleteOneAsync(Builders<SectionData>.Filter.Eq("_id", id));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor <SectionData> cursor = await sectionCollection.FindAsync(Builders<SectionData>.Filter.Eq("_id", id));
            return await cursor.AnyAsync();
        }

        //service
        public async Task<List<string>> GetAllTrees(string id)
        {
            IAsyncCursor<SectionData> cursor = await sectionCollection.FindAsync(Builders<SectionData>.Filter.Eq("_id", id));
            var section = await cursor.SingleOrDefaultAsync();
            return section.TreeIds;
        }

        public async Task<List<string>> GetAllGlass(string id)
        {
            IAsyncCursor<SectionData> cursor = await sectionCollection.FindAsync(Builders<SectionData>.Filter.Eq("_id", id));
            var section = await cursor.SingleOrDefaultAsync();
            return section.glassIds;
        }

        public async Task<Boolean> AddTreeToSection(string sectionId, string treeId)
        {
            var section = await Find(sectionId);
            if (section == null)
                return false;
            section.TreeIds.Add(treeId);
            return await Update(section);

        }

        public async Task<Boolean> AddGlassToSection(string sectionId, string glassId)
        {
            var section = await Find(sectionId);
            if (section == null)
                return false;
            section.glassIds.Add(glassId);
            return await Update(section);

        }

        public async Task<Boolean> RemoveTreeOutOfSection(string sectionId, string treeId)
        {
            var section = await Find(sectionId);
            if (section == null)
                return false;
            section.TreeIds.Remove(treeId);
            return await Update(section);
        }

        public async Task<Boolean> RemoveGlassOutOfSection(string sectionId, string glassId)
        {
            var section = await Find(sectionId);
            if (section == null)
                return false;
            section.glassIds.Remove(glassId);
            return await Update(section);
        }
    }
}
