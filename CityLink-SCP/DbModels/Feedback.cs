namespace CityLink_SCP.DbModels
{
	public class Feedback
	{
		public int Id { get; set; }
		public string Message { get; set; } = null!;
		public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
		public string? Resolution_Message { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? ResolvedAt { get; set; }
		
		// Foreign key to Staff
		public int StaffId { get; set; }
		public Staff Staff { get; set; } = null!;
		
		// Foreign key to User
		public int UserId { get; set; }
		public User User { get; set; } = null!;
	}
	public enum FeedbackStatus
	{
		Pending,
		InProgress,
		Resolved,
		Closed
	}
}
