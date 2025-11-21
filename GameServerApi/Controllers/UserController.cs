using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameServerApi.Models;
using Microsoft.AspNetCore.Identity;

namespace GameServerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        public UserController(UserContext ctx)
        {
            _context = ctx;
        }

        [HttpGet("All")]
        public async Task<ActionResult<IEnumerable<UserPublic>>> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new UserPublic(u.id, u.username, u.role))
                .ToListAsync();

            if (users.Count == 0)
            {
                return NotFound(new ErrorResponse("No users found", "NO_USERS"));
            }
            return Ok(users);
        }

        [HttpGet("AllAdmin")]
        public async Task<ActionResult<IEnumerable<UserPublic>>> GetAllAdmins()
        {
            var admins = await _context.Users
                .Where(u => u.role == Role.Admin)
                .Select(u => new UserPublic(u.id, u.username, u.role))
                .ToListAsync();

            if (admins.Count == 0)
            {
                return NotFound(new ErrorResponse("No admins found", "NO_ADMINS"));
            }
            return Ok(admins);
        }

        [HttpGet("Search/{name}")]
        public async Task<ActionResult<IEnumerable<UserPublic>>> SearchUsers(string name)
        {
            var users = await _context.Users
                .Where(u => u.username != null && u.username.Contains(name))
                .Select(u => new UserPublic(u.id, u.username, u.role))
                .ToListAsync();

            if (users.Count == 0)
            {
                return NotFound(new ErrorResponse("No users found", "NO_USERS"));
            }
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserPublic>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponse("User not found", "USER_NOT_FOUND"));
            }
            return Ok(new UserPublic(user.id, user.username, user.role));
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserPublic>> Register(UserPass userCreation)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.username == userCreation.username);
            if (existingUser != null)
            {
                return BadRequest(new ErrorResponse("Username already exists", "USERNAME_EXISTS"));
            }

            var newUser = new User
            {
                username = userCreation.username,
                role = Role.User
            };

            var hasher = new PasswordHasher<User>();
            newUser.password = hasher.HashPassword(newUser, userCreation.password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = newUser.id }, new UserPublic(newUser.id, newUser.username, newUser.role));
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserPublic>> Login(UserPass loginData)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.username == loginData.username);
            if (user == null)
                return Unauthorized(new ErrorResponse("Username ou mot de passe incorrect", "AUTH_FAILED"));

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.password, loginData.password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new ErrorResponse("Username ou mot de passe incorrect", "AUTH_FAILED"));

            return Ok(new UserPublic(user.id, user.username, user.role));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserPublic>> PutUser(int id, UserUpdate userUpdate)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponse("User not found", "USER_NOT_FOUND"));
            }

            user.username = userUpdate.username;
            user.role = userUpdate.role;

            if (!string.IsNullOrEmpty(userUpdate.password))
            {
                var hasher = new PasswordHasher<User>();
                user.password = hasher.HashPassword(user, userUpdate.password);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, new ErrorResponse("Erreur de concurrence", "CONCURRENCY_ERROR"));
            }

            return Ok(new UserPublic(user.id, user.username, user.role));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new ErrorResponse("User not found", "USER_NOT_FOUND"));
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("All")]
        public async Task<IActionResult> DeleteAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            if (users.Count == 0)
            {
                return NotFound(new ErrorResponse("No users to delete", "NO_USERS"));
            }
            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}