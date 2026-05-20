namespace CityLink_SCP.DbModels
{
	public class Staff : User
	{
		public string Role { get; set; } = null!;
		public ICollection<XML_Configurations> XML_Configurations { get; set; } = [];
		public ICollection<Service> Services { get; set; } = [];
		public ICollection<Event> Events { get; set; } = [];
		public Staff() : base()
		{
		}
	}
}
