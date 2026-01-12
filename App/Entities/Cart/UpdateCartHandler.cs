
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Data;
using SampleShopApi.App.Utils;

namespace SampleShopApi.App.Entities.Cart;

public class UpdateCartCommand {
	[Range(1, int.MaxValue)]
	public required int AlbumId { get; set; }
	[Range(1, int.MaxValue)]
	public required int Quantity { get; set; }
}


public class UpdateCartHandler(ShopApiContext db) {
	public async Task<Result<Cart>> Handle(int cartId, UpdateCartCommand command) {

		var cart = await db.Carts
		  .Include(c => c.Items)
		  .FirstOrDefaultAsync(c => c.Id == cartId);

		if (cart is null) {
			return Result<Cart>.NotFound($"Cart not found.", new() {
				["cartId"] = [$"No cart with Id {cartId}"]
			});
		}

		var cartItem = cart.Items.FirstOrDefault(i => i.AlbumId == command.AlbumId);
		if (cartItem is null) {
			cart.Items.Add(new CartItem {
				AlbumId = command.AlbumId,
				Quantity = command.Quantity,
				CartId = cart.Id
			});
		}
		else {
			cartItem.Quantity = command.Quantity;
		}
		cart.UpdatedAt = DateTime.UtcNow;

		await db.SaveChangesAsync();
		return Result<Cart>.Success(cart);
	}
}
