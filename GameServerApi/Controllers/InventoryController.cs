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

        [HttpGet("Inventory/userInventory/{userId}")]
        public async Task<ActionResult<InventoryEntry>> UserInventory(int userId)
        {
            var items = _context.Inventories.Where(i => i.userId == userId).ToListAsync();
            return Ok(items);
        }

        [HttpPost("Inventory/Buy/{userId}/{itemId}")]
        public async Task<ActionResult<InventoryEntry>> buy(int userId, int itemId)
        {
            var user = _context.Users.Where(u => u.id == userId);
            if(user == null)
            {
                return BadRequest(new ErrorResponse("User not found", "USER_NOT_FOUND"));
            }
            var maxQuantity = await _context.Items.Where(i => i.id == itemId).Select(i => i.maxQuantity).FirstOrDefaultAsync();
            var quantity = await _context.Inventories.Where(i => i.userId == userId).Select(i => i.quantity).FirstOrDefaultAsync();
            if(quantity < maxQuantity)
            {
                var item = _context.Items.Where(i => i.id == itemId);
                if(item == null)
                {
                    return BadRequest(new ErrorResponse("Item not found", "ITEM_NOT_FOUND"));
                }
                else
                {
                    var count = await _context.Progressions.Where(u => u.id == userId).Select(p => p.count).FirstOrDefaultAsync();
                    var cost = await _context.Items.Where(i => i.id == itemId).Select(i => i.price).FirstOrDefaultAsync();
                    if(count < cost)
                    {
                        return BadRequest(new ErrorResponse("Not enough money to buy the item", "NOT_ENOUGH_MONEY"));
                    }
                    else
                    {
                        return Ok();
                    }
                }
            }
            else
            {
                return BadRequest(new ErrorResponse("Inventory is full", "INVENTORY_FULL"));
            }
        }
    }
}