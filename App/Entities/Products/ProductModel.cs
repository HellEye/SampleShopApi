using App.Utils;
using SampleShopApi.App.Queries;

namespace SampleShopApi.App.Entities.Products;

public class Product : ToDto<ProductDto> {
	public int Id { get; set; }
	[Sortable("name")]
	public required string Name { get; set; }
	[Sortable("price")]
	public required decimal Price { get; set; }
	public string Description { get; set; } = string.Empty;


	public ProductDto AsDto() =>
	  new ProductDto {
		  Id = Id,
		  Name = Name,
		  Price = Price,
		  Description = Description
	  };
}

public record class ProductDto {
	public required int Id { get; set; }
	public required string Name { get; set; }
	public required decimal Price { get; set; }
	public string? Description { get; set; }
}

public enum ProductSortField {
	name,
	price
}

