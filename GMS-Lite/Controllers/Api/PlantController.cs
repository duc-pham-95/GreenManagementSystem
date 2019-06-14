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
    public class PlantController : ControllerBase
    {
        PlantRepository repos = new PlantRepository();
        SectionRepository secRepos = new SectionRepository();
        AreaRepository areaRepos = new AreaRepository();

        // GET: api/Plant
        [HttpGet]
        public async Task<List<Plant>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/Plant/id=value
        [HttpGet("id={id}")]
        public async Task<Plant> Get(string id)
        {
            return await repos.Find(id);
        }

        // GET: api/Plant/name=value
        [HttpGet("name={name}")]
        public async Task<List<Plant>> GetByAttrName(string name)
        {
            return await repos.Find(name, "name");
        }

        // GET: api/Plant/family=value
        [HttpGet("family={family}")]
        public async Task<List<Plant>> GetByAttrFamily(string family)
        {
            return await repos.Find(family, "family");
        }

        // GET: api/Plant/status=value
        [HttpGet("status={status}")]
        public async Task<List<Plant>> GetbyAttrStatus(string status)
        {
            return await repos.Find(status, "status");
        }

        // GET: api/Plant/employee=value
        [HttpGet("employee={id}")]
        public async Task<List<Plant>> GetbyAttrEmployee(string id)
        {
            return await repos.Find(id, "empl");
        }

        // GET: api/Plant/managedunit=value
        [HttpGet("managedunit={id}")]
        public async Task<List<Plant>> GetbyAttrManagedUnit(string id)
        {
            return await repos.Find(id, "unit");
        }

        // GET: api/Plant/area=value
        [HttpGet("area={id}")]
        public async Task<List<Plant>> GetbyAttrArea(string id)
        {
            List<Plant> result = new List<Plant>();
            //check existence for area
            var area = await areaRepos.Find(id);
            if (area == null)
                return result;
            //get all plant's reference directly from section's storage if area is playing section role
            if (id.StartsWith("S"))
            {
                var section = await secRepos.Find(id);
                if(section != null)
                {
                    foreach (var treeId in section.TreeIds)
                    {
                        var tree = await repos.Find(treeId);
                        if (tree != null)
                        {
                            result.Add(tree);
                        }
                    }
                    foreach (var glassId in section.glassIds)
                    {
                        var glass = await repos.Find(glassId);
                        if (glass != null)
                            result.Add(glass);
                    }
                }
                return result;
            }
            //get all section's reference from area if area is not playing section role
            //then get all plant reference
            foreach (var secId in area.leavesIds)
            {
                var section = await secRepos.Find(secId);
                if(section != null)
                {
                    foreach (var treeId in section.TreeIds)
                    {
                        var tree = await repos.Find(treeId);
                        if (tree != null)
                        {
                            result.Add(tree);
                        }
                    }
                    foreach (var glassId in section.glassIds)
                    {
                        var glass = await repos.Find(glassId);
                        if (glass != null)
                            result.Add(glass);
                    }
                }
                
            }
            return result;
        }

        // DELETE: api/Plant/value
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            //check for valid tree object
            var objectId = new ObjectId();
            if (!ObjectId.TryParse(id, out objectId))
            {
                return BadRequest("Id is not valid");
            }
            //check existence for plant
            if (!await repos.isExisted(id))
                return NotFound(new { id, error = "There was no plant with an id of " + id + "." });
            var plant = await repos.Find(id);
            //check existence for section's storage
            if (!await secRepos.isExisted(plant.Location.SectionId))
            {
                return BadRequest("section does not exist");
            }
            //delete plant out of collection
            if (await repos.Delete(id))
            {
                //remove plant reference out of section's storage
                if (plant.Type == PlantType.Tree)
                {
                    if(!await secRepos.RemoveTreeOutOfSection(plant.Location.SectionId, plant.Id))
                        return BadRequest(new { section = plant.Location.SectionId, error = "cannot remove tree with id of " + id + "out of section" });
                }
                else{
                    if(!await secRepos.RemoveGlassOutOfSection(plant.Location.SectionId, plant.Id))
                        return BadRequest(new { section = plant.Location.SectionId, error = "cannot remove glass with id of " + id + "out of section" });
                }
                return Ok();
            }
            else
                return BadRequest("Failed");
        }
    }
}
