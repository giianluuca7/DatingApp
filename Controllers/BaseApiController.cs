

using api.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ServiceFilter(typeof(LongUserActivity))]
    [ApiController]
    [Route("api/[controller]")] 
    
    public class BaseApiController : ControllerBase
    {
        
    }
}