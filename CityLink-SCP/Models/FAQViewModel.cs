namespace CityLink_SCP.Models
{
	public class FAQViewModel : IXmlViewModel
    {
		public string PartialName => "FAQ";
        public string Question { get; set; }
		public string Answer { get; set; }
	}
}
