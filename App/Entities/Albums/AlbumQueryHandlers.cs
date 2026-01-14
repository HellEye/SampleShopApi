using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Data;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Albums;

public record class AlbumListQuery : IPaginatedQuery, ISortParams, ISearchParams {
	public int? Page { get; set; } = 1;
	public int? PageSize { get; set; } = 10;
	public string? Sort { get; set; }
	public Order? Order { get; set; } = Queries.Order.asc;
	public string? Search { get; set; }
}

public class GetAlbumListHandler(ShopApiContext db) {

	public async Task<PaginatedResponse<AlbumDto>> HandleAsync(AlbumListQuery query) {
		return await db.Albums
			.Include(a => a.Artist)
			.WithSearch(query, a => a.Name, a => a.Artist.Name)
			.WithSorting(query)
			.ToPaginated(query);
	}
}

public class GetAlbumByIdHandler(ShopApiContext db) {
	public async Task<AlbumDtoWithSongs?> HandleAsync(int id) {
		var album = await db.Albums
			.Include(a => a.Artist)
			.Include(a => a.Songs)
			.FirstOrDefaultAsync(a => a.Id == id);

		return album?.AsDtoWithSongs();
	}
}

