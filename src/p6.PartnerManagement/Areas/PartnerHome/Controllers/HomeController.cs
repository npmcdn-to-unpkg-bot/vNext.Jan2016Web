// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using p6.PartnerManagement.Models;

namespace p6.PartnerManagement.Areas.PartnerHome.Controllers
{
    [Area("PartnerHome")]
    public class HomeController : Controller
    {
        // GET: /<controller>/
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Index(RequestPartnerAccessModel model)
        {
            return View("DeveloperRequestSuccess", model);
        }
    }
}
