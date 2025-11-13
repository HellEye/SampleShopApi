namespace SampleShopApi.App.Entities.Cart;

public static class CartServiceRegistration {
	public static IServiceCollection AddCartServices(this IServiceCollection services) {
		services.AddScoped<CartService>();
		services.AddScoped<GetCartHandler>();
		services.AddScoped<AddToCartHandler>();
		services.AddScoped<UpdateCartHandler>();
		services.AddScoped<DeleteFromCartHandler>();
		services.AddHostedService<CartCleanupService>();

		return services;
	}
}
