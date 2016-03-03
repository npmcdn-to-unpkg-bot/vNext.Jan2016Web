using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace WebApplication1.IdentityServerTestApi
{
    [Route("[controller]")]
    [Authorize]
    public class Identity4Controller
    {
        private readonly ClaimsPrincipal _caller;

        public Identity4Controller(ClaimsPrincipal caller)
        {
            _caller = caller;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return new JsonResult(_caller.Claims.Select(
                c => new { c.Type, c.Value }));
        }
    }
}
