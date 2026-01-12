using SampleShopApi.App.Data;
using SampleShopApi.App.Queries;
namespace SampleShopApi.App.Entities.Songs;

public static class SongEndpoints {
	public static void MapSongEndpoints(this WebApplication app) {
		var songs = app.MapGroup("/albums/{albumId:int}/songs").WithTags("Songs");

		songs.MapGet("/", async (int albumId, GetSongsForAlbumHandler handler) => {
			var songs = await handler.HandleAsync(albumId);
			return songs.ToHttpResult();
		})
		.WithName("GetSongsForAlbum")
		.WithDescription("Get songs for a specific album")
		.Produces<List<SongDto>>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);

	}
}
