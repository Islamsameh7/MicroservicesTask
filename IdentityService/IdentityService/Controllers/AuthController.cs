using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly IConfiguration configuration;

        public AuthController(ApplicationDbContext db, IConfiguration configuration)
        {
            this.db = db;
            this.configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] DbUser request)
        {
            var user = new DbUser
            {
                Username = request.Username,
                Password = request.Password
            };

            await db.DbUsers.AddAsync(user);
            await db.SaveChangesAsync();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] DbUser request)
        {
            var user = await db.DbUsers.FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user is null) return BadRequest("User not found");

            var token = CreateToken(user);

            return Ok(token);
        }

        [NonAction]
        public string CreateToken(DbUser user)
        {
            List<Claim> claims = new List<Claim> {
                new(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("Jwt:key").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}