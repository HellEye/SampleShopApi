namespace SampleShopApi.App.Entities.Cart;

public static class CartExtensions {
	public static void WithCartIdCookie(this HttpResponse response, int cartId) {
		response.Cookies.Append("cartId", cartId.ToString(), new CookieOptions {
			SameSite = SameSiteMode.Strict,
			Expires = DateTimeOffset.UtcNow.AddDays(7)
		});
	}
}
