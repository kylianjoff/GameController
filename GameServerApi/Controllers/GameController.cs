using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameServerApi.Models;

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

        [HttpGet("Progression/{userId}")]
        public async Task<ActionResult<Progression>> Progression(int userId)
        {
            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if (progression == null)
            {
                return NotFound(new ErrorResponse("User does not have a progression", "NO_PROGRESSION"));
            }
            return Ok(progression);
        }

        [HttpGet("Click/{userId}")]
        public async Task<ActionResult> Click(int userId)
        {
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

        [HttpGet("Initialize/{userId}")]
        public async Task<ActionResult<Progression>> InitializeProgression(int userId)
        {
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
                totalClickValue = 0  // Initialiser totalClickValue à 0
            };
            _context.Progressions.Add(newProgression);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Progression), new { userId = userId }, newProgression);
        }

        [HttpPost("Reset/{userId}")]
        public async Task<ActionResult<Progression>> Reset(int userId)
        {
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
            progression.count = 0;
            progression.multiplier += 1;
            await _context.SaveChangesAsync();
            return Ok(progression);
        }

        [HttpGet("ResetCost/{userId}")]
        public async Task<ActionResult<int>> ResetCost(int userId)
        {
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