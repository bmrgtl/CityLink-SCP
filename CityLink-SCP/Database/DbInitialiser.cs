using CityLink_SCP.DbModels;
using CityLink_SCP.Extensions;
using CityLink_SCP.Models;
using CityLink_SCP.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CityLink_SCP.Database
{
    public static class DbInitialiser
    {
        public static void Initialise(CityLinksContext context, DatabaseService dbService, XmlConfigService xmlService)
        {
            if (context.Users.Any())
            {
                return;
                //context.XML_Configurations.RemoveRange(context.XML_Configurations);
                //context.EventRegistrations.RemoveRange(context.EventRegistrations);
                //context.ServiceBookings.RemoveRange(context.ServiceBookings);
                //context.Services.RemoveRange(context.Services);
                //context.Events.RemoveRange(context.Events);
                //context.Staff.RemoveRange(context.Staff);
                //context.Users.RemoveRange(context.Users);
                //context.SaveChanges();
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
            var xmlConfig = new XmlConfig
            {
                XmlContent = xmlService.ToXml<FooterModel>(footer),
                Type = GetFriendlyName(typeof(FooterModel)),
                Version = "1.0",
                IsActive = true,
                UploadedAt = DateTime.Now,
                Label = "Initial Footer Config",
                Staff = staff[0]
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

            var faqsConfig = new XmlConfig
            {
                XmlContent = xmlService.ToXml<FAQViewModel>(faqs),
                Type = GetFriendlyName(typeof(FAQViewModel)),
                Version = "1.0",
                IsActive = true,
                UploadedAt = DateTime.Now,
                Label = "Initial FAQ Config",
                Staff = staff[0]
            };

            var eventsConfig = new XmlConfig
            {
                XmlContent = xmlService.ToXml<EventsViewModel>(eventsRange.Take(6).ToList().ToCardViewModel()),
                Type = GetFriendlyName(typeof(EventsViewModel)),
                Version = "1.0",
                IsActive = true,
                UploadedAt = DateTime.Now,
                Label = "Initial Events Config",
                Staff = staff[0]
            };
            var servicesConfig = new XmlConfig
            {
                XmlContent = xmlService.ToXml<ServicesViewModel>(servicesRange.Take(6).ToList().ToCardViewModel()),
                Type = GetFriendlyName(typeof(ServicesViewModel)),
                Version = "1.0",
                IsActive = true,
                UploadedAt = DateTime.Now,
                Label = "Initial Services Config",
                Staff = staff[0]
            };

			context.XML_Configurations.Add(xmlConfig);
            context.XML_Configurations.Add(faqsConfig);
            context.XML_Configurations.Add(eventsConfig);
            context.XML_Configurations.Add(servicesConfig);
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