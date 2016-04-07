using System.Linq;
using Microsoft.AspNet.Mvc;
using P6.Main.IdentityServerHost.Models;

namespace P6.Main.IdentityServerHost.Areas.Main.Controllers
{
     [Area("Main")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            var result = HttpContext.User.Claims.Select(
              c => new ClaimType { Type = c.Type, Value = c.Value });

            //return new JsonResult(result);

            return View(result);
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
