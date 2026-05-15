using CityLink_SCP.Models;
using CityLink_SCP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CityLink_SCP.Controllers
{
    public class AdminController : Controller
    {
        private readonly XmlConfigService _xmlService;
        private readonly DatabaseService _dbService;
        private readonly ILogger _logger;

        public AdminController(XmlConfigService xmlConfigService, DatabaseService dbService, ILogger<AdminController> logger)
        {
            _xmlService = xmlConfigService;
            _dbService = dbService;
            _logger = logger;
        }
        // GET: AdminController
        public ActionResult Index()
        {
            return View();
        }

        // GET: AdminController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AdminController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AdminController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AdminController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // Returns just the XML string
        [HttpGet]
        public IActionResult LoadXml(int id)
        {
            var content = _xmlService.GetXmlContentById(id);
            if (content == null) return NotFound($"XML not found for ID: {id}");
            return Content(content, "application/xml");
        }

        // Returns rendered HTML that the specified xml refers to
        [HttpGet]
        public async Task<IActionResult> GetXmlPreview(int id = 1)
        {
            var model = await Task.Run(() => _xmlService.GetXmlViewModel(id));
            if (model == null)
            {
                return NotFound();
            }
            if (model is not IXmlViewModel xmlViewModel)
            {
                return BadRequest();
            }
            return PartialView(xmlViewModel.PartialName, model);
        }

        // Saves XML, returns rendered card HTML
        [HttpPost]
        public async Task<IActionResult> UploadCards([FromForm] XmlConfigDto xmlConfig)
        {
            try
            {
                // Validate XML before saving
                var validXml = _xmlService.Validate(xmlConfig.Type, xmlConfig.XmlContent);
                if (!validXml.valid)
                {
                    return BadRequest("Invalid XML: " + validXml.error);
                }
                // User Identity call to manager here, later
                await _xmlService.SaveNewVersionAsync(_dbService._context.Staff.First(), xmlConfig);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid XML format: " + ex.Message);
            }
        }

        // Saves XML, returns the XML back
        [HttpPost]
        public IActionResult UploadXml([FromBody] string xml)
        {
            try
            {
                System.IO.File.WriteAllText("XML\\Card.xml", xml);
                return Content(xml, "application/xml");
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid XML format: " + ex.Message);
            }
        }
    }
}
