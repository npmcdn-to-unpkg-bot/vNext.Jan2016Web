// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace Hello.ReactJS.Areas.HelloReactJS.Controllers
{
    [Area("HelloReactJS")]
    public class HomeController : Controller
    {
        // GET: /<controller>/
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        
    }
}
