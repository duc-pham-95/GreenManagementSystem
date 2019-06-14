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
    public class GlassController : ControllerBase
    {
        GlassRepository repos = new GlassRepository();
        SectionRepository secRepos = new SectionRepository();
        AreaRepository areaRepos = new AreaRepository();
        // GET: api/Glass
        [HttpGet]
        public async Task<List<Glass>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/Glass
        [HttpGet("id={id}")]
        public async Task<Glass> Get(string id)
        {
            return await repos.Find(id);
        }

        // GET: api/Glass/name=value
        [HttpGet("name={name}")]
        public async Task<List<Glass>> GetbyAttrName(string name)
        {
            return await repos.Find(name, "name");
        }

        // GET: api/Glass/family=value
        [HttpGet("family={family}")]
        public async Task<List<Glass>> GetbyAttrFamimly(string family)
        {
            return await repos.Find(family, "family");
        }

        // GET: api/Glass/status=value
        [HttpGet("status={status}")]
        public async Task<List<Glass>> GetbyAttrStatus(string status)
        {
            return await repos.Find(status, "status");
        }

        // GET: api/Glass/employee=value
        [HttpGet("employee={id}")]
        public async Task<List<Glass>> GetbyAttrEmployee(string id)
        {
            return await repos.Find(id, "empl");
        }

        // GET: api/Glass/managedunit=value
        [HttpGet("managedunit={id}")]
        public async Task<List<Glass>> GetbyAttrManagedUnit(string id)
        {
            return await repos.Find(id, "unit");
        }

        // GET: api/Glass/area=value
        [HttpGet("area={id}")]
        public async Task<List<Glass>> GetbyAttrArea(string id)
        {
            List<Glass> result = new List<Glass>();
            //check existence for area
            var area = await areaRepos.Find(id);
            if (area == null)
                return result;
            //get all glass' reference directly from section's storage if area is playing section role
            if (id.StartsWith("S"))
            {
                var section = await secRepos.Find(id);
                if(section != null)
                {
                    foreach (var glassId in section.glassIds)
                    {
                        var glass = await repos.Find(glassId);
                        if(glass != null)
                            result.Add(glass);
                    }
                }   
                return result;
            }
            //get all section's reference from area if area is not playing section role
            //then get all glass' reference
            foreach (var secId in area.leavesIds)
            {
                var section = await secRepos.Find(secId);
                if (section != null)
                {
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

        // POST: api/Glass
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]  Models.Entities.Collections.Glass glass)
        {
            //check for valid glass object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for valid given info
            if (
                glass.Polygon == null
                || glass.Type == null
                || glass.Type != PlantType.Glass
                || glass.Name == null
                || glass.Location == null
                || glass.Location.SectionId == null)
            {
                return BadRequest("glass' information is invalid !.");
            }
            //check existence for section's storage
            if (!await secRepos.isExisted(glass.Location.SectionId))
            {
                return BadRequest("section does not exist !.");
            }
            //insert new glass to collection
            var result = await repos.Create(glass);
            //add glass' reference to section's storage
            if (!await secRepos.AddGlassToSection(result.Location.SectionId, result.Id))
            {
                await repos.Delete(result.Id);
                return BadRequest(new { section = glass.Location.SectionId, error = "cannot add glass with id of " + result.Id + "to section" });
            }
            return Ok(result.Id);
        }

        // DELETE: api/Glass/value
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            //check for valid id
            var objectId = new ObjectId();
            if (!ObjectId.TryParse(id, out objectId))
            {
                return BadRequest("Id is not valid");
            }
            //check existence for glass
            if (!await repos.isExisted(id))
                return NotFound(new { id, error = "There was no glass with an id of " + id + "." });
            var glass = await repos.Find(id);
            //check existence for section's storage
            if (!await secRepos.isExisted(glass.Location.SectionId))
            {
                return BadRequest("section does not exist");
            }
            //delete glass out of collection
            if (await repos.Delete(id))
            {
                //remove glass' reference out of section's storage
                if (!await secRepos.RemoveGlassOutOfSection(glass.Location.SectionId, glass.Id))
                {
                    return BadRequest(new { section = glass.Location.SectionId, error = "cannot remove glass with id of " + id + "out of section" });
                }
                return Ok();
            }
            else
                return BadRequest("Failed");
        }

        // PUT: api/Glass/
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Models.Entities.Collections.Glass newGlass)
        {
            //check for valid glass object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for valid given info
            if (
                newGlass.Type == null
                || newGlass.Type != PlantType.Glass
                || newGlass.Name == null
                || newGlass.Location == null
                || newGlass.Location.SectionId == null)
            {
                return BadRequest("glass' information is invalid");
            }
            //check for valid id
            var objectId = new ObjectId();
            if (!ObjectId.TryParse(newGlass.Id, out objectId))
            {
                return BadRequest("Id is not valid");
            }
            //check existence for glass
            if (!await repos.isExisted(newGlass.Id))
                return NotFound(new { newGlass.Id, error = "There was no tree with an id of " + newGlass.Id + "." });
            //check existence for section's storage
            if (!await secRepos.isExisted(newGlass.Location.SectionId))
            {
                return BadRequest("section does not exist");
            }
            var oldGlass = await repos.Find(newGlass.Id);
            //check if location is changed
            if (oldGlass.Location.SectionId != newGlass.Location.SectionId)
            {
                //assign new location to glass if needed
                newGlass.Location = await areaRepos.GetLocation(newGlass.Location.SectionId);
            }
            //update glass 
            if (await repos.Update(newGlass))
            {
                //check if location is changed
                if (oldGlass.Location.SectionId != newGlass.Location.SectionId)
                {
                    //change section's storage
                    if (!await secRepos.RemoveGlassOutOfSection(oldGlass.Location.SectionId, oldGlass.Id))
                    {
                        return BadRequest(new { section = oldGlass.Location.SectionId, error = "cannot remove glass with id of " + oldGlass.Id + "out of section" });
                    }
                    if (!await secRepos.AddGlassToSection(newGlass.Location.SectionId, newGlass.Id))
                    {
                        return BadRequest(new { section = newGlass.Location.SectionId, error = "cannot add glass with id of " + newGlass.Id + "to new section" });
                    }
                }
                return Ok();
            }
            else
                return BadRequest("Failed");
        }
    }
}
