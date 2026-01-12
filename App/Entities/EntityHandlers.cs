using SampleShopApi.App.Entities.Albums;
using SampleShopApi.App.Entities.Artists;
using SampleShopApi.App.Entities.Cart;
using SampleShopApi.App.Entities.Songs;
namespace SampleShopApi.App.Entities;

public static class EntityHandlersExtensions {
	public static WebApplication MapEntityHandlers(this WebApplication app) {
		app.MapSongEndpoints();
		app.MapCartEndpoints();
		app.MapArtistEndpoints();
		app.MapAlbumEndpoints();
		return app;
	}

}
