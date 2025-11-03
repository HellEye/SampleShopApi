using App.Cart;
using App.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public static class CartEndpointsExtensions {
	public static void MapCartEndpoints(this WebApplication app) {
		var cartGroup = app.MapGroup("/cart").WithTags("Cart");
		cartGroup.MapGet("/", async (HttpRequest request, ShopApiContext db) => {
			if (!request.Cookies.TryGetValue("cartId", out var cartIdString) || !int.TryParse(cartIdString, out var cartId)) {
				// No cart cookie, return empty cart
				return Results.NotFound(new { Message = "No cart found." });
			}

			var cart = await db.Carts
				.Include(c => c.Items)
				.ThenInclude(i => i.Product)
				.FirstOrDefaultAsync(c => c.Id == cartId);

			if (cart is null) {
				return Results.NotFound(new { Message = $"Cart with ID {cartId} not found." });
			}

			return Results.Ok(cart.AsDto());
		})
		.WithName("GetCart")
		.WithDescription("Get the current user's cart")
		.WithSummary("Retrieves the shopping cart associated with the user's session.")
		.Produces<CartDto>(StatusCodes.Status200OK)
		.Produces(StatusCodes.Status404NotFound);

		cartGroup.MapPost("/", async (AddToCartDto data, HttpRequest request, HttpResponse response, ShopApiContext db) => {
			EntityEntry<Cart>? cart = null;
			if (request.Cookies.TryGetValue("cartId", out var cartId)) {
				var cartModel = await db.Carts.FindAsync(cartId);
				if (cartModel is null) {
					return Results.NotFound(new { Message = $"Cart with ID {cartId} not found." });
				}
				cart = db.Carts.Entry(cartModel);
			}
			else {
				cart = await db.Carts.AddAsync(new Cart() {
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow,
					Items = [],
				});
			}
			var cartItemEntry = await db.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.Entity.Id && i.ProductId == data.ProductId);

			if (cartItemEntry is null) {
				var product = await db.Products.FindAsync(data.ProductId);
				if (product is null) {
					return Results.NotFound(new { Message = $"Product with ID {data.ProductId} not found." });
				}
				var newCartItem = await db.CartItems.AddAsync(new CartItem {
					CartId = cart.Entity.Id,
					ProductId = data.ProductId,
					Quantity = data.Quantity,
				});
				cart.Entity.Items.Add(newCartItem.Entity);
			}
			else {
				var cartItemEntity = db.CartItems.Entry(cartItemEntry);
				cartItemEntity.Entity.Quantity += data.Quantity;
			}
			cart.Entity.UpdatedAt = DateTime.UtcNow;

			await db.SaveChangesAsync();
			response.Cookies.Append("cartId", cart.Entity.Id.ToString(), new CookieOptions {
				SameSite = SameSiteMode.Strict,
				Expires = DateTimeOffset.UtcNow.AddDays(7)
			});


			return Results.Ok(cart.Entity.AsDto());
		})
		.WithName("AddToCart")
		.WithDescription("Add an item to the cart")
		.WithSummary("Adds a specified product and quantity to the user's shopping cart. Creates a new cart if one does not exist.")
		.Produces<CartDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);

		cartGroup.MapDelete("/", async (RemoveFromCartDto data, HttpRequest request, ShopApiContext db) => {
			if (!request.Cookies.TryGetValue("cartId", out var cartIdString) || !int.TryParse(cartIdString, out var cartId)) {
				return Results.NotFound(new { Message = "No cart found." });
			}


			var cart = await db.Carts
				.Include(c => c.Items)
				.FirstOrDefaultAsync(c => c.Id == cartId);

			if (cart is null) {
				return Results.NotFound(new { Message = $"Cart with ID {cartId} not found." });
			}

			var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == data.ProductId);
			if (cartItem is null) {
				return Results.NotFound(new { Message = $"Product with ID {data.ProductId} not found in cart." });
			}

			db.CartItems.Remove(cartItem);
			await db.SaveChangesAsync();

			return Results.Ok(cart.AsDto());
		})
		.WithName("RemoveFromCart")
		.WithDescription("Remove an item from the cart")
		.WithSummary("Removes a specified product from the user's shopping cart.")
		.Produces<CartDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);


	}


}
