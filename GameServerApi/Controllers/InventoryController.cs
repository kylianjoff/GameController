using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameServerApi.Models;
using GameServerApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GameServerApi.Controllers
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

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
            {
                return -1;
            }
            return userId;
        }

        [AllowAnonymous]
        [HttpGet("Seed")]
        public async Task<ActionResult<bool>> Seed()
        {
            try
            {
                // 1. Vider les tables Inventories et Items
                var inventories = await _context.Inventories.ToListAsync();
                _context.Inventories.RemoveRange(inventories);
                
                var items = await _context.Items.ToListAsync();
                _context.Items.RemoveRange(items);
                
                await _context.SaveChangesAsync();

                // 2. Récupérer les items depuis l'URL
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync("https://csharp.nouvet.fr/front4/items.json");
                
                // 3. Désérialiser le JSON
                var itemsFromJson = System.Text.Json.JsonSerializer.Deserialize<List<Item>>(response);
                
                if (itemsFromJson == null || itemsFromJson.Count == 0)
                {
                    return BadRequest(new ErrorResponse("Failed to seed inventory", "SEED_FAILED"));
                }

                // 4. Insérer les items en base
                _context.Items.AddRange(itemsFromJson);
                await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch
            {
                return BadRequest(new ErrorResponse("Failed to seed inventory", "SEED_FAILED"));
            }
        }

        [AllowAnonymous]
        [HttpGet("Items")]
        public async Task<ActionResult<IEnumerable<Item>>> Items()
        {
            var items = await _context.Items.OrderBy(i => i.id).ToListAsync();
            if (items.Count == 0)
            {
                return NotFound(new ErrorResponse("No items found", "NO_ITEMS"));
            }
            return Ok(items);
        }

        [Authorize]
        [HttpGet("UserInventory")]
        public async Task<ActionResult<IEnumerable<InventoryEntry>>> UserInventory()
        {
            int userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized(new ErrorResponse("Invalid token", "INVALID_TOKEN"));
            }

            var items = await _context.Inventories.Where(i => i.userId == userId).ToListAsync();
            return Ok(items);
        }

        [Authorize]
        [HttpPost("Buy/{itemId}")]
        public async Task<ActionResult<IEnumerable<InventoryEntry>>> Buy(int itemId)
        {
            int userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized(new ErrorResponse("Invalid token", "INVALID_TOKEN"));
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest(new ErrorResponse("User not found", "USER_NOT_FOUND"));
            }

            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return BadRequest(new ErrorResponse("Item not found", "ITEM_NOT_FOUND"));
            }

            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if (progression == null)
            {
                return BadRequest(new ErrorResponse("User does not have a progression", "NO_PROGRESSION"));
            }

            if (progression.count < item.price)
            {
                return BadRequest(new ErrorResponse("Not enough money to buy the item", "NOT_ENOUGH_MONEY"));
            }

            var inventoryEntry = await _context.Inventories.FirstOrDefaultAsync(i => i.userId == userId && i.itemId == itemId);

            if (inventoryEntry != null)
            {
                if (inventoryEntry.quantity >= item.maxQuantity)
                {
                    return BadRequest(new ErrorResponse("Inventory is full", "INVENTORY_FULL"));
                }
                inventoryEntry.quantity++;
            }
            else
            {
                inventoryEntry = new InventoryEntry
                {
                    userId = userId,
                    itemId = itemId,
                    quantity = 1
                };
                _context.Inventories.Add(inventoryEntry);
            }

            // Déduire le prix et augmenter totalClickValue
            progression.count -= item.price;
            progression.totalClickValue += item.clickValue;

            await _context.SaveChangesAsync();

            // Retourner tout l'inventaire de l'utilisateur
            var userInventory = await _context.Inventories.Where(i => i.userId == userId).ToListAsync();

            return Ok(userInventory);
        }
    }
}