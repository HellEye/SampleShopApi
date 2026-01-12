namespace SampleShopApi.App.Entities.Songs;

public static class SongServiceRegistration {
	public static IServiceCollection AddSongServices(this IServiceCollection services) {
		services.AddScoped<GetSongsForAlbumHandler>();
		return services;
	}
}
