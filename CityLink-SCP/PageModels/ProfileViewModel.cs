using CityLink_SCP.DbModels;

namespace CityLink_SCP.PageModels
{
    public class ProfileViewModel
    {
        public string  FirstName   { get; set; } = "";
        public string  LastName    { get; set; } = "";
        public string  Email       { get; set; } = "";
        public string  PhoneNumber { get; set; } = "";
        public string  Address     { get; set; } = "";
        public bool    IsStaff     { get; set; }
        public string? JobTitle    { get; set; }

        public List<EventRegistration> EventRegistrations { get; set; } = new();
        public List<ServiceBooking>    ServiceBookings    { get; set; } = new();

        // Convenience
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool   IsGuest  => string.IsNullOrEmpty(Email);
    }
}
