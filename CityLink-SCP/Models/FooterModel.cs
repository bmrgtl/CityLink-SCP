using System.Xml.Serialization;

namespace CityLink_SCP.Models
{
    [XmlRoot("Footer")]
    public class FooterModel : IXmlViewModel
    {
        public string PartialName => "Footer";
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Disclaimer { get; set; }
        public List<TitleLink> QuickLinks { get; set; }
        public List<TitleLink> SocialMedia { get; set; }

    }
    public class TitleLink
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
