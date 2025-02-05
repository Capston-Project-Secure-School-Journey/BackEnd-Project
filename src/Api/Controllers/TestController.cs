using Api.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        public TestController()
        {
        }
        
        [HttpGet]
        public async Task<IActionResult> Test()
        {
            return Ok("oke");
        }
    }
}
