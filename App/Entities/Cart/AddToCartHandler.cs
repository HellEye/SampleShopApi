using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SampleShopApi.App.Data;
using SampleShopApi.App.Utils;

namespace SampleShopApi.App.Entities.Cart;

public class AddToCartCommand {
	[Range(1, int.MaxValue)]
	public required int ProductId { get; set; }
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
		var cartItemEntry = await cartService.GetItemByProductId(cart.Entity.Id, command.ProductId);

		if (cartItemEntry is null) {
			var product = await db.Products.FindAsync(command.ProductId);
			if (product is null) {
				return Result<Cart>.BadRequest("Product not found", new() { ["productId"] = ["The specified product does not exist."] });
			}
			cart.Entity.Items.Add(new() {
				CartId = cart.Entity.Id,
				ProductId = command.ProductId,
				Quantity = command.Quantity,
			});
		}
		else {
			var cartItemEntity = db.CartItems.Entry(cartItemEntry);
			cartItemEntity.Entity.Quantity += command.Quantity;
		}
		cart.Entity.UpdatedAt = DateTime.UtcNow;

		await db.SaveChangesAsync();
		response.Cookies.Append("cartId", cart.Entity.Id.ToString(), new CookieOptions {
			SameSite = SameSiteMode.Strict,
			Expires = DateTimeOffset.UtcNow.AddDays(7)
		});


		return Result<Cart>.Success(cart.Entity);
	}
}
