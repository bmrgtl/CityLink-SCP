using System.Diagnostics;
using CityLink_SCP.Models;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using System.Linq;
using System.Xml;
using System.IO;
using CityLink_SCP.Services;
using CityLink_SCP.Extensions;
namespace CityLink_SCP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly DatabaseService _dbService;
		public HomeController(ILogger<HomeController> logger, DatabaseService dbService)
        {
            _logger = logger;
			_dbService = dbService;
		}
		public IActionResult Index()
        {
			return View(GetIndexViewModel());
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

		#region Helper Methods
		private IndexViewModel GetIndexViewModel()
		{
			var events = _dbService._context.Events.Select(e => e.ToCardViewModel());
			var services = _dbService._context.Services.Select(s => s.ToCardViewModel());
			var model = new IndexViewModel
			{
				Events = events.Any() ? events.ToList() : GetEventsModel(),
				Services = services.Any() ? services.ToList() : GetServicesModel(),
				FAQs = GetFAQsModel()
			};
			return model;
		}
		private List<CardViewModel> GetEventsModel()
		{
			var content = System.IO.File.ReadAllText("XML\\EventsDefault.xml");
			var xdoc = XDocument.Parse(content);
			return xdoc.Descendants("Card").Select(x => new CardViewModel
			{
				Title = (string)x.Element("Title"),
				Description = (string)x.Element("Description"),
				ButtonLabel = (string)x.Element("ButtonLabel") 
			}).ToList();
		}
		private List<CardViewModel> GetServicesModel()
		{
			var content = System.IO.File.ReadAllText("XML\\ServicesDefault.xml");
			var xdoc = XDocument.Parse(content);
			return xdoc.Descendants("Card").Select(x => new CardViewModel
			{
				Title = (string)x.Element("Title"),
				Description = (string)x.Element("Description"),
				ButtonLabel = (string)x.Element("ButtonLabel")
			}).ToList();
		}
		private List<FAQViewModel> GetFAQsModel()
		{
			var content = System.IO.File.ReadAllText("XML\\FAQsDefault.xml");
			var xdoc = XDocument.Parse(content);
			return xdoc.Descendants("FAQItem").Select(x => new FAQViewModel
			{
				Question = (string)x.Element("Question"),
				Answer = (string)x.Element("Answer")
			}).ToList();
		}
		#endregion
	}
}
