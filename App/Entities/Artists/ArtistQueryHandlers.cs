using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Data;
using SampleShopApi.App.Entities.Albums;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Artists;

public record class ArtistListQuery : IPaginatedQuery, ISortParams, ISearchParams {
	public int? Page { get; set; } = 1;
	public int? PageSize { get; set; } = 10;
	public string? Sort { get; set; }
	public Order? Order { get; set; } = Queries.Order.Asc;
	public string? Search { get; set; }
}

public class GetArtistListHandler(ShopApiContext db) {

	public async Task<PaginatedResponse<ArtistDto>> HandleAsync(ArtistListQuery query) {
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
