using CityLink_SCP.Models;
using CityLink_SCP.DbModels;

namespace CityLink_SCP.Extensions
{
	public static class DbModelExtensions
	{
		public static ServicesViewModel ToCardViewModel(this List<Service> services)
		{
			var model = new ServicesViewModel();
			foreach (var item in services)
			{
				model.Services.Add(new ServiceViewModel
				{
					Id = item.Id,
					Title = item.Title,
					Description = item.Description,
					ButtonLabel = "Book Now"
				});
			}
			return model;
		}
		public static EventsViewModel ToCardViewModel(this List<Event> events)
		{
			var model = new EventsViewModel();
			model.Events = new();
			foreach (var item in events)
			{
				model.Events.Add(new EventViewModel
				{
					Id = item.Id,
					Title = item.Title,
					Description = item.Description,
					ButtonLabel = "Register"
				});
			}
			return model;
		}
		public static XmlConfigDto ToViewModel(this XmlConfig config)
		{
			return new XmlConfigDto
			{
				Id = config.Id,
                XmlContent = config.XmlContent,
				Type = config.Type,
				Version = config.Version,
				IsActive = config.IsActive,
				UploadedAt = config.UploadedAt,
				Label = config.Label
			};
        }
    }
}
