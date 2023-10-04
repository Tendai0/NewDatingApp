using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : ApiBaseClass
    {
        private readonly DataContext _context;

        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;


        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _tokenService = tokenService;
            _mapper = mapper;
            _context = context;

        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDTOs>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.username))
            {
                return BadRequest("Username is taken");
            }
            var user = _mapper.Map<AppUser>(registerDto);
            using var hmac = new HMACSHA512();



            user.UserName = registerDto.username.ToLower();
            user.PassWordHarsh = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password));
            user.PassWordSalt = hmac.Key;

            _context.users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDTOs
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs
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
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == logInDtos.username);

            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PassWordSalt);

            var ComputedHarsh = hmac.ComputeHash(Encoding.UTF8.GetBytes(logInDtos.password));

            for (int i = 0; i < ComputedHarsh.Length; i++)
            {

                if (ComputedHarsh[i] != user.PassWordHarsh[i]) return Unauthorized("Invalid Password");
            }
            return new UserDTOs
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };
        }
    }
}