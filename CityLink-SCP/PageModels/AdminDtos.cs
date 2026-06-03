using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityLink_SCP.PageModels
{
    /// <summary>A pair used in the XML editor type dropdown: Value is the internal C# type name; Label is what admins see.</summary>
    public record XmlConfigTypeOption(string Value, string Label);

    public class AdminLoginViewModel
    {
        [Required] [EmailAddress]
        public string Email { get; set; } = "";

        [Required] [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class EventDto
    {
        public int    Id          { get; set; }
        public string Title       { get; set; } = "";
        public string Description { get; set; } = "";
        public string Location    { get; set; } = "";
        public double Cost        { get; set; }
        public int    Max_Capcity { get; set; }
        public DateTime Start_Date_Time { get; set; }
        public DateTime End_Date_Time   { get; set; }
        public string StaffId { get; set; } = "";  
    }


    public class ServiceDto
    {
        public int    Id          { get; set; }
        public string Title       { get; set; } = "";
        public string Description { get; set; } = "";
        public string Location    { get; set; } = "";
        public double Cost        { get; set; }
        public TimeOnly Available_Start_Time { get; set; }
        public TimeOnly Available_End_Time   { get; set; }
        public string StaffId { get; set; } = "";   
    }

    public class FeedbackDto
    {
        public int    Id                  { get; set; }
        public string? Resolution_Message { get; set; }
        public int    Status              { get; set; }
        public string StaffId { get; set; } = "";   
    }


    public class UserDto
    {
        public string Id         { get; set; } = "";  
        public string First_Name { get; set; } = "";
        public string Last_Name  { get; set; } = "";
        public string Email      { get; set; } = "";
        public string Phone_Number { get; set; } = "";
        public string Address    { get; set; } = "";
        public string? Password  { get; set; }
    }

    public class RegisterViewModel
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = "";

        [MaxLength(100)]
        public string LastName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Phone]
        public string? PhoneNumber { get; set; }

        public string Address { get; set; } = "";

        [Required, DataType(DataType.Password), MinLength(8)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class StaffDto
    {
        public string Id         { get; set; } = "";  
        public string First_Name { get; set; } = "";
        public string Last_Name  { get; set; } = "";
        public string Email      { get; set; } = "";
        public string Phone_Number { get; set; } = "";
        public string Address    { get; set; } = "";
        public string JobTitle   { get; set; } = "";
        public string? Password  { get; set; }
    }
}
