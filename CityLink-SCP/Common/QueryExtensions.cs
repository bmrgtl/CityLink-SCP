using CityLink_SCP.DbModels;

namespace CityLink_SCP.Common;

public static class QueryExtensions
{
    public static IQueryable<Event> ApplyQuery(
        this IQueryable<Event> source,
        EventQueryParams query)
    {
        if (query.EventId.HasValue)
            source = source.Where(e => e.Id == query.EventId);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            source = source.Where(e =>
                e.Title.Contains(query.SearchTerm) ||
                e.Description.Contains(query.SearchTerm));

        if (!string.IsNullOrWhiteSpace(query.Location))
            source = source.Where(e => e.Location.Contains(query.Location));

        if (query.MinCost.HasValue)
            source = source.Where(e => e.Cost >= (double)query.MinCost);

        if (query.MaxCost.HasValue)
            source = source.Where(e => e.Cost <= (double)query.MaxCost);

        if (query.TicketsAvaliable.HasValue && query.TicketsAvaliable.Value)
            source = source.Where(e => e.EventRegistrations.Sum(er => er.NumberOfAttendees) < e.Max_Capcity);

        if (query.From.HasValue)
            source = source.Where(e => e.Start_Date_Time >= query.From);

        if (query.To.HasValue)
            source = source.Where(e => e.End_Date_Time <= query.To);

        if (!string.IsNullOrWhiteSpace(query.StaffId))
            source = source.Where(e => e.StaffId == query.StaffId);

        source = query.SortBy switch
        {
            "Title" => query.SortOrder == "asc" ? source.OrderBy(e => e.Title) : source.OrderByDescending(e => e.Title),
            "Cost" => query.SortOrder == "asc" ? source.OrderBy(e => e.Cost) : source.OrderByDescending(e => e.Cost),
            "Start_Date_Time" => query.SortOrder == "asc" ? source.OrderBy(e => e.Start_Date_Time) : source.OrderByDescending(e => e.Start_Date_Time),
            "Max_Capcity" => query.SortOrder == "asc" ? source.OrderBy(e => e.Max_Capcity) : source.OrderByDescending(e => e.Max_Capcity),
            _ => query.SortOrder == "asc" ? source.OrderBy(e => e.Id) : source.OrderByDescending(e => e.Id),
        };

        return source
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);
    }

    public static IQueryable<EventRegistration> ApplyQuery(
        this IQueryable<EventRegistration> source,
        EventRegistrationQueryParams query)
    {
        if (query.MinAttendees.HasValue)
            source = source.Where(er => er.NumberOfAttendees >= query.MinAttendees);

        if (query.MaxAttendees.HasValue)
            source = source.Where(er => er.NumberOfAttendees <= query.MaxAttendees);

        if (query.MinCost.HasValue)
            source = source.Where(er => er.TotalCost >= query.MinCost);

        if (query.MaxCost.HasValue)
            source = source.Where(er => er.TotalCost <= query.MaxCost);

        if (!string.IsNullOrWhiteSpace(query.UserId))
            source = source.Where(er => er.UserId == query.UserId);

        if (query.EventId.HasValue)
            source = source.Where(er => er.EventId == query.EventId);

        source = query.SortBy switch
        {
            "NumberOfAttendees" => query.SortOrder == "asc" ? source.OrderBy(er => er.NumberOfAttendees) : source.OrderByDescending(er => er.NumberOfAttendees),
            "TotalCost" => query.SortOrder == "asc" ? source.OrderBy(er => er.TotalCost) : source.OrderByDescending(er => er.TotalCost),
            _ => query.SortOrder == "asc" ? source.OrderBy(er => er.EventId) : source.OrderByDescending(er => er.EventId),
        };

        return source
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);
    }

    public static IQueryable<Service> ApplyQuery(
        this IQueryable<Service> source,
        ServiceQueryParams query)
    {
        if (query.ServiceId.HasValue)
            source = source.Where(s => s.Id == query.ServiceId);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            source = source.Where(s =>
                s.Title.Contains(query.SearchTerm) ||
                s.Description.Contains(query.SearchTerm) ||
                s.Location.Contains(query.SearchTerm));

        if (query.From.HasValue)
            source = source.Where(s => s.Available_Start_Time >= query.From);

        if (query.To.HasValue)
            source = source.Where(s => s.Available_End_Time <= query.To);

        if (!string.IsNullOrWhiteSpace(query.StaffId))
            source = source.Where(s => s.StaffId == query.StaffId);

        source = query.SortBy switch
        {
            "Title" => query.SortOrder == "asc" ? source.OrderBy(s => s.Title) : source.OrderByDescending(s => s.Title),
            "Cost" => query.SortOrder == "asc" ? source.OrderBy(s => s.Cost) : source.OrderByDescending(s => s.Cost),
            "Available_Start_Time" => query.SortOrder == "asc" ? source.OrderBy(s => s.Available_Start_Time) : source.OrderByDescending(s => s.Available_Start_Time),
            _ => query.SortOrder == "asc" ? source.OrderBy(s => s.Id) : source.OrderByDescending(s => s.Id),
        };

        return source
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);
    }

    public static IQueryable<ServiceBooking> ApplyQuery(
        this IQueryable<ServiceBooking> source,
        ServiceBookingQueryParams query)
    {
        if (query.MinCost.HasValue)
            source = source.Where(sb => sb.TotalCost >= query.MinCost);

        if (query.MaxCost.HasValue)
            source = source.Where(sb => sb.TotalCost <= query.MaxCost);

        if (query.From.HasValue)
            source = source.Where(sb => sb.Start_Time >= query.From);

        if (query.To.HasValue)
            source = source.Where(sb => sb.End_Time <= query.To);

        if (!string.IsNullOrWhiteSpace(query.UserId))
            source = source.Where(sb => sb.UserId == query.UserId);

        if (query.ServiceId.HasValue)
            source = source.Where(sb => sb.ServiceId == query.ServiceId);

        source = query.SortBy switch
        {
            "TotalCost" => query.SortOrder == "asc" ? source.OrderBy(sb => sb.TotalCost) : source.OrderByDescending(sb => sb.TotalCost),
            "Start_Time" => query.SortOrder == "asc" ? source.OrderBy(sb => sb.Start_Time) : source.OrderByDescending(sb => sb.Start_Time),
            "End_Time" => query.SortOrder == "asc" ? source.OrderBy(sb => sb.End_Time) : source.OrderByDescending(sb => sb.End_Time),
            _ => query.SortOrder == "asc" ? source.OrderBy(sb => sb.ServiceId) : source.OrderByDescending(sb => sb.ServiceId),
        };

        return source
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);
    }

    public static IQueryable<Feedback> ApplyQuery(
        this IQueryable<Feedback> source,
        FeedBackQueryParams query)
    {
        if (query.FeedBackId.HasValue)
            source = source.Where(f => f.Id == query.FeedBackId);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            source = source.Where(f =>
                f.Message.Contains(query.SearchTerm) ||
                (f.Resolution_Message != null && f.Resolution_Message.Contains(query.SearchTerm)));

        if (query.Status.HasValue)
            source = source.Where(f => f.Status == query.Status);

        if (query.CreatedAfter.HasValue)
            source = source.Where(f => f.CreatedAt >= query.CreatedAfter);

        if (query.CreatedBefore.HasValue)
            source = source.Where(f => f.CreatedAt <= query.CreatedBefore);

        if (query.ResolvedAfter.HasValue)
            source = source.Where(f => f.ResolvedAt >= query.ResolvedAfter);

        if (query.ResolvedBefore.HasValue)
            source = source.Where(f => f.ResolvedAt <= query.ResolvedBefore);

        if (!string.IsNullOrWhiteSpace(query.UserId))
            source = source.Where(f => f.UserId == query.UserId);

        if (!string.IsNullOrWhiteSpace(query.StaffId))
            source = source.Where(f => f.StaffId == query.StaffId);

        source = query.SortBy switch
        {
            "Status" => query.SortOrder == "asc" ? source.OrderBy(f => f.Status) : source.OrderByDescending(f => f.Status),
            "CreatedAt" => query.SortOrder == "asc" ? source.OrderBy(f => f.CreatedAt) : source.OrderByDescending(f => f.CreatedAt),
            "ResolvedAt" => query.SortOrder == "asc" ? source.OrderBy(f => f.ResolvedAt) : source.OrderByDescending(f => f.ResolvedAt),
            _ => query.SortOrder == "asc" ? source.OrderBy(f => f.Id) : source.OrderByDescending(f => f.Id),
        };

        return source
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);
    }

    public static IQueryable<XmlConfig> ApplyQuery(
        this IQueryable<XmlConfig> source,
        XmlConfigQueryParams query)
    {
        if (query.XmlConfigId.HasValue)
            source = source.Where(x => x.Id == query.XmlConfigId);

        if (!string.IsNullOrWhiteSpace(query.Label))
            source = source.Where(x => x.Label.Contains(query.Label));

        if (!string.IsNullOrWhiteSpace(query.Type))
            source = source.Where(x => x.Type == query.Type);

        if (!string.IsNullOrWhiteSpace(query.Version))
            source = source.Where(x => x.Version == query.Version);

        if (query.IsActive.HasValue)
            source = source.Where(x => x.IsActive == query.IsActive);

        if (query.UploadedAfter.HasValue)
            source = source.Where(x => x.UploadedAt >= query.UploadedAfter);

        if (query.UploadedBefore.HasValue)
            source = source.Where(x => x.UploadedAt <= query.UploadedBefore);

        if (!string.IsNullOrWhiteSpace(query.StaffId))
            source = source.Where(x => x.StaffId == query.StaffId);

        source = query.SortBy switch
        {
            "Label" => query.SortOrder == "asc" ? source.OrderBy(x => x.Label) : source.OrderByDescending(x => x.Label),
            "Type" => query.SortOrder == "asc" ? source.OrderBy(x => x.Type) : source.OrderByDescending(x => x.Type),
            "Version" => query.SortOrder == "asc" ? source.OrderBy(x => x.Version) : source.OrderByDescending(x => x.Version),
            "UploadedAt" => query.SortOrder == "asc" ? source.OrderBy(x => x.UploadedAt) : source.OrderByDescending(x => x.UploadedAt),
            _ => query.SortOrder == "asc" ? source.OrderBy(x => x.Id) : source.OrderByDescending(x => x.Id),
        };

        return source
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);
    }

    public static IQueryable<ApplicationUser> ApplyQuery(
        this IQueryable<ApplicationUser> source,
        UserQueryParams query)
    {
        if (!string.IsNullOrWhiteSpace(query.UserId))
            source = source.Where(u => u.Id == query.UserId);

        // Combined search across name + email (OR)
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            source = source.Where(u =>
                u.FirstName.Contains(query.SearchTerm) ||
                u.LastName.Contains(query.SearchTerm) ||
                (u.Email != null && u.Email.Contains(query.SearchTerm)));

        // Individual field filters (used by API callers)
        if (!string.IsNullOrWhiteSpace(query.FirstName))
            source = source.Where(u => u.FirstName.Contains(query.FirstName));

        if (!string.IsNullOrWhiteSpace(query.LastName))
            source = source.Where(u => u.LastName.Contains(query.LastName));

        if (!string.IsNullOrWhiteSpace(query.Email))
            source = source.Where(u => u.Email != null && u.Email.Contains(query.Email));

        if (!string.IsNullOrWhiteSpace(query.PhoneNumber))
            source = source.Where(u => u.PhoneNumber == query.PhoneNumber);

        if (!string.IsNullOrWhiteSpace(query.Address))
            source = source.Where(u => u.Address.Contains(query.Address));

        source = query.SortBy switch
        {
            "FirstName" => query.SortOrder == "asc" ? source.OrderBy(u => u.FirstName) : source.OrderByDescending(u => u.FirstName),
            "LastName"  => query.SortOrder == "asc" ? source.OrderBy(u => u.LastName)  : source.OrderByDescending(u => u.LastName),
            "Email"     => query.SortOrder == "asc" ? source.OrderBy(u => u.Email)     : source.OrderByDescending(u => u.Email),
            _           => query.SortOrder == "asc" ? source.OrderBy(u => u.Id)        : source.OrderByDescending(u => u.Id),
        };

        return source
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);
    }

    public static IQueryable<ApplicationStaff> ApplyQuery(
        this IQueryable<ApplicationStaff> source,
        StaffQueryParams query)
    {
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            source = source.Where(s =>
                s.FirstName.Contains(query.SearchTerm) ||
                s.LastName.Contains(query.SearchTerm) ||
                (s.Email != null && s.Email.Contains(query.SearchTerm)));

        if (!string.IsNullOrWhiteSpace(query.JobTitle))
            source = source.Where(s => s.JobTitle != null && s.JobTitle.Contains(query.JobTitle));

        source = query.SortBy switch
        {
            "FirstName" => query.SortOrder == "asc" ? source.OrderBy(s => s.FirstName)  : source.OrderByDescending(s => s.FirstName),
            "LastName"  => query.SortOrder == "asc" ? source.OrderBy(s => s.LastName)   : source.OrderByDescending(s => s.LastName),
            "Email"     => query.SortOrder == "asc" ? source.OrderBy(s => s.Email)      : source.OrderByDescending(s => s.Email),
            "JobTitle"  => query.SortOrder == "asc" ? source.OrderBy(s => s.JobTitle)   : source.OrderByDescending(s => s.JobTitle),
            _           => query.SortOrder == "asc" ? source.OrderBy(s => s.Id)         : source.OrderByDescending(s => s.Id),
        };

        return source
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size);
    }
}