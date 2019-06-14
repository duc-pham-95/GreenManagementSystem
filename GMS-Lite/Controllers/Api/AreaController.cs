using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GMS.Models.Entities.Collections;
using GMS.Models.Repositories;
using GMS.Models.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreaController : ControllerBase
    {
        AreaRepository repos = new AreaRepository();

        // GET: api/Area
        [HttpGet]
        public async Task<List<Area>> GetAll()
        {
            return await repos.FindAll();
        }

        // GET: api/Area/id=value
        [HttpGet("id={id}")]
        public async Task<Area> Get(string id)
        {
            return await repos.Find(id);
        }

        // GET: api/Area/name=value
        [HttpGet("name={name}")]
        public async Task<List<Area>> GetByAttrName(string name)
        {
            return await repos.FindByAttr(name, "name");
        }

        // GET: api/Area/managedunit=value
        [HttpGet("managedunit={id}")]
        public async Task<List<Area>> GetByAttrUnit(string id)
        {
            return await repos.FindByAttr(id, "unit");
        }

        // GET: api/Area/province
        [HttpGet("provice")]
        public async Task<List<Area>> GetProvinces()
        {
            return await repos.FindByIdPrefix("P");
        }

        // GET: api/Area/district
        [HttpGet("district")]
        public async Task<List<Area>> GetDistricts()
        {
            return await repos.FindByIdPrefix("D");
        }

        // GET: api/Area/ward
        [HttpGet("ward")]
        public async Task<List<Area>> GetWards()
        {
            return await repos.FindByIdPrefix("W");
        }

        // GET: api/Area/unit
        [HttpGet("unit")]
        public async Task<List<Area>> GetUnits()
        {
            return await repos.FindByIdPrefix("U");
        }

        // GET: api/Area/section
        [HttpGet("section")]
        public async Task<List<Area>> GetSections()
        {
            return await repos.FindByIdPrefix("S");
        }

        // GET: api/Area/child/id=value
        [HttpGet("child/id={id}")]
        public async Task<List<Area>> GetChilds(string id)
        {
            return await repos.GetAllChildNodes(id);
        }

        // GET: api/Area/base/id=value
        [HttpGet("base/id={id}")]
        public async Task<List<Area>> GetBases(string id)
        {
            return await repos.GetAllBaseNode(id);
        }

        // GET: api/Area/location/section=value
        [HttpGet("location/section={id}")]
        public async Task<Object> GetLocation(string id)
        {
            if (id == null)
                return null;
            var section = await repos.Find(id);
            var unit = await repos.Find(section?.ParentAreaId);
            var ward = await repos.Find(unit?.ParentAreaId);
            var district = await repos.Find(ward?.ParentAreaId);
            var province = await repos.Find(district?.ParentAreaId);
            return new
            {
                proId = province?.Id,
                proName = province?.Name,
                disId = district?.Id,
                disName = district?.Name,
                wardId = ward?.Id,
                wardName = ward?.Name,
                unitId = unit?.Id,
                unitName = unit?.Name,
                secId = section?.Id,
                secName = section?.Name
            };
        }

        // POST: api/Area
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]  Area area)
        {
            //check for valid object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for node's existence
            if (await repos.isExisted(area.Id))
            {
                return BadRequest("Area with id = " + area.Id + " already exist.");
            }
            //if node has parent, then check conditions
            if (area.ParentAreaId != null)
            {
                //check for parent node's existence
                if (!await repos.isExisted(area.ParentAreaId))
                    return BadRequest("Parent area with id = " + area.ParentAreaId + " doesn't exist.");
                //check for the relationship level
                //if (!Service.IsRelationshipLevelsCorrect(area.ParentAreaId[0].ToString(), area.Id[0].ToString()))
                //    return BadRequest("the relationship's level of the area is invalid.");
            }
            //if node has childs, then check conditions
            //if (area.ChildAreaIds.Count > 0)
            //{
            //    foreach (var id in area.ChildAreaIds)
            //    {
            //        //check for child node's existence
            //        if (!await repos.isExisted(id))
            //        {
            //            return BadRequest("Child area with id = " + id + " doesn't exist");
            //        }
            //        //check for child node's relationship
            //        var child = await repos.Find(id);
            //        if (child.ParentAreaId != null)
            //        {
            //            if (!Service.IsRelationshipLevelsCorrect(area.Id[0].ToString(), id[0].ToString()))
            //                return BadRequest("the relationship's level of the area is invalid.");
            //        }
            //    }
            //    //remove old relationship for inserting
            //    foreach (var id in area.ChildAreaIds)
            //    {
            //        var child = await repos.Find(id);
            //        if (child.ParentAreaId != null)
            //        {
            //            //modify child node, update leaves
            //            var parent = await repos.Find(child.ParentAreaId);
            //            parent.ChildAreaIds.Remove(child.Id);
            //            child.ParentAreaId = null;
            //            await repos.Update(child);
            //            await repos.Update(parent);
            //            if (child.Id.StartsWith("S"))
            //            {
            //                await repos.RemoveLeafNodeReferenceRecursively(parent.Id, child.Id);
            //            }
            //            else
            //            {
            //                await repos.RemoveLeafNodeReferenceRecursively(parent.Id, child.leavesIds);
            //            }
            //        }
            //    }
            //}


            //insert to database
            await repos.Create(area);
            //assign relationship with its parent
            if (!await repos.AddRelationship(area.ParentAreaId, area.Id))
            {
                //if unsuccess, then delete new node
                if (!await repos.Delete(area.Id))
                {
                    //if unsuccess, then return error
                    return BadRequest(
                        "Error occured !." +
                        "The uncompleted object with id = " + area.Id + " has been added to the area collection in the database." +
                        "The system tried to handle by automatically removing it out of the database, but didn't work." +
                        "Please, handle it manually !"
                        );
                }
                return BadRequest("Couldn't assign a new relationship between id = " + area.ParentAreaId + " and id = " + area.Id);
            }
            //assign to the tree relationship
            await Service.AssignNode(area);
            return Ok();
        }

        // DELETE: api/Area/value
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            //check for node's existence
            if (!await repos.isExisted(id))
            {
                return NotFound("Area with id = " + id + " doesn't exist.");
            }
            var area = await repos.Find(id);
            //delete out of database
            await repos.Delete(id);
            //unassign relationship with its parent
            await repos.RemoveRelationship(area.ParentAreaId, area.Id);
            //unassign to the tree relationship
            await Service.UnassignNode(area);
            ////setting new relationship for parent and child
            //if (await repos.isExisted(area.ParentAreaId) && area.ChildAreaIds.Count > 0)
            //{
            //    var parentArea = await repos.Find(area.ParentAreaId);
            //    foreach (string childId in area.ChildAreaIds)
            //    {
            //        var childArea = await repos.Find(childId);
            //        if (childArea != null)
            //        {
            //            parentArea.ChildAreaIds.Add(childId);
            //            childArea.ParentAreaId = parentArea.Id;
            //            await repos.Update(childArea);
            //            await repos.Update(parentArea);
            //            if (childArea.Id.StartsWith("S"))
            //            {
            //                await repos.AddLeafNodeReferenceRecursively(parentArea.Id, childArea.Id);
            //            }
            //            else
            //            {
            //                await repos.AddLeafNodeReferenceRecursively(parentArea.Id, childArea.leavesIds);
            //            }
            //        }
            //    }
            //}
            return Ok();
        }

        // PUT: api/Area/
        [Authorize(Roles = "Admin")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Area updateArea)
        {
            //check for valid object
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //check for node's existence
            if (!await repos.isExisted(updateArea.Id))
            {
                return BadRequest("Area with id = " + updateArea.Id + " doesn't exist.");
            }
            //if node has parent, then check conditions
            if (updateArea.ParentAreaId != null)
            {
                //check for parent node's existence
                if (!await repos.isExisted(updateArea.ParentAreaId))
                    return BadRequest("Parent area with id = " + updateArea.ParentAreaId + " doesn't exist.");
            }
            ////if node has childs, then check conditions
            //if (updateArea.ChildAreaIds.Count > 0)
            //{
            //    var current = await repos.Find(updateArea.Id);
            //    foreach (var id in updateArea.ChildAreaIds)
            //    {
            //        if (!current.ChildAreaIds.Contains(id))
            //        {
            //            //check for child node's existence
            //            if (!await repos.isExisted(id))
            //            {
            //                return BadRequest("Child area with id = " + id + " doesn't exist");
            //            }
            //            //check for child node's relationship
            //            var child = await repos.Find(id);
            //            if (child.ParentAreaId != null)
            //            {
            //                return BadRequest("Child area with id = " + id + " already has relationship");
            //            }
            //        }
            //    }
            //}

            //check for leaf node references modifies
            //var currentArea = await repos.Find(updateArea.Id);
            //if (Service.IsLeavesModified(currentArea, updateArea))
            //{
            //    return BadRequest(
            //        "cannot modify the leaf node's references (leaves)." +
            //        "The system will handle this automatically."
            //        );
            //}  
            //if parent change
            //if (currentArea.ParentAreaId != updateArea.ParentAreaId)
            //{
            //    //if node has parent, then modify relationship and references with old parent and new parent
            //    if (updateArea.ParentAreaId != null)
            //    {
            //        await Service.ModifyParentNode(currentArea, updateArea);
            //    }
            //    //if node has no parent, then remove relationship and references with old parent
            //    else
            //    {
            //        await repos.RemoveRelationship(currentArea.ParentAreaId, currentArea.Id);
            //        await Service.RemoveNodeReferences(currentArea);
            //    }
            //    //update references
            //    await Service.AddNodeReferences(updateArea);
            //}
            //update to database
            await repos.Update(updateArea);
            //if child change
            //foreach (var id in updateArea.ChildAreaIds)
            //{
            //    if (!currentArea.ChildAreaIds.Contains(id))
            //    {
            //        //modify child node, update leaves
            //        var childArea = await repos.Find(id);
            //        childArea.ParentAreaId = updateArea.Id;
            //        await repos.Update(childArea);
            //        if (childArea.Id.StartsWith("S"))
            //        {
            //            await repos.AddLeafNodeReferenceRecursively(updateArea.Id, childArea.Id);
            //        }
            //        else
            //        {
            //            await repos.AddLeafNodeReferenceRecursively(updateArea.Id, childArea.leavesIds);
            //        }
            //    }
            //}
            //foreach (var id in currentArea.ChildAreaIds)
            //{
            //    if (! updateArea.ChildAreaIds.Contains(id))
            //    {
            //        //modify child node, update leaves
            //        var childArea = await repos.Find(id);
            //        childArea.ParentAreaId = null;
            //        await repos.Update(childArea);
            //        if (childArea.Id.StartsWith("S"))
            //        {
            //            await repos.RemoveLeafNodeReferenceRecursively(updateArea.Id, childArea.Id);
            //        }
            //        else
            //        {
            //            await repos.RemoveLeafNodeReferenceRecursively(updateArea.Id, childArea.leavesIds);
            //        }
            //    }
            //}
            return Ok();
        }
    }
}
