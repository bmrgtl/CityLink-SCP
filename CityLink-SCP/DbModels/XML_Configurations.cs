namespace CityLink_SCP.DbModels
{
	public class XML_Configurations
	{
		public int Id { get; set; }
		public string XML { get; set; } = string.Empty;
		public string Type { get; set; } = string.Empty;
		public string Version { get; set; } = string.Empty;
		public DateTime Uploaded_Date_Time { get; set; }
		public DateTime Edited_Date_Time { get; set; }

		// Foreign key to Staff
		public int StaffId { get; set; }
		public Staff Staff { get; set; } = null!;
	}
}
