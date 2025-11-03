using App.Data;
using App.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Products;

public static class ProductEndpoints {
	public static void MapProductEndpoints(this WebApplication app) {
		var products = app.MapGroup("/products").WithTags("Products");

		products.MapGet("/", async ([AsParameters] ProductListQueryDto query, ShopApiContext db) => {
			var res = await db.Products
				.AsNoTracking()
				.WithSearch(query, (p) => [p.Name, p.Description])
				.WithSorting(query)
				.ToPaginated(query);


			return Results.Ok(res);

		})
		.WithParameterValidation()
		.WithName("GetProducts")
		.WithDescription("Get a list of products with optional pagination, search, and sorting")
		.Produces<PaginatedResponse<ProductDto>>(StatusCodes.Status200OK);

		products.MapGet("/{id:int}", async (int id, ShopApiContext db) => {
			var product = await db.Products
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == id);

			if (product == null) {
				return Results.NotFound(new { Message = $"Product with ID {id} not found." });
			}

			return Results.Ok(product.AsDto());
		})
		.WithName("GetProductById")
		.WithDescription("Get a product by its ID")
		.Produces<ProductDto>(StatusCodes.Status200OK)
		.ProducesProblem(StatusCodes.Status404NotFound);

		products.MapPost("/", async (ProductCreateDto productDto, ShopApiContext db) => {
			var product = new Product {
				Name = productDto.Name,
				Price = (decimal)productDto.Price,
				Description = productDto.Description
			};

			db.Products.Add(product);
			await db.SaveChangesAsync();

			return Results.Created($"/products/{product.Id}", product.AsDto());
		})
		.WithName("CreateProduct")
		.WithDescription("Create a new product")
		.WithParameterValidation()
		.Produces<ProductDto>(StatusCodes.Status201Created);

		products.MapPut("/{id:int}", async (int id, ProductCreateDto productDto, ShopApiContext db) => {
			var product = await db.Products.FindAsync(id);
			if (product == null) {
				return Results.NotFound(new { Message = $"Product with ID {id} not found." });
			}

			product.Name = productDto.Name;
			product.Price = productDto.Price;
			product.Description = productDto.Description;

			await db.SaveChangesAsync();

			return Results.Ok(product.AsDto());
		})
		.WithName("UpdateProduct")
		.WithDescription("Update an existing product by its ID")
		.WithParameterValidation()
		.Produces<ProductDto>(StatusCodes.Status200OK);

		products.MapDelete("/{id:int}", async (int id, ShopApiContext db) => {
			var product = await db.Products.FindAsync(id);
			if (product == null) {
				return Results.NotFound(new { Message = $"Product with ID {id} not found." });
			}

			db.Products.Remove(product);
			await db.SaveChangesAsync();

			return Results.NoContent();
		})
		.WithName("DeleteProduct")
		.WithDescription("Delete a product by its ID")
		.WithParameterValidation()
		.Produces(StatusCodes.Status204NoContent);
	}
}
