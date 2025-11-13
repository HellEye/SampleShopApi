using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SampleShopApi.App.Data;
using SampleShopApi.App.Utils;
namespace SampleShopApi.App.Entities.Cart;

public static class CartEndpointsExtensions {
	public static void MapCartEndpoints(this WebApplication app) {
		var cartGroup = app.MapGroup("/cart").WithTags("Cart");

		cartGroup.MapGet("/", async (HttpRequest request, GetCartHandler handler) => {
			var res = await handler.Handle(request);
			if (res == null) {
				return Results.NotFound(new { Message = "No cart found." });
			}
			return Results.Ok(res);
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
			AddToCartCommand data,
			HttpRequest request,
			HttpResponse response,
			AddToCartHandler handler
		) => {
			int? cartId = null;
			// Handling cartId differently in AddToCart, since that's the main way to also create a cart
			// Update/delete should not be called without the cartId cookie present
			// For AddToCart, if no cartId cookie is present, we create a new cart
			if (request.Cookies.TryGetValue("cartId", out var cartIdString)) {
				if (int.TryParse(cartIdString, out var parsedCartId)) {
					cartId = parsedCartId;
				}
				else {
					return Result<Cart>
						.BadRequest("Invalid cartId cookie",
						new() {
							["cartId"] = ["The cartId cookie is not a valid integer."]
						}).ToHttpResult();
				}
			}
			var res = await handler.Handle(cartId, data, response);
			return res.ToHttpResult(c => c.AsDto());
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
			UpdateCartCommand data,
			HttpRequest request,
			HttpResponse response,
			UpdateCartHandler handler
		) => {
			if (!request.Cookies.TryGetValue("cartId", out var cartIdString) || !int.TryParse(cartIdString, out var cartId)) {
				return Result<Cart>.NotFound("No cart found.", new() {
					["cartId"] = ["No cartId cookie present."]
				}).ToHttpResult();
			}
			var res = await handler.Handle(cartId, data);
			if (!res.IsSuccess) {
				return res.ToHttpResult();
			}
			response.WithCartIdCookie(res.Value!.Id);
			return res.ToHttpResult(c => c.AsDto());
		})
		.WithName("UpdateCart")
		.WithDescription("Update the quantity of an item in the cart")
		.Produces<CartDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);

		cartGroup.MapDelete("/{productId}", async (
			int productId,
			HttpRequest request,
			HttpResponse response,
			DeleteFromCartHandler handler
		) => {
			if (!request.Cookies.TryGetValue("cartId", out var cartIdString) || !int.TryParse(cartIdString, out var cartId)) {
				return Result<Cart>.NotFound("No cart found.", new() {
					["cartId"] = ["No cartId cookie present."]
				}).ToHttpResult();
			}

			var res = await handler.Handle(cartId, productId);
			if (!res.IsSuccess) {
				return res.ToHttpResult();
			}
			response.WithCartIdCookie(res.Value!.Id);

			return res.ToHttpResult(c => c.AsDto());
		})
		.WithName("RemoveFromCart")
		.WithDescription("Remove an item from the cart")
		.Produces<CartDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);


	}


}
