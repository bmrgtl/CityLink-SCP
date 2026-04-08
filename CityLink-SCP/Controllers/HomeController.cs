using System.Diagnostics;
using CityLink_SCP.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using System.Linq;
using System.Xml;
using System.IO;

namespace CityLink_SCP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }


        public IActionResult Index()
        {
            var content = System.IO.File.ReadAllText("XML\\Card.xml");
            var xdoc = XDocument.Parse(content);
            var model = new IndexViewModel
            {
                Cards = xdoc.Descendants("Card").Select(x => new CardViewModel
                {
                    Title = (string)x.Element("Title"),
                    Description = (string)x.Element("Description"),
                    ButtonLabel = (string)x.Element("ButtonLabel")
                }).ToList()
            };

			return View(model);
        }

        public IActionResult Signin()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
        public IActionResult WhatsOn()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
