namespace CityLink_SCP.DbModels
{
	public class ServiceBooking
	{
		public double TotalCost { get; set; }
		public DateTime Start_Time { get; set; }
		public DateTime End_Time { get; set; }
		
		// Foreign key to User
		public int UserId { get; set; }
		public User User { get; set; } = null!;
		
		// Foreign key to Service
		public int ServiceId { get; set; }
		public Service Service { get; set; } = null!;
	}
}
