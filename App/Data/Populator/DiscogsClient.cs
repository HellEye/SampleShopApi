using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SampleShopApi.App.Data.Populator;

public class DiscogsClient {
	private readonly HttpClient _http;
	private readonly string _userAgent;
	private readonly string _token;
	private readonly TimeSpan _rateLimit = TimeSpan.FromSeconds(1);
	private DateTime _lastRequest = DateTime.MinValue;

	public DiscogsClient() {
		_userAgent = "SampleShopApi/0.1.0";
		_token = Environment.GetEnvironmentVariable("DISCOGS_API_TOKEN") ?? throw new InvalidOperationException("DISCOGS_API_TOKEN environment variable is not set.");
		_http = new HttpClient();
		_http.BaseAddress = new Uri("https://api.discogs.com/");
		_http.DefaultRequestHeaders.Add("User-Agent", _userAgent);
		if (!string.IsNullOrEmpty(_token))
			_http.DefaultRequestHeaders.Add("Authorization", $"Discogs token={_token}");
	}

	private async Task<T> GetAsync<T>(string url) {
		var sinceLast = DateTime.UtcNow - _lastRequest;
		if (sinceLast < _rateLimit)
			await Task.Delay(_rateLimit - sinceLast);
		_lastRequest = DateTime.UtcNow;
		Console.WriteLine($"Requesting {url}");
		var response = await _http.GetAsync(url);
		response.EnsureSuccessStatusCode();
		// Console.WriteLine(" === Response === ");
		// Console.WriteLine(await response.Content.ReadAsStringAsync());
		return await response.Content.ReadFromJsonAsync<T>();
	}

	public async Task<DiscogsArtist?> SelectArtistAsync(string name) {
		Console.WriteLine($"Searching for artist '{name}'...\n");
		var data = await GetAsync<DiscogsSearchResponse>(
			$"database/search?q={Uri.EscapeDataString(name)}&type=artist"
		);

		if (data.Results.Count == 0) {
			Console.WriteLine("No results found.");
			return null;
		}

		Console.WriteLine("--------------------------------------------------------------");
		Console.WriteLine($"{"#",3} {"Name",-25}");
		Console.WriteLine("--------------------------------------------------------------");
		var displayed = data.Results.Take(10).ToList();

		for (int i = 0; i < displayed.Count; i++) {
			var a = displayed[i];
			Console.WriteLine($"{i,3}: {a.Title,-25}");
		}

		Console.WriteLine("\nSelect an artist index (0–9), or -1 to skip:");
		Console.Write("> ");
		if (!int.TryParse(Console.ReadLine(), out var index) || index < 0 || index >= displayed.Count) {
			Console.WriteLine("Skipped.\n");
			return null;
		}

		var artistId = displayed[index].Id;
		var artist = await GetAsync<DiscogsArtist>($"artists/{artistId}");
		return artist;
	}

	private bool IsLikelyStudioAlbum(DiscogsArtistRelease release, string artistName) {
		// 1. Must be the main artist, not various/collaboration
		if (!string.Equals(release.Artist?.Trim(), artistName?.Trim(),
			StringComparison.OrdinalIgnoreCase))
			return false;

		// 2. Filter by suspicious title keywords
		static bool ContainsKeyword(string value, params string[] words)
			=> !string.IsNullOrEmpty(value) &&
			   words.Any(w => value.Contains(w, StringComparison.OrdinalIgnoreCase));

		if (ContainsKeyword(release.Title, "live", "greatest", "hits", "collection",
			"compilation", "soundtrack", "tribute", "remix", "deluxe", "anniversary",
			"single", "ep"))
			return false;

		// 3. Release role and type basic sanity
		if (!string.Equals(release.Role, "Main", StringComparison.OrdinalIgnoreCase))
			return false;

		if (!string.Equals(release.Type, "master", StringComparison.OrdinalIgnoreCase))
			return false;

		return true;
	}

	public async IAsyncEnumerable<DiscogsAlbum> GetArtistAlbumsAsync(long artistId, string artistName) {
		int page = 1;
		const int perPage = 100;

		Console.WriteLine("Fetching albums...\n");
		var limit = 0;
		while (limit++ < 5) {
			var data = await GetAsync<DiscogsArtistReleasesResponse>(
				$"artists/{artistId}/releases?sort=year&sort_order=asc&per_page={perPage}&page={page}"
			);

			var masters = data.Releases
				.Where(r => r.Type == "master" && r.Role == "Main")
				.Where(r => IsLikelyStudioAlbum(r, artistName))
				.GroupBy(r => r.Title)
				.Select(g => g.First())
				.ToList();

			foreach (var m in masters) {
				// Fetch master details for deeper filtering
				var master = await GetAsync<DiscogsMaster>($"masters/{m.Id}");

				bool IsInvalidCategory(IEnumerable<string> list) {
					if (list == null) return false;
					foreach (var val in list) {
						if (val.Contains("Live", StringComparison.OrdinalIgnoreCase) ||
							val.Contains("Compilation", StringComparison.OrdinalIgnoreCase) ||
							val.Contains("Remix", StringComparison.OrdinalIgnoreCase) ||
							val.Contains("Soundtrack", StringComparison.OrdinalIgnoreCase) ||
							val.Contains("EP", StringComparison.OrdinalIgnoreCase) ||
							val.Contains("Tribute", StringComparison.OrdinalIgnoreCase))
							return true;
					}
					return false;
				}

				if (IsInvalidCategory(master.Genres) || IsInvalidCategory(master.Styles))
					continue; // skip live/compilation/soundtrack/etc.

				if (master.Tracklist.Count < 3)
					continue; // skip releases with too few tracks

				yield return new DiscogsAlbum {
					Id = m.Id,
					Title = m.Title,
					Year = m.Year,
					CoverImageUrl = m.Thumb,
					Tracks = master.Tracklist
				};
			}

			if (data.Pagination.Page >= data.Pagination.Pages)
				break;

			page++;
		}

	}
}

// DTOs
// DTOs
public class DiscogsSearchResponse {
	[JsonPropertyName("results")]
	public List<DiscogsSearchResult> Results { get; set; } = new();
}

public class DiscogsSearchResult {
	[JsonPropertyName("id")] public long Id { get; set; }
	[JsonPropertyName("title")] public string Title { get; set; }
	[JsonPropertyName("thumb")] public string Thumb { get; set; }
	[JsonPropertyName("type")] public string Type { get; set; }
}

public class DiscogsArtist {
	[JsonPropertyName("id")] public long Id { get; set; }
	[JsonPropertyName("name")] public string Name { get; set; }
	[JsonPropertyName("realname")] public string RealName { get; set; }
	[JsonPropertyName("profile")] public string Profile { get; set; }
	[JsonPropertyName("uri")] public string Url { get; set; }
	[JsonPropertyName("images")] public List<ArtistImage> Images { get; set; } = new();
}

public class ArtistImage {
	[JsonPropertyName("type")] public string Type { get; set; }
	[JsonPropertyName("uri")] public string Uri { get; set; }
}

public class DiscogsArtistReleasesResponse {
	[JsonPropertyName("pagination")] public DiscogsPagination Pagination { get; set; }
	[JsonPropertyName("releases")] public List<DiscogsArtistRelease> Releases { get; set; } = new();
}

public class DiscogsPagination {
	[JsonPropertyName("page")] public int Page { get; set; }
	[JsonPropertyName("pages")] public int Pages { get; set; }
}

public class DiscogsArtistRelease {
	[JsonPropertyName("id")] public long Id { get; set; }
	[JsonPropertyName("title")] public string Title { get; set; }
	[JsonPropertyName("artist")] public string Artist { get; set; }
	[JsonPropertyName("year")] public int Year { get; set; }
	[JsonPropertyName("thumb")] public string Thumb { get; set; }
	[JsonPropertyName("type")] public string Type { get; set; }
	[JsonPropertyName("role")] public string Role { get; set; }
}

public class DiscogsMaster {
	[JsonPropertyName("id")] public long Id { get; set; }

	[JsonPropertyName("genres")] public List<string> Genres { get; set; }
	[JsonPropertyName("styles")] public List<string> Styles { get; set; }
	[JsonPropertyName("tracklist")] public List<DiscogsTrack> Tracklist { get; set; }
}

public class DiscogsMainRelease {
	[JsonPropertyName("tracklist")] public List<DiscogsTrackInfo> Tracklist { get; set; } = new();
}

public class DiscogsTrackInfo {
	[JsonPropertyName("title")] public string Title { get; set; }
	[JsonPropertyName("duration")] public string Duration { get; set; }
}

public class DiscogsAlbum {
	public long Id { get; set; }
	public string Title { get; set; }
	public int Year { get; set; }
	public string CoverImageUrl { get; set; }
	public List<DiscogsTrack> Tracks { get; set; } = new();
}

public class DiscogsTrack {
	public string Title { get; set; }
	public string Duration { get; set; }
}


public class Root {
	public int id { get; set; }
	public List<string> genres { get; set; }
	public List<string> styles { get; set; }
	public int year { get; set; }
	public string title { get; set; }
	public string data_quality { get; set; }
}
