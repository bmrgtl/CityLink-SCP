namespace CityLink_SCP.Models
{
    public class ServiceViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ButtonLabel { get; set; }
        //public Uri ImageUrl { get; set; }
	}
    public class ServicesViewModel : IXmlViewModel
    {
        public string PartialName => "_ServiceCards";
        public List<ServiceViewModel> Services { get; set; } = new();
	}
}
