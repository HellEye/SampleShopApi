
using App.Cart;
using App.Data;
using Microsoft.EntityFrameworkCore;
namespace App.Cart;

public class CartCleanupService(ILogger<CartCleanupService> logger, ShopApiContext dbContext) : IHostedService, IDisposable {
	private Timer? timer = null;

	private async void DoWork(object? state) {
		logger.LogInformation("Starting cart cleanup task.");
		var cutoff = DateTime.UtcNow.AddDays(-7);
		var oldCarts = await dbContext.Carts
			.Where(c => c.UpdatedAt < cutoff)
			.Include(c => c.Items)
			.ToListAsync();

		dbContext.Carts.RemoveRange(oldCarts);
		await dbContext.SaveChangesAsync();
		logger.LogInformation("Cart cleanup task completed. Removed {Count} old carts.", oldCarts.Count);
	}

	public void Dispose() => timer?.Dispose();

	public Task StartAsync(CancellationToken cancellationToken) {
		logger.LogInformation("Cart cleanup service starting.");
		timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(6));
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken) {
		logger.LogInformation("Cart cleanup service stopping.");
		timer?.Change(Timeout.Infinite, 0);
		return Task.CompletedTask;
	}
}
