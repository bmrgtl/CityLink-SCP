using System.Xml.Serialization;

namespace CityLink_SCP.Models
{
	public class FAQ 
	{
		public string Question { get; set; }
		public string Answer { get; set; }
	}
	public class FAQViewModel : IXmlViewModel
	{
		public string PartialName => "_FAQ";
		public List<FAQ> FAQs { get; set; }
	}
}
