using App.Utils;
using SampleShopApi.App.Entities.Artists;
using SampleShopApi.App.Entities.Songs;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Albums;

public class Album : ToDto<AlbumDto> {
	public int Id { get; set; }
	[Sortable("name")]
	public required string Name { get; set; }
	[Sortable("price")]
	public required decimal Price { get; set; }
	[Sortable("releaseYear")]
	public required int ReleaseYear { get; set; }

	public required int ArtistId { get; set; }
	public Artist Artist { get; set; } = null!;

	public string AlbumCoverUrl { get; set; } = string.Empty;
	public ICollection<Song> Songs { get; set; } = new List<Song>();
	public AlbumDto AsDto() =>
	  new AlbumDto {
		  Id = Id,
		  Name = Name,
		  Price = Price,
		  ReleaseYear = ReleaseYear,
		  AlbumCoverUrl = AlbumCoverUrl,
		  ArtistName = Artist?.Name ?? string.Empty,
	  };
	public AlbumDtoWithSongs AsDtoWithSongs() =>
	  new AlbumDtoWithSongs {
		  Id = Id,
		  Name = Name,
		  Price = Price,
		  ReleaseYear = ReleaseYear,
		  AlbumCoverUrl = AlbumCoverUrl,
		  ArtistName = Artist?.Name ?? string.Empty,
		  Songs = [.. Songs.Select(s => s.AsDto())]
	  };
}

public record class AlbumDto {
	public required int Id { get; set; }
	public required string Name { get; set; }
	public required decimal Price { get; set; }
	public required int ReleaseYear { get; set; }
	public required string AlbumCoverUrl { get; set; }
	public required string ArtistName { get; set; }
	// public ICollection<SongDto> Songs { get; set; } = new List<SongDto>();
}

public record class AlbumDtoWithSongs : AlbumDto {
	public ICollection<SongDto> Songs { get; set; } = new List<SongDto>();
}

public enum AlbumSortField {
	name,
	price,
	release_date,
}

