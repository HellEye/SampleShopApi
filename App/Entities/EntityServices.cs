using SampleShopApi.App.Entities.Cart;
using SampleShopApi.App.Entities.Products;

namespace SampleShopApi.App.Entities;

public static class EntityServices {
	public static IServiceCollection AddEntityServices(this IServiceCollection services) {
		services.AddProductServices();
		services.AddCartServices();
		return services;
	}
}
