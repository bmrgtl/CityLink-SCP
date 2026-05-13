using CityLink_SCP.DbModels;
using CityLink_SCP.Services;

namespace CityLink_SCP.Database
{
	public static class DbInitialiser
	{
		public static void Initialise(CityLinksContext context, DatabaseService dbService)
		{
			if (context.Users.Any())
			{
				context.EventRegistrations.RemoveRange(context.EventRegistrations);
				context.ServiceBookings.RemoveRange(context.ServiceBookings);
				context.Services.RemoveRange(context.Services);
				context.Events.RemoveRange(context.Events);
				context.Staff.RemoveRange(context.Staff);
				context.Users.RemoveRange(context.Users);
				context.SaveChanges();
			}
			int usersNum = 20;
            int servicesNum = 20;
			int eventsNum = 20;

            var staff = new List<Staff>
			{
				new Staff { Id = 1, Email = "admin@admin.com",		 Role = "Admin",	PasswordHash = "admin" },
				new Staff { Id = 2, Email = "it_admin@it_admin.com", Role = "IT Admin", PasswordHash = "itadmin" }
			};
			var usersRange = Enumerable.Range(0, usersNum).Select(i => 
				new User {
					First_Name = $"John_{i}",
					Last_Name = $"Doe_{i}",
                    Email = $"user_{i}@user_{i}.com",
                    Phone_Number = $"0{400000000 + i}",
                    Address = $"{i} Waverly Place, 60{i}, Australia",
                    PasswordHash = $"user_{i}"
                }
			);
			var eventsRange = Enumerable.Range(0, eventsNum).Select(i =>
				new Event
				{
					Title = $"Event_{i}",
					Description = $"Description for Event_{i}",
					Location = $"Location_{i}",
					Cost = i * 10.0f,
					Max_Capcity = i,
					Start_Date_Time = DateTime.Now.AddDays(i),
					End_Date_Time = DateTime.Now.AddDays(i + i),
					StaffId = 1,
				}
			);
			var servicesRange = Enumerable.Range(0, servicesNum).Select(i =>
                new Service
                {
                    Title = $"Service_{i}",
                    Description = $"Description for Service_{i}",
                    Location = $"Location_{i}",
                    Cost = i * 10.0f,
                    Available_Start_Time = TimeOnly.Parse($"08:00:00"),
                    Available_End_Time = TimeOnly.Parse($"17:00:00"),
                    StaffId = 1,
                }
			);

			context.Staff.AddRange(staff);
			context.Users.AddRange(usersRange);
			context.Events.AddRange(eventsRange);
			context.Services.AddRange(servicesRange);
			context.SaveChanges();
			var users = context.Users.ToList();
			var events = context.Events.ToList();
			var services = context.Services.ToList();

			int j = 0;
			foreach (var user in users)
			{
				foreach(var service in services)
				{
					var start = DateTime.Parse("2024-01-01T08:00:00").AddMinutes(j * 30);
					var end = start.AddMinutes(30);

					dbService.AddServiceBooking(user.Id, service.Id, start, end);
					j++;
				}
				j = 0;
				foreach (var ev in events)
				{
					dbService.AddEventRegistration(user.Id, ev.Id, (j % 4) + 1);
					j++;
				}
			}
        } 
	}
}
