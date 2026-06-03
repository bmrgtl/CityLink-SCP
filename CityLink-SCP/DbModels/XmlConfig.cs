namespace CityLink_SCP.DbModels
{
    public class XmlConfig
    {
        public int Id { get; set; }
        public string XmlContent { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public DateTime UploadedAt { get; set; }
        public string Label { get; set; } = string.Empty;

        // Foreign key to Staff
        public string StaffId { get; set; }
        public ApplicationStaff Staff { get; set; } = null!;
    }
}
