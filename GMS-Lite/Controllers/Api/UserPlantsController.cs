using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMS.Models.Entities.Collections;
using GMS.Models.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPlantsController : ControllerBase
    {
        UserPlantingRepository repos = new UserPlantingRepository();

        // GET: api/UserPlants
        [HttpGet]
        public async Task<List<UserPlanting>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/UserPlants/id=value
        [HttpGet("id={id}")]
        public async Task<UserPlanting> Get(string id)
        {
            if(!await repos.isExisted(id))
            {
                await repos.Create(new UserPlanting(id));
            }
            return await repos.Find(id);
        }

        // POST: api/UserPlants
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]  UserPlanting userPlants)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await repos.Create(userPlants);
            return Ok();
        }

        //GET: api/UserPlants/deleteTree/userId=value&&treeId=value
        [HttpDelete("deleteTree/userId={userId}&&treeId={treeId}")]
        public async Task<IActionResult> DeleteTree(string userId, string treeId)
        {
            if (!await repos.isExisted(userId))
                return NotFound(new { userId, error = "There was no data with an id of " + userId + "." });
            var userData = await repos.Find(userId);
            userData.plants.Remove(treeId);
            if (await repos.Update(userData))
                return Ok();
            else
                return BadRequest("Failed");
        }


        // DELETE: api/UserPlants/value
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await repos.isExisted(id))
                return NotFound(new { id, error = "There was no data with an id of " + id + "." });
            if (await repos.Delete(id))
                return Ok();
            else
                return BadRequest("Failed");
        }

        // PUT: api/UserPlants/value
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserPlanting userPlants)
        {
            if (! ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!await repos.isExisted(userPlants.userId))
            {
                await repos.Create(new UserPlanting(userPlants.userId));
            }
            if (await repos.Update(userPlants))
                return Ok();
            else
                return BadRequest("Failed");
        }
    }
}
