using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Data;
namespace SampleShopApi.App.Data.Populator;

public class DatabasePopulator(ShopApiContext db, DiscogsClient discogsClient) {
	public static HashSet<string> GoalArtists = [
		"Queen",
		"The Beatles",
		"Led Zeppelin",
		"Pink Floyd",
		"Coma",
		"Placebo",
		"IAMX",
		"Zmarłym",
		"Tides from Nebula",
		"Jann",
		"Metallica",
		"Nirvana",
		"Imagine Dragons",
		"Linkin Park",
		"Coldplay",
		"Beyoncé",
		"Taylor Swift",
		"Ed Sheeran",
		"Adele",
		"Drake",
		"Eminem",
		"Jimi Hendrix",
		"Bob Dylan",
  ];
	public async Task PopulateAsync() {
		foreach (var artist in GoalArtists) {

			var artistEntity = await db.Artists.FirstOrDefaultAsync(a => a.Name == artist);
			if (artistEntity != null) {
				Console.WriteLine($"Artist {artist} already exists, skipping...");
				continue;
			}
			var dArtist = await discogsClient.SelectArtistAsync(artist);
			if (dArtist == null) {
				Console.WriteLine($"Artist {artist} skipped...");
				continue;
			}
			artistEntity = new Entities.Artists.Artist {
				Name = dArtist.Name,
				ArtistPhotoUrl = dArtist.Images.FirstOrDefault(i => i.Type == "primary")?.Uri ?? "",
			};
			await db.Artists.AddAsync(artistEntity);

			await db.SaveChangesAsync();


			var dbArtistId = artistEntity.Id;

			await foreach (var album in discogsClient.GetArtistAlbumsAsync(dArtist.Id, artist)) {
				var randomPrice = new Random().Next(5, 20) + 0.99m;
				var albumEntity = new Entities.Albums.Album {
					ArtistId = dbArtistId,
					Name = album.Title,
					ReleaseYear = album.Year,
					Price = randomPrice,
					AlbumCoverUrl = album.CoverImageUrl,
				};
				await db.Albums.AddAsync(albumEntity);
				await db.SaveChangesAsync();
				var dbAlbumId = albumEntity.Id;
				int order = 1;
				await db.Songs.AddRangeAsync(album.Tracks.Select(t => new Entities.Songs.Song {
					AlbumId = dbAlbumId,
					Title = t.Title,
					AlbumOrder = order++,

				}));
				await db.SaveChangesAsync();

			}
		}
	}
}
