using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMS.Models.Entities.Collections;
using GMS.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace GMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreeController : ControllerBase
    {
        TreeRepository repos = new TreeRepository();
        SectionRepository secRepos = new SectionRepository();
        AreaRepository areaRepos = new AreaRepository();

        // GET: api/Tree
        [HttpGet]
        public async Task<List<Tree>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/Tree/id=value
        [HttpGet("id={id}")]
        public async Task<Tree> Get(string id)
        {
            return await repos.Find(id);
        }

        // GET: api/Tree/name=value
        [HttpGet("name={name}")]
        public async Task<List<Tree>> GetbyAttrName(string name)
        {
            return await repos.Find(name, "name");
        }

        // GET: api/Tree/family=value
        [HttpGet("family={family}")]
        public async Task<List<Tree>> GetbyAttrFamily(string family)
        {
            return await repos.Find(family, "family");
        }

        // GET: api/Tree/status=value
        [HttpGet("status={status}")]
        public async Task<List<Tree>> GetbyAttrStatus(string status)
        {
            return await repos.Find(status, "status");
        }

        // GET: api/Tree/employee=value
        [HttpGet("employee={id}")]
        public async Task<List<Tree>> GetbyAttrEmployee(string id)
        {
            return await repos.Find(id, "empl");
        }

        // GET: api/Tree/managedunit=value
        [HttpGet("managedunit={id}")]
        public async Task<List<Tree>> GetbyAttrManagedUnit(string id)
        {
            return await repos.Find(id, "unit");
        }

        // GET: api/Tree/area=value
        [HttpGet("area={id}")]
        public async Task<List<Tree>> GetbyAttrArea(string id)
        {
            List<Tree> result = new List<Tree>();
            //check existence for area
            var area = await areaRepos.Find(id);
            if (area == null)
                return result;
            //get all tree's reference directly from section's storage if area is playing section role
            if (id.StartsWith("S"))
            {
                var section = await secRepos.Find(id);
                if (section != null)
                {
                    foreach (var treeId in section.TreeIds)
                    {
                        var tree = await repos.Find(treeId);
                        if (tree != null)
                        {
                            result.Add(tree);
                        }
                    }
                }
                return result;

            }
            //get all section's reference from area if area is not playing section role
            //then get all tree's reference
            foreach (var secId in area.leavesIds)
            {
                var section = await secRepos.Find(secId);
                if (section != null)
                {
                    foreach (var treeId in section.TreeIds)
                    {
                        var tree = await repos.Find(treeId);
                        if (tree != null)
                        {
                            result.Add(tree);
                        }
                    }
                }
            }
            return result;
        }

        // POST: api/Tree
        [Authorize(Roles = "Admin,Member")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]  Models.Entities.Collections.Tree tree)
        {
            //check for valid tree object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for valid given info
            if (
                tree.Type == null
                || tree.Coord == null
                || tree.Type != PlantType.Tree
                || tree.Name == null
                || tree.Location == null
                || tree.Location.SectionId == null)
            {
                return BadRequest("tree's info is invalid !.");
            }
            //check existence for section's storage
            if (!await secRepos.isExisted(tree.Location.SectionId))
            {
                return BadRequest("section does not exist !.");
            }
            //insert new tree to collection
            var result = await repos.Create(tree);
            //add tree's reference to section's storage
            if (!await secRepos.AddTreeToSection(result.Location.SectionId, result.Id))
            {
                await repos.Delete(result.Id);
                return BadRequest("cannot add tree with id = " + result.Id + "to section" );
            }
            return Ok(result.Id);
        }

        // DELETE: api/Tree/value
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            //check for valid id
            var objectId = new ObjectId();
            if (!ObjectId.TryParse(id, out objectId))
            {
                return BadRequest("Id is not valid !.");
            }
            //check existence for tree
            if (!await repos.isExisted(id))
                return NotFound(new { id, error = "There was no tree with an id of " + id + "." });
            var tree = await repos.Find(id);
            //check existence for section's storage
            if (!await secRepos.isExisted(tree.Location.SectionId))
            {
                return BadRequest("section does not exist !.");
            }
            //delete tree out of collection
            if (await repos.Delete(id))
            {
                //remove tree's reference out of section's storage
                if (!await secRepos.RemoveTreeOutOfSection(tree.Location.SectionId, tree.Id))
                {
                    return BadRequest(new { section = tree.Location.SectionId, error = "cannot remove tree with id of " + id + "out of section" });
                }
                return Ok();
            }
            else
                return BadRequest("Failed !.");
        }

        // PUT: api/Tree/
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Models.Entities.Collections.Tree newTree)
        {
            //check for valid tree object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for valid given info
            if (
                newTree.Type == null
                || newTree.Type != PlantType.Tree
                || newTree.Name == null
                || newTree.Location == null
                || newTree.Location.SectionId == null)
            {
                return BadRequest("tree's information is invalid !.");
            }
            //check for valid id
            var objectId = new ObjectId();
            if (!ObjectId.TryParse(newTree.Id, out objectId))
            {
                return BadRequest("Id is not valid !.");
            }
            //check existence for tree
            if (!await repos.isExisted(newTree.Id))
                return NotFound(new { newTree.Id, error = "There was no tree with an id of " + newTree.Id + "." });
            //check existence for section's storage
            if (!await secRepos.isExisted(newTree.Location.SectionId))
            {
                return BadRequest("section does not exist !.");
            }
            var oldTree = await repos.Find(newTree.Id);
            //check if location is changed
            if (oldTree.Location.SectionId != newTree.Location.SectionId)
            {
                //assign new location to tree if needed
                newTree.Location = await areaRepos.GetLocation(newTree.Location.SectionId);
            }
            //update tree
            if (await repos.Update(newTree))
            {
                //check if location is changed
                if (oldTree.Location.SectionId != newTree.Location.SectionId)
                {
                    //change section's storage
                    if (!await secRepos.RemoveTreeOutOfSection(oldTree.Location.SectionId, oldTree.Id))
                    {
                        return BadRequest(new { section = oldTree.Location.SectionId, error = "cannot remove tree with id of " + oldTree.Id + "out of section" });
                    }
                    if (!await secRepos.AddTreeToSection(newTree.Location.SectionId, newTree.Id))
                    {
                        return BadRequest(new { section = newTree.Location.SectionId, error = "cannot add tree with id of " + newTree.Id + "to new section" });
                    }
                }
                return Ok();
            }
            else
                return BadRequest("Failed");
        }
    }
}
