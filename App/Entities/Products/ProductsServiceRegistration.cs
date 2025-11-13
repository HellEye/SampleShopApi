namespace SampleShopApi.App.Entities.Products;

public static class ProductServiceRegistration {
	public static IServiceCollection AddProductServices(this IServiceCollection services) {
		services.AddScoped<GetProductListHandler>();
		services.AddScoped<GetProductByIdHandler>();
		services.AddScoped<CreateProductHandler>();
		services.AddScoped<UpdateProductHandler>();
		services.AddScoped<DeleteProductHandler>();
		return services;
	}
}
