using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Uphbt.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SomethingController : ControllerBase
    {
        [HttpGet]
        public string Greeting(int a, int b)
        {
            return "Hello " + (a * b);
        }
    }
}
