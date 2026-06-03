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

		// userId is now a string GUID (Identity's default key type)
		public DbActionResult<ServiceBooking> AddServiceBooking(string userId, int serviceId, DateTime start, DateTime end)
		{
			try
			{
				var availability = IsServiceBookingTimeAvailable(serviceId, start, end);
				if (!availability.Success) return new DbActionResult<ServiceBooking>(false, availability.Message);

				var service = _context.Services.Find(serviceId);
				if (service == null) return new DbActionResult<ServiceBooking>(false, "Service not found");

				var booking = new ServiceBooking
				{
					UserId = userId,
					ServiceId = serviceId,
					Start_Time = start,
					End_Time = end,
					TotalCost = service.Cost * Math.Max(1, (end - start).TotalHours)
				};

				_context.ServiceBookings.Add(booking);
				_context.SaveChanges();
				return new DbActionResult<ServiceBooking>(true, "Service booking added successfully") { Data = booking };
			}
			catch (Exception ex) { return new DbActionResult<ServiceBooking>(false, ex.Message); }
		}

		public DbActionResult IsServiceBookingTimeAvailable(int serviceId, DateTime start, DateTime end)
		{
			try
			{
				if (start >= end) return new DbActionResult(false, "Invalid time period");

				var service = _context.Services.Find(serviceId);
				if (service == null) return new DbActionResult(false, "Service not found");

				var reqStart = TimeOnly.FromDateTime(start);
				var reqEnd = TimeOnly.FromDateTime(end);

				if (reqStart < service.Available_Start_Time || reqEnd > service.Available_End_Time)
					return new DbActionResult(false, "Requested time is outside service availability");

				var hasConflict = _context.ServiceBookings.Any(b =>
					b.ServiceId == serviceId && b.Start_Time < end && b.End_Time > start);

				return hasConflict
					? new DbActionResult(false, "Service is already booked for this time")
					: new DbActionResult(true, "Available");
			}
			catch (Exception ex) { return new DbActionResult(false, ex.Message); }
		}

		public DbActionResult<EventRegistration> AddEventRegistration(string userId, int eventId, int numOfAttendees)
		{
			try
			{
				var ev = _context.Events.Find(eventId);
				if (ev == null) return new DbActionResult<EventRegistration>(false, "Event not found");

				var registration = new EventRegistration
				{
					UserId = userId,
					EventId = eventId,
					NumberOfAttendees = numOfAttendees,
					TotalCost = numOfAttendees * ev.Cost
				};

				_context.EventRegistrations.Add(registration);
				_context.SaveChanges();
				return new DbActionResult<EventRegistration>(true, "Event registration added successfully", registration);
			}
			catch (Exception ex) { return new DbActionResult<EventRegistration>(false, ex.Message); }
		}
	}
}
