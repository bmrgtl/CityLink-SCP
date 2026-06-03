namespace CityLink_SCP.DbModels
{
    public class ApplicationStaff : ApplicationUser
    {
        public string JobTitle { get; set; } = string.Empty;
        public ICollection<XmlConfig> XML_Configurations { get; set; } = [];
        public ICollection<Service> Services { get; set; } = [];
        public ICollection<Event> Events { get; set; } = [];
        public ICollection<Feedback> Feedbacks { get; set; } = [];
    }
}
