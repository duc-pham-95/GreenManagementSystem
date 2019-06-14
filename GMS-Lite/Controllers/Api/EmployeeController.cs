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
    public class EmployeeController : ControllerBase
    {
        EmployeeRepository repos = new EmployeeRepository();

        // GET: api/Employee
        [HttpGet]
        public async Task<List<Employee>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/Employee/id=value
        [HttpGet("id={id}")]
        public async Task<Employee> Get(string id)
        {
            return await repos.Find(id);
        }

        // GET: api/Employee/name=value
        [HttpGet("name={name}")]
        public async Task<List<Employee>> GetByAttrName(string name)
        {
            return await repos.Find(name, "name");
        }

        // GET: api/Employee/gender=value
        [HttpGet("gender={gender}")]
        public async Task<List<Employee>> GetByAttrGender(string gender)
        {
            return await repos.Find(gender, "gen");
        }

        // GET: api/Employee/address=value
        [HttpGet("address={address}")]
        public async Task<List<Employee>> GetByAttrAddress(string address)
        {
            return await repos.Find(address, "address");
        }

        // GET: api/Employee/phone=value
        [HttpGet("phone={phone}")]
        public async Task<List<Employee>> GetByAttrPhone(string phone)
        {
            return await repos.Find(phone, "phone");
        }

        // GET: api/Employee/position=value
        [HttpGet("position={position}")]
        public async Task<List<Employee>> GetByAttrPosition(string position)
        {
            return await repos.Find(position, "pos");
        }

        // GET: api/Employee/manager=value
        [HttpGet("manager={id}")]
        public async Task<List<Employee>> GetByAttrManager(string id)
        {
            return await repos.Find(id, "super");
        }

        // POST: api/Employee
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]  Models.Entities.Collections.Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await repos.Create(employee);
            return Ok();
        }

        // DELETE: api/Employee/value
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var objectId = new ObjectId();
            if (!ObjectId.TryParse(id, out objectId))
            {
                return BadRequest("Id is not valid");
            }
            if (!await repos.isExisted(id))
                return NotFound(new { id, error = "There was no employee with an id of " + id + "." });
            if (await repos.Delete(id))
                return Ok();
            else
                return BadRequest("Failed");
        }

        // PUT: api/Employee/value
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] Models.Entities.Collections.Employee employee)
        {
            if (! ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var objectId = new ObjectId();
            if (!ObjectId.TryParse(employee.Id, out objectId))
            {
                return BadRequest("Id is not valid");
            }
            if (!await repos.isExisted(employee.Id))
                return NotFound(new { employee.Id, error = "There was no employee with an id of " + employee.Id + "." });
            if (await repos.Update(employee))
                return Ok();
            else
                return BadRequest("Failed");
        }


    }
}
