using SampleShopApi.App.Data;
using SampleShopApi.App.Queries;
namespace SampleShopApi.App.Entities.Products;

public static class ProductEndpoints {
	public static void MapProductEndpoints(this WebApplication app) {
		var products = app.MapGroup("/products").WithTags("Products");

		products.MapGet("/", async ([AsParameters] ProductListQuery query, GetProductListHandler handler) => {
			return Results.Ok(await handler.HandleAsync(query));
		})
		.WithParameterValidation()
		.WithName("GetProducts")
		.WithDescription("Get a list of products with optional pagination, search, and sorting")
		.Produces<PaginatedResponse<ProductDto>>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status400BadRequest);

		products.MapGet("/{id:int}", async (int id, GetProductByIdHandler handler) => {
			var product = await handler.HandleAsync(id);

			if (product == null) {
				return Results.NotFound(new { Message = $"Product with ID {id} not found." });
			}

			return Results.Ok(product);
		})
		.WithName("GetProductById")
		.WithDescription("Get a product by its ID")
		.Produces<ProductDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);

		products.MapPost("/", async (CreateProductCommand productDto, CreateProductHandler handler) => {
			var product = await handler.Handle(productDto);
			return Results.Created($"/products/{product.Id}", product.AsDto());
		})
		.WithName("CreateProduct")
		.WithDescription("Create a new product")
		.WithParameterValidation()
		.Produces<ProductDto>(StatusCodes.Status201Created);

		products.MapPut("/{id:int}", async (int id, CreateProductCommand productDto, UpdateProductHandler handler) => {
			var product = await handler.Handle(id, productDto);
			if (product == null) {
				return Results.NotFound(new { Message = $"Product with ID {id} not found." });
			}

			return Results.Ok(product.AsDto());
		})
		.WithName("UpdateProduct")
		.WithDescription("Update an existing product by its ID")
		.WithParameterValidation()
		.Produces<ProductDto>(StatusCodes.Status200OK);

		products.MapDelete("/{id:int}", async (int id, DeleteProductHandler handler) => {
			var success = await handler.Handle(id);
			if (!success) {
				return Results.NotFound(new { Message = $"Product with ID {id} not found." });
			}

			return Results.NoContent();
		})
		.WithName("DeleteProduct")
		.WithDescription("Delete a product by its ID")
		.WithParameterValidation()
		.Produces(StatusCodes.Status204NoContent);
	}
}
