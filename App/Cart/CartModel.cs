using App.Products;
using App.Utils;

namespace App.Cart;

public class Cart : ToDto<CartDto> {
	public required int Id { get; set; }
	public required List<CartItem> Items { get; set; } = [];
	public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	public CartDto AsDto() => new() {
		Id = Id,
		Items = [.. Items.Select(item => item.AsDto())],
	};
}

public class CartItem : ToDto<CartItemDto> {
	public required int Id { get; set; }
	public required int Quantity { get; set; }

	public required int ProductId { get; set; }
	public Product Product { get; set; } = null!;

	public required int CartId { get; set; }
	public Cart Cart { get; set; } = null!;

	public CartItemDto AsDto() => new() {
		Product = Product!.AsDto()!,
		Quantity = Quantity
	};
}
