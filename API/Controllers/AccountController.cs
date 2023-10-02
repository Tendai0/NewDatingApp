using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : ApiBaseClass
    {
        private readonly DataContext _context;

        private readonly ITokenService _tokenService;

        public AccountController(DataContext context,ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;

        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDTOs>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.username))
            {
                return BadRequest("Username is taken");
            }
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDto.username.ToLower(),
                PassWordHarsh = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
                PassWordSalt = hmac.Key


            };
            _context.users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDTOs{
               Username=user.UserName,
               Token= _tokenService.CreateToken(user) 
            };
        }
        private async Task<bool> UserExists(string username)
        {
            return await _context.users.AnyAsync(x => x.UserName == username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTOs>> Login(LogInDtos logInDtos)
        {
            var user = await _context.users
            .Include(p =>p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == logInDtos.username);

            if (user == null) return Unauthorized("Invalid username");

             using var hmac = new HMACSHA512(user.PassWordSalt);

             var ComputedHarsh =  hmac.ComputeHash(Encoding.UTF8.GetBytes(logInDtos.password));

             for(int i = 0;i<ComputedHarsh.Length;i++){

                if(ComputedHarsh[i]!=user.PassWordHarsh[i]) return Unauthorized("Invalid Password");
             }
            return new UserDTOs{
               Username=user.UserName,
               Token= _tokenService.CreateToken(user) ,
               PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
            };
        }
    }
}