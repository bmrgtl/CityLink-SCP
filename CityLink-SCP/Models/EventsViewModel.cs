namespace CityLink_SCP.Models
{
    public class EventViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
		public string ButtonLabel { get; set; } = string.Empty;
		//public Uri ImageUrl { get; set; }
	}
    public class EventsViewModel : IXmlViewModel
    {
        public string PartialName => "_EventCards";
        public List<EventViewModel> Events { get; set; } = new();
	}
}
