using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    // [ApiController]
    // [Route("api/[controller]")] // GET /api/users
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _UserRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _UserRepository = userRepository;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _UserRepository.GetMembersAsync();

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
           return await _UserRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> updateUser(MemberUpdateDTO memberUpdateDTO)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var user = await _UserRepository.GetUserByUserNameAsync(username);

            if(user == null) return NotFound();

            _mapper.Map(memberUpdateDTO, user);

            if(await _UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Falló la actualización del usuario");
        }
    }
}
