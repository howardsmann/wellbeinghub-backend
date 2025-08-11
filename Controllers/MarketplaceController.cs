using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WellbeingHub.Models;

namespace WellbeingHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketplaceController : ControllerBase
    {
        private readonly DataStore _store;

        public MarketplaceController(DataStore store)
        {
            _store = store;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(MarketplaceItemDto itemDto)
        {
            var item = new MarketplaceItem
            {
                Id = System.Random.Shared.Next(1, 100000),
                Title = itemDto.Title,
                Description = itemDto.Description,
                Price = itemDto.Price,
                CreatedBy = itemDto.CreatedBy
            };
            await _store.AddMarketplaceItemAsync(item);
            return Ok(item);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<MarketplaceItem>>> GetAll()
        {
            var items = await _store.GetAllMarketplaceItemsAsync();
            return Ok(items);
        }
    }

    public record MarketplaceItemDto(string Title, string Description, decimal Price, int CreatedBy);
}
