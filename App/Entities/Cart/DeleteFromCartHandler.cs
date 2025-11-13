using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Data;
using SampleShopApi.App.Utils;

namespace SampleShopApi.App.Entities.Cart;

public class DeleteFromCartHandler(ShopApiContext db) {
	public async Task<Result<Cart>> Handle(int cartId, int productId) {
		var cart = await db.Carts
				.Include(c => c.Items)
				.FirstOrDefaultAsync(c => c.Id == cartId);

		if (cart is null) {
			return Result<Cart>.NotFound("Cart not found", new() { ["cartId"] = [$"No cart with Id {cartId}"] });
		}

		var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
		if (cartItem is null) {
			return Result<Cart>.NotFound("Cart item not found", new() { ["productId"] = [$"No product with Id {productId} in cart"] });
		}

		db.CartItems.Remove(cartItem);
		await db.SaveChangesAsync();
		return Result<Cart>.Success(cart);
	}
}
