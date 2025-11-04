using App.Cart;
using App.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.OpenApi.Extensions;

public static class CartEndpointsExtensions {
	public static void MapCartEndpoints(this WebApplication app) {
		var cartGroup = app.MapGroup("/cart").WithTags("Cart");

		cartGroup.MapGet("/", async (HttpRequest request, ShopApiContext db) => {
			if (!request.Cookies.TryGetValue("cartId", out var cartIdString)
			|| !int.TryParse(cartIdString, out var cartId)) {
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
		.Produces<CartDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound)
		.WithOpenApi(options => {
			options.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter {
				Name = "cartId",
				In = Microsoft.OpenApi.Models.ParameterLocation.Cookie,
				Description = "The cartId cookie containing the user's cart ID.",
				Required = false,
				Schema = new Microsoft.OpenApi.Models.OpenApiSchema {
					Type = "string",
					Example = new Microsoft.OpenApi.Any.OpenApiString("cartId=1")
				}
			});
			return options;
		});

		cartGroup.MapPost("/", async (
			AddToCartDto data,
			HttpRequest request,
			HttpResponse response,
			ShopApiContext db
		) => {
			EntityEntry<Cart>? cart = null;
			if (request.Cookies.TryGetValue("cartId", out var cartIdString) && int.TryParse(cartIdString, out var cartId)) {
				var cartModel = await db.Carts
					.Include(c => c.Items)
					.FirstAsync(c => c.Id == cartId);
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
			var cartItemEntry = await db.CartItems.Include(i => i.Product).FirstOrDefaultAsync(i => i.CartId == cart.Entity.Id && i.ProductId == data.ProductId);

			if (cartItemEntry is null) {
				var product = await db.Products.FindAsync(data.ProductId);
				if (product is null) {
					return Results.NotFound(new { Message = $"Product with ID {data.ProductId} not found." });
				}
				cart.Entity.Items.Add(new() {
					CartId = cart.Entity.Id,
					ProductId = data.ProductId,
					Quantity = data.Quantity,
				});
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
		.Produces<List<CartItemDto>>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound)
		.WithOpenApi(options => {
			options.Responses["200"].Headers.Add("Set-Cookie",
			new Microsoft.OpenApi.Models.OpenApiHeader {
				Description = "Sets the cartId cookie for the user's cart.",
				Schema = new Microsoft.OpenApi.Models.OpenApiSchema {
					Type = "string",
					Example = new Microsoft.OpenApi.Any.OpenApiString("cartId=1; Expires=Wed, 21 Oct 2025 07:28:00 GMT; Path=/; SameSite=Strict")
				}
			});
			return options;
		});

		cartGroup.MapPut("/", async (
			UpdateCartItemDto data,
			HttpRequest request,
			HttpResponse response,
			ShopApiContext db
		) => {
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
				cart.Items.Add(new CartItem {
					ProductId = data.ProductId,
					Quantity = data.Quantity,
					CartId = cart.Id
				});
				// return Results.NotFound(new { Message = $"Product with ID {data.ProductId} not found in cart." });
			}
			else {
				cartItem.Quantity = data.Quantity;
			}
			cart.UpdatedAt = DateTime.UtcNow;

			await db.SaveChangesAsync();
			response.Cookies.Append("cartId", cart.Id.ToString(), new CookieOptions {
				SameSite = SameSiteMode.Strict,
				Expires = DateTimeOffset.UtcNow.AddDays(7)
			});

			return Results.Ok(cart.AsDto());
		})
		.WithName("UpdateCart")
		.WithDescription("Update the quantity of an item in the cart")
		.Produces<CartDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);

		cartGroup.MapDelete("/{productId}", async (
			int productId,
			HttpRequest request,
			ShopApiContext db
		) => {
			if (!request.Cookies.TryGetValue("cartId", out var cartIdString) || !int.TryParse(cartIdString, out var cartId)) {
				return Results.NotFound(new { Message = "No cart found." });
			}


			var cart = await db.Carts
				.Include(c => c.Items)
				.FirstOrDefaultAsync(c => c.Id == cartId);

			if (cart is null) {
				return Results.NotFound(new { Message = $"Cart with ID {cartId} not found." });
			}

			var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
			if (cartItem is null) {
				return Results.NotFound(new { Message = $"Product with ID {productId} not found in cart." });
			}

			db.CartItems.Remove(cartItem);
			await db.SaveChangesAsync();

			return Results.Ok(cart.AsDto());
		})
		.WithName("RemoveFromCart")
		.WithDescription("Remove an item from the cart")
		.Produces<CartDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);


	}


}
