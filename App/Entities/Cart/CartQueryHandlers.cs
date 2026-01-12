using Microsoft.EntityFrameworkCore;
using SampleShopApi.App.Data;
using SampleShopApi.App.Entities.Albums;

namespace SampleShopApi.App.Entities.Cart;

public class CartDto {

	public required int Id { get; set; }
	public required List<CartItemDto> Items { get; set; }
	public decimal TotalPrice =>
	  Items.Sum(item => item.Album.Price * item.Quantity);

	public static CartDto Empty => new() {
		Items = [],
		Id = -1,
	};
}

public class CartItemDto {
	public required AlbumDto Album { get; set; }
	public required int Quantity { get; set; }
}

public class GetCartHandler(ShopApiContext db) {

	public async Task<CartDto?> Handle(HttpRequest request) {
		if (!request.Cookies.TryGetValue("cartId", out var cartIdString)
			|| !int.TryParse(cartIdString, out var cartId)) {
			// No cart cookie, return empty cart
			return CartDto.Empty;
		}

		var cart = await db.Carts
		  .Include(c => c.Items)
		  .ThenInclude(i => i.Album)
		  .FirstOrDefaultAsync(c => c.Id == cartId);

		if (cart is null) {
			return null;
		}

		return cart.AsDto();
	}
}
