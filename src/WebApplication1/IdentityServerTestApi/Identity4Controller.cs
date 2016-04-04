using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace WebApplication1.IdentityServerTestApi
{
    [Area("api")]
    [Route("[controller]")]
    [Authorize]
    public class Identity4Controller: Controller
    {
        private readonly ClaimsPrincipal _caller;

        public Identity4Controller(ClaimsPrincipal caller)
        {
            _caller = caller;
        }

        [HttpGet]
        public ActionResult Get()
        {
            var result = _caller.Claims.Select(
                c => new {c.Type, c.Value});

            return new JsonResult(result);
        }
    }

    [Area("apiv1")]
    [Route("[area]/[controller]")]
    public class Identity4AuthController : Controller
    {
        private readonly ClaimsPrincipal _caller;

        public Identity4AuthController(ClaimsPrincipal caller)
        {
            _caller = caller;
        }

        [HttpGet]
        public ActionResult Get()
        {
            var result = _caller.Claims.Select(
                c => new { c.Type, c.Value });

            return new JsonResult(result);
        }
    }
}
