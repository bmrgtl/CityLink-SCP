using CityLink_SCP.Models;
using CityLink_SCP.DbModels;

namespace CityLink_SCP.Extensions
{
	public static class DbModelExtensions
	{
		public static CardViewModel ToCardViewModel(this Service service)
		{
			return new CardViewModel
			{
				Title = service.Title,
				Description = service.Description,
				ButtonLabel = "Book Now"
			};
		}
		public static CardViewModel ToCardViewModel(this Event evnt)
		{
			return new CardViewModel
			{
				Title = evnt.Title,
				Description = evnt.Description,
				ButtonLabel = "Register"
			};
		}
		
	}
}
