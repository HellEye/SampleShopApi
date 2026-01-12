using SampleShopApi.App.Entities.Albums;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Artists;

public static class ArtistEndpoints {
	public static void MapArtistEndpoints(this IEndpointRouteBuilder routes) {
		var artists = routes.MapGroup("/artists").WithTags("Artists");

		artists.MapGet("/{id:int}/albums", async (int id, GetArtistAlbumListHandler handler, AlbumListQuery query) => {
			var albums = await handler.HandleAsync(id, query);

			if (albums == null) {
				return Results.NotFound();
			}

			return Results.Ok(albums);
		})
		.WithName("GetArtistById")
		.WithDescription("Get an artist by its ID")
		.Produces<ArtistDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);

		artists.MapGet("/", async (GetArtistListHandler handler, ArtistListQuery query) => {
			var artists = await handler.HandleAsync(query);
			return Results.Ok(artists);
		})
		.WithName("GetArtistList")
		.WithDescription("Get a list of artists")
		.Produces<PaginatedResponse<ArtistDto>>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest);

	}
}
