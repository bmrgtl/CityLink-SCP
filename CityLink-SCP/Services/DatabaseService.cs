using CityLink_SCP.Common;
using CityLink_SCP.Database;
using CityLink_SCP.DbModels;

namespace CityLink_SCP.Services
{
    public class DatabaseService 
    {
        public readonly CityLinksContext _context;
        public DatabaseService(CityLinksContext context)
        {
            _context = context;
        }

        public DbActionResult<ServiceBooking> AddServiceBooking(int userId, int serviceId, DateTime start, DateTime end)
        {
            try
            {
                // Check time slot availability
                var request = IsServiceBookingTimeAvailable(serviceId, start, end);
                if (!request.Success)
                {
                    return request;
                }

                // Guard
                var service = _context.Services.Find(serviceId);
                if (service == null)
                {
                    return new DbActionResult(false, "Service not found");
                }

                var booking = new ServiceBooking
                {
                    UserId = userId,
                    ServiceId = serviceId,
                    Start_Time = start,
                    End_Time = end,
                    TotalCost = service!.Cost * Math.Max(1, (end - start).TotalHours)
                    // Example Calc: Might add xml config that is function to calc this -> total cost = hours * cost, min 1 hour cost no matter time
                };

                _context.ServiceBookings.Add(booking);
                _context.SaveChanges();

                return new DbActionResult<ServiceBooking>(true, "Service booking added successfully") { Data = booking };
            }
            catch (Exception ex)
            {
                return new DbActionResult(false, ex.Message);
            }
        }
        public DbActionResult IsServiceBookingTimeAvailable(int serviceId, DateTime start, DateTime end)
        {
            try
            {
                // Reject invalid queries
                if (start > end || start == end)
                {
                    return new DbActionResult(false, "Request time period is invalid");
                }

                // Get the service to check against its available window
                var service = _context.Services.Find(serviceId);
                if (service == null)
                {
                    return new DbActionResult(false, "Service not found");
                }

                // Check requested times fall within service's available window
                var requestedStart = TimeOnly.FromDateTime(start);
                var requestedEnd = TimeOnly.FromDateTime(end);

                if (requestedStart < service.Available_Start_Time || requestedEnd > service.Available_End_Time)
                {
                    return new DbActionResult(false, "Requested time is outside service availability");
                }

                // Check for overlapping bookings on the same service
                var hasConflict = _context.ServiceBookings.Any(b =>
                    b.ServiceId == serviceId &&
                    b.Start_Time < end &&
                    b.End_Time > start
                );

                if (hasConflict)
                {
                    return new DbActionResult(false, "Service is already booked for this time");
                }

                return new DbActionResult(true, "Service booking time is available");
            }
            catch (Exception ex)
            {
                return new DbActionResult(false, ex.Message);
            }
        }
        public DbActionResult<EventRegistration> AddEventRegistration(int userId, int eventId, int numOfAttendees)
        {
            try
            {
                // Guard
                var eventt = _context.Events.Find(eventId);
                if (eventt == null)
                {
                    return new DbActionResult(false, "Event not Found");
                }
                var registration = new EventRegistration
                {
                    UserId = userId,
                    EventId = eventId,
                    NumberOfAttendees = numOfAttendees,
                    TotalCost = numOfAttendees * eventt.Cost
                };

                _context.EventRegistrations.Add(registration);
                _context.SaveChanges();
                
                return new DbActionResult<EventRegistration>(true, "Event registration added successfully", registration);
            }
            catch (Exception ex)
            {
                return new DbActionResult(false, ex.Message);
            }
        }
        public DbActionResult<User> AddUser(User newUser)
        {
            try
            {
                var emailConflict = _context.Users.Any(u => u.Email == newUser.Email);
                if (emailConflict)
                {
                    return new DbActionResult(false, "Email already in use");
                }

                _context.Users.Add(newUser);
                _context.SaveChanges();
                return new DbActionResult<User>(true, "User added successfully", newUser);
            }
            catch (Exception ex)
            {
                return new DbActionResult(false, ex.Message);
            }
        }


    }
}
