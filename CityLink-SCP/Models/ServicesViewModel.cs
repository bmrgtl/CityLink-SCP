namespace CityLink_SCP.Models
{
    public class ServiceViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ButtonLabel { get; set; } = string.Empty;
        //public Uri ImageUrl { get; set; }
	}
    public class ServicesViewModel : IXmlViewModel
    {
        public string PartialName => "_ServiceCards";
        public List<ServiceViewModel> Services { get; set; } = new();
	}
}
