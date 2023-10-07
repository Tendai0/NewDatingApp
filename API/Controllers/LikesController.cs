using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController:ApiBaseClass
    {
        private readonly IUserRepository _userRepository;

        private readonly ILikesRepository _likesRepository;


        public LikesController(IUserRepository userRepository,ILikesRepository likesRepository)
        {
            _userRepository = userRepository;

            _likesRepository = likesRepository;

        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId =User.GetUserId();
            var likedUser=await _userRepository.GetUserByUserNameAsync(username);
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);

            if(likedUser==null) return NotFound();
            if(sourceUser.UserName==username) return BadRequest("You can not like your self");

            var userLike = await _likesRepository.GetUserLike(sourceUserId,likedUser.id);

            if(userLike!=null)return BadRequest("You aleady like this user");
            userLike=new UserLike
            {
              SourceUserId=sourceUserId,
              TargetUserId=likedUser.id
            };
            sourceUser.LikedUsers.Add(userLike);

            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like user");

        }
        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDTO>>> GetUsersLike([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users =await _likesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages));
            return Ok(users);
        }
    }
}