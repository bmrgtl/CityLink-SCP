namespace CityLink_SCP.DbModels
{
	public class User
	{
		public int Id { get; set; }
		public string First_Name { get; set; } = string.Empty;
		public string Last_Name { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Phone_Number { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public ICollection<EventRegistration> EventRegistrations { get; set; } = [];
		public ICollection<ServiceBooking> ServiceBookings { get; set; } = [];
		public ICollection<Feedback> Feedbacks { get; set; } = [];
	}
}
