namespace SampleShopApi.App.Entities.Albums;

public static class AlbumServiceRegistration {
	public static IServiceCollection AddAlbumServices(this IServiceCollection services) {
		services.AddScoped<GetAlbumByIdHandler>();
		services.AddScoped<GetAlbumListHandler>();
		return services;
	}
}
