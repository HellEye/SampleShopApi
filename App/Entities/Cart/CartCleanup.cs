using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Data;
namespace SampleShopApi.App.Entities.Cart;

public class CartCleanupService(ILogger<CartCleanupService> logger, IServiceScopeFactory scopeFactory) : IHostedService, IDisposable {
	private Timer? timer = null;

	private async void DoWork(object? state) {
		logger.LogInformation("Starting cart cleanup task.");
		using var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<ShopApiContext>();
		var cutoff = DateTime.UtcNow.AddDays(-7);
		var oldCartsCount = await dbContext.Carts
			.Where(c => c.UpdatedAt < cutoff)
			.ExecuteDeleteAsync();

		logger.LogInformation("Cart cleanup task completed. Removed {Count} old carts.", oldCartsCount);
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
