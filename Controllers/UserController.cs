using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            var user = new User
            {
                Id = System.Random.Shared.Next(1, 100000),
                Name = userDto.Name,
                Email = userDto.Email,
                Password = userDto.Password,
                Role = userDto.Role,
                Location = userDto.Location
            };

            // ✅ Make Cosmos PK value (id) match our numeric Id
            user.id = user.Id.ToString();

            await _store.AddUserAsync(user);
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _store.GetUserByEmailAsync(request.Email);
            if (user == null || user.Password != request.Password) return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"] ?? _config["Jwt__Key"] ?? "ChangeThisDevOnlyKey");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = System.DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token), user });
        }
    }

    public record LoginRequest(string Email, string Password);
    public record UserDto(string Name, string Email, string Password, string Role, string Location);
}
