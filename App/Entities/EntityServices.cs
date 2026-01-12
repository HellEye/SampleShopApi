using SampleShopApi.App.Entities.Albums;
using SampleShopApi.App.Entities.Artists;
using SampleShopApi.App.Entities.Cart;
using SampleShopApi.App.Entities.Songs;

namespace SampleShopApi.App.Entities;

public static class EntityServices {
	public static IServiceCollection AddEntityServices(this IServiceCollection services) {
		services.AddSongServices()
			.AddCartServices()
			.AddArtistServices()
			.AddAlbumServices();

		return services;
	}
}
