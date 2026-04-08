using CityLink_SCP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CityLink_SCP.Controllers
{
    public class AdminController : Controller
    {
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
        public IActionResult LoadXml()
        {
            var content = System.IO.File.ReadAllText("XML\\Card.xml");
            return Content(content, "application/xml");
        }

        // Returns rendered card HTML
        public IActionResult LoadCards()
        {
            var cards = GetCardsFromXml();
            return PartialView("_Card", cards);
        }

        // Saves XML, returns rendered card HTML
        [HttpPost]
        public IActionResult UploadCards([FromBody] string xml)
        {
            try
            {
                System.IO.File.WriteAllText("XML\\Card.xml", xml);
                var cards = GetCardsFromXml();
                return PartialView("_Card", cards);
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

        // Extract the repeated parsing logic
        private List<CardViewModel> GetCardsFromXml()
        {
            var content = System.IO.File.ReadAllText("XML\\Card.xml");
            var xdoc = XDocument.Parse(content);
            return xdoc.Descendants("Card").Select(x => new CardViewModel
            {
                Title = (string)x.Element("Title"),
                Description = (string)x.Element("Description"),
                ButtonLabel = (string)x.Element("ButtonLabel")
            }).ToList();
        }

    }
}
