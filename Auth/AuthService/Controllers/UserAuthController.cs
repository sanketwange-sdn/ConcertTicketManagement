using AuthService.Abstract;
using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Solution.Core.Entity;
using Solution.Persistence;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserDbContext _dbContext;

        public UserAuthController(ITokenService tokenService, UserDbContext dbContext)
        {
            _tokenService = tokenService;
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            var user = await _dbContext.Users
                        .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim("name",user.FullName)
            };

            var token = _tokenService.GenerateToken(claims);

            return Ok(new
            {
                Token = token,
                User = new { user.Id, user.Username, user.Role }
            });
        }
        [HttpPost("add-update")]
        public async Task<IActionResult> AddOrUpdateUser([FromBody] AddOrUpdateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            var existingUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id || u.Username == request.Username);

            // 🔄 Update
            if (existingUser != null && (request.Id != null&&request.Id!=0))
            {
                existingUser.Email = request.Email;
                existingUser.FullName = request.FullName;
                existingUser.Role = request.Role;
                existingUser.Password = request.Password;

                _dbContext.Users.Update(existingUser);
            }
            // ➕ Add
            else if (existingUser == null)
            {
                var newUser = new Users
                {
                    Username = request.Username,
                    Email = request.Email,
                    FullName = request.FullName,
                    Role = request.Role,
                    Password = request.Password,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Users.Add(newUser);
            }
            else
            {
                return Conflict("User already exists.");
            }

            await _dbContext.SaveChangesAsync();
            return Ok("User added/updated successfully.");
        }

    }
}
