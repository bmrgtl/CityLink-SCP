namespace CityLink_SCP.Models
{
    public class XmlConfigDto
    {
        public int Id { get; set; }
        public string XmlContent { get; set; }
        public string Type { get; set; }
        public string Version { get; set; }
        public bool IsActive { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Label { get; set; }
    }
}
