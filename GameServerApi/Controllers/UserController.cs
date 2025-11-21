using Microsoft.AspNetCore.Http;
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
            var users = await _context.Users.Select(u => new UserPublic
            {
                id = u.id,
                username = u.pseudo,
                role = u.role
            }).ToListAsync();
            if(users == null || users.Count == 0)
            {
                return NotFound(new ErrorResponse("User not found", "USER_NOT_FOUND"));
            }
            return Ok(users);
        }

        [HttpGet("AllAdmin")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllAdmins()
        {
            return await _context.Users.Where(u => u.role == Role.Admin).ToListAsync();
        }

        [HttpGet("Search/{name}")]
        public async Task<ActionResult<IEnumerable<User>>> SearchUsers(string name)
        {
            return await _context.Users.Where(u => u.pseudo != null && u.pseudo.Contains(name)).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new
            {
                user.id,
                user.pseudo,
                user.role
            });
        }

        public class UserCreation
        {
            public int id { get; set; }
            public string? pseudo { get; set; }
            public string? password { get; set; }
            public Role role { get; set; }
        }

        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register(User userCreation)
        {

            var newUser = new User
            {
                pseudo = userCreation.pseudo,
                role = userCreation.role
            };

            var hasher = new PasswordHasher<User>();
            newUser.password = hasher.HashPassword(newUser, userCreation.password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = newUser.id }, new
            {
                newUser.id,
                newUser.pseudo,
                newUser.role
            });
        }

        [HttpPost("Login")]
        public async Task<ActionResult<User>> Login(User loginData)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.pseudo == loginData.pseudo);
            if (user == null) return Unauthorized("Pseudo ou mot de passe incorrect.");

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.password, loginData.password);

            if(result == PasswordVerificationResult.Failed) return Unauthorized("Pseudo ou mot de passe incorrect.");

            return Ok(new
            {
                user.id,
                user.pseudo,
                user.role
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(User userUpdate)
        {
            User user = await _context.Users.FindAsync(userUpdate.id);
            if (user == null)
            {
                return NotFound();
            }
            user.pseudo = userUpdate.pseudo;
            user.password = userUpdate.password;
            user.role = userUpdate.role;
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Erreur de concurrence");
            }
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            User user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("All")]
        public async Task<IActionResult> DeleteAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
