using CityLink_SCP.DbModels;
using CityLink_SCP.Extensions;
using CityLink_SCP.Models;
using CityLink_SCP.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CityLink_SCP.Database
{
    public static class DbInitialiser
    {
        public static async Task InitialiseAsync(
            CityLinksContext context,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			DatabaseService dbService,
			XmlConfigService xmlService)
        {
            if (context.AppUsers.Any())
            {
                return;
            }

			// Ensure roles exist
			foreach (var role in new[] { "Staff", "User" })
			{
				if (!await roleManager.RoleExistsAsync(role))
					await roleManager.CreateAsync(new IdentityRole(role));
			}

			// Seed staff 
			var staffSeed = new[]
			{
				new { Email = "admin@citylink.wa.gov.au",    Password = "Admin1234!", JobTitle = "Admin"    },
				new { Email = "it@citylink.wa.gov.au",       Password = "Admin1234!", JobTitle = "IT Admin" }
			};

			var seededStaff = new List<ApplicationStaff>();
			foreach (var s in staffSeed)
			{
				var staff = new ApplicationStaff
				{
					UserName = s.Email,
					Email = s.Email,
					FirstName = s.JobTitle,
					LastName = "Staff",
					JobTitle = s.JobTitle,
					EmailConfirmed = true
				};
				var result = await userManager.CreateAsync(staff, s.Password);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(staff, "Staff");
					seededStaff.Add(staff);
				}
			}

			// Grab the primary staff member for FK assignments below
			var primaryStaff = seededStaff.First();
			
			var seededUsers = new List<ApplicationUser>();
			for (int i = 0; i < 5; i++)
			{
				var user = new ApplicationUser
				{
					UserName = $"user_{i}@example.com",
					Email = $"user_{i}@example.com",
					FirstName = $"John_{i}",
					LastName = $"Doe_{i}",
					PhoneNumber = $"0{400000000 + i}",
					Address = $"{i} Waverly Place, Perth WA 600{i}",
					EmailConfirmed = true
				};
				var result = await userManager.CreateAsync(user, "User1234!");
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(user, "User");
					seededUsers.Add(user);
				}
			}

            // Seed events and services 
            var events = LoadEventsFromCsv("SeedData\\events_seed.csv", primaryStaff.Id);
            var services = LoadServicesFromCsv("SeedData\\services_seed.csv", primaryStaff.Id);



            context.Events.AddRange(events);
			context.Services.AddRange(services);
			await context.SaveChangesAsync();

			// Seed Bookings and Registrations
			int slot = 0;
			foreach (var user in seededUsers)
			{
				foreach (var svc in services)
				{
					var start = DateTime.Parse("2025-01-01T08:00:00").AddMinutes(slot * 60);
					var end = start.AddHours(1);
					dbService.AddServiceBooking(user.Id, svc.Id, start, end);
					slot++;
				}
				slot = 0;
				int j = 0;
				foreach (var ev in events)
				{
					dbService.AddEventRegistration(user.Id, ev.Id, (j % 4) + 1);
					j++;
				}
			}

			// Seed Xml Default Configurations
			var footer = new FooterModel
            {
                Email = "info@citylink.wa.gov.au",
                Phone = "(08) 9999 9999",
                Address = "Fictional Address, Perth WA 6000",
                QuickLinks = new List<TitleLink>
                {
                    new TitleLink { Title = "Announcements", Url = "/Home/WhatsOn" },
                    new TitleLink { Title = "Our Services", Url = "/Home/Services" },
                    new TitleLink { Title = "FAQs", Url = "/Home/FAQs" },
                    new TitleLink { Title = "Feedback Form", Url = "/Home/ContactUs" },
                    new TitleLink { Title = "Staff Portal", Url = "/Admin/Index" }
                },
                SocialMedia = new List<TitleLink>
                {
                    new TitleLink { Title = "Facebook", Url = "https://www.facebook.com/CityLinkSCP" },
                    new TitleLink { Title = "Instagram", Url = "https://www.instagram.com/CityLinkSCP" },
                    new TitleLink { Title = "LinkedIn", Url = "https://www.linkedin.com/company/CityLinkSCP" }
                }
            };
 
            var faqs = new FAQViewModel
            {
                FAQs =  new List<FAQ>
                {
                    new FAQ { Question = "What is CityLink Initiatives?", Answer = "CityLink Initiatives is a government platform that streamlines citizens' access to local services, announcements, and community events all in one place." },
                    new FAQ { Question = "How do I report an issue or concern?", Answer = "Navigate to the Services section and click \"Feedback\". Fill out the online form and a council representative will get back to you within 3 business days." },
                    new FAQ { Question = "Where can I find the waste collection schedule?", Answer = "The waste collection schedule is available under Waste Management in the Services section. You can also subscribe to receive email reminders." },
                    new FAQ { Question = "How do I book a community event?", Answer = "Visit the What's Happening section and click \"Book Now\" on any event. You'll need a free CityLink account or to log in to complete your booking." },
                    new FAQ { Question = "Is this service available 24/7?", Answer = "Yes, the CityLink online platform is available around the clock. For phone or in-person support, check our Contact Us page for office hours." }
				}
			};
			var announcements = new AnnouncementsViewModel
			{
				Eyebrow = "Announcements",
				Heading = "What's On",
				Items   = new List<Announcement>
				{
					new() {
						Title       = "Welcome to CityLink Initiatives",
						Body        = "CityLink is your one-stop online portal for local council services, community events, and important announcements. We're here to make government services more accessible to everyone.",
						ButtonLabel = "Our Services",
						ButtonUrl   = "/Home/Services"
					},
					new() {
						Title       = "Community Events Now Open",
						Body        = "Register for upcoming community events online. From farmers markets to cultural workshops, there is something for every resident. Browse the full events calendar and secure your spot today.",
						ButtonLabel = "View Events",
						ButtonUrl   = "/Home/Events"
					}
				}
			};
			string xmlAnnouncementsContent = xmlService.ToXml(announcements);
			File.WriteAllText("XML\\AnnouncementsDefault.xml", xmlAnnouncementsContent);

			string xmlFaqContent = xmlService.ToXml<FAQViewModel>(faqs);
            File.WriteAllText("XML\\FAQsDefault.xml", xmlFaqContent);

            string xmlEventContent = xmlService.ToXml<EventsViewModel>(events.Take(6).ToList().ToCardViewModel());
			File.WriteAllText("XML\\EventsDefault.xml", xmlEventContent);

            string xmlServiceContent = xmlService.ToXml<ServicesViewModel>(services.Take(6).ToList().ToCardViewModel());
			File.WriteAllText("XML\\ServicesDefault.xml", xmlServiceContent);

			var xmlConfigs = new List<XmlConfig>
			{
				new() {
					XmlContent = xmlAnnouncementsContent,
					Type       = GetFriendlyName(typeof(AnnouncementsViewModel)),
					Version    = "1.0",
					IsActive   = true,
					UploadedAt = DateTime.Now,
					Label      = "Initial Announcements",
					Staff      = primaryStaff
				},
				new() {
					XmlContent = xmlService.ToXml(footer),
					Type = GetFriendlyName(typeof(FooterModel)),
					Version = "1.0",
					IsActive = true,
					UploadedAt = DateTime.Now,
					Label = "Initial Footer",   Staff = primaryStaff
				},
				new() {
					XmlContent = xmlService.ToXml(faqs),
					Type = GetFriendlyName(typeof(FAQViewModel)),
					Version = "1.0", IsActive = true,
					UploadedAt = DateTime.Now,
					Label = "Initial FAQs",
					Staff = primaryStaff },
				new() {
					XmlContent = xmlService.ToXml(events.Take(6).ToList().ToCardViewModel()),
					Type = GetFriendlyName(typeof(EventsViewModel)),
					Version = "1.0",
					IsActive = true,
					UploadedAt = DateTime.Now,
					Label = "Initial Events",
					Staff = primaryStaff
				},
				new() {
					XmlContent = xmlService.ToXml(services.Take(6).ToList().ToCardViewModel()),
					Type = GetFriendlyName(typeof(ServicesViewModel)),
					Version = "1.0",
					IsActive = true,
					UploadedAt = DateTime.Now,
					Label = "Initial Services",
					Staff = primaryStaff
				},
			};

			context.XML_Configurations.AddRange(xmlConfigs);
			await context.SaveChangesAsync();

		}

        /// <summary>
        /// Reads events from a CSV file and returns a list of Event entities.
        /// Expected columns (with header row):
        ///   Title, Description, Location, Cost, Max_Capcity, Start_Date_Time, End_Date_Time
        ///
        /// Descriptions may be quoted and contain commas. Falls back to generated
        /// placeholder data if the file is missing or contains no valid rows.
        /// </summary>
        public static List<Event> LoadEventsFromCsv(string csvPath, string staffId)
        {
            if (!File.Exists(csvPath))
            {
                Console.WriteLine($"[DbInitialiser] events CSV not found at '{csvPath}'. Using generated fallback data.");
                return GenerateFallbackEvents(staffId);
            }

            var events = new List<Event>();
            var lines = File.ReadAllLines(csvPath);

            // Skip header row (index 0)
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = ParseCsvLine(line);
                if (cols.Count < 7) continue;

                if (!double.TryParse(cols[3], System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double cost))
                    cost = 0;

                if (!int.TryParse(cols[4], out int maxCapacity))
                    maxCapacity = 50;

                if (!DateTime.TryParse(cols[5], out DateTime startDt))
                    startDt = DateTime.Now.AddDays(7);

                if (!DateTime.TryParse(cols[6], out DateTime endDt))
                    endDt = startDt.AddHours(2);

                events.Add(new Event
                {
                    Title = cols[0].Trim(),
                    Description = cols[1].Trim(),
                    Location = cols[2].Trim(),
                    Cost = cost,
                    Max_Capcity = maxCapacity,
                    Start_Date_Time = startDt,
                    End_Date_Time = endDt,
                    StaffId = staffId
                });
            }

            if (events.Count == 0)
            {
                return GenerateFallbackEvents(staffId);
            }
            return events;
        }

        /// <summary>
        /// Reads services from a CSV file and returns a list of Service entities.
        /// Expected columns (with header row):
        ///   Title, Description, Location, Cost, Available_Start_Time, Available_End_Time
        ///
        /// Descriptions may be quoted and contain commas. Falls back to generated
        /// placeholder data if the file is missing or contains no valid rows.
        /// </summary>
        public static List<Service> LoadServicesFromCsv(string csvPath, string staffId)
        {
            if (!File.Exists(csvPath))
            {
                return GenerateFallbackServices(staffId);
            }

            var services = new List<Service>();
            var lines = File.ReadAllLines(csvPath);

            // Skip header row (index 0)
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = ParseCsvLine(line);
                if (cols.Count < 6) continue;

                if (!double.TryParse(cols[3], System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double cost))
                    cost = 0;

                if (!TimeOnly.TryParse(cols[4], out TimeOnly startTime))
                    startTime = TimeOnly.Parse("08:00");

                if (!TimeOnly.TryParse(cols[5], out TimeOnly endTime))
                    endTime = TimeOnly.Parse("17:00");

                services.Add(new Service
                {
                    Title = cols[0].Trim(),
                    Description = cols[1].Trim(),
                    Location = cols[2].Trim(),
                    Cost = cost,
                    Available_Start_Time = startTime,
                    Available_End_Time = endTime,
                    StaffId = staffId
                });
            }

            if (services.Count == 0)
            {
                return GenerateFallbackServices(staffId);
            }

            return services;
        }

        /// <summary>
        /// Minimal RFC-4180-compliant CSV line parser.
        /// Handles quoted fields that may contain commas and escaped double-quotes ("").
        /// </summary>
        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // Peek ahead: escaped quote ("") stays in field
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++; // skip second quote
                        }
                        else
                        {
                            inQuotes = false; // closing quote
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        fields.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            fields.Add(current.ToString()); // last field
            return fields;
        }

        // Fallback generators when CSV files are absent 
        private static List<Event> GenerateFallbackEvents(string staffId) =>
            Enumerable.Range(0, 10).Select(i => new Event
            {
                Title = $"Event_{i}",
                Description = $"Description for Event_{i}",
                Location = $"Location_{i}",
                Cost = i * 10.0,
                Max_Capcity = 50 + i,
                Start_Date_Time = DateTime.Now.AddDays(i),
                End_Date_Time = DateTime.Now.AddDays(i + 1),
                StaffId = staffId
            }).ToList();

        private static List<Service> GenerateFallbackServices(string staffId) =>
            Enumerable.Range(0, 10).Select(i => new Service
            {
                Title = $"Service_{i}",
                Description = $"Description for Service_{i}",
                Location = $"Location_{i}",
                Cost = i * 10.0,
                Available_Start_Time = TimeOnly.Parse("08:00:00"),
                Available_End_Time = TimeOnly.Parse("17:00:00"),
                StaffId = staffId
            }).ToList();

        public static string GetFriendlyName(Type type)
        {
            if (type.IsGenericType)
            {
                // Get the name without the `1 arity suffix
                string name = type.Name.Split('`')[0];
                // Get the friendly names of the generic arguments
                var args = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName));
                return $"{name}<{args}>";
            }
            return type.Name;
        }
	}
}