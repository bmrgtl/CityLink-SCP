using Microsoft.AspNetCore.Mvc;
using CityLink_SCP.Database;
using CityLink_SCP.Services;
using CityLink_SCP.Models;

namespace CityLink_SCP.ViewComponents;

public class FooterViewComponent : ViewComponent
{
	private readonly XmlConfigService _xml;
	public FooterViewComponent(XmlConfigService xml)
	{
		_xml = xml;
	}
	private FooterModel GetFooterDefault() => _xml.ToViewModel<FooterModel>(System.IO.File.ReadAllText("XML\\FooterDefault.xml"))!;
	public IViewComponentResult Invoke()
	{
		var footer = _xml.GetActive<FooterModel>() ?? GetFooterDefault();
		return View("~/Views/Shared/Footer.cshtml", footer);
	}
}
