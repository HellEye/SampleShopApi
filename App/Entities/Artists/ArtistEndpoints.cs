using SampleShopApi.App.Entities.Albums;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Artists;

public static class ArtistEndpoints {
	public static void MapArtistEndpoints(this IEndpointRouteBuilder routes) {
		var artists = routes.MapGroup("/artists").WithTags("Artists");
		artists.MapGet("/", async (GetArtistListHandler handler, [AsParameters] ArtistListQuery query) => {
			var artists = await handler.HandleAsync(query);
			return Results.Ok(artists);
		})
		.WithName("GetArtistList")
		.WithDescription("Get a list of artists")
		.Produces<PaginatedResponse<ArtistDto>>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest);

		artists.MapGet("/{id:int}", async (int id, GetArtistByIdHandler handler) => {
			var artist = await handler.HandleAsync(id);

			if (artist == null) {
				return Results.NotFound($"Artist with id {id} not found.");
			}

			return Results.Ok(artist);
		})
		.WithName("GetArtistById")
		.WithDescription("Get specific artist")
		.Produces<ArtistDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);

		artists.MapGet("/{id:int}/albums", async (int id, GetArtistAlbumListHandler handler, [AsParameters] AlbumListQuery query) => {
			var albums = await handler.HandleAsync(id, query);

			if (albums == null) {
				return Results.NotFound();
			}

			return Results.Ok(albums);
		})
		.WithName("GetArtistAlbums")
		.WithDescription("Get albums for a specific artist")
		.Produces<PaginatedResponse<AlbumDto>>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);



	}
}
