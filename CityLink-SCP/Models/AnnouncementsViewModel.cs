using System.Xml.Serialization;

namespace CityLink_SCP.Models
{
    public class Announcement
    {
        public string Title       { get; set; } = "Announcement";
        public string Body        { get; set; } = "";
        public string ButtonLabel { get; set; } = "Read More";
        public string ButtonUrl   { get; set; } = "#";
    }

    public class AnnouncementsViewModel : IXmlViewModel
    {
        [XmlIgnore]
        public string PartialName => "_Announcements";

        public string Eyebrow { get; set; } = "Announcements";
        public string Heading { get; set; } = "What's On";

        [XmlArrayItem("Announcement")]
        public List<Announcement> Items { get; set; } = new();
    }
}
