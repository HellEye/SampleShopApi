using App.Utils;
using SampleShopApi.App.Entities.Albums;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Artists;

public class Artist : ToDto<ArtistDto> {
	public int Id { get; set; }
	[Sortable("name")]
	public required string Name { get; set; }
	public string ArtistPhotoUrl { get; set; } = string.Empty;
	public ICollection<Album> Albums { get; set; } = new List<Album>();
	public ArtistDto AsDto() =>
	  new ArtistDto {
		  Id = Id,
		  Name = Name,
		  ArtistPhotoUrl = ArtistPhotoUrl,
	  };
}
public record class ArtistDto {
	public required int Id { get; set; }
	public required string Name { get; set; }
	public string ArtistPhotoUrl { get; set; } = string.Empty;
}
public record class ArtistDtoWithAlbums : ArtistDto {
	public ICollection<AlbumDto> Albums { get; set; } = new List<AlbumDto>();
}

public enum ArtistSortField {
	name,
}
