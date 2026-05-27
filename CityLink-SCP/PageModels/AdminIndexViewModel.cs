using CityLink_SCP.DbModels;
using CityLink_SCP.Models;
using System.Xml.Serialization;

namespace CityLink_SCP.PageModels
{
    /// <summary>
    /// View model for the Admin dashboard Index page.
    /// </summary>
    public class AdminIndexViewModel
    {
		public List<XmlConfigDto> XmlConfigs { get; set; } = new();
        public List<Service> Services { get; set; } = new();
		public List<Event> Events { get; set; } = new();
		public List<Feedback> Feedbacks { get; set; } = new();

        /// <summary>Registered XML config type names for the editor dropdown.</summary>
        public List<string> AvailableTypes { get; set; } = new();
    }
}
