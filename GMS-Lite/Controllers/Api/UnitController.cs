using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMS.Models.Entities.Collections;
using GMS.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitController : ControllerBase
    {
        UnitRepository repos = new UnitRepository();

        // GET: api/Unit
        [HttpGet]
        public async Task<List<ManagedUnit>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/Unit/id=value
        [HttpGet("id={id}")]
        public async Task<ManagedUnit> Get(string id)
        {
            return await repos.Find(id);
        }

        // GET: api/Unit/name=value
        [HttpGet("name={name}")]
        public async Task<List<ManagedUnit>> GetByAttrName(string name)
        {
            return await repos.Find(name, "name");
        }

        // POST: api/Unit
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ManagedUnit unit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if(await repos.isExisted(unit.Id))
            {
                return BadRequest("Unit with id = " + unit.Id + " already exist.");
            }
            await repos.Create(unit);
            return Ok();
        }

        // DELETE: api/Unit/value
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await repos.isExisted(id))
                return NotFound(new { id, error = "There was no unit with an id of " + id + "." });
            if (await repos.Delete(id))
                return Ok();
            else
                return BadRequest("Failed");
        }

        // PUT: api/Unit/value
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] ManagedUnit unit)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (! await repos.isExisted(unit.Id))
            {
                return NotFound("Unit with id = " + unit.Id + " doesn't exist.");
            }
            if (await repos.Update(unit))
                return Ok();
            else
                return BadRequest("Failed");
        }
    }
}
