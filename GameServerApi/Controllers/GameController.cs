using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameServerApi.Models;
using Microsoft.AspNetCore.Identity;

namespace GameServerApi.controller
{
    [Route("api/Game/[controller]")]
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
            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if(progression != null)
            {
                return BadRequest(new ErrorResponse("User already has a progression", "PROGRESSION_EXISTS"));
            }
            try
            {
                
            }
            catch
            {
                return BadRequest(new ErrorResponse("Failed to initialize progression", "INITIALIZATION_FAILED"));
            }
            return Ok(progression);
        }

        [HttpPost("Reset/{userId}")]
        public async Task<ActionResult<Progression>> Reset(int userId)
        {
            var progression = await _context.Progressions.FirstOrDefaultAsync(p => p.userId == userId);
            if(progression == null)
            {
                return BadRequest(new ErrorResponse("User does not have a progression", "NO_PROGRESSION"));
            }
            if(progression.count > progression.bestScore)
            {
                progression.bestScore = progression.count;
            }
            progression.count = 0;
            progression.multiplier += 1;
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
            var progressions = await _context.Progressions.ToListAsync();
            if(progressions == null || progressions.Count == 0)
            {
                return NotFound(new ErrorResponse("No progressions found", "NO_PROGRESSIONS"));
            }
            return Ok(progressions);
        }
    }
}