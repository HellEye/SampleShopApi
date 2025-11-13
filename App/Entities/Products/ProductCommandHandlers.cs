using System.ComponentModel;
using SampleShopApi.App.Data;

namespace SampleShopApi.App.Entities.Products;

public record class CreateProductCommand {
	[DefaultValue("New Product")]
	public required string Name { get; set; }

	[DefaultValue(typeof(decimal), "9.99")]
	public required decimal Price { get; set; }

	[DefaultValue("A brand new product")]
	public string Description { get; set; } = string.Empty;
}

public class CreateProductHandler(ShopApiContext db) {
	public async Task<Product> Handle(CreateProductCommand command) {
		var product = new Product {
			Name = command.Name,
			Price = command.Price,
			Description = command.Description
		};

		db.Products.Add(product);
		await db.SaveChangesAsync();

		return product;
	}
}

public class UpdateProductHandler(ShopApiContext db) {
	public async Task<Product?> Handle(int id, CreateProductCommand command) {
		var product = await db.Products.FindAsync(id);
		if (product == null) {
			return null;
		}

		product.Name = command.Name;
		product.Price = command.Price;
		product.Description = command.Description;

		await db.SaveChangesAsync();

		return product;
	}
}

public class DeleteProductHandler(ShopApiContext db) {
	public async Task<bool> Handle(int id) {
		var product = await db.Products.FindAsync(id);
		if (product == null) {
			return false;
		}

		db.Products.Remove(product);
		await db.SaveChangesAsync();

		return true;
	}
}
