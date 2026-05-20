using CityLink_SCP.Models;
using CityLink_SCP.Services;
using CityLink_SCP.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CityLink_SCP.Controllers
{
    public class AdminController : Controller
    {
        private readonly XmlConfigService _xmlService;
        private readonly DatabaseService _dbService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(XmlConfigService xmlConfigService, DatabaseService dbService, ILogger<AdminController> logger)
        {
            _xmlService = xmlConfigService;
            _dbService = dbService;
            _logger = logger;
        }

        // GET: /Admin
        public IActionResult Index()
        {
            var configs = _dbService._context.XML_Configurations
                .OrderByDescending(x => x.UploadedAt)
                .Select(x => x.ToViewModel())
                .ToList();

            var events = _dbService._context.Events
                .OrderByDescending(x => x.Id)
                .ToList();

            var feedbacks = _dbService._context.Feedbacks
                .OrderByDescending(x => x.Id)
                .ToList();

            var vm = new AdminIndexViewModel
            {
                XmlConfigs = configs,
                Events = events,
                Feedbacks = feedbacks,
                AvailableTypes = XmlConfigService.GetAvailableTypes()
            };

            return View(vm);
        }

        // GET: /Admin/LoadXml?id=1
        // Returns the raw pretty-printed XML string for a given config record
        [HttpGet]
        public IActionResult LoadXml(int id)
        {
            var content = _xmlService.GetXmlContentById(id);
            if (content == null) return NotFound($"XML config not found for ID: {id}");
            return Content(content, "application/xml");
        }

        // GET: /Admin/LoadTemplate?typeName=FooterModel
        // Returns an empty template XML for the given ViewModel type
        [HttpGet]
        public IActionResult LoadTemplate(string typeName)
        {
            var template = _xmlService.GenerateTemplate(typeName);
            if (string.IsNullOrEmpty(template))
                return NotFound($"No template available for type: {typeName}");
            return Content(template, "application/xml");
        }

        // GET: /Admin/GetXmlPreview?id=1
        // Returns the rendered partial view HTML for a given config record
        [HttpGet]
        public async Task<IActionResult> GetXmlPreview(int id)
        {
            var model = await Task.Run(() => _xmlService.GetXmlViewModel(id));
            if (model == null) return NotFound($"XML config not found for ID: {id}");

            if (model is not IXmlViewModel xmlViewModel)
                return BadRequest("This config type does not support preview.");

            return PartialView(xmlViewModel.PartialName, model);
        }

        // GET: /Admin/GetActivePreview?typeName=FooterModel
        // Returns the rendered partial view for the currently active config of a type
        [HttpGet]
        public IActionResult GetActivePreview(string typeName)
        {
            var vmType = XmlConfigService.GetViewModelType(typeName);
            if (vmType == null) return NotFound($"Unknown type: {typeName}");

            var model = _xmlService.GetActive(vmType);
            if (model == null || model.GetType() is not IXmlViewModel xmlViewModel)
            {
                return BadRequest("This type does not support preview.");
            }
            return PartialView(xmlViewModel.PartialName, model);
        }

        // POST: /Admin/UploadXmlConfig
        // Validates, then saves a new version of an XML config.
        [HttpPost]
        public async Task<IActionResult> UploadXmlConfig([FromForm] XmlConfigDto xmlConfig)
        {
            if (string.IsNullOrWhiteSpace(xmlConfig.XmlContent))
                return BadRequest("XML content cannot be empty.");

            if (string.IsNullOrWhiteSpace(xmlConfig.Type))
                return BadRequest("Config type must be specified.");

            var (valid, error) = _xmlService.Validate(xmlConfig.Type, xmlConfig.XmlContent);
            if (!valid)
                return BadRequest("Invalid XML: " + error);

            try
            {
                // TODO: Replace with real authenticated staff from identity middleware
                var staff = _dbService._context.Staff.First();
                await _xmlService.SaveNewVersionAsync(staff, xmlConfig);
                return Ok(new { message = "Configuration saved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save XML config");
                return StatusCode(500, "Failed to save configuration: " + ex.Message);
            }
        }

        // POST: /Admin/ActivateVersion?recordId=3
        // Rolls back to / activates a specific historical version.
        [HttpPost]
        public async Task<IActionResult> ActivateVersion(int recordId)
        {
            var success = await _xmlService.ActivateVersionAsync(recordId);
            if (!success) return NotFound($"Config record not found for ID: {recordId}");
            return Ok(new { message = $"Version {recordId} is now active." });
        }

        // GET: /Admin/GetConfigHistory?typeName=FooterModel
        // Returns all versions of a config type (for the history panel).
        [HttpGet]
        public IActionResult GetConfigHistory(string typeName)
        {
            var records = _dbService._context.XML_Configurations
                .Where(x => x.Type == typeName)
                .OrderByDescending(x => x.UploadedAt)
                .Select(x => x.ToViewModel())
                .ToList();

            return Json(records);
        }
    }
}