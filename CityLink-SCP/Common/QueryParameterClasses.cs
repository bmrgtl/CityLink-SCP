// One class per controller/resource (X = your entity name, e.g. Product, Order, User)
// Properties = columns you want to filter/search on
// Nullable = optional filter (null means "don't filter on this")
// Non-nullable = always applied (rare, usually just leave things nullable)

using CityLink_SCP.Common;
using CityLink_SCP.DbModels;

public class EventQueryParams : QueryParameters
{
    public int? EventId { get; set; }

    // Title and/or Description 
    public string? SearchTerm { get; set; }
    public string? Location { get; set; }
    public decimal? MinCost { get; set; }
    public decimal? MaxCost { get; set; }
    public bool? TicketsAvaliable { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    // Attributes so that its not binded in a controller
    // other than admin or by a non-admin user
    [UserRole("Staff")]
    [QueryController("Admin")]
    public string? StaffId { get; set; }
}

public class EventRegistrationQueryParams : QueryParameters
{
    public int? MinAttendees { get; set; }
    public int? MaxAttendees { get; set; }
    public double? MinCost { get; set; }
    public double? MaxCost { get; set; }

    // Attributes so that its not binded in a controller
    // other than admin or by a non-admin user
    [UserRole("Staff")]
    [QueryController("Admin")]
    public string? UserId { get; set; }
    
    [UserRole("Staff")]
    [QueryController("Admin")]
    public int? EventId { get; set; }
}

public class ServiceQueryParams : QueryParameters
{
    public int? ServiceId { get; set; }
    // Title, Description and/or Location
    public string? SearchTerm { get; set; }
    public TimeOnly? From { get; set; }
    public TimeOnly? To { get; set; }

    // Attributes so that its not binded in a controller
    // other than admin or by a non-admin user
    [UserRole("Staff")]
    [QueryController("Admin")]
    public string? StaffId { get; set; }
} 

public class ServiceBookingQueryParams : QueryParameters
{
    public double? MinCost { get; set; }
    public double? MaxCost { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    // Attributes so that its not binded in a controller
    // other than admin or by a non-admin user
    [UserRole("Staff")]
    [QueryController("Admin")]
    public string? UserId { get; set; }

    [UserRole("Staff")]
    [QueryController("Admin")]
    public int? ServiceId { get; set; }
}
public class FeedBackQueryParams : QueryParameters
{
    public int? FeedBackId { set; get; }
    // User Message, and Resolution Message
    public string? SearchTerm { set; get; }
    public FeedbackStatus? Status { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? ResolvedAfter { get; set; }
    public DateTime? ResolvedBefore { get; set; }

    // Attributes so that its not binded in a controller
    // other than admin or by a non-admin user
    [UserRole("Staff")]
    [QueryController("Admin")]
    public string? UserId { get; set; }

    [UserRole("Staff")]
    [QueryController("Admin")]
    public string? StaffId { get; set; }
}

public class XmlConfigQueryParams : QueryParameters
{
    public int? XmlConfigId { set; get; }
    public string? Label { set; get; }
    public string? Type { set; get; }
    public string? Version { set; get; }
    public bool? IsActive { get; set; }
    public DateTime? UploadedAfter { get; set; }
    public DateTime? UploadedBefore { get; set; }

    // Attributes so that its not binded in a controller
    // other than admin or by a non-admin user
    [UserRole("Staff")]
    [QueryController("Admin")]
    public string? StaffId { get; set; }
}

public class UserQueryParams : QueryParameters
{
    public string? UserId { set; get; }
    /// <summary>Searches across FirstName, LastName and Email (OR match).</summary>
    public string? SearchTerm { get; set; }
    public string? FirstName { set; get; }
    public string? LastName { set; get; }
    public string? Email { set; get; }
    public string? PhoneNumber { set; get; }
    public string? Address { set; get; }
}

public class StaffQueryParams : QueryParameters
{
    /// <summary>Searches across FirstName, LastName and Email (OR match).</summary>
    public string? SearchTerm { get; set; }
    public string? JobTitle { get; set; }
}
