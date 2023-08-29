using api.Extensions;
using api.Helpers;
using api.Interfaces;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    // [ApiController]
    // [Route("api/[controller]")] // GET /api/users
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _UserRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _UserRepository = userRepository;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUser = await _UserRepository.GetUserByUserNameAsync(User.GetUsername());
            userParams.CurrentUsername = currentUser.UserName;

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }

            var users = await _UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
           return await _UserRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var username = User.GetUsername();
            var user = await _UserRepository.GetUserByUserNameAsync(username);

            if(user == null) return NotFound();

            _mapper.Map(memberUpdateDTO, user);

            if(await _UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Falló la actualización del usuario");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _UserRepository.GetUserByUserNameAsync(User.GetUsername());

            if(user == null) return NotFound();

            var result = await _photoService.AddPhotoAsync(file);

            if(result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if(user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if(await _UserRepository.SaveAllAsync())
            {
                return CreatedAtAction(nameof(GetUser), new {username = user.UserName}, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Hay problemas para añadir la foto");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoID)
        {
            var user = await _UserRepository.GetUserByUserNameAsync(User.GetUsername());
            if(user == null) return NotFound();
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoID);

            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("Esta ya es tu foto principal");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if(currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if(await _UserRepository.SaveAllAsync()) return NoContent();
            
            return BadRequest("Hubo un problema en la configuracion de la foto");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _UserRepository.GetUserByUserNameAsync(User.GetUsername());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if(photo == null) return NotFound();
            if(photo.IsMain) return BadRequest("No puedes borrar tu foto principal");
            if( photo.PublicId != null )
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if(await _UserRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problema al borrar la foto");
        }
    }

    
}
