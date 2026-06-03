namespace CityLink_SCP.DbModels
{
	public class Event
	{
		public int Id { get; set; }
		public string Title { get; set; } = null!;
		public string Description { get; set; } = null!;
		public string Location { get; set; } = null!;
		public double Cost { get; set; }
		public int Max_Capcity { get; set; }
		public DateTime Start_Date_Time { get; set; }
		public DateTime End_Date_Time { get; set; }

        // Navigation property for backwards queries
        public ICollection<EventRegistration> EventRegistrations { get; set; } = [];

        // Foreign key to Staff Member that created the Event
        public string StaffId { get; set; }
		public ApplicationStaff Staff { get; set; } = null!;
	}
}
