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
    public class AreaRepository : IRepository
    {
        private IMongoCollection<Area> AreaCollection;
        private readonly Dictionary<string, int> AreaLevels;

        public AreaRepository()
        {
            AreaCollection = db.GetCollection<Area>("area");
            AreaLevels = new Dictionary<string, int>();
            AreaLevels.Add("S", 0);
            AreaLevels.Add("U", 1);
            AreaLevels.Add("W", 2);
            AreaLevels.Add("D", 3);
            AreaLevels.Add("P", 4);
            AreaLevels.Add("C", 5);
        }

        //Find
        public async Task<List<Area>> FindAll()
        {
            return await AreaCollection.AsQueryable<Area>().ToListAsync();
        }

        public async Task<Area> Find(string id)
        {
            IAsyncCursor<Area> cursor = await AreaCollection
                .FindAsync(Builders<Area>.Filter.Eq("_id", id));
            return await cursor.SingleOrDefaultAsync();
        }

        public async Task<List<Area>> FindByAttr(string attr, string options)
        {
            IAsyncCursor<Area> cursor = await AreaCollection
                .FindAsync(Builders<Area>.Filter.Eq(options, attr));
            return await cursor.ToListAsync();
        }

        public async Task<List<Area>> FindByIdPrefix(string prefix)
        {
            IAsyncCursor<Area> cursor = await AreaCollection
                .FindAsync(Builders<Area>.Filter.Where(a => a.Id.StartsWith(prefix)));
            return await cursor.ToListAsync();
        }

        //Create
        public async Task<Area> Create(Area Area)
        {
            await AreaCollection.InsertOneAsync(Area);
            return Area;
        } 

        //Update
        public async Task<Boolean> Update(Area Area)
        {
            if (Area == null)
                return false;
            UpdateResult result = await AreaCollection.UpdateOneAsync(
                Builders<Area>.Filter.Eq("_id", Area.Id),
                Builders<Area>.Update
                .Set("name", Area.Name)
                .Set("poly", Area.Polygon)
                .Set("center", Area.CenterCoord)
                .Set("farthest", Area.FarthestDistance)
                .Set("parent",Area.ParentAreaId)
                .Set("childs", Area.ChildAreaIds)
                .Set("leaves", Area.leavesIds)
                .Set("unit",Area.ManagedUnitId)
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
           DeleteResult result = await AreaCollection
                .DeleteOneAsync(Builders<Area>.Filter.Eq("_id", id));
            if (result.IsAcknowledged)
            {
                return result.DeletedCount > 0;
            }
            return false;
        }

        //Check
        public async Task<Boolean> isExisted(string id)
        {
            IAsyncCursor<Area> cursor = await AreaCollection
                .FindAsync(Builders<Area>.Filter.Eq("_id", id));
            return await cursor.AnyAsync();
        }

        //Service
        public async Task<Location> GetLocation(string id)
        {
            if (id == null)
                return null;
            var section = await Find(id);
            if (section == null)
                return null;
            var unit = await Find(section?.ParentAreaId);
            var ward = await Find(unit?.ParentAreaId);
            var district = await Find(ward?.ParentAreaId);
            var province = await Find(district?.ParentAreaId);
            return new Location(province?.Id, district?.Id, ward?.Id, unit?.Id, section?.Id);
        }

        public async Task<List<Area>> GetAllChildNodes(string parent)
        {
            var Area = await Find(parent);
            if (Area == null || Area.ChildAreaIds.Count == 0)
                return null;
            List<Area> result = new List<Area>();
            foreach(var id in Area.ChildAreaIds)
            {
                var child = await Find(id);
                if(child != null)
                {
                    result.Add(child);
                }              
            }
            return result;
        }

        public async Task<List<Area>> GetAllBaseNode(string parent)
        {
            var Area = await Find(parent);
            if (Area == null || Area.leavesIds.Count == 0)
                return null;
            List<Area> result = new List<Area>();
            foreach (var id in Area.leavesIds)
            {
                var leaf = await Find(id);
                if (leaf != null)
                {
                    result.Add(leaf);
                }
            }
            return result;
        }

        public async Task AddNewNode(Area node)
        {
            await Create(node);
            await AddRelationship(node.ParentAreaId, node.Id);
        }

        public async Task<Boolean> AddRelationship(string parent, string child)
        {
            if(parent != null)
            {
                var Area = await Find(parent);
                if (Area != null)
                {
                    Area.ChildAreaIds.Add(child);
                    return await Update(Area);
                }
                return false;
            }
            return true;
        }

        public async Task<Boolean> RemoveRelationship(string parent, string child)
        {
            if (parent != null)
            {
                var Area = await Find(parent);
                if (Area != null)
                {
                    Area.ChildAreaIds.Remove(child);
                    return await Update(Area);
                }
                return false;
            }
            return true;
        }

        public async Task AddLeafNodeReferenceRecursively(string current, string leaf)
        {
            var Area = await Find(current);
            if(Area != null)
            {
                Area.leavesIds.Add(leaf);
                await Update(Area);
                await AddLeafNodeReferenceRecursively(Area.ParentAreaId, leaf);
            }
            
        }

        public async Task AddLeafNodeReferenceRecursively(string current, List<string> leaves)
        {
            var Area = await Find(current);
            if (Area != null)
            {
                Area.leavesIds.AddRange(leaves);
                await Update(Area);
                await AddLeafNodeReferenceRecursively(Area.ParentAreaId, leaves);
            }

        }

        public async Task RemoveLeafNodeReferenceRecursively(string current, string leaf)
        {
            var Area = await Find(current);
            if (Area != null)
            {
                Area.leavesIds.Remove(leaf);
                await Update(Area);
                await RemoveLeafNodeReferenceRecursively(Area.ParentAreaId, leaf);
            }

        }

        public async Task RemoveLeafNodeReferenceRecursively(string current, List<string> leaves)
        {
            var Area = await Find(current);
            if (Area != null)
            {
                foreach(var id in leaves)
                {
                    Area.leavesIds.Remove(id);
                }
                await Update(Area);
                await RemoveLeafNodeReferenceRecursively(Area.ParentAreaId, leaves);
            }

        }

        public bool IsRelationshipLevelsCorrect(string child, string parent)
        {
            if (AreaLevels[child] < AreaLevels[parent])
                return true;
            return false;
        }
    }
}
