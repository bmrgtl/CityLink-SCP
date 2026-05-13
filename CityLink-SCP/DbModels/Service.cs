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
		public ICollection<ServiceBooking> ServiceBookings { get; set; } = [];

        // Foreign key to Staff Member that created the Service
        public int StaffId { get; set; }
        public Staff Staff { get; set; } = null!;
    }
}
