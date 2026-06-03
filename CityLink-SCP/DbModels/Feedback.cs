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
		public string StaffId { get; set; }
		public ApplicationStaff Staff { get; set; } = null!;
		
		// Foreign key to User (nullable — allows guest submissions)
		public string? UserId { get; set; }
		public ApplicationUser? User { get; set; }
	}
	public enum FeedbackStatus
	{
		Pending,
		InProgress,
		Resolved,
		Closed
	}
}
