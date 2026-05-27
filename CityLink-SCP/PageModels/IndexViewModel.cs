using CityLink_SCP.Models;
using CityLink_SCP.ViewModels;
using System.Xml.Serialization;

namespace CityLink_SCP.PageModels
{
    public class IndexViewModel : IXmlViewModel
    {
        public string PartialName => "Index";
        public EventsViewModel Events { get; set; } = new();
        public ServicesViewModel Services { get; set; } = new();
        public FAQViewModel FAQs { get; set; } = new ();
        public Enquiry Enquiry { get; set; } = new ();
	}
}
