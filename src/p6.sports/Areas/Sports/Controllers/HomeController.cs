// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNet.Mvc;

namespace p6.sports.Areas.Sports.Controllers
{
    [Area("Sports")]
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            var s = "";
            return View();
        }
        public IActionResult Open()
        {
            var s = "";
            return View();
        }
    }
}
