// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

using Microsoft.AspNet.Mvc;

namespace p6.PartnerManagement.Areas.PartnerManagement.Controllers
{
    [Area("PartnerManagement")]
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
          
            return View();
        }
    }
}
