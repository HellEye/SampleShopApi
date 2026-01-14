using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SampleShopApi.App.Data;
using SampleShopApi.App.Utils;

namespace SampleShopApi.App.Entities.Cart;

public class AddToCartCommand {
	[Range(1, int.MaxValue)]
	public required int AlbumId { get; set; }
	[Range(1, int.MaxValue)]
	public required int Quantity { get; set; }
}

public class AddToCartHandler(ShopApiContext db, CartService cartService) {
	public async Task<Result<Cart>> Handle(int? cartId, AddToCartCommand command, HttpResponse response) {
		EntityEntry<Cart>? cart;
		if (cartId is null) {
			cart = await cartService.CreateEmpty();
		}
		else {
			cart = await cartService.GetById(cartId.Value);
			if (cart is null) {
				return Result<Cart>.NotFound("Cart not found", new() { ["cartId"] = [$"No cart with Id {cartId.Value}"] });
			}
		}
		var cartItemEntry = await cartService.GetItemByAlbumId(cart.Entity.Id, command.AlbumId);

		if (cartItemEntry is null) {
			var song = await db.Songs.FindAsync(command.AlbumId);
			if (song is null) {
				return Result<Cart>.BadRequest("Song not found", new() { ["songId"] = ["The specified song does not exist."] });
			}
			cart.Entity.Items.Add(new() {
				CartId = cart.Entity.Id,
				AlbumId = command.AlbumId,
				Quantity = command.Quantity,
			});
		}
		else {
			var cartItemEntity = db.CartItems.Entry(cartItemEntry);
			cartItemEntity.Entity.Quantity += command.Quantity;
		}
		cart.Entity.UpdatedAt = DateTime.UtcNow;

		await db.SaveChangesAsync();
		response.WithCartIdCookie(cart.Entity.Id);

		return Result<Cart>.NoContent();
	}
}
