using System.Xml.Serialization;

namespace CityLink_SCP.Models
{
    public class IndexViewModel : IXmlViewModel
    {
        public string PartialName => "Index";
        public EventsViewModel Events { get; set; } = new();
        public ServicesViewModel Services { get; set; } = new();
        public FAQViewModel FAQs { get; set; } = new ();
	}
}
