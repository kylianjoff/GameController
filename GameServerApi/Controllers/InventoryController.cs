using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameServerApi.Models;
using Microsoft.AspNetCore.Identity;

namespace GameServerApi.controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly UserContext _context;
        public InventoryController(UserContext ctx)
        {
            _context = ctx;
        }

        [HttpGet("Inventory/Seed")]
        public async Task<ActionResult<InventoryEntry>> Seed()
        {
            try
            {
                return Ok(true);
            }
            catch
            {
                return BadRequest(new ErrorResponse("Failed to seed inventory", "SEED_FAILED"));
            }
        }

        [HttpGet("Inventory/Items")]
        public async Task<ActionResult<InventoryEntry>> Items()
        {
            var items = _context.Inventories.OrderByDescending(i => i.id).ToListAsync();
            if(items == null)
            {
                return NotFound(new ErrorResponse("No items found", "NO_FOUND"));
            }
            return Ok(items);
        }
    }
}