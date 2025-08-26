using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using WellbeingHub.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;

namespace WellbeingHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly DataStore _store;
        private readonly IConfiguration _config;

        public UserController(DataStore store, IConfiguration config)
        {
            _store = store;
            _config = config;
        }

        // POST: /api/User/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            var user = new User
            {
                Id = System.Random.Shared.Next(1, 100000),
                Name = userDto.Name,
                Email = userDto.Email,
                Password = userDto.Password, // plain text for now; hashing patch coming next
                Role = userDto.Role,
                Location = userDto.Location
            };

            // Ensure Cosmos PK value (id) matches our numeric Id
            user.id = user.Id.ToString();

            await _store.AddUserAsync(user);

            // Return a safe view model (no password)
            var view = new UserView(user.Id, user.Name, user.Email, user.Role, user.Location);
            return Ok(view);
        }

        // POST: /api/User/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _store.GetUserByEmailAsync(request.Email);
            if (user == null || user.Password != request.Password)
                return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyString = _config["Jwt:Key"] ?? _config["Jwt__Key"] ?? "ChangeThisDevOnlyKey";
            var key = Encoding.ASCII.GetBytes(keyString);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = System.DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            var view = new UserView(user.Id, user.Name, user.Email, user.Role, user.Location);
            return Ok(new { token = jwt, user = view });
        }

        // GET: /api/User/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            // We put Email in the JWT when logging in
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized();

            var user = await _store.GetUserByEmailAsync(email);
            if (user == null)
                return NotFound();

            var view = new UserView(user.Id, user.Name, user.Email, user.Role, user.Location);
            return Ok(view);
        }
    }

    // Request/Response records
    public record LoginRequest(string Email, string Password);
    public record UserDto(string Name, string Email, string Password, string Role, string Location);
    public record UserView(int numericId, string Name, string Email, string Role, string Location);
}
