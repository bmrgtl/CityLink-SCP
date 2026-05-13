namespace CityLink_SCP.DbModels
{
	public class EventRegistration
	{
		public int NumberOfAttendees { get; set; }
		public double TotalCost { get; set; }

		// Foreign key to User
		public int UserId { get; set; }
		public User User { get; set; } = null!;
		
		// Foreign key to Event
		public int EventId { get; set; }
		public Event Event { get; set; } = null!;
	}
}
