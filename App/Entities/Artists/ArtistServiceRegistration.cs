namespace SampleShopApi.App.Entities.Artists;

public static class ArtistServiceRegistration {
	public static IServiceCollection AddArtistServices(this IServiceCollection services) {
		services.AddScoped<GetArtistAlbumListHandler>();
		services.AddScoped<GetArtistListHandler>();
		services.AddScoped<GetArtistByIdHandler>();
		return services;
	}
}
