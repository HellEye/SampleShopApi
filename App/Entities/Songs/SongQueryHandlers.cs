
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Data;
using SampleShopApi.App.Queries;
using SampleShopApi.App.Utils;

namespace SampleShopApi.App.Entities.Songs;

public record class SongListQuery {
	public int AlbumId { get; set; }
}

public class GetSongsForAlbumHandler(ShopApiContext db) {
	public async Task<Result<List<SongDto>>> HandleAsync(int albumId) {
		var res = await db.Songs
		  .AsNoTracking()
		  .Where(s => s.AlbumId == albumId)
		  .OrderBy(s => s.AlbumOrder)
		  .Select(s => s.AsDto())
		  .ToListAsync();
		if (res.Count == 0) {
			return Result<List<SongDto>>.NotFound($"No songs found for album with ID {albumId}");
		}
		return Result<List<SongDto>>.Success(res);
	}
}
