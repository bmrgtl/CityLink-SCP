using CityLink_SCP.DbModels;
using CityLink_SCP.Extensions;
using CityLink_SCP.Models;
using CityLink_SCP.ViewModels;
using CityLink_SCP.PageModels;
using CityLink_SCP.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
namespace CityLink_SCP.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly DatabaseService _dbService;
		private readonly XmlConfigService _xmlService;
		public HomeController(ILogger<HomeController> logger, DatabaseService dbService, XmlConfigService xmlService)
        {
            _logger = logger;
			_dbService = dbService;
			_xmlService = xmlService;
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
            return View(GetIndexViewModel());
        }
		public IActionResult News()
        {
            return View();
        }

		public IActionResult Events()
		{
			return View();
		}

		public IActionResult BookEvent()
		{
			return View();
		}

        public IActionResult Services()
		{
            return View(GetIndexViewModel());
        }
        public IActionResult BookService()
        {
            return View();
        }


		public IActionResult ContactUs()
		{
			return View(new ContactUs());
		}

		


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

		#region Helper Methods
		private IndexViewModel GetIndexViewModel()
		{
			var events = _dbService._context.Events.ToList().ToCardViewModel();
			var services = _dbService._context.Services.ToList().ToCardViewModel();
			var faqs = _xmlService.GetActive<FAQViewModel>();
			var model = new IndexViewModel
			{
				Events = events.Events.Count > 0 ? events : GetEventsDefault(),
				Services = services.Services.Count > 0 ? services : GetServicesDefault(),
				FAQs = faqs?.FAQs.Count > 0 ? faqs : GetFAQsDefault(),
			};
			return model;
		}
		private EventsViewModel GetEventsDefault()
		{
			var content = System.IO.File.ReadAllText("XML\\EventsDefault.xml");
			return _xmlService.ToViewModel<EventsViewModel>(content)!;
		}
		private ServicesViewModel GetServicesDefault()
		{
			var content = System.IO.File.ReadAllText("XML\\ServicesDefault.xml");
            return _xmlService.ToViewModel<ServicesViewModel>(content)!;
        }
		private FAQViewModel GetFAQsDefault()
		{
			var content = System.IO.File.ReadAllText("XML\\FAQsDefault.xml");
			return _xmlService.ToViewModel<FAQViewModel>(content);
		}
		#endregion
	}
}
