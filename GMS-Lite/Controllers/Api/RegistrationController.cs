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
    public class RegistrationController : ControllerBase
    {
        RegistrationRepository repos = new RegistrationRepository();

        // GET: api/Registration
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<List<Registration>> Get()
        {
            return await repos.FindAll();
        }

        // GET: api/Registration/user=value
        [Authorize(Roles = "Admin,Member")]
        [HttpGet("user={id}")]
        public async Task<List<Registration>> Get(string id)
        {
            return await repos.Find(id);
        }

        // POST: api/Registration
        [Authorize(Roles = "Admin,Member")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Registration regis)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await repos.Create(regis);
            return Ok();
        }

        // DELETE: api/Registration/value
        [Authorize(Roles = "Admin,Member")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var objectId = new ObjectId();
            if (!ObjectId.TryParse(id, out objectId))
            {
                return BadRequest("Id is not valid");
            }
            if (!await repos.isExisted(id))
                return NotFound(new { id, error = "There was no registration with an id of " + id + "." });
            if (await repos.Delete(id))
                return Ok();
            else
                return BadRequest("Failed");
        }
    }
}
