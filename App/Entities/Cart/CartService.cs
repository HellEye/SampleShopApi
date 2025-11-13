using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SampleShopApi.App.Data;

namespace SampleShopApi.App.Entities.Cart;

public class CartService(ShopApiContext db) {

	public async Task<EntityEntry<Cart>> CreateEmpty() {
		var cart = await db.Carts.AddAsync(new Cart() {
			Items = [],
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		});
		return cart;
	}

	public async Task<EntityEntry<Cart>?> GetById(int cartId) {
		var cart = await db.Carts
			.Include(c => c.Items)
			.FirstAsync(c => c.Id == cartId);
		if (cart is null) {
			return null;
		}
		return db.Carts.Entry(cart);
	}

	public async Task<CartItem?> GetItemByProductId(int cartId, int productId) {
		var cartItem = await db.CartItems
			.FirstOrDefaultAsync(i =>
				i.CartId == cartId
				&& i.ProductId == productId
			);
		return cartItem;
	}
}
