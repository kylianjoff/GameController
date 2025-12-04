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
    public class GameController : ControllerBase
    {
        private readonly UserContext _context;
        public GameController(UserContext ctx)
        {
            _context = ctx;
        }

        private int CalculateResetCost(int multiplier)
        {
            double baseCost = 100.0;
            double growthFactor = 1.5;
            double cost = baseCost * Math.Pow(growthFactor, multiplier - 1);
            return (int)Math.Floor(cost);
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

        [Authorize]
        [HttpGet("Progression")]
        public async Task<ActionResult<Progression>> Progression()
        {
            int userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized(new ErrorResponse("Invalid token", "INVALID_TOKEN"));
            }

            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if (progression == null)
            {
                return NotFound(new ErrorResponse("User does not have a progression", "NO_PROGRESSION"));
            }
            return Ok(progression);
        }

        [Authorize]
        [HttpGet("Click")]
        public async Task<ActionResult> Click()
        {
            int userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized(new ErrorResponse("Invalid token", "INVALID_TOKEN"));
            }

            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if (progression == null)
            {
                return NotFound(new ErrorResponse("User does not have progression", "NO_PROGRESSION"));
            }
            
            int effectiveIncrement = progression.multiplier * (1 + progression.totalClickValue);        
            progression.count += effectiveIncrement;
            
            if (progression.count > progression.bestScore)
            {
                progression.bestScore = progression.count;
            }
            await _context.SaveChangesAsync();
            
            return Ok(new 
            {
                count = progression.count,
                multiplier = progression.multiplier,
                totalClickValue = progression.totalClickValue
            });
        }

        [Authorize]
        [HttpGet("Initialize")]
        public async Task<ActionResult<Progression>> InitializeProgression()
        {
            int userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized(new ErrorResponse("Invalid token", "INVALID_TOKEN"));
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new ErrorResponse("User not found", "USER_NOT_FOUND"));
            }
            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if (progression != null)
            {
                return BadRequest(new ErrorResponse("User already has a progression", "PROGRESSION_EXISTS"));
            }
            var newProgression = new Progression
            {
                userId = userId,
                count = 0,
                multiplier = 1,
                bestScore = 0,
                totalClickValue = 0
            };
            _context.Progressions.Add(newProgression);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Progression), newProgression);
        }

        [Authorize]
        [HttpPost("Reset")]
        public async Task<ActionResult<Progression>> Reset()
        {
            int userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized(new ErrorResponse("Invalid token", "INVALID_TOKEN"));
            }

            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if (progression == null)
            {
                return NotFound(new ErrorResponse("User does not have a progression", "NO_PROGRESSION"));
            }
            
            int resetCost = CalculateResetCost(progression.multiplier);
            if (progression.count < resetCost)
            {
                return BadRequest(new ErrorResponse("Not enough points", "INSUFFICIENT_POINTS"));
            }
            
            if (progression.count > progression.bestScore)
            {
                progression.bestScore = progression.count;
            }
            
            var userInventory = await _context.Inventories
                .Where(i => i.userId == userId)
                .ToListAsync();
            _context.Inventories.RemoveRange(userInventory);
            
            progression.count = 0;
            progression.multiplier += 1;
            progression.totalClickValue = 0;
            
            await _context.SaveChangesAsync();
            
            return Ok(progression);
        }

        [Authorize]
        [HttpGet("ResetCost")]
        public async Task<ActionResult<int>> ResetCost()
        {
            int userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized(new ErrorResponse("Invalid token", "INVALID_TOKEN"));
            }

            var multiplier = await _context.Progressions.Where(u => u.userId == userId).Select(u => u.multiplier).FirstOrDefaultAsync();
            if(multiplier == 0)
            {
                return NotFound(new ErrorResponse("User does not have a progresson", "NO_PROGRESSION"));
            }
            var cost = CalculateResetCost(multiplier);
            return Ok(new
            {
                cost
            });
        }

        [Authorize]
        [HttpGet("BestScore")]
        public async Task<ActionResult<IEnumerable<object>>> BestScore()
        {
            var progressions = await _context.Progressions
                .OrderByDescending(p => p.bestScore)
                .FirstOrDefaultAsync();
            
            if (progressions == null)
            {
                return NotFound(new ErrorResponse("No progressions found", "NO_PROGRESSIONS"));
            }
            
            return Ok(new
            {
                progressions.userId,
                progressions.bestScore
            });
        }
    }
}