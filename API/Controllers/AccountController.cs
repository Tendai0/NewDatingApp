using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : ApiBaseClass
    {
        private readonly DataContext _context;

        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public UserManager<AppUser> _userManager { get;set; }

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
          

        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDTOs>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.username))
            {
                return BadRequest("Username is taken");
            }
            var user = _mapper.Map<AppUser>(registerDto);
            user.UserName = registerDto.username.ToLower();
           

            var result = await _userManager.CreateAsync(user,registerDto.password);
            if(!result.Succeeded) return BadRequest(result.Errors);
            var roleResult = await _userManager.AddToRoleAsync(user,"Member");
            if(!roleResult.Succeeded) return BadRequest(result.Errors);


            return new UserDTOs
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTOs>> Login(LogInDtos logInDtos)
        {
            var user = await _userManager.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == logInDtos.username);

            if (user == null) return Unauthorized("Invalid username");
            var result = await _userManager.CheckPasswordAsync(user,logInDtos.password);
            if(!result)return Unauthorized("InValid Password");
            return new UserDTOs
            {
                Username = user.UserName,
                Token =await  _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
    }
}