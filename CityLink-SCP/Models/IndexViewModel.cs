using System.Xml.Serialization;

namespace CityLink_SCP.Models
{
    [XmlRoot("HomeIndex")]
    public class IndexViewModel : IXmlViewModel
    {
        public string PartialName => "Index";
        public List<CardViewModel> Events { get; set; } = new();
        public List<CardViewModel> Services { get; set; } = new();
        public List<FAQViewModel> FAQs { get; set; } = new ();
	}
}
