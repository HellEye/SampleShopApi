using App.Utils;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Songs;

public class Song : ToDto<SongDto> {
	public int Id { get; set; }
	public int AlbumOrder { get; set; }
	public int AlbumId { get; set;}
	public TimeSpan Duration { get; set; }

	public SongDto AsDto() =>
	  new SongDto {
		  Id = Id,
		  AlbumOrder = AlbumOrder,
		  AlbumId = AlbumId,
		  Duration = Duration,
	  };
}

public record class SongDto {
	public required int Id { get; set; }
	public int AlbumId { get; set; }
	public int AlbumOrder { get; set; }
	public TimeSpan Duration { get; set; }
}

