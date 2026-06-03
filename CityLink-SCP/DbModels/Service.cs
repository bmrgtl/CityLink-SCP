namespace CityLink_SCP.DbModels
{
	public class Service
	{
		public int Id { get; set; }
		public string Title { get; set; } = null!;
		public string Description { get; set; } = null!;
		public string Location { get; set; } = null!;
		public double Cost { get; set; }
		public TimeOnly Available_Start_Time { get; set; }
		public TimeOnly Available_End_Time { get; set; }

		// Navigation property for backwards queries
		public ICollection<ServiceBooking> ServiceBookings { get; set; } = [];

        // Foreign key to Staff Member that created the Service
        public string StaffId { get; set; }
        public ApplicationStaff Staff { get; set; } = null!;
    }
}
