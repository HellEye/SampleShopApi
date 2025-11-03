using App.Pagination;
using App.Utils;

namespace App.Products;

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
