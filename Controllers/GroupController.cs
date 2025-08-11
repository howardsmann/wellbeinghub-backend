using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WellbeingHub.Models;

namespace WellbeingHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly DataStore _store;

        public GroupController(DataStore store)
        {
            _store = store;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(GroupDto groupDto)
        {
            var group = new Group
            {
                Id = System.Random.Shared.Next(1, 100000),
                Name = groupDto.Name,
                Location = groupDto.Location,
                MemberIds = groupDto.MemberIds ?? new List<int>()
            };
            await _store.AddGroupAsync(group);
            return Ok(group);
        }

        [Authorize]
        [HttpGet("by-location/{location}")]
        public async Task<ActionResult<List<Group>>> GetByLocation(string location)
        {
            var groups = await _store.GetGroupsByLocationAsync(location);
            return Ok(groups);
        }
    }

    // DTO for this controller
    public record GroupDto(string Name, string Location, List<int>? MemberIds);
}
