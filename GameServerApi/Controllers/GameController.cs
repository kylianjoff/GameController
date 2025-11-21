using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameServerApi.Models;
using Microsoft.AspNetCore.Identity;

namespace GameServerApi.controller
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
            if(progression == null)
            {
                return BadRequest(new ErrorResponse("User does not have a progression", "NO_PROGRESSION"));
            }
            return Ok(progression);
        }

        [HttpGet("Click/{userId}")]
        public async Task<ActionResult<Progression>> Click(int userId)
        {
            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if(progression == null)
            {
                return BadRequest(new ErrorResponse("User does not have progression", "NO_PROGRESSION"));
            }
            progression.count += progression.multiplier;
            if(progression.count > progression.bestScore)
            {
                progression.bestScore = progression.count;
            }
            await _context.SaveChangesAsync();
            return Ok(progression);
        }

        [HttpGet("Initialize/{userId}")]
        public async Task<ActionResult<Progression>> InitializeProgression(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if(user == null)
            {
                return NotFound(new ErrorResponse("User not found", "USER_NOT_FOUND"));
            }
            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if(progression != null)
            {
                return BadRequest(new ErrorResponse("User already hass a progression", "PROGRESSION_EXISTS"));
            }
            var newProgression = new Progression
            {
                userId = userId,
                count = 0,
                multiplier = 1,
                bestScore = 0
            };
            _context.Progressions.Add(newProgression);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Progression), new { userId = userId}, newProgression);
        }

        [HttpPost("Reset/{userId}")]
        public async Task<ActionResult<Progression>> Reset(int userId)
        {
            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if(progression == null)
            {
                return BadRequest(new ErrorResponse("User does not have a progression", "NO_PROGRESSION"));
            }
            int resetCost = CalculateResetCost(progression.multiplier);
            if(progression.count < resetCost)
            {
                return BadRequest(new ErrorResponse("Not enought point", "INSUFFICIENT_POINTS"));
            }
            if(progression.count > progression.bestScore)
            {
                progression.bestScore = progression.count;
            }
            progression.count = 0;
            progression.multiplier += 1;
            await _context.SaveChangesAsync();
            return Ok(progression);
        }

        [HttpGet("ResetCost/{userId}")]
        public async Task<ActionResult<Progression>> ResetCost(int userId)
        {
            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if(progression == null)
            {
                return BadRequest(new ErrorResponse("User does not have a progression", "NO_PROGRESSION"));
            }
            return Ok(CalculateResetCost(progression.multiplier));
        }

        [HttpGet("BestScore/")]
        public async Task<ActionResult<Progression>> BestScore()
        {
            var progressions = await _context.Progressions.OrderByDescending(p => p.bestScore).ToListAsync();
            if(progressions == null || progressions.Count == 0)
            {
                return NotFound(new ErrorResponse("No progressions found", "NO_PROGRESSIONS"));
            }
            return Ok(progressions);
        }
    }
}