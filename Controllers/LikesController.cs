using api.DTOs;
using api.Entities;
using api.Extensions;
using api.Helpers;
using api.Interfaces;
using API.Controllers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    public class LikesController : BaseApiController
    {
        public readonly IUserRepository _userRepository;
        public readonly ILikesRepository _likesRepository;
       
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _likesRepository = likesRepository;
            _userRepository = userRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await _userRepository.GetUserByUserNameAsync(username);
            var sourceUser  = await _likesRepository.GetUserWithLikes(sourceUserId);

            if(likedUser == null) return NotFound();

            if(sourceUser.UserName == username) return BadRequest("No podes gustarte a vos mismo... o si?");

            var userLike = await _likesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if(userLike != null) return BadRequest("Ya te gusta este usuario ❤");

            userLike = new UserLike
            {
                SoucerUserId = sourceUserId,
                TargetUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Falló al gustar al usuario");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();

            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}