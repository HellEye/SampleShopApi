using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Entities.Albums;
using SampleShopApi.App.Entities.Artists;
using SampleShopApi.App.Entities.Cart;
using SampleShopApi.App.Entities.Songs;
namespace SampleShopApi.App.Data;

public partial class ShopApiContext(DbContextOptions<ShopApiContext> options)
  : DbContext(options) {

	public DbSet<Song> Songs => Set<Song>();
	public DbSet<Album> Albums => Set<Album>();
	public DbSet<Artist> Artists => Set<Artist>();
	public DbSet<Cart> Carts => Set<Cart>();
	public DbSet<CartItem> CartItems => Set<CartItem>();

	// As railway gives the connection string in a different format, we need to parse it
	// Instead we could accept separate host, password, port, user, db env vars, but this is more convenient
	[GeneratedRegex(
	  @"postgresql:\/\/(?<username>[^:]+):(?<password>[^@]+)@(?<host>[^:]+):(?<port>\d+)\/(?<database>[^?]*)"
	)]
	private static partial Regex ConnectionRegex();

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
		var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
		if (string.IsNullOrEmpty(connectionString)) {
			throw new InvalidOperationException("DATABASE_URL environment variable is not set.");
		}


		var match = ConnectionRegex().Match(connectionString);
		if (!match.Success) {
			throw new InvalidOperationException("DATABASE_URL is not in the correct format.");
		}

		optionsBuilder.UseNpgsql(
		  $"Host={match.Groups["host"].Value};" +
		  $"Port={match.Groups["port"].Value};" +
		  $"Database={match.Groups["database"].Value};" +
		  $"Username={match.Groups["username"].Value};" +
		  $"Password={match.Groups["password"].Value};" +
		  "Pooling=true;"
		);
	}
}
