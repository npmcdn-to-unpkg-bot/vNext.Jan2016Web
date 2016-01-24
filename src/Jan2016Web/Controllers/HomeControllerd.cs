using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jan2016Web.ViewModels;
using Microsoft.AspNet.Mvc;

namespace Jan2016Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            string appName = "Your First ASP.NET 5 App";
            ViewBag.Message = "Your Application Name: " + appName;

            var serverInfo = new ServerInfoViewModel()
            {
                Name = Environment.MachineName,
                Software = Environment.OSVersion.ToString()
            };
            return View(serverInfo);
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
