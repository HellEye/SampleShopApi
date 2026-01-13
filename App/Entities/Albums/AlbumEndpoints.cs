using MinimalApis.Extensions;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Albums;

public static class AlbumEndpoints {
	public static void MapAlbumEndpoints(this WebApplication app) {
		var albums = app.MapGroup("/albums").WithTags("Albums");

		albums.MapGet("/", async ([AsParameters] AlbumListQuery query, GetAlbumListHandler handler) => {
			var albumList = await handler.HandleAsync(query);
			return Results.Ok(albumList);
		})
		.WithName("GetAlbumList")
		.WithDescription("Get a list of albums with optional filtering and sorting")
		.Produces<PaginatedResponse<AlbumDto>>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest);

		albums.MapGet("/{id:int}", async (int id, GetAlbumByIdHandler handler) => {
			var album = await handler.HandleAsync(id);

			if (album == null) {
				return Results.NotFound();
			}

			return Results.Ok(album);
		})
		.WithName("GetAlbumById")
		.WithDescription("Get an album by its ID")
		.WithParameterValidation()
		.Produces<AlbumDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);
	}
}
