using System.ComponentModel.DataAnnotations;
using App.Products;
namespace App.Cart;

public class CartDto {

	public required int Id { get; set; }
	public required List<CartItemDto> Items { get; set; }
	public decimal TotalPrice =>
		Items.Sum(item => item.Product.Price * item.Quantity);
}

public class CartItemDto {
	public required ProductDto Product { get; set; }
	public required int Quantity { get; set; }
}
public class UpdateCartItemDto {
	[Range(1, int.MaxValue)]
	public required int ProductId { get; set; }
	[Range(1, int.MaxValue)]
	public required int Quantity { get; set; }
}

public class AddToCartDto {
	[Range(1, int.MaxValue)]
	public required int ProductId { get; set; }
	[Range(1, int.MaxValue)]
	public required int Quantity { get; set; }
}

