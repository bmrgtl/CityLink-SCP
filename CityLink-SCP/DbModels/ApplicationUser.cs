using Microsoft.AspNetCore.Identity;

namespace CityLink_SCP.DbModels
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public string Address  { get; set; } = string.Empty;

        // Navigation properties 
        public ICollection<EventRegistration>  EventRegistrations { get; set; } = [];
        public ICollection<ServiceBooking> ServiceBookings { get; set; } = [];
        public ICollection<Feedback> Feedbacks { get; set; } = [];
    }
}
