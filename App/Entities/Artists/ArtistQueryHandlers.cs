using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Data;
using SampleShopApi.App.Entities.Albums;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Artists;

public record class ArtistListQuery : IPaginatedQuery, ISortParams, ISearchParams {
	public int? Page { get; set; } = 1;
	public int? PageSize { get; set; } = 10;
	public string? Sort { get; set; }
	public Order? Order { get; set; } = Queries.Order.asc;
	public string? Search { get; set; }
}

public class GetArtistListHandler(ShopApiContext db) {

	public async Task<PaginatedResponse<ArtistDto>> HandleAsync(ArtistListQuery query) {
		Console.WriteLine("=======");
		Console.WriteLine($"{query.Search}, {query.Sort}, {query.Order}, {query.Page}, {query.PageSize}");
		return await db.Artists
			.WithSearch(query, a => a.Name)
			.WithSorting(query)
			.ToPaginated(query);
	}
}

public class GetArtistAlbumListHandler(ShopApiContext db) {
	public async Task<PaginatedResponse<AlbumDto>> HandleAsync(int artistId, AlbumListQuery query) {
		return await db.Albums
			.Where(a => a.ArtistId == artistId)
			.WithSearch(query, a => a.Name)
			.WithSorting(query)
			.ToPaginated(query);
	}
}

public class GetArtistByIdHandler(ShopApiContext db) {
	public async Task<ArtistDto?> HandleAsync(int id) {
		var artist = await db.Artists
			.FirstOrDefaultAsync(a => a.Id == id);

		if (artist == null) {
			return null;
		}

		var artistDto = new ArtistDto {
			Id = artist.Id,
			Name = artist.Name,
			ArtistPhotoUrl = artist.ArtistPhotoUrl,
		};

		return artistDto;
	}
}
